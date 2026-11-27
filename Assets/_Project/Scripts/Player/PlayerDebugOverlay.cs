using System.Text;
using PulauKabut.Core;
using PulauKabut.Lantern;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        private GUIStyle _mintBadgeStyle;
        private GUIStyle _btnTextStyle;
        private GUIStyle _goldTitleStyle;
        private GUIStyle _mintTitleStyle;
        private GUIStyle _seafoamLabelStyle;
        private GUIStyle _rightValCreamStyle;
        private GUIStyle _rightValMintStyle;
        private GUIStyle _rightValGoldStyle;
        private Texture2D _solidTex;
        private string _lanternRightStatus = "Siap (100%)";
        private string _fpsValText = "0";
        private string _groundValText = "YA";
        private string _speedValText = "0.00 m/s";
        private string _velYValText = "0.00 m/s";
        private string _stateValText = "Playing";
        private string _lanternHintLabel = "Lentera: PADAM";
        private string _povHintLabel = "Kamera: Third-Person";

        private int _frameCount;
        private float _elapsedTime;
        private float _currentFps;
        private bool _showPauseControls;
        private bool _showPauseSaveSlots;
        private string _saveFeedback = string.Empty;
        private GameSaveData[] _pauseSlotCache;

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
                _fpsValText = Mathf.RoundToInt(_currentFps).ToString();
                if (playerController != null)
                {
                    _groundValText = playerController.IsGrounded
                        ? "YA"
                        : (playerController.IsGliding ? "GLIDE" : "TIDAK");
                    _speedValText = playerController.HorizontalSpeed.ToString("0.00") + " m/s";
                    _velYValText = playerController.Velocity.y.ToString("0.00") + " m/s";
                }
                if (GameManager.Instance != null)
                {
                    _stateValText = GameManager.Instance.CurrentState.ToString();
                }
            }

            if (showControlHints)
            {
                _lanternHintLabel = "Lentera: " + GetLanternStatusLabel();
                _povHintLabel = "Kamera: " + (cameraController != null && cameraController.IsFirstPerson ? "First-Person" : "Third-Person");
            }

            if (lanternController != null && lanternController.HasDurationLimit)
            {
                int pct = Mathf.RoundToInt(lanternController.ChargeNormalized * 100f);
                int sec = Mathf.CeilToInt(lanternController.RemainingSeconds);
                if (lanternController.IsLit)
                    _lanternRightStatus = $"{sec} dtk ({pct}%)";
                else if (!lanternController.CanRelight)
                    _lanternRightStatus = $"Habis ({pct}%)";
                else if (lanternController.IsRecharging)
                    _lanternRightStatus = $"Mengisi ({pct}%)";
                else
                    _lanternRightStatus = $"Siap ({pct}%)";
            }
        }

        private string GetLanternStatusLabel()
        {
            if (lanternController == null || !lanternController.HasLantern) return "BELUM ADA";
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
                rightPanelY = 186f;
            }

            // Panel Debug FPS bergaya Hud.dc.html (bingkai mint + grid 2 kolom rapi)
            if (showOverlay)
            {
                DrawDebugPanel(Screen.width - 224f - 14f, rightPanelY, 224f, 152f);
            }

            // Panel KONTROL di pojok kiri bawah dengan keycap emas (persis Hud.dc.html!)
            if (showControlHints)
            {
                DrawControlsPanel(14f, Screen.height - 286f - 14f, 296f, 286f);
            }

            // Kartu Daya Lentera di pojok KANAN BAWAH baru muncul setelah pemain diberi Lentera oleh Mbah Sauh!
            if (lanternController != null && lanternController.HasLantern && lanternController.HasDurationLimit)
            {
                DrawLanternChargeBar(Screen.width - 306f - 14f, Screen.height - 82f - 14f, 306f, 82f);
            }

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                if (_showPauseControls)
                {
                    DrawPauseControlsMenu();
                }
                else if (_showPauseSaveSlots)
                {
                    DrawPauseSaveMenu();
                }
                else
                {
                    const float w = 340f;
                    const float h = 282f;
                    Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
                    GUI.Box(card, GUIContent.none, _boxStyle);

                    GUI.Label(new Rect(card.x, card.y + 16f, card.width, 32f), "GAME DIJEDA", _pauseStyle);

                    float bx = card.x + 28f;
                    float bw = card.width - 56f;
                    const float bh = 40f;

                    if (UI.StylizedUITheme.DrawKeyButton(new Rect(bx, card.y + 62f, bw, bh), "Esc", "Lanjutkan", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
                    {
                        GameManager.Instance.TogglePause();
                    }

                    if (UI.StylizedUITheme.DrawKeyButton(new Rect(bx, card.y + 112f, bw, bh), "H", "Setting (Kontrol)", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
                    {
                        _showPauseControls = true;
                        RebuildTexts();
                    }

                    if (UI.StylizedUITheme.DrawKeyButton(new Rect(bx, card.y + 162f, bw, bh), "S", "Simpan Progres", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
                    {
                        _pauseSlotCache = SaveSystem.LoadAllSlots();
                        _saveFeedback = string.Empty;
                        _showPauseSaveSlots = true;
                    }

                    if (UI.StylizedUITheme.DrawKeyButton(new Rect(bx, card.y + 212f, bw, bh), "Q", "Kembali ke Menu Utama", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
                    {
                        SaveLoadBridge.Instance?.SaveAutoAtCheckpoint();
                        Time.timeScale = 1f;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        SceneManager.LoadScene("MainMenu");
                    }
                }
            }
            else
            {
                _showPauseControls = false;
                _showPauseSaveSlots = false;
            }
        }

        private void DrawDebugPanel(float x, float y, float w, float h)
        {
            Rect card = new Rect(x, y, w, h);
            GUI.Box(card, GUIContent.none, _mintBoxStyle);

            GUI.Label(new Rect(x + 14f, y + 10f, 160f, 22f), "Pulau Kabut Debug", _mintTitleStyle);
            GUI.Box(new Rect(card.xMax - 40f, y + 10f, 26f, 20f), "F3", _mintBadgeStyle);

            float lx = x + 14f;
            float rw = w - 28f;
            float ry = y + 38f;
            DrawDebugRow(lx, ry, rw, "FPS", _fpsValText, _rightValCreamStyle); ry += 21f;
            DrawDebugRow(lx, ry, rw, "Grounded", _groundValText, _rightValMintStyle); ry += 21f;
            DrawDebugRow(lx, ry, rw, "Speed XZ", _speedValText, _rightValCreamStyle); ry += 21f;
            DrawDebugRow(lx, ry, rw, "Vel Y", _velYValText, _rightValCreamStyle); ry += 21f;
            DrawDebugRow(lx, ry, rw, "State", _stateValText, _rightValGoldStyle);
        }

        private void DrawDebugRow(float x, float y, float w, string label, string val, GUIStyle valStyle)
        {
            GUI.Label(new Rect(x, y, 100f, 20f), label, _seafoamLabelStyle);
            GUI.Label(new Rect(x, y, w, 20f), val, valStyle);
        }

        private void DrawControlsPanel(float x, float y, float w, float h)
        {
            Rect card = new Rect(x, y, w, h);
            GUI.Box(card, GUIContent.none, _boxStyle);

            GUI.Label(new Rect(x + 16f, y + 10f, 120f, 24f), "KONTROL", _goldTitleStyle);
            GUI.Box(new Rect(card.xMax - 102f, y + 11f, 24f, 22f), "H", _keycapStyle);
            GUI.Label(new Rect(card.xMax - 74f, y + 11f, 64f, 22f), "sembunyi", _seafoamLabelStyle);

            float ry = y + 42f;
            DrawControlRow(x + 16f, ry, "WASD", "Jalan", 48f); ry += 26f;
            DrawControlRow(x + 16f, ry, "Shift", "Sprint", 48f); ry += 26f;
            DrawControlRow(x + 16f, ry, "Mouse", "Arahkan Kamera", 52f); ry += 26f;
            DrawControlRow(x + 16f, ry, "Spasi", "Lompat (Tahan = Glide)", 52f); ry += 26f;
            DrawControlRow(x + 16f, ry, "E", "Interaksi / Bicara NPC", 28f); ry += 26f;
            DrawControlRow(x + 16f, ry, "F", _lanternHintLabel, 28f); ry += 26f;
            DrawControlRow(x + 16f, ry, "M", "Perbesar / Perkecil Peta", 28f); ry += 26f;
            DrawControlRow(x + 16f, ry, "V", _povHintLabel, 28f); ry += 26f;
            DrawControlRow(x + 16f, ry, "Esc", "Jeda / Lanjut", 36f);
        }

        private void DrawPauseControlsMenu()
        {
            const float w = 340f;
            const float h = 368f;
            Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(card, GUIContent.none, _boxStyle);

            GUI.Label(new Rect(card.x, card.y + 14f, card.width, 30f), "SETTING KONTROL", _pauseStyle);

            float rx = card.x + 24f;
            float ry = card.y + 52f;
            DrawControlRow(rx, ry, "WASD", "Jalan", 52f); ry += 26f;
            DrawControlRow(rx, ry, "Shift", "Sprint", 52f); ry += 26f;
            DrawControlRow(rx, ry, "Mouse", "Arahkan Kamera", 52f); ry += 26f;
            DrawControlRow(rx, ry, "Spasi", "Lompat (Tahan = Glide)", 52f); ry += 26f;
            DrawControlRow(rx, ry, "E", "Interaksi / Bicara NPC", 32f); ry += 26f;
            DrawControlRow(rx, ry, "F", "Nyalakan / Padam Lentera", 32f); ry += 26f;
            DrawControlRow(rx, ry, "M", "Perbesar / Perkecil Peta", 32f); ry += 26f;
            DrawControlRow(rx, ry, "J", "Daftar Misi", 32f); ry += 26f;
            DrawControlRow(rx, ry, "V", "Ganti Kamera (POV)", 32f); ry += 26f;
            DrawControlRow(rx, ry, "Esc", "Jeda / Lanjut", 38f);

            if (UI.StylizedUITheme.DrawKeyButton(new Rect(card.x + 28f, card.yMax - 52f, card.width - 56f, 38f), "←", "Kembali", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
            {
                _showPauseControls = false;
            }
        }

        private void DrawPauseSaveMenu()
        {
            const float w = 390f;
            const float h = 356f;
            Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(card, GUIContent.none, _boxStyle);

            GUI.Label(new Rect(card.x, card.y + 14f, card.width, 30f), "SIMPAN PERJALANAN", _pauseStyle);

            float bx = card.x + 24f;
            float bw = card.width - 48f;
            float ry = card.y + 52f;

            GUI.Label(new Rect(bx, ry, bw, 22f), "Auto Save otomatis tersimpan di tiap Checkpoint.", _seafoamLabelStyle);
            ry += 28f;

            for (int slot = 1; slot <= 3; slot++)
            {
                GameSaveData d = (_pauseSlotCache != null && slot < _pauseSlotCache.Length) ? _pauseSlotCache[slot] : null;
                string summary = d != null
                    ? $"Perjalanan {slot} · {d.checkpointName} ({d.savedTimestamp})"
                    : $"Perjalanan {slot} · Slot Kosong";

                if (UI.StylizedUITheme.DrawKeyButton(new Rect(bx, ry, bw, 46f), slot.ToString(), summary, _toggleBtnStyle, _keycapStyle, _btnTextStyle))
                {
                    SaveLoadBridge.Instance?.SaveToSlot(slot, forceCheckpointPosition: false);
                    SaveLoadBridge.Instance?.SaveAutoAtCheckpoint();
                    _pauseSlotCache = SaveSystem.LoadAllSlots();
                    _saveFeedback = $"Berhasil disimpan ke Perjalanan {slot}!";
                }
                ry += 56f;
            }

            if (!string.IsNullOrEmpty(_saveFeedback))
            {
                GUI.Label(new Rect(bx, ry + 2f, bw, 22f), _saveFeedback, _mintTitleStyle);
            }

            if (UI.StylizedUITheme.DrawKeyButton(new Rect(card.x + 28f, card.yMax - 52f, card.width - 56f, 38f), "←", "Kembali", _toggleBtnStyle, _keycapStyle, _btnTextStyle))
            {
                _showPauseSaveSlots = false;
            }
        }

        private void DrawControlRow(float x, float y, string key, string desc, float keyW)
        {
            GUI.Box(new Rect(x, y, keyW, 21f), key, _keycapStyle);
            GUI.Label(new Rect(x + 62f, y, 210f, 21f), desc, _btnTextStyle);
        }

        private void DrawLanternChargeBar(float x, float y, float width, float height)
        {
            Rect panelRect = new Rect(x, y, width, height);
            GUI.Box(panelRect, GUIContent.none, _boxStyle);

            // Ikon Lentera + "Lentera" + keycap [F] + status kanan mint ("Siap (100%)")
            GUI.DrawTexture(new Rect(x + 14f, y + 12f, 24f, 24f), UI.StylizedUITheme.LanternIconTex);
            GUI.Label(new Rect(x + 44f, y + 12f, 74f, 24f), "Lentera", _goldTitleStyle);
            GUI.Box(new Rect(x + 112f, y + 13f, 24f, 22f), "F", _keycapStyle);

            GUIStyle statusStyle = (!lanternController.CanRelight || (lanternController.IsLit && lanternController.IsLowCharge))
                ? _rightValGoldStyle
                : _rightValMintStyle;
            GUI.Label(new Rect(x + 140f, y + 12f, width - 156f, 24f), _lanternRightStatus, statusStyle);

            // 12 segmen jajar genjang miring (persis Hud.dc.html!)
            const int segCount = 12;
            float startX = x + 16f;
            float segY = y + 48f;
            float totalW = width - 32f;
            float segSlot = totalW / segCount;
            float segW = segSlot - 2f;
            float filledSegments = lanternController.ChargeNormalized * segCount;

            Color prevColor = GUI.color;
            Color activeCol;
            if (!lanternController.CanRelight || (lanternController.IsLit && lanternController.IsLowCharge))
                activeCol = new Color(0.96f, 0.38f, 0.24f, 1f);
            else if (lanternController.IsRecharging)
                activeCol = UI.StylizedUITheme.MintAccent;
            else
                activeCol = UI.StylizedUITheme.GoldAccent;

            Color dimCol = new Color(0.965f, 0.937f, 0.851f, 0.16f);

            for (int i = 0; i < segCount; i++)
            {
                bool lit = (i + 0.35f) <= filledSegments;
                GUI.color = lit ? activeCol : dimCol;
                GUI.DrawTexture(new Rect(startX + i * segSlot, segY, segW, 18f), UI.StylizedUITheme.SlantSegTex);
            }

            GUI.color = prevColor;
        }

        private void EnsureStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = UI.StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 14, 10);
                _mintBoxStyle = UI.StylizedUITheme.CreateMintPanelStyle(13, TextAnchor.UpperLeft, 14, 10);
                _barLabelStyle = UI.StylizedUITheme.CreateHeadingLabelStyle(14, UI.StylizedUITheme.GoldAccent, TextAnchor.MiddleLeft);
                _toggleBtnStyle = UI.StylizedUITheme.CreatePillStyle(13, TextAnchor.MiddleCenter, 8, 4);
                _keycapStyle = UI.StylizedUITheme.CreateKeycapStyle(12);
                _mintBadgeStyle = UI.StylizedUITheme.CreateMintBadgeStyle(11);
                _btnTextStyle = UI.StylizedUITheme.CreateBodyLabelStyle(13, UI.StylizedUITheme.CreamText, TextAnchor.MiddleLeft);
                _goldTitleStyle = UI.StylizedUITheme.CreateHeadingLabelStyle(15, UI.StylizedUITheme.GoldAccent, TextAnchor.MiddleLeft);
                _mintTitleStyle = UI.StylizedUITheme.CreateHeadingLabelStyle(14, UI.StylizedUITheme.MintAccent, TextAnchor.MiddleLeft);
                _seafoamLabelStyle = UI.StylizedUITheme.CreateBodyLabelStyle(13, UI.StylizedUITheme.SeafoamText, TextAnchor.MiddleLeft);
                _rightValCreamStyle = UI.StylizedUITheme.CreateBodyLabelStyle(13, UI.StylizedUITheme.CreamText, TextAnchor.MiddleRight);
                _rightValMintStyle = UI.StylizedUITheme.CreateHeadingLabelStyle(13, UI.StylizedUITheme.MintAccent, TextAnchor.MiddleRight);
                _rightValGoldStyle = UI.StylizedUITheme.CreateHeadingLabelStyle(13, UI.StylizedUITheme.GoldAccent, TextAnchor.MiddleRight);
                _pauseStyle = UI.StylizedUITheme.CreateGoldPanelStyle(16, TextAnchor.MiddleCenter, 16, 12);
            }

            if (_solidTex == null)
            {
                _solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _solidTex.SetPixel(0, 0, Color.white);
                _solidTex.Apply();
            }
        }
    }
}
