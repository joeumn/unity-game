using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Player
{
    /// <summary>
    /// Basic third-person character controller powered by Unity's CharacterController.
    /// Handles walking, sprinting, crouching, and gravity using the new Input System.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField]
        private InputActionAsset inputActions;

        [SerializeField]
        private string actionMapName = "Gameplay";

        [SerializeField]
        private string moveActionName = "Move";

        [SerializeField]
        private string sprintActionName = "Sprint";

        [SerializeField]
        private string crouchActionName = "Crouch";

        [Header("Movement Settings")]
        [SerializeField, Min(0f)]
        private float moveSpeed = 4.5f;

        [SerializeField, Min(1f)]
        private float sprintMultiplier = 1.6f;

        [SerializeField, Range(0.1f, 1f)]
        private float crouchMultiplier = 0.55f;

        [SerializeField, Range(0f, 20f)]
        private float rotationSpeed = 12f;

        [SerializeField]
        private float gravity = -20f;

        [SerializeField]
        private float groundedGravity = -2f;

        [Tooltip("Optional camera transform override. Defaults to Camera.main when left empty.")]
        [SerializeField]
        private Transform cameraTransform;

        private CharacterController controller;
        private InputActionAsset actionsInstance;
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction crouchAction;

        private Vector2 moveInput;
        private bool sprintHeld;
        private bool crouchToggled;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("PlayerCharacter requires a CharacterController component.", this);
            }
        }

        private void OnEnable()
        {
            SetupInput();
        }

        private void OnDisable()
        {
            TeardownInput();
        }

        private void Update()
        {
            HandleMovement(Time.deltaTime);
        }

        /// <summary>
        /// Allows the camera rig to set itself on spawn.
        /// </summary>
        public void SetCameraTransform(Transform cameraRoot)
        {
            cameraTransform = cameraRoot;
        }

        private void SetupInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("PlayerCharacter has no InputActionAsset assigned.", this);
                return;
            }

            actionsInstance = Instantiate(inputActions);
            var map = actionsInstance.FindActionMap(actionMapName, true);

            moveAction = map.FindAction(moveActionName, true);
            sprintAction = map.FindAction(sprintActionName, true);
            crouchAction = map.FindAction(crouchActionName, true);

            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;

            sprintAction.performed += OnSprintPerformed;
            sprintAction.canceled += OnSprintCanceled;

            crouchAction.performed += OnCrouchPerformed;

            map.Enable();
        }

        private void TeardownInput()
        {
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }

            if (sprintAction != null)
            {
                sprintAction.performed -= OnSprintPerformed;
                sprintAction.canceled -= OnSprintCanceled;
            }

            if (crouchAction != null)
            {
                crouchAction.performed -= OnCrouchPerformed;
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

            moveAction = null;
            sprintAction = null;
            crouchAction = null;
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            moveInput = Vector2.zero;
        }

        private void OnSprintPerformed(InputAction.CallbackContext ctx)
        {
            sprintHeld = ctx.ReadValueAsButton();
        }

        private void OnSprintCanceled(InputAction.CallbackContext ctx)
        {
            sprintHeld = false;
        }

        private void OnCrouchPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValueAsButton())
            {
                crouchToggled = !crouchToggled;
            }
        }

        private void HandleMovement(float deltaTime)
        {
            if (controller == null)
            {
                return;
            }

            Transform reference = ResolveCameraTransform();
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (reference != null)
            {
                forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;

                if (forward.sqrMagnitude < 0.01f)
                {
                    forward = Vector3.forward;
                }

                if (right.sqrMagnitude < 0.01f)
                {
                    right = Vector3.right;
                }
            }

            Vector3 movement = (right * moveInput.x) + (forward * moveInput.y);
            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            float speed = moveSpeed;
            if (crouchToggled)
            {
                speed *= crouchMultiplier;
            }
            else if (sprintHeld)
            {
                speed *= sprintMultiplier;
            }

            Vector3 horizontalVelocity = movement * speed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedGravity;
            }

            verticalVelocity += gravity * deltaTime;
            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(velocity * deltaTime);

            if (movement.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }

        private Transform ResolveCameraTransform()
        {
            if (cameraTransform != null)
            {
                return cameraTransform;
            }

            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            return cameraTransform;
        }
    }
}
