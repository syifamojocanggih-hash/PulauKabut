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
        [Tooltip("Tampilkan panel debug FPS & status. Default tersembunyi — tekan F3 atau klik tombol [F3] FPS.")]
        [SerializeField] private bool showOverlay = false;

        [Tooltip("Tampilkan petunjuk tombol kontrol. Default tersembunyi — tekan H atau klik tombol [H] Kontrol.")]
        [SerializeField] private bool showControlHints = false;

        [Header("Referensi (Opsional)")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private LanternController lanternController;

        private readonly StringBuilder _debugBuilder = new StringBuilder(180);
        private readonly StringBuilder _hintBuilder = new StringBuilder(260);
        private readonly StringBuilder _lanternBarBuilder = new StringBuilder(64);
        private string _cachedDebugText = string.Empty;
        private string _cachedHintText = string.Empty;
        private string _cachedLanternBarText = string.Empty;

        private GUIStyle _boxStyle;
        private GUIStyle _mintBoxStyle;
        private GUIStyle _pauseStyle;
        private GUIStyle _barLabelStyle;
        private GUIStyle _toggleBtnStyle;
        private GUIStyle _keycapStyle;
        private GUIStyle _btnTextStyle;
        private Texture2D _solidTex;
        private Rect _debugRect = new Rect(12f, 12f, 240f, 128f);

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

            bool showLanternBar = lanternController != null && lanternController.HasDurationLimit;
            if (!showOverlay && !showControlHints && !showLanternBar)
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
                string lanternStatus = GetLanternStatusLabel();
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

            if (lanternController != null && lanternController.HasDurationLimit)
            {
                int pct = Mathf.RoundToInt(lanternController.ChargeNormalized * 100f);
                int sec = Mathf.CeilToInt(lanternController.RemainingSeconds);
                _lanternBarBuilder.Clear();
                _lanternBarBuilder.Append("Lentera [F]  ");
                if (lanternController.IsLit)
                {
                    _lanternBarBuilder.Append(sec).Append(" dtk (").Append(pct).Append("%)");
                }
                else if (!lanternController.CanRelight)
                {
                    _lanternBarBuilder.Append("Daya habis - Mengisi (").Append(pct).Append("%)");
                }
                else if (lanternController.IsRecharging)
                {
                    _lanternBarBuilder.Append("Mengisi... (").Append(pct).Append("%)");
                }
                else
                {
                    _lanternBarBuilder.Append("Siap (").Append(pct).Append("%)");
                }

                _cachedLanternBarText = _lanternBarBuilder.ToString();
            }
        }

        private string GetLanternStatusLabel()
        {
            if (lanternController == null) return "PADAM";
            if (lanternController.IsLit) return "NYALA";
            if (!lanternController.CanRelight) return "DAYA HABIS";
            if (lanternController.IsRecharging) return "MENGISI";
            return "PADAM";
        }

        private void OnDestroy()
        {
            if (_solidTex != null)
            {
                Destroy(_solidTex);
                _solidTex = null;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            // Bar tombol toggle chamfered gaya HUD Lengkap di pojok kanan atas ([J] Misi | [H] Kontrol | [F3] FPS)
            const float btnH = 34f;
            const float gap = 8f;
            const float fpsW = 88f;
            const float hintBtnW = 104f;
            const float misiBtnW = 84f;
            float rightX = Screen.width - 14f;

            Rect fpsBtnRect = new Rect(rightX - fpsW, 14f, fpsW, btnH);
            Rect hintBtnRect = new Rect(fpsBtnRect.x - gap - hintBtnW, 14f, hintBtnW, btnH);
            Rect misiBtnRect = new Rect(hintBtnRect.x - gap - misiBtnW, 14f, misiBtnW, btnH);

            if (UI.StylizedUITheme.DrawKeyButton(misiBtnRect, "J", "Misi", _toggleBtnStyle, _keycapStyle, _btnTextStyle) && UI.ObjectiveGuide.Instance != null)
            {
                UI.ObjectiveGuide.Instance.ToggleMissionList();
            }

            if (UI.StylizedUITheme.DrawKeyButton(hintBtnRect, "H", "Kontrol", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
            {
                showControlHints = !showControlHints;
                RebuildTexts();
            }

            if (UI.StylizedUITheme.DrawKeyButton(fpsBtnRect, "F3", "FPS", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
            {
                showOverlay = !showOverlay;
                RebuildTexts();
            }

            // Jika panel Daftar Misi terbuka, geser panel FPS sedikit ke bawah supaya tidak bertumpuk
            float rightPanelY = 56f;
            if (UI.ObjectiveGuide.Instance != null && UI.ObjectiveGuide.Instance.ShowMissionList)
            {
                rightPanelY = 236f;
            }

            if (showOverlay && !string.IsNullOrEmpty(_cachedDebugText))
            {
                Rect rightTopRect = new Rect(Screen.width - _debugRect.width - 14f, rightPanelY, _debugRect.width, _debugRect.height);
                GUI.Box(rightTopRect, _cachedDebugText, _mintBoxStyle);
            }

            const float hintWidth = 310f;
            const float hintHeight = 166f;
            float lanternBarBottomY = Screen.height - 14f;

            if (showControlHints && !string.IsNullOrEmpty(_cachedHintText))
            {
                Rect hintRect = new Rect(14f, Screen.height - hintHeight - 14f, hintWidth, hintHeight);
                GUI.Box(hintRect, _cachedHintText, _boxStyle);
                lanternBarBottomY = hintRect.y - 8f;
            }

            if (lanternController != null && lanternController.HasDurationLimit)
            {
                DrawLanternChargeBar(14f, lanternBarBottomY - 46f, hintWidth, 46f);
            }

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                const float w = 360f;
                const float h = 68f;
                Rect pauseRect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
                GUI.Box(pauseRect, "GAME DIJEDA\nTekan [Esc] untuk melanjutkan", _pauseStyle);
            }
        }

        private void DrawLanternChargeBar(float x, float y, float width, float height)
        {
            Rect panelRect = new Rect(x, y, width, height);
            GUI.Box(panelRect, string.Empty, _boxStyle);

            Rect labelRect = new Rect(x + 10f, y + 4f, width - 20f, 18f);
            GUI.Label(labelRect, _cachedLanternBarText, _barLabelStyle);

            Rect bgBarRect = new Rect(x + 10f, y + 25f, width - 20f, 12f);
            Color prevColor = GUI.color;

            GUI.color = new Color(0.12f, 0.14f, 0.18f, 0.9f);
            GUI.DrawTexture(bgBarRect, _solidTex);

            float fillRatio = Mathf.Clamp01(lanternController.ChargeNormalized);
            Rect fillBarRect = new Rect(bgBarRect.x + 1f, bgBarRect.y + 1f, (bgBarRect.width - 2f) * fillRatio, bgBarRect.height - 2f);

            if (!lanternController.CanRelight || (lanternController.IsLit && lanternController.IsLowCharge))
            {
                GUI.color = new Color(0.95f, 0.35f, 0.22f, 1f);
            }
            else if (lanternController.IsLit)
            {
                GUI.color = new Color(1f, 0.80f, 0.32f, 1f);
            }
            else if (lanternController.IsRecharging)
            {
                GUI.color = new Color(0.45f, 0.82f, 0.92f, 1f);
            }
            else
            {
                GUI.color = new Color(0.86f, 0.76f, 0.45f, 0.85f);
            }

            if (fillBarRect.width > 0.5f)
            {
                GUI.DrawTexture(fillBarRect, _solidTex);
            }

            GUI.color = prevColor;
        }

        private void EnsureStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = UI.StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 14, 10);
            }

            if (_mintBoxStyle == null)
            {
                _mintBoxStyle = UI.StylizedUITheme.CreateMintPanelStyle(13, TextAnchor.UpperLeft, 14, 10);
            }

            if (_barLabelStyle == null)
            {
                _barLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
                _barLabelStyle.normal.textColor = UI.StylizedUITheme.GoldAccent;
            }

            if (_solidTex == null)
            {
                _solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _solidTex.SetPixel(0, 0, Color.white);
                _solidTex.Apply();
            }

            if (_toggleBtnStyle == null)
            {
                _toggleBtnStyle = UI.StylizedUITheme.CreatePillStyle(12, TextAnchor.MiddleCenter, 8, 4);
            }

            if (_keycapStyle == null)
            {
                _keycapStyle = UI.StylizedUITheme.CreateKeycapStyle(11);
            }

            if (_btnTextStyle == null)
            {
                _btnTextStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
                _btnTextStyle.normal.textColor = UI.StylizedUITheme.CreamText;
            }

            if (_pauseStyle == null)
            {
                _pauseStyle = UI.StylizedUITheme.CreateGoldPanelStyle(16, TextAnchor.MiddleCenter, 16, 12);
            }
        }
    }
}
