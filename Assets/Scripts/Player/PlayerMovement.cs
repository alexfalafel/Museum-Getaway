using UnityEngine;
using UnityEngine.InputSystem;

namespace MuseumGetaway
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 12f;

        private CharacterController controller;
        private InputAction moveAction;
        private float verticalVelocity;

        public void Configure(MuseumGameManager manager, float speed)
        {
            gameManager = manager;
            moveSpeed = speed;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            PlayerInput playerInput = GetComponent<PlayerInput>();
            moveAction = playerInput != null
                ? playerInput.actions?.FindAction("Move", false)
                : null;
        }

        private void Update()
        {
            if (!gameManager.IsPlaying)
            {
                return;
            }

            Vector2 input = moveAction != null
                ? moveAction.ReadValue<Vector2>()
                : ReadKeyboardFallback();
            input = Vector2.ClampMagnitude(input, 1f);

            Camera activeCamera = Camera.main;
            Vector3 forward = Vector3.forward;
            SimpleFollowCamera followCamera = null;
            if (activeCamera != null)
            {
                followCamera = activeCamera.GetComponent<SimpleFollowCamera>();
                forward = followCamera != null
                    ? followCamera.HorizontalForward
                    : activeCamera.transform.forward;
            }

            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            // Rebuild a level, perpendicular right axis from the same camera heading.
            // This keeps W/S and A/D in one stable camera-relative frame.
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 movement = forward * input.y + right * input.x;
            bool shiftLock = followCamera != null && followCamera.IsShiftLockActive;
            Vector3 facingDirection = shiftLock ? forward : movement;
            if (facingDirection.sqrMagnitude > 0.001f &&
                (shiftLock || movement.sqrMagnitude > 0.001f))
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(facingDirection),
                    turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            Vector3 velocity = movement * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private Vector2 ReadKeyboardFallback()
        {
            if (Keyboard.current == null)
            {
                return Vector2.zero;
            }

            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            return input;
        }
    }
}
