using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.Player
{
    /// <summary>
    /// Kontroler gerak dasar pemain berbasis CharacterController:
    /// gerak relatif terhadap kamera, percepatan/perlambatan halus, deteksi tanah stabil, dan rotasi visual terpisah.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float MinSmoothTime = 0.01f;
        private const float GroundCheckRadiusScale = 0.95f;
        private const float InputDeadzoneSqr = 0.0001f;
        private const float MinRotationSpeedSqr = 0.01f;
        private const float MinMoveSqr = 0.0001f;
        private const float FallbackGizmoCheckDistance = 0.15f;

        [Header("Referensi")]
        [Tooltip("Aset pengaturan gerak pemain (ScriptableObject).")]
        [SerializeField] private PlayerMovementSettings settings;

        [Tooltip("Transform wadah visual karakter (hanya objek ini yang berputar, bukan root).")]
        [SerializeField] private Transform visuals;

        [Tooltip("Transform kamera sebagai acuan arah gerak. Jika kosong, otomatis memakai Main Camera.")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController _controller;
        private PlayerCameraController _cameraController;
        private PlayerInputActions _input;
        private Vector2 _moveInput;
        private bool _jumpPressedThisFrame;
        private bool _jumpReleasedThisFrame;
        private bool _jumpHeld;
        private bool _sprintHeld;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private Vector3 _groundNormal = Vector3.up;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _lastInContextPov;
        private bool _lockMoveBasisUntilRelease;
        private Vector3 _lockedCamForward = Vector3.forward;
        private Vector3 _lockedCamRight = Vector3.right;

        /// <summary>True jika karakter sedang berdiri di atas permukaan yang valid.</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>Jika true, input gerak sementara ditahan (misal saat momen alur kamera POV 2 berlangsung).</summary>
        public bool InputLocked { get; set; }

        /// <summary>True jika karakter sedang naik di udara (lompat).</summary>
        public bool IsJumping => !IsGrounded && _verticalVelocity > 0f;

        /// <summary>True jika karakter sedang melayang (glide) di udara.</summary>
        public bool IsGliding { get; private set; }

        /// <summary>True jika karakter sedang berlari cepat (Shift).</summary>
        public bool IsSprinting => IsGrounded && _sprintHeld && _moveInput.sqrMagnitude > InputDeadzoneSqr;

        /// <summary>Besar kecepatan mendatar karakter saat ini (m/s), berguna untuk animasi.</summary>
        public float HorizontalSpeed => _horizontalVelocity.magnitude;

        /// <summary>Vektor kecepatan gabungan (mendatar + vertikal) karakter saat ini.</summary>
        public Vector3 Velocity => _horizontalVelocity + Vector3.up * _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _cameraController = GetComponent<PlayerCameraController>();

            // Hentikan komponen jika aset settings belum dipasang di Inspector
            if (settings == null)
            {
                Debug.LogError($"[{nameof(PlayerController)}] PlayerMovementSettings belum diisi pada '{name}'. Komponen dinonaktifkan.", this);
                enabled = false;
                return;
            }

            _input = new PlayerInputActions();

            // Selaraskan parameter lereng & tangga CharacterController dengan settings
            _controller.slopeLimit = settings.SlopeLimit;
            _controller.stepOffset = settings.StepOffset;
        }

        private void Start()
        {
            // Cache Main Camera sekali saja di Start agar tidak mencari kamera di setiap Update
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            _input?.Player.Enable();
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
        }

        private void OnDestroy()
        {
            _input?.Dispose();
        }

        private void Update()
        {
            // Jangan proses input atau gerak saat permainan sedang dijeda
            if (Time.timeScale <= 0f)
            {
                return;
            }

            float dt = Time.deltaTime;

            ReadInput();
            UpdateGrounded();
            UpdateJumpTimers(dt);
            UpdateHorizontalVelocity(dt);
            UpdateVerticalVelocity(dt);
            ApplyMovement(dt);
            RotateVisuals(dt);
        }

        private void ReadInput()
        {
            if (InputLocked)
            {
                _moveInput = Vector2.zero;
                _jumpPressedThisFrame = false;
                _jumpReleasedThisFrame = false;
                _jumpHeld = false;
                _sprintHeld = false;
                return;
            }

            // Baca input langsung setiap frame tanpa callback dan tanpa alokasi memori
            _moveInput = _input.Player.Move.ReadValue<Vector2>();
            _jumpPressedThisFrame = _input.Player.Jump.WasPressedThisFrame();
            _jumpReleasedThisFrame = _input.Player.Jump.WasReleasedThisFrame();
            _jumpHeld = _input.Player.Jump.IsPressed();
            _sprintHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        }

        private void UpdateGrounded()
        {
            // Titik tengah CharacterController dalam koordinat dunia
            Vector3 origin = transform.TransformPoint(_controller.center);
            float radius = _controller.radius * GroundCheckRadiusScale;
            float castDistance = (_controller.height * 0.5f - radius) + settings.GroundCheckDistance;

            bool hasHit = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out RaycastHit hit,
                castDistance,
                settings.GroundMask,
                QueryTriggerInteraction.Ignore
            );

            // Dianggap menapak tanah jika mengenai collider DAN kemiringan lereng masih dalam batas slopeLimit
            if (hasHit && Vector3.Angle(hit.normal, Vector3.up) <= settings.SlopeLimit)
            {
                IsGrounded = true;
                _groundNormal = hit.normal;
            }
            else
            {
                IsGrounded = false;
                _groundNormal = Vector3.up;
            }
        }

        private void UpdateHorizontalVelocity(float dt)
        {
            bool hasInput = _moveInput.sqrMagnitude > InputDeadzoneSqr;
            bool inContextPov = _cameraController != null && _cameraController.IsInContextPov;

            // Jika POV berubah saat tombol gerak sedang ditahan, kunci arah acuan kamera sementara
            // sampai tombol dilepas agar karakter tidak mendadak berbelok/bolak-balik di batas zona
            if (inContextPov != _lastInContextPov)
            {
                if (hasInput)
                {
                    _lockMoveBasisUntilRelease = true;
                }
                _lastInContextPov = inContextPov;
            }

            if (!hasInput)
            {
                _lockMoveBasisUntilRelease = false;
            }

            Vector3 camForward = _lockedCamForward;
            Vector3 camRight = _lockedCamRight;

            // Perbarui arah acuan dari kamera aktif jika tidak sedang dikunci saat transisi POV
            if (!_lockMoveBasisUntilRelease && cameraTransform != null)
            {
                camForward = cameraTransform.forward;
                camForward.y = 0f;
                if (camForward.sqrMagnitude > MinMoveSqr)
                {
                    camForward.Normalize();
                }

                camRight = cameraTransform.right;
                camRight.y = 0f;
                if (camRight.sqrMagnitude > MinMoveSqr)
                {
                    camRight.Normalize();
                }

                _lockedCamForward = camForward;
                _lockedCamRight = camRight;
            }

            // Batasi panjang arah maksimal 1 agar gerak diagonal tidak lebih cepat dari gerak lurus
            Vector3 moveDirection = Vector3.ClampMagnitude(camForward * _moveInput.y + camRight * _moveInput.x, 1f);
            float groundSpeed = _sprintHeld ? settings.SprintSpeed : settings.MoveSpeed;
            float maxSpeed = IsGliding ? settings.GlideMoveSpeed : groundSpeed;
            Vector3 targetVelocity = moveDirection * maxSpeed;

            // Pilih waktu percepatan saat ada input (atau saat glide), dan perlambatan saat tombol dilepas
            float baseTime = IsGliding
                ? settings.GlideAccelerationTime
                : (hasInput ? settings.AccelerationTime : settings.DecelerationTime);
            float smoothTime = Mathf.Max(baseTime, MinSmoothTime);
            float changeRate = maxSpeed / smoothTime;

            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, changeRate * dt);
        }

        private void UpdateJumpTimers(float dt)
        {
            // Isi ulang coyote time saat menapak tanah dan tidak sedang naik
            if (IsGrounded && _verticalVelocity <= 0f)
            {
                _coyoteTimer = settings.CoyoteTime;
            }
            else if (_coyoteTimer > 0f)
            {
                _coyoteTimer -= dt;
            }

            // Catat input lompat ke buffer agar tombol yang ditekan sesaat sebelum mendarat tetap terhitung
            if (_jumpPressedThisFrame)
            {
                _jumpBufferTimer = settings.JumpBufferTime;
            }
            else if (_jumpBufferTimer > 0f)
            {
                _jumpBufferTimer -= dt;
            }
        }

        private void UpdateVerticalVelocity(float dt)
        {
            // Eksekusi lompat jika buffer aktif dan masih dalam coyote time
            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                // Rumus kinematika v = sqrt(-2 * g * h) agar tinggi lompat sesuai JumpHeight
                _verticalVelocity = Mathf.Sqrt(-2f * settings.Gravity * settings.JumpHeight);
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                IsGrounded = false;
                IsGliding = false;
                return;
            }

            // Potong kecepatan naik jika tombol lompat dilepas lebih awal (tinggi lompat variabel)
            if (_jumpReleasedThisFrame && _verticalVelocity > 0f)
            {
                _verticalVelocity *= settings.JumpCutMultiplier;
            }

            if (IsGrounded && _verticalVelocity < 0f)
            {
                // Gaya tekan kecil agar karakter menempel stabil saat menuruni lereng
                _verticalVelocity = settings.GroundedStickForce;
                IsGliding = false;
            }
            else
            {
                // Aktifkan glide jika tombol lompat ditahan saat sedang jatuh di udara
                IsGliding = settings.EnableGlide && !IsGrounded && _jumpHeld && _verticalVelocity < 0f;

                if (IsGliding)
                {
                    _verticalVelocity = Mathf.MoveTowards(_verticalVelocity, settings.GlideFallSpeed, Mathf.Abs(settings.Gravity) * dt);
                }
                else
                {
                    _verticalVelocity += settings.Gravity * dt;
                }
            }

            _verticalVelocity = Mathf.Max(_verticalVelocity, settings.MaxFallSpeed);
        }

        private void ApplyMovement(float dt)
        {
            Vector3 horizontal = _horizontalVelocity;

            // Proyeksikan kecepatan mendatar ke bidang lereng agar kecepatan tetap stabil dan tidak memantul
            if (IsGrounded && horizontal.sqrMagnitude > MinMoveSqr)
            {
                float speed = horizontal.magnitude;
                horizontal = Vector3.ProjectOnPlane(horizontal, _groundNormal).normalized * speed;
            }

            Vector3 motion = (horizontal + Vector3.up * _verticalVelocity) * dt;
            _controller.Move(motion);
        }

        private void RotateVisuals(float dt)
        {
            if (visuals == null || _horizontalVelocity.sqrMagnitude < MinRotationSpeedSqr)
            {
                return;
            }

            Vector3 flatDirection = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
            visuals.rotation = Quaternion.RotateTowards(visuals.rotation, targetRotation, settings.RotationSpeed * dt);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            CharacterController controller = _controller != null ? _controller : GetComponent<CharacterController>();
            if (controller == null)
            {
                return;
            }

            Vector3 origin = transform.TransformPoint(controller.center);
            float radius = controller.radius * GroundCheckRadiusScale;
            float checkDistance = settings != null ? settings.GroundCheckDistance : FallbackGizmoCheckDistance;
            float castDistance = (controller.height * 0.5f - radius) + checkDistance;
            Vector3 footSphereCenter = origin + Vector3.down * castDistance;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(footSphereCenter, radius);
        }
#endif
    }
}
