using UnityEngine;
using UnityEngine.InputSystem;

namespace MuseumGetaway
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float shoulderOffset = 1.2f;
        [SerializeField] private float lookHeight = 1.45f;
        [SerializeField] private float initialPitch = 20f;
        [SerializeField] private float minimumPitch = -10f;
        [SerializeField] private float maximumPitch = 65f;
        [SerializeField] private float horizontalSensitivity = 150f;
        [SerializeField] private float verticalSensitivity = 110f;
        [SerializeField] private float inputScale = 0.01f;
        [SerializeField] private float orbitSmoothTime = 0.05f;
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private LayerMask collisionMask = 1 << 8;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private float collisionPadding = 0.18f;
        [SerializeField] private float minimumDistance = 1.35f;
        [SerializeField] private float collisionInSmoothTime = 0.05f;
        [SerializeField] private float collisionOutSmoothTime = 0.3f;

        private InputAction lookAction;
        private InputAction cameraOrbitAction;
        private InputAction shiftLockAction;
        private Vector3 positionVelocity;
        private float targetYaw;
        private float targetPitch;
        private float currentYaw;
        private float currentPitch;
        private float yawVelocity;
        private float pitchVelocity;
        private bool initialized;
        private bool shiftLockActive;

        public Vector3 HorizontalForward =>
            Quaternion.Euler(0f, currentYaw, 0f) * Vector3.forward;
        public bool IsShiftLockActive => shiftLockActive;

        public void Configure(
            MuseumGameManager manager,
            Transform followTarget,
            float orbitDistance,
            float cameraShoulderOffset,
            float cameraLookHeight,
            float startingPitch,
            float yawSensitivity,
            float pitchSensitivity)
        {
            gameManager = manager;
            target = followTarget;
            distance = orbitDistance;
            shoulderOffset = cameraShoulderOffset;
            lookHeight = cameraLookHeight;
            initialPitch = startingPitch;
            horizontalSensitivity = yawSensitivity;
            verticalSensitivity = pitchSensitivity;
            InitializeOrbit();
            FindLookAction();
            SnapToTarget();
        }

        private void Start()
        {
            InitializeOrbit();
            FindLookAction();
            SetShiftLock(false);
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            ReadOrbitInput();
            currentYaw = Mathf.SmoothDampAngle(
                currentYaw,
                targetYaw,
                ref yawVelocity,
                orbitSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
            currentPitch = Mathf.SmoothDampAngle(
                currentPitch,
                targetPitch,
                ref pitchVelocity,
                orbitSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            Vector3 lookTarget = GetLookTarget();
            Vector3 unobstructedPosition = GetUnobstructedPosition();
            Vector3 desiredPosition = ResolveCameraPosition(
                lookTarget,
                unobstructedPosition,
                out bool isObstructed);

            float fullDistance = Vector3.Distance(lookTarget, unobstructedPosition);
            float currentDistance = Vector3.Distance(lookTarget, transform.position);
            float smoothTime = isObstructed
                ? collisionInSmoothTime
                : currentDistance < fullDistance - collisionPadding
                    ? collisionOutSmoothTime
                    : positionSmoothTime;

            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            // Smoothing must never leave the camera on the far side of a wall.
            transform.position = ResolveCameraPosition(lookTarget, smoothedPosition, out _);
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        }

        private void InitializeOrbit()
        {
            if (target == null || initialized)
            {
                return;
            }

            targetYaw = target.eulerAngles.y;
            currentYaw = targetYaw;
            targetPitch = Mathf.Clamp(initialPitch, minimumPitch, maximumPitch);
            currentPitch = targetPitch;
            initialized = true;
        }

        private void FindLookAction()
        {
            if (target == null)
            {
                return;
            }

            PlayerInput playerInput = target.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                return;
            }

            lookAction = playerInput.actions?.FindAction("Look", false);
            cameraOrbitAction = playerInput.actions?.FindAction("CameraOrbit", false);
            shiftLockAction = playerInput.actions?.FindAction("ShiftLock", false);
        }

        private void ReadOrbitInput()
        {
            if (gameManager != null && !gameManager.IsPlaying)
            {
                if (shiftLockActive)
                {
                    SetShiftLock(false);
                }
                return;
            }

            bool shiftPressed = shiftLockAction != null
                ? shiftLockAction.WasPressedThisFrame()
                : Keyboard.current != null &&
                  (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                   Keyboard.current.rightShiftKey.wasPressedThisFrame);
            if (shiftPressed)
            {
                SetShiftLock(!shiftLockActive);
            }

            bool rightMouseHeld = cameraOrbitAction != null
                ? cameraOrbitAction.IsPressed()
                : Mouse.current != null && Mouse.current.rightButton.isPressed;
            if (!shiftLockActive && !rightMouseHeld)
            {
                return;
            }

            Vector2 look = lookAction != null
                ? lookAction.ReadValue<Vector2>()
                : Mouse.current != null
                    ? Mouse.current.delta.ReadValue()
                    : Vector2.zero;

            targetYaw += look.x * horizontalSensitivity * inputScale;
            targetPitch = Mathf.Clamp(
                targetPitch - look.y * verticalSensitivity * inputScale,
                minimumPitch,
                maximumPitch);
        }

        private void SetShiftLock(bool active)
        {
            bool changed = shiftLockActive != active;
            shiftLockActive = active;
            Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !active;
            if (changed)
            {
                Debug.Log($"Shift Lock: {(active ? "ON" : "OFF")}");
            }
        }

        private void OnDisable()
        {
            SetShiftLock(false);
        }

        private void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            Vector3 lookTarget = GetLookTarget();
            transform.position = ResolveCameraPosition(
                lookTarget,
                GetUnobstructedPosition(),
                out _);
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            positionVelocity = Vector3.zero;
        }

        private Vector3 GetLookTarget()
        {
            return target.position + Vector3.up * lookHeight;
        }

        private Vector3 GetUnobstructedPosition()
        {
            Quaternion orbit = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 orbitForward = orbit * Vector3.forward;
            Vector3 horizontalRight = Quaternion.Euler(0f, currentYaw, 0f) * Vector3.right;
            return GetLookTarget()
                - orbitForward * distance
                + horizontalRight * shoulderOffset;
        }

        private Vector3 ResolveCameraPosition(
            Vector3 lookTarget,
            Vector3 desiredPosition,
            out bool isObstructed)
        {
            isObstructed = false;
            Vector3 offset = desiredPosition - lookTarget;
            float desiredDistance = offset.magnitude;
            if (desiredDistance <= 0.001f)
            {
                return desiredPosition;
            }

            Vector3 direction = offset / desiredDistance;
            RaycastHit[] hits = Physics.SphereCastAll(
                lookTarget,
                collisionRadius,
                direction,
                desiredDistance,
                collisionMask,
                QueryTriggerInteraction.Collide);

            float allowedDistance = desiredDistance;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == target || hit.transform.IsChildOf(target))
                {
                    continue;
                }

                isObstructed = true;
                allowedDistance = Mathf.Min(
                    allowedDistance,
                    Mathf.Max(minimumDistance, hit.distance - collisionPadding));
            }

            return lookTarget + direction * allowedDistance;
        }
    }
}
