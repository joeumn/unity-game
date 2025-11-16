using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Raycast-based gadget that detects and triggers interactables from the camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class HackGadget : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private InputActionAsset inputActions;

        [SerializeField]
        private string actionMapName = "Gameplay";

        [SerializeField]
        private string hackActionName = "Hack";

        [Header("Targeting")]
        [SerializeField]
        private float maxDistance = 12f;

        [SerializeField]
        private LayerMask interactableLayers = ~0;

        [SerializeField]
        private float cooldownSeconds = 0.75f;

        [SerializeField]
        private bool drawDebugRay;

        [Tooltip("Optional explicit interactor. Defaults to the root object of this camera.")]
        [SerializeField]
        private GameObject interactorOverride;

        private Camera sourceCamera;
        private InputActionAsset actionsInstance;
        private InputAction hackAction;
        private float cooldownTimer;
        private IInteractable currentTarget;
        private GameObject resolvedInteractor;

        public event Action<IInteractable> OnTargetChanged;
        public event Action<IInteractable> OnHackPerformed;

        private void Awake()
        {
            sourceCamera = GetComponent<Camera>();
            resolvedInteractor = interactorOverride != null ? interactorOverride : transform.root.gameObject;
        }

        private void OnEnable()
        {
            SetupInput();
        }

        private void Update()
        {
            cooldownTimer -= Time.deltaTime;
            AcquireTarget();
        }

        private void OnDisable()
        {
            ClearCurrentTarget();
            TeardownInput();
        }

        public void SetInteractor(GameObject newInteractor)
        {
            interactorOverride = newInteractor;
            resolvedInteractor = newInteractor;
        }

        private void SetupInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("HackGadget has no InputActionAsset assigned.", this);
                return;
            }

            actionsInstance = Instantiate(inputActions);
            var map = actionsInstance.FindActionMap(actionMapName, true);
            hackAction = map.FindAction(hackActionName, true);
            hackAction.performed += OnHackPerformedInput;
            map.Enable();
        }

        private void TeardownInput()
        {
            if (hackAction != null)
            {
                hackAction.performed -= OnHackPerformedInput;
            }

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

            hackAction = null;
        }

        private void OnHackPerformedInput(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            TryHack();
        }

        private void TryHack()
        {
            if (cooldownTimer > 0f || currentTarget == null)
            {
                return;
            }

            var owner = resolvedInteractor != null ? resolvedInteractor : gameObject;
            if (!currentTarget.CanInteract(owner))
            {
                return;
            }

            currentTarget.Interact(owner);
            cooldownTimer = cooldownSeconds;
            OnHackPerformed?.Invoke(currentTarget);
        }

        private void AcquireTarget()
        {
            if (sourceCamera == null)
            {
                return;
            }

            Ray ray = sourceCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayers, QueryTriggerInteraction.Ignore))
            {
                if (drawDebugRay)
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.cyan);
                }

                var target = hit.collider.GetComponentInParent<IInteractable>();
                UpdateTarget(target);
            }
            else
            {
                if (drawDebugRay)
                {
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.magenta);
                }

                UpdateTarget(null);
            }
        }

        private void UpdateTarget(IInteractable newTarget)
        {
            if (ReferenceEquals(currentTarget, newTarget))
            {
                return;
            }

            if (currentTarget != null)
            {
                currentTarget.Highlight(false);
            }

            currentTarget = newTarget;

            if (currentTarget != null)
            {
                currentTarget.Highlight(true);
            }

            OnTargetChanged?.Invoke(currentTarget);
        }

        private void ClearCurrentTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.Highlight(false);
                currentTarget = null;
                OnTargetChanged?.Invoke(null);
            }
        }
    }
}
