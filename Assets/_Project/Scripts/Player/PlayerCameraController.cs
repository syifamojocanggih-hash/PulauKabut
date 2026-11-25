using PulauKabut.Lantern;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.Player
{
    /// <summary>
    /// Mengendalikan kamera manual pemain (toggle V antara Third-Person di belakang karakter
    /// dan First-Person dari mata karakter), penguncian kursor, serta visibilitas Visuals otomatis.
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        private const int ActiveManualPriority = 11;
        private const int InactiveManualPriority = 10;

        [Header("Referensi")]
        [Tooltip("Aset pengaturan pemain yang menyimpan sensitivitas, damping, jarak, dan batas sudut kamera.")]
        [SerializeField] private PlayerMovementSettings settings;

        [Tooltip("Titik pivot mata/bahu karakter (CameraTarget) yang diikuti kamera Third-Person dan First-Person.")]
        [SerializeField] private Transform cameraTarget;

        [Tooltip("Objek Visuals karakter. Otomatis disembunyikan saat kamera tepat di mata (First-Person) dan tampil penuh saat Third-Person/Plot.")]
        [SerializeField] private GameObject visualsRoot;

        [Tooltip("Tangan kanan & lentera khusus mode First-Person (child dari CameraTarget). Tampil saat mode First-Person & mengangkat lentera.")]
        [SerializeField] private Transform firstPersonHandRoot;

        [Header("Animasi Tangan First-Person")]
        [SerializeField] private Vector3 fpHandLitLocalPos = new Vector3(0.36f, -0.26f, 0.56f);
        [SerializeField] private Vector3 fpHandUnlitLocalPos = new Vector3(0.36f, -0.68f, 0.42f);
        [SerializeField] private float fpHandMoveSpeed = 10f;

        [Header("Kamera Manual (Cinemachine 3)")]
        [Tooltip("Kamera Third-Person di belakang karakter.")]
        [SerializeField] private CinemachineCamera thirdPersonCamera;

        [Tooltip("Kamera First-Person dari pandangan mata karakter.")]
        [SerializeField] private CinemachineCamera firstPersonCamera;

        [Header("Kursor")]
        [Tooltip("Kunci dan sembunyikan kursor otomatis saat mulai bermain.")]
        [SerializeField] private bool lockCursorOnStart = true;

        private PlayerInputActions _input;
        private Transform _mainCameraTransform;
        private CinemachineThirdPersonFollow _thirdPersonFollow;
        private LanternController _lanternController;
        private PlayerController _playerController;
        private float _fpSwayTime;
        private float _targetYaw;
        private float _targetPitch;
        private float _currentYaw;
        private float _currentPitch;
        private bool _isCursorLocked;
        private bool _isVisualsVisible = true;
        private bool _isFirstPerson;

        /// <summary>True jika kursor sedang terkunci untuk kontrol kamera.</summary>
        public bool IsCursorLocked => _isCursorLocked;

        /// <summary>True jika pemain sedang memilih mode First-Person (pandangan mata).</summary>
        public bool IsFirstPerson => _isFirstPerson;

        /// <summary>True jika kamera aktif saat ini bukan First-Person (Third-Person atau POV 2 Plot).</summary>
        public bool IsInContextPov => !_isFirstPerson && _isVisualsVisible;

        private void Awake()
        {
            if (settings == null || cameraTarget == null)
            {
                Debug.LogError($"[{nameof(PlayerCameraController)}] Settings atau CameraTarget belum diisi pada '{name}'. Komponen dinonaktifkan.", this);
                enabled = false;
                return;
            }

            _input = new PlayerInputActions();
            _lanternController = GetComponent<LanternController>();
            _playerController = GetComponent<PlayerController>();

            if (thirdPersonCamera != null)
            {
                _thirdPersonFollow = thirdPersonCamera.GetComponent<CinemachineThirdPersonFollow>();
            }

            // Ambil sudut awal dari CameraTarget agar tidak melompat saat mulai bermain
            Vector3 initialEuler = cameraTarget.eulerAngles;
            _targetYaw = initialEuler.y;
            _currentYaw = initialEuler.y;

            float normalizedPitch = initialEuler.x > 180f ? initialEuler.x - 360f : initialEuler.x;
            _targetPitch = Mathf.Clamp(normalizedPitch, settings.MinPitch, settings.MaxPitch);
            _currentPitch = _targetPitch;

            _isFirstPerson = settings.StartInFirstPerson;
            ApplyManualCameraMode();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }

            if (lockCursorOnStart)
            {
                SetCursorLocked(true);
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
            // Jangan memutar kamera jika game dijeda atau kursor sedang bebas
            if (Time.timeScale <= 0f || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            // Toggle manual antara Third-Person (belakang karakter) dan First-Person (mata karakter) dengan tombol V
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            {
                ToggleManualPov();
            }

            // Terapkan jarak Third-Person secara live dari ScriptableObject
            if (_thirdPersonFollow != null)
            {
                _thirdPersonFollow.CameraDistance = settings.ThirdPersonDistance;
            }

            // Baca input Look langsung tanpa alokasi memori (berlaku untuk Third-Person maupun First-Person)
            Vector2 lookInput = _input.Player.Look.ReadValue<Vector2>();
            float sensitivity = settings.LookSensitivity;

            _targetYaw += lookInput.x * sensitivity;
            float pitchDelta = lookInput.y * sensitivity * (settings.InvertY ? 1f : -1f);
            _targetPitch = Mathf.Clamp(_targetPitch + pitchDelta, settings.MinPitch, settings.MaxPitch);

            // Terapkan damping halus yang tidak bergantung pada framerate
            if (settings.LookSmoothing > 0f)
            {
                float t = 1f - Mathf.Exp(-settings.LookSmoothing * Time.deltaTime);
                _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, t);
                _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, t);
            }
            else
            {
                _currentYaw = _targetYaw;
                _currentPitch = _targetPitch;
            }

            cameraTarget.rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        private void LateUpdate()
        {
            UpdateVisualsVisibility();
            UpdateFirstPersonHand();
        }

        /// <summary>Berpindah antara kamera belakang karakter (Third-Person) dan pandangan mata (First-Person).</summary>
        public void ToggleManualPov()
        {
            _isFirstPerson = !_isFirstPerson;
            ApplyManualCameraMode();
        }

        /// <summary>Mengunci atau membebaskan kursor (juga dipanggil oleh GameManager saat jeda/resume).</summary>
        public void SetCursorLocked(bool locked)
        {
            _isCursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void ApplyManualCameraMode()
        {
            if (firstPersonCamera != null)
            {
                firstPersonCamera.Priority.Enabled = true;
                firstPersonCamera.Priority.Value = _isFirstPerson ? ActiveManualPriority : InactiveManualPriority;
            }

            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.Priority.Enabled = true;
                thirdPersonCamera.Priority.Value = _isFirstPerson ? InactiveManualPriority : ActiveManualPriority;
            }
        }

        private void UpdateVisualsVisibility()
        {
            if (visualsRoot == null || _mainCameraTransform == null)
            {
                return;
            }

            // Sembunyikan tubuh karakter hanya saat lensa kamera berada tepat di kepala (First-Person)
            float hideDistSqr = settings.FirstPersonHideDistance * settings.FirstPersonHideDistance;
            float currentDistSqr = (_mainCameraTransform.position - cameraTarget.position).sqrMagnitude;
            bool shouldShowVisuals = currentDistSqr >= hideDistSqr;

            if (shouldShowVisuals != _isVisualsVisible)
            {
                _isVisualsVisible = shouldShowVisuals;
                visualsRoot.SetActive(_isVisualsVisible);
            }
        }

        private void UpdateFirstPersonHand()
        {
            if (firstPersonHandRoot == null)
            {
                return;
            }

            // Tangan First-Person hanya tampil ketika tubuh penuh sedang disembunyikan (lensa di mata)
            bool showFpHand = !_isVisualsVisible;
            if (firstPersonHandRoot.gameObject.activeSelf != showFpHand)
            {
                firstPersonHandRoot.gameObject.SetActive(showFpHand);
            }

            if (!showFpHand || Time.timeScale <= 0f)
            {
                return;
            }

            bool isLit = _lanternController != null && _lanternController.IsLit;
            Vector3 targetPos = isLit ? fpHandLitLocalPos : fpHandUnlitLocalPos;

            // Ayunan lembut saat berjalan agar tangan First-Person terasa hidup
            float speed = _playerController != null ? _playerController.HorizontalSpeed : 0f;
            if (speed > 0.2f && _playerController.IsGrounded)
            {
                _fpSwayTime += Time.deltaTime * 9f;
                targetPos.x += Mathf.Cos(_fpSwayTime * 0.5f) * 0.015f;
                targetPos.y += Mathf.Abs(Mathf.Sin(_fpSwayTime * 0.5f)) * 0.02f;
            }

            float t = 1f - Mathf.Exp(-fpHandMoveSpeed * Time.deltaTime);
            firstPersonHandRoot.localPosition = Vector3.Lerp(firstPersonHandRoot.localPosition, targetPos, t);
        }
    }
}
