using System.Text;
using PulauKabut.Core;
using PulauKabut.Lantern;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.Player
{
    /// <summary>
    /// Overlay debug ringan (pojok kiri atas, toggle F3) dan petunjuk kontrol (pojok kiri bawah, toggle H)
    /// untuk memantau FPS, status gerak/glide, status lentera, POV kamera, serta tombol kontrol.
    /// </summary>
    public class PlayerDebugOverlay : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;

        [Header("Pengaturan Tampilan")]
        [Tooltip("Tampilkan panel debug FPS & status di pojok kiri atas. Tekan F3 untuk toggle.")]
        [SerializeField] private bool showOverlay = true;

        [Tooltip("Tampilkan petunjuk tombol kontrol di pojok kiri bawah. Tekan H untuk toggle.")]
        [SerializeField] private bool showControlHints = true;

        [Header("Referensi (Opsional)")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private LanternController lanternController;

        private readonly StringBuilder _debugBuilder = new StringBuilder(180);
        private readonly StringBuilder _hintBuilder = new StringBuilder(240);
        private string _cachedDebugText = string.Empty;
        private string _cachedHintText = string.Empty;

        private GUIStyle _boxStyle;
        private GUIStyle _pauseStyle;
        private Rect _debugRect = new Rect(12f, 12f, 255f, 124f);

        private int _frameCount;
        private float _elapsedTime;
        private float _currentFps;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (cameraController == null) cameraController = GetComponent<PlayerCameraController>();
            if (lanternController == null) lanternController = GetComponent<LanternController>();
        }

        private void Start()
        {
            RebuildTexts();
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.f3Key.wasPressedThisFrame)
                {
                    showOverlay = !showOverlay;
                }

                if (Keyboard.current.hKey.wasPressedThisFrame)
                {
                    showControlHints = !showControlHints;
                }
            }

            if (!showOverlay && !showControlHints)
            {
                return;
            }

            // Hitung FPS dalam interval singkat agar angka stabil & hemat alokasi string
            _frameCount++;
            _elapsedTime += Time.unscaledDeltaTime;

            if (_elapsedTime >= RefreshInterval)
            {
                _currentFps = _frameCount / _elapsedTime;
                _frameCount = 0;
                _elapsedTime = 0f;
                RebuildTexts();
            }
        }

        private void RebuildTexts()
        {
            if (showOverlay)
            {
                _debugBuilder.Clear();
                _debugBuilder.Append("Pulau Kabut Debug [F3]\n");
                _debugBuilder.Append("FPS      : ").Append(Mathf.RoundToInt(_currentFps)).Append('\n');

                if (playerController != null)
                {
                    string groundLabel = playerController.IsGrounded
                        ? "YA"
                        : (playerController.IsGliding ? "GLIDE" : "TIDAK");
                    _debugBuilder.Append("Grounded : ").Append(groundLabel).Append('\n');
                    _debugBuilder.Append("Speed XZ : ").Append(playerController.HorizontalSpeed.ToString("0.00")).Append(" m/s\n");
                    _debugBuilder.Append("Vel Y    : ").Append(playerController.Velocity.y.ToString("0.00")).Append(" m/s");
                }

                if (GameManager.Instance != null)
                {
                    _debugBuilder.Append("\nState    : ").Append(GameManager.Instance.CurrentState);
                }

                _cachedDebugText = _debugBuilder.ToString();
            }

            if (showControlHints)
            {
                string lanternStatus = lanternController != null && lanternController.IsLit ? "NYALA" : "PADAM";
                string povStatus = cameraController != null && cameraController.IsFirstPerson ? "First-Person" : "Third-Person";

                _hintBuilder.Clear();
                _hintBuilder.Append("KONTROL [H sembunyi]\n");
                _hintBuilder.Append("[WASD]   Jalan  |  [Shift] Sprint\n");
                _hintBuilder.Append("[Mouse]  Arahkan Kamera\n");
                _hintBuilder.Append("[Spasi]  Lompat (Tahan di udara = Glide)\n");
                _hintBuilder.Append("[E]      Interaksi / Bicara NPC\n");
                _hintBuilder.Append("[F]      Lentera : ").Append(lanternStatus).Append('\n');
                _hintBuilder.Append("[V]      Kamera  : ").Append(povStatus).Append('\n');
                _hintBuilder.Append("[Esc]    Jeda / Lanjut");

                _cachedHintText = _hintBuilder.ToString();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (showOverlay && !string.IsNullOrEmpty(_cachedDebugText))
            {
                Rect rightTopRect = new Rect(Screen.width - _debugRect.width - 12f, 12f, _debugRect.width, _debugRect.height);
                GUI.Box(rightTopRect, _cachedDebugText, _boxStyle);
            }

            if (showControlHints && !string.IsNullOrEmpty(_cachedHintText))
            {
                const float hintWidth = 320f;
                const float hintHeight = 166f;
                Rect hintRect = new Rect(12f, Screen.height - hintHeight - 12f, hintWidth, hintHeight);
                GUI.Box(hintRect, _cachedHintText, _boxStyle);
            }

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                const float w = 360f;
                const float h = 68f;
                Rect pauseRect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
                GUI.Box(pauseRect, "GAME DIJEDA\nTekan [Esc] untuk melanjutkan", _pauseStyle);
            }
        }

        private void EnsureStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 13,
                    padding = new RectOffset(10, 10, 8, 8)
                };
                _boxStyle.normal.textColor = Color.white;
            }

            if (_pauseStyle == null)
            {
                _pauseStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(12, 12, 10, 10)
                };
                _pauseStyle.normal.textColor = Color.white;
            }
        }
    }
}
