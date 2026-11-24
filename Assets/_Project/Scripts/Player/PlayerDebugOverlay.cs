using System.Text;
using PulauKabut.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.Player
{
    /// <summary>
    /// Overlay debug ringan di pojok kiri atas untuk memantau FPS, status di tanah,
    /// kecepatan pemain, dan status jeda. Bisa dinyalakan/dimatikan lewat Inspector atau tombol F3.
    /// </summary>
    public class PlayerDebugOverlay : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;

        [Header("Pengaturan Debug")]
        [Tooltip("Tampilkan overlay debug saat Play Mode. Tekan F3 untuk menyalakan/mematikan.")]
        [SerializeField] private bool showOverlay = true;

        [Tooltip("Referensi PlayerController (jika kosong, otomatis diambil dari objek ini).")]
        [SerializeField] private PlayerController playerController;

        private readonly StringBuilder _builder = new StringBuilder(160);
        private string _cachedDisplayText = string.Empty;
        private GUIStyle _boxStyle;
        private Rect _panelRect = new Rect(12f, 12f, 250f, 108f);

        private int _frameCount;
        private float _elapsedTime;
        private float _currentFps;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            // Toggle tampilan debug menggunakan tombol F3 (Input System, tanpa alokasi memori)
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            {
                showOverlay = !showOverlay;
            }

            if (!showOverlay)
            {
                return;
            }

            // Hitung rata-rata FPS dalam interval singkat agar stabil terbaca dan minim alokasi string
            _frameCount++;
            _elapsedTime += Time.unscaledDeltaTime;

            if (_elapsedTime >= RefreshInterval)
            {
                _currentFps = _frameCount / _elapsedTime;
                _frameCount = 0;
                _elapsedTime = 0f;
                RebuildDisplayText();
            }
        }

        private void RebuildDisplayText()
        {
            _builder.Clear();
            _builder.Append("Pulau Kabut Debug [F3]\n");
            _builder.Append("FPS      : ").Append(Mathf.RoundToInt(_currentFps)).Append('\n');

            if (playerController != null)
            {
                _builder.Append("Grounded : ").Append(playerController.IsGrounded ? "YA" : "TIDAK").Append('\n');
                _builder.Append("Speed XZ : ").Append(playerController.HorizontalSpeed.ToString("0.00")).Append(" m/s\n");
                _builder.Append("Vel Y    : ").Append(playerController.Velocity.y.ToString("0.00")).Append(" m/s");
            }
            else
            {
                _builder.Append("PlayerController tidak ditemukan");
            }

            if (GameManager.Instance != null)
            {
                _builder.Append("\nState    : ").Append(GameManager.Instance.CurrentState);
            }

            _cachedDisplayText = _builder.ToString();
        }

        private void OnGUI()
        {
            if (!showOverlay || string.IsNullOrEmpty(_cachedDisplayText))
            {
                return;
            }

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

            GUI.Box(_panelRect, _cachedDisplayText, _boxStyle);
        }
    }
}
