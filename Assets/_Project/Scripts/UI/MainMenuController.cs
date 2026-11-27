using PulauKabut.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PulauKabut.UI
{
    /// <summary>
    /// Layar Menu Utama Pulau Kabut bergaya chamfered teal-emas (selaras dengan HUD Lengkap)
    /// dengan skala adaptif layar, dukungan keyboard + mouse, kartu progres petualangan,
    /// dan diorama pesisir 3D hidup di sisi kanan.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private enum MenuPage
        {
            Main,
            Options
        }

        [Header("Scene Tujuan")]
        [SerializeField] private string firstLevelScene = "Zone_Mangrove";

        [Header("Kamera & Diorama 3D")]
        [SerializeField] private Transform orbitPivot;
        [SerializeField] private float swayAmplitudeDegrees = 11f;
        [SerializeField] private float swaySpeed = 0.22f;

        private GameSaveData _cachedSave;
        private MenuPage _currentPage = MenuPage.Main;
        private int _selectedIndex;
        private int _hoveredIndex = -1;
        private string _activeDescription = string.Empty;
        private float _basePivotYaw;

        private float _masterVolume = 1f;
        private int _qualityIndex = 1;
        private bool _isFullscreen = true;

        private Texture2D _leftScrimTex;
        private Texture2D _lanternIcon;
        private Texture2D _menuBtnNormalTex;
        private GUIStyle _titleStyle;
        private GUIStyle _titleShadowStyle;
        private GUIStyle _subtitleBadgeStyle;
        private GUIStyle _btnNormalStyle;
        private GUIStyle _btnActiveStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _cardHeaderStyle;
        private GUIStyle _cardBodyStyle;
        private GUIStyle _cardValueStyle;
        private GUIStyle _descStyle;
        private GUIStyle _footerStyle;
        private GUIStyle _keycapStyle;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _cachedSave = SaveSystem.Load();

            _masterVolume = PlayerPrefs.GetFloat("PK_MasterVolume", 1f);
            AudioListener.volume = _masterVolume;
            _qualityIndex = QualitySettings.GetQualityLevel();
            _isFullscreen = Screen.fullScreen;

            _lanternIcon = Resources.Load<Texture2D>("UI/Icon_HandLantern");
            if (_lanternIcon == null) _lanternIcon = StylizedUITheme.LanternIconTex;

            if (orbitPivot != null)
            {
                _basePivotYaw = orbitPivot.eulerAngles.y;
            }
        }

        private void OnDestroy()
        {
            if (_leftScrimTex != null) { Destroy(_leftScrimTex); _leftScrimTex = null; }
            if (_menuBtnNormalTex != null) { Destroy(_menuBtnNormalTex); _menuBtnNormalTex = null; }
        }

        private void Update()
        {
            if (orbitPivot != null)
            {
                float yaw = _basePivotYaw + Mathf.Sin(Time.time * swaySpeed) * swayAmplitudeDegrees;
                Vector3 e = orbitPivot.eulerAngles;
                orbitPivot.eulerAngles = new Vector3(e.x, yaw, e.z);
            }

            if (Keyboard.current == null) return;

            if (_currentPage == MenuPage.Options && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _currentPage = MenuPage.Main;
                _selectedIndex = 0;
                return;
            }

            int itemCount = GetCurrentItemCount();
            if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            {
                _selectedIndex = (_selectedIndex + 1) % itemCount;
            }
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            {
                _selectedIndex = (_selectedIndex - 1 + itemCount) % itemCount;
            }
        }

        private int GetCurrentItemCount()
        {
            if (_currentPage == MenuPage.Options) return 3;
            return _cachedSave != null ? 4 : 3;
        }

        private void OnGUI()
        {
            EnsureStyles();

            // Skala GUI adaptif berbasis tinggi 720p agar ukuran selalu proporsional & tajam di monitor apapun
            float uiScale = Mathf.Clamp(Screen.height / 720f, 0.85f, 1.8f);
            float vw = Screen.width / uiScale;
            float vh = Screen.height / uiScale;

            Matrix4x4 prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));

            // 1. Gradasi deep-ocean-teal dari kiri ke kanan (tidak menutupi pulau & menara di kanan)
            float scrimW = Mathf.Max(520f, vw * 0.48f);
            GUI.DrawTexture(new Rect(0f, 0f, scrimW, vh), _leftScrimTex, ScaleMode.StretchToFill);

            float marginLeft = Mathf.Clamp(vw * 0.052f, 42f, 88f);
            float topY = 44f;

            // 2. Badge mint kecil + Header Judul dengan ikon Lentera 3D & aura emas
            GUI.Box(new Rect(marginLeft, topY, 214f, 24f), "PETUALANGAN LENTERA  ·  V0.1", _subtitleBadgeStyle);

            float headerY = topY + 34f;
            float glowPulse = 0.78f + 0.22f * Mathf.Sin(Time.unscaledTime * 2.4f);
            Color prevCol = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, glowPulse);
            GUI.DrawTexture(new Rect(marginLeft - 10f, headerY - 8f, 76f, 76f), StylizedUITheme.UnlockGlowTex, ScaleMode.ScaleToFit);
            GUI.color = prevCol;
            if (_lanternIcon != null)
            {
                GUI.DrawTexture(new Rect(marginLeft, headerY + 2f, 56f, 56f), _lanternIcon, ScaleMode.ScaleToFit);
            }

            float titleX = marginLeft + 66f;
            DrawShadowedLabel(new Rect(titleX, headerY - 2f, 420f, 48f), "PULAU KABUT", _titleStyle, _titleShadowStyle);
            GUI.Label(new Rect(titleX + 2f, headerY + 42f, 380f, 22f), "Menyibak kabut rawa & kenangan Desa Pelita", _footerStyle);

            // Garis pemisah emas halus
            GUI.color = new Color(StylizedUITheme.GoldAccent.r, StylizedUITheme.GoldAccent.g, StylizedUITheme.GoldAccent.b, 0.65f);
            GUI.DrawTexture(new Rect(marginLeft, headerY + 72f, 356f, 2f), StylizedUITheme.SolidTex);
            GUI.color = prevCol;

            // 3. Tombol menu utama / pengaturan
            float menuY = headerY + 90f;
            if (_currentPage == MenuPage.Main)
            {
                DrawMainPage(marginLeft, menuY, vh);
            }
            else
            {
                DrawOptionsPage(marginLeft, menuY, vh);
            }

            // 4. Footer kiri & kanan bawah
            float footerY = vh - 38f;
            if (_currentPage == MenuPage.Options)
            {
                GUI.Box(new Rect(marginLeft, footerY, 44f, 24f), "ESC", _keycapStyle);
                if (GUI.Button(new Rect(marginLeft + 52f, footerY - 1f, 120f, 26f), "KEMBALI", _footerStyle))
                {
                    _currentPage = MenuPage.Main;
                    _selectedIndex = 0;
                }
            }
            else
            {
                GUI.Label(new Rect(marginLeft, footerY, 380f, 24f), "Cozy Stylized 3D  ·  Target 60 FPS", _footerStyle);
            }

            // Hint navigasi ringkas di kanan bawah
            float hintX = vw - 318f;
            GUI.Box(new Rect(hintX, footerY, 44f, 22f), "W/S", _keycapStyle);
            GUI.Label(new Rect(hintX + 50f, footerY - 1f, 76f, 24f), "Pilih", _footerStyle);
            GUI.Box(new Rect(hintX + 122f, footerY, 64f, 22f), "ENTER", _keycapStyle);
            GUI.Label(new Rect(hintX + 192f, footerY - 1f, 110f, 24f), "Konfirmasi", _footerStyle);

            GUI.matrix = prevMatrix;
        }

        private void DrawMainPage(float x, float startY, float vh)
        {
            bool hasSave = _cachedSave != null;
            float y = startY;
            const float btnW = 356f;
            const float btnH = 44f;
            const float gap = 10f;
            int idx = 0;
            _hoveredIndex = -1;
            _activeDescription = string.Empty;

            if (hasSave)
            {
                if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "LANJUTKAN",
                    "Lanjutkan petualangan menyibak kabut dari titik tersimpan."))
                {
                    string targetScene = string.IsNullOrEmpty(_cachedSave.lastSceneName) ? firstLevelScene : _cachedSave.lastSceneName;
                    SceneManager.LoadScene(targetScene);
                }
                y += btnH + gap;
            }

            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "MULAI BARU",
                "Mulai perjalanan baru dari dermaga Desa Pelita."))
            {
                SaveSystem.DeleteSave();
                SceneManager.LoadScene(firstLevelScene);
            }
            y += btnH + gap;

            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "PENGATURAN",
                "Atur volume audio, kualitas grafis, dan mode layar penuh."))
            {
                _currentPage = MenuPage.Options;
                _selectedIndex = 0;
            }
            y += btnH + gap;

            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "KELUAR",
                "Keluar dari permainan Pulau Kabut."))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            y += btnH + 22f;

            // Kartu Info / Progres Petualangan di kiri bawah agar komposisi kiri terasa seimbang & kaya
            float cardH = hasSave ? 166f : 116f;
            float maxCardY = vh - cardH - 52f;
            float cardY = Mathf.Min(y, maxCardY);
            Rect card = new Rect(x, cardY, btnW, cardH);
            GUI.Box(card, GUIContent.none, _cardStyle);

            if (hasSave)
            {
                int shards = _cachedSave.collectedShards != null ? _cachedSave.collectedShards.Count : 0;
                int memories = _cachedSave.collectedMemories != null ? _cachedSave.collectedMemories.Count : 0;
                int stage = Mathf.Clamp(shards / 4, 0, 5);

                GUI.Label(new Rect(card.x + 18f, card.y + 12f, 260f, 24f), "JEJAK PETUALANGAN", _cardHeaderStyle);
                DrawStatRow(card.x + 18f, card.y + 42f, card.width - 36f, StylizedUITheme.PentagonIconTex, "Pecahan Cahaya", $"{shards} / 24");
                DrawStatRow(card.x + 18f, card.y + 68f, card.width - 36f, StylizedUITheme.HexagonIconTex, "Serpihan Ingatan", $"{memories} / 15");
                DrawStatRow(card.x + 18f, card.y + 94f, card.width - 36f, StylizedUITheme.HouseIconTex, "Desa Pelita", $"Tahap {stage}/5");

                string desc = string.IsNullOrEmpty(_activeDescription)
                    ? "Dermaga & Rawa Kunang menanti lenteramu."
                    : _activeDescription;
                GUI.Label(new Rect(card.x + 18f, card.y + 122f, card.width - 36f, 36f), desc, _descStyle);
            }
            else
            {
                GUI.Label(new Rect(card.x + 18f, card.y + 14f, 280f, 24f), "CATATAN PELAUT", _cardHeaderStyle);
                string desc = string.IsNullOrEmpty(_activeDescription)
                    ? "Bawa lentera menyibak kabut Desa Pelita, susuri akar Rawa Kunang, dan pulihkan pecahan cahaya."
                    : _activeDescription;
                GUI.Label(new Rect(card.x + 18f, card.y + 42f, card.width - 36f, 60f), desc, _descStyle);
            }
        }

        private void DrawStatRow(float x, float y, float width, Texture2D icon, string label, string val)
        {
            if (icon != null)
            {
                GUI.DrawTexture(new Rect(x, y + 2f, 16f, 16f), icon, ScaleMode.ScaleToFit);
            }
            GUI.Label(new Rect(x + 24f, y, width - 110f, 22f), label, _cardBodyStyle);
            GUI.Label(new Rect(x + width - 86f, y, 86f, 22f), val, _cardValueStyle);
        }

        private void DrawOptionsPage(float x, float startY, float vh)
        {
            float y = startY;
            const float btnW = 376f;
            const float btnH = 44f;
            const float gap = 10f;
            int idx = 0;
            _hoveredIndex = -1;
            _activeDescription = string.Empty;

            int volPct = Mathf.RoundToInt(_masterVolume * 100f);
            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, $"VOLUME UTAMA  :  {volPct}%",
                "Klik untuk mengubah tingkat volume keseluruhan (0% – 100%)."))
            {
                _masterVolume = _masterVolume >= 0.99f ? 0f : Mathf.Clamp01(_masterVolume + 0.25f);
                AudioListener.volume = _masterVolume;
                PlayerPrefs.SetFloat("PK_MasterVolume", _masterVolume);
                PlayerPrefs.Save();
            }
            y += btnH + gap;

            string[] qualityNames = QualitySettings.names;
            string qLabel = (_qualityIndex >= 0 && _qualityIndex < qualityNames.Length) ? qualityNames[_qualityIndex] : "Default";
            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, $"KUALITAS GRAFIS  :  {qLabel.ToUpperInvariant()}",
                "Klik untuk mengganti tingkat detail pencahayaan dan bayangan."))
            {
                if (qualityNames.Length > 0)
                {
                    _qualityIndex = (_qualityIndex + 1) % qualityNames.Length;
                    QualitySettings.SetQualityLevel(_qualityIndex, true);
                }
            }
            y += btnH + gap;

            string screenLabel = _isFullscreen ? "LAYAR PENUH" : "JENDELA";
            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, $"TAMPILAN  :  {screenLabel}",
                "Klik untuk beralih antara mode Layar Penuh dan Jendela."))
            {
                _isFullscreen = !_isFullscreen;
                Screen.fullScreen = _isFullscreen;
            }
            y += btnH + 22f;

            float cardY = Mathf.Min(y, vh - 148f);
            Rect card = new Rect(x, cardY, btnW, 88f);
            GUI.Box(card, GUIContent.none, _cardStyle);
            GUI.Label(new Rect(card.x + 18f, card.y + 12f, 260f, 24f), "PENGATURAN", _cardHeaderStyle);
            string desc = string.IsNullOrEmpty(_activeDescription)
                ? "Klik opsi di atas atau tekan Esc untuk kembali."
                : _activeDescription;
            GUI.Label(new Rect(card.x + 18f, card.y + 40f, card.width - 36f, 40f), desc, _descStyle);
        }

        private bool DrawMenuItem(Rect rect, int index, string label, string description)
        {
            Vector2 mousePos = Event.current.mousePosition;
            bool isHovered = rect.Contains(mousePos);
            if (isHovered)
            {
                _hoveredIndex = index;
                _selectedIndex = index;
                _activeDescription = description;
            }

            bool isActive = isHovered || _selectedIndex == index;
            if (isActive && string.IsNullOrEmpty(_activeDescription))
            {
                _activeDescription = description;
            }

            Rect drawRect = isActive ? new Rect(rect.x + 8f, rect.y, rect.width, rect.height) : rect;
            GUIStyle style = isActive ? _btnActiveStyle : _btnNormalStyle;

            bool clicked = GUI.Button(drawRect, label, style);
            if (isActive && Keyboard.current != null &&
                (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                clicked = true;
            }

            // Ikon berlian emas di kiri teks saat terpilih
            if (isActive)
            {
                Color prev = GUI.color;
                GUI.color = StylizedUITheme.GoldAccent;
                GUI.DrawTexture(new Rect(drawRect.x + 16f, drawRect.y + (drawRect.height - 12f) * 0.5f, 12f, 12f), StylizedUITheme.DiamondTex);
                GUI.color = prev;
            }

            return clicked;
        }

        private static void DrawShadowedLabel(Rect rect, string text, GUIStyle mainStyle, GUIStyle shadowStyle)
        {
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2.5f, rect.width, rect.height), text, shadowStyle);
            GUI.Label(rect, text, mainStyle);
        }

        private void EnsureStyles()
        {
            if (_leftScrimTex == null)
            {
                const int w = 128;
                _leftScrimTex = new Texture2D(w, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                Color[] px = new Color[w];
                for (int x = 0; x < w; x++)
                {
                    float t = x / (float)(w - 1);
                    float alpha = Mathf.Lerp(0.88f, 0f, Mathf.Pow(t, 0.78f));
                    px[x] = new Color(0.032f, 0.088f, 0.112f, alpha);
                }
                _leftScrimTex.SetPixels(px);
                _leftScrimTex.Apply();
            }

            if (_menuBtnNormalTex == null)
            {
                _menuBtnNormalTex = CreateSubtleMenuButtonTex();
            }

            if (_titleStyle != null) return;

            _titleStyle = StylizedUITheme.CreateHeadingLabelStyle(42, StylizedUITheme.CreamText, TextAnchor.MiddleLeft);
            _titleShadowStyle = StylizedUITheme.CreateHeadingLabelStyle(42, new Color(0.02f, 0.05f, 0.06f, 0.92f), TextAnchor.MiddleLeft);
            _subtitleBadgeStyle = StylizedUITheme.CreateMintBadgeStyle(11);

            _btnNormalStyle = StylizedUITheme.CreatePillStyle(17, TextAnchor.MiddleLeft, 24, 8);
            _btnNormalStyle.font = StylizedUITheme.HeadingFont;
            _btnNormalStyle.normal.background = _menuBtnNormalTex;
            _btnNormalStyle.normal.textColor = StylizedUITheme.CreamText;

            _btnActiveStyle = StylizedUITheme.CreatePillStyle(18, TextAnchor.MiddleLeft, 36, 8);
            _btnActiveStyle.font = StylizedUITheme.HeadingFont;
            _btnActiveStyle.normal.background = StylizedUITheme.GoldPanelHoverTex;
            _btnActiveStyle.normal.textColor = StylizedUITheme.GoldAccent;

            _cardStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 16, 12);
            _cardHeaderStyle = StylizedUITheme.CreateHeadingLabelStyle(14, StylizedUITheme.GoldAccent, TextAnchor.MiddleLeft);
            _cardBodyStyle = StylizedUITheme.CreateBodyLabelStyle(13, StylizedUITheme.SeafoamText, TextAnchor.MiddleLeft);
            _cardValueStyle = StylizedUITheme.CreateHeadingLabelStyle(14, StylizedUITheme.GoldAccent, TextAnchor.MiddleRight);

            _descStyle = StylizedUITheme.CreateBodyLabelStyle(13, new Color(0.965f, 0.937f, 0.851f, 0.86f), TextAnchor.UpperLeft, wordWrap: true);
            _footerStyle = StylizedUITheme.CreateBodyLabelStyle(12, new Color(0.663f, 0.812f, 0.784f, 0.84f), TextAnchor.MiddleLeft);
            _keycapStyle = StylizedUITheme.CreateKeycapStyle(11);
        }

        private static Texture2D CreateSubtleMenuButtonTex()
        {
            const int w = 128, h = 64;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            Color[] px = new Color[w * h];
            Color bgTop = new Color(0.075f, 0.205f, 0.240f, 0.78f);
            Color bgBot = new Color(0.045f, 0.130f, 0.160f, 0.84f);
            Color borderCol = new Color(0.490f, 0.953f, 0.847f, 0.35f);

            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float tl = 18f - ((h - y) + x);
                    float br = 18f - (y + (w - x));
                    float tr = 6f - ((h - y) + (w - x));
                    float bl = 6f - (y + x);
                    float maxCut = Mathf.Max(Mathf.Max(tl, br), Mathf.Max(tr, bl));
                    bool inside = maxCut <= 0f && x >= 1 && x < w - 1 && y >= 1 && y < h - 1;
                    if (!inside) { px[y * w + x] = Color.clear; continue; }

                    bool edge = maxCut > -3.2f || x <= 2 || x >= w - 3 || y <= 2 || y >= h - 3;
                    px[y * w + x] = edge ? borderCol : Color.Lerp(bgBot, bgTop, v);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
