using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Player
{
    /// <summary>
    /// Simple over-the-shoulder orbit camera driven by the new Input System.
    /// Keeps a shoulder offset and clamps pitch to avoid flipping over the player.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlayerCameraRig : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Vector3 shoulderOffset = new Vector3(0.5f, 1.6f, 0f);

        [SerializeField]
        private float followDistance = 4.5f;

        [SerializeField]
        private float followSmoothing = 12f;

        [Header("Look Settings")]
        [SerializeField]
        private InputActionAsset inputActions;

        [SerializeField]
        private string actionMapName = "Gameplay";

        [SerializeField]
        private string lookActionName = "Look";

        [SerializeField]
        private float horizontalSensitivity = 120f;

        [SerializeField]
        private float verticalSensitivity = 90f;

        [SerializeField]
        private float minVerticalAngle = -60f;

        [SerializeField]
        private float maxVerticalAngle = 75f;

        private Camera cachedCamera;
        private InputActionAsset actionsInstance;
        private InputAction lookAction;
        private Vector2 lookInput;
        private float yaw;
        private float pitch;

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }

        private void OnEnable()
        {
            SetupInput();
        }

        private void Start()
        {
            AlignWithTargetInstantly();
        }

        private void OnDisable()
        {
            TeardownInput();
        }

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            AlignWithTargetInstantly();
        }

        private void Update()
        {
            if (lookAction == null)
            {
                return;
            }

            lookInput = lookAction.ReadValue<Vector2>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            yaw += lookInput.x * horizontalSensitivity * deltaTime;
            pitch -= lookInput.y * verticalSensitivity * deltaTime;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * shoulderOffset.y;
            Vector3 offset = rotation * new Vector3(shoulderOffset.x, 0f, -followDistance);
            Vector3 desiredPosition = pivot + offset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSmoothing * deltaTime));
            transform.rotation = rotation;
        }

        private void SetupInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("PlayerCameraRig has no InputActionAsset assigned.", this);
                return;
            }

            actionsInstance = Instantiate(inputActions);
            var map = actionsInstance.FindActionMap(actionMapName, true);
            lookAction = map.FindAction(lookActionName, true);
            map.Enable();
        }

        private void TeardownInput()
        {
            if (actionsInstance != null)
            {
                actionsInstance.Disable();
#if UNITY_EDITOR
                DestroyImmediate(actionsInstance);
#else
                Destroy(actionsInstance);
#endif
                actionsInstance = null;
            }

            lookAction = null;
        }

        private void AlignWithTargetInstantly()
        {
            if (target == null)
            {
                return;
            }

            Vector3 euler = target.eulerAngles;
            yaw = euler.y;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            transform.position = target.position + Vector3.up * shoulderOffset.y - target.forward * followDistance;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
