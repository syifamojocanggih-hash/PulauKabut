using PulauKabut.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PulauKabut.UI
{
    /// <summary>
    /// Layar Menu Utama Pulau Kabut bergaya minimalis editorial (menu teks rata kiri dengan
    /// gradasi deep-ocean-teal halus di sisi kiri dan diorama 3D Cozy Stylized Pulau Kabut di kanan).
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
        [SerializeField] private float orbitSpeed = 2.4f;

        private GameSaveData _cachedSave;
        private MenuPage _currentPage = MenuPage.Main;
        private int _hoveredIndex = -1;
        private string _activeDescription = string.Empty;

        private float _masterVolume = 1f;
        private int _qualityIndex = 1;
        private bool _isFullscreen = true;

        private Texture2D _leftScrimTex;
        private GUIStyle _titleStyle;
        private GUIStyle _titleShadowStyle;
        private GUIStyle _itemNormalStyle;
        private GUIStyle _itemHoverStyle;
        private GUIStyle _continueMetaStyle;
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

            // Geser sedikit kamera diorama agar subjek utama tampil lebih seimbang di sisi kanan layar
            if (Camera.main != null && orbitPivot != null)
            {
                Vector3 camPos = Camera.main.transform.localPosition;
                if (Mathf.Approximately(camPos.x, 0f))
                {
                    Camera.main.transform.localPosition = new Vector3(-3.2f, camPos.y, camPos.z);
                }
            }
        }

        private void OnDestroy()
        {
            if (_leftScrimTex != null)
            {
                Destroy(_leftScrimTex);
                _leftScrimTex = null;
            }
        }

        private void Update()
        {
            if (orbitPivot != null)
            {
                orbitPivot.Rotate(0f, orbitSpeed * Time.deltaTime, 0f, Space.World);
            }

            if (_currentPage == MenuPage.Options && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _currentPage = MenuPage.Main;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            // Gradasi deep-ocean-teal dari kiri ke transparan di kanan agar teks terbaca jelas tanpa menutupi diorama 3D
            float scrimW = Mathf.Max(540f, Screen.width * 0.56f);
            GUI.DrawTexture(new Rect(0f, 0f, scrimW, Screen.height), _leftScrimTex, ScaleMode.StretchToFill);

            float marginLeft = Mathf.Clamp(Screen.width * 0.065f, 44f, 110f);
            float topY = Mathf.Clamp(Screen.height * 0.14f, 64f, 150f);

            // Judul PULAU KABUT di kiri atas (gaya judul TLOU)
            DrawShadowedLabel(new Rect(marginLeft, topY, 520f, 52f), "PULAU KABUT", _titleStyle, _titleShadowStyle);

            if (_currentPage == MenuPage.Main)
            {
                DrawMainPage(marginLeft, topY + 62f);
            }
            else
            {
                DrawOptionsPage(marginLeft, topY + 62f);
            }

            // Footer kiri bawah
            float footerY = Screen.height - 54f;
            if (_currentPage == MenuPage.Options)
            {
                GUI.Box(new Rect(marginLeft, footerY, 44f, 24f), "ESC", _keycapStyle);
                if (GUI.Button(new Rect(marginLeft + 52f, footerY - 1f, 120f, 26f), "KEMBALI", _footerStyle))
                {
                    _currentPage = MenuPage.Main;
                }
            }
            else
            {
                GUI.Label(new Rect(marginLeft, footerY, 340f, 24f), "Petualangan Lentera  ·  Cozy Stylized 3D", _footerStyle);
            }
        }

        private void DrawMainPage(float x, float startY)
        {
            bool hasSave = _cachedSave != null;
            float y = startY;
            const float rowH = 34f;
            int idx = 0;
            _hoveredIndex = -1;
            _activeDescription = string.Empty;

            if (hasSave)
            {
                int shards = _cachedSave.collectedShards != null ? _cachedSave.collectedShards.Count : 0;
                int stage = Mathf.Clamp(shards / 4, 0, 5);
                string meta = $"Zone Mangrove  ·  {shards}/24 Pecahan  ·  Tahap {stage}/5";

                if (DrawMenuItem(new Rect(x + 28f, y, 220f, rowH), idx++, "LANJUTKAN",
                    "Lanjutkan petualangan menyibak kabut dari titik tersimpan.", meta))
                {
                    string targetScene = string.IsNullOrEmpty(_cachedSave.lastSceneName) ? firstLevelScene : _cachedSave.lastSceneName;
                    SceneManager.LoadScene(targetScene);
                }
                y += rowH + 16f;
            }

            if (DrawMenuItem(new Rect(x, y, 260f, rowH), idx++, "MULAI BARU",
                "Mulai perjalanan baru dari dermaga Desa Pelita."))
            {
                SaveSystem.DeleteSave();
                SceneManager.LoadScene(firstLevelScene);
            }
            y += rowH + 6f;

            if (DrawMenuItem(new Rect(x, y, 260f, rowH), idx++, "PENGATURAN",
                "Atur volume audio, kualitas grafis, dan mode layar penuh."))
            {
                _currentPage = MenuPage.Options;
            }
            y += rowH + 18f;

            if (DrawMenuItem(new Rect(x, y, 260f, rowH), idx++, "KELUAR",
                "Keluar dari permainan Pulau Kabut."))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            y += rowH + 42f;

            if (string.IsNullOrEmpty(_activeDescription))
            {
                _activeDescription = hasSave
                    ? "Pilih Lanjutkan untuk kembali ke Pulau Kabut, atau Mulai Baru."
                    : "Mulai petualangan baru mengungkap misteri Pulau Kabut.";
            }

            GUI.Label(new Rect(x + 28f, y, 420f, 44f), _activeDescription, _descStyle);
        }

        private void DrawOptionsPage(float x, float startY)
        {
            float y = startY;
            const float rowH = 36f;
            int idx = 0;
            _hoveredIndex = -1;
            _activeDescription = string.Empty;

            int volPct = Mathf.RoundToInt(_masterVolume * 100f);
            if (DrawMenuItem(new Rect(x, y, 340f, rowH), idx++, $"VOLUME UTAMA  :  {volPct}%",
                "Klik untuk mengubah tingkat volume keseluruhan (25% / 50% / 75% / 100% / Bisu)."))
            {
                _masterVolume = _masterVolume >= 0.99f ? 0f : Mathf.Clamp01(_masterVolume + 0.25f);
                AudioListener.volume = _masterVolume;
                PlayerPrefs.SetFloat("PK_MasterVolume", _masterVolume);
                PlayerPrefs.Save();
            }
            y += rowH + 8f;

            string[] qualityNames = QualitySettings.names;
            string qLabel = (_qualityIndex >= 0 && _qualityIndex < qualityNames.Length) ? qualityNames[_qualityIndex] : "Default";
            if (DrawMenuItem(new Rect(x, y, 360f, rowH), idx++, $"KUALITAS GRAFIS  :  {qLabel.ToUpperInvariant()}",
                "Klik untuk mengganti tingkat detail pencahayaan dan bayangan."))
            {
                if (qualityNames.Length > 0)
                {
                    _qualityIndex = (_qualityIndex + 1) % qualityNames.Length;
                    QualitySettings.SetQualityLevel(_qualityIndex, true);
                }
            }
            y += rowH + 8f;

            string screenLabel = _isFullscreen ? "LAYAR PENUH" : "JENDELA";
            if (DrawMenuItem(new Rect(x, y, 340f, rowH), idx++, $"TAMPILAN  :  {screenLabel}",
                "Klik untuk beralih antara mode Layar Penuh dan Jendela."))
            {
                _isFullscreen = !_isFullscreen;
                Screen.fullScreen = _isFullscreen;
            }
            y += rowH + 42f;

            if (string.IsNullOrEmpty(_activeDescription))
            {
                _activeDescription = "Klik salah satu opsi di atas untuk mengubah pengaturan.";
            }
            GUI.Label(new Rect(x, y, 420f, 44f), _activeDescription, _descStyle);
        }

        private bool DrawMenuItem(Rect rect, int index, string label, string description, string rightMeta = null)
        {
            Vector2 mousePos = Event.current.mousePosition;
            bool isHovered = rect.Contains(mousePos);
            if (isHovered)
            {
                _hoveredIndex = index;
                _activeDescription = description;
            }

            if (isHovered)
            {
                // Garis aksen emas tipis di kiri item yang sedang disorot
                Color prev = GUI.color;
                GUI.color = StylizedUITheme.GoldAccent;
                GUI.DrawTexture(new Rect(rect.x - 14f, rect.y + 7f, 3f, rect.height - 14f), StylizedUITheme.SolidTex);
                GUI.color = prev;
            }

            GUIStyle style = isHovered ? _itemHoverStyle : _itemNormalStyle;
            bool clicked = GUI.Button(rect, label, style);

            if (!string.IsNullOrEmpty(rightMeta))
            {
                Vector2 labelSize = style.CalcSize(new GUIContent(label));
                GUI.Label(new Rect(rect.x + labelSize.x + 18f, rect.y + 2f, 320f, rect.height), rightMeta, _continueMetaStyle);
            }

            return clicked;
        }

        private static void DrawShadowedLabel(Rect rect, string text, GUIStyle mainStyle, GUIStyle shadowStyle)
        {
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, shadowStyle);
            GUI.Label(rect, text, mainStyle);
        }

        private void EnsureStyles()
        {
            if (_leftScrimTex == null)
            {
                // Gradasi horizontal deep-ocean-teal (#09161b) dari kiri ke transparan di kanan
                const int w = 128;
                _leftScrimTex = new Texture2D(w, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                Color[] px = new Color[w];
                for (int x = 0; x < w; x++)
                {
                    float t = x / (float)(w - 1);
                    float alpha = Mathf.Lerp(0.86f, 0f, Mathf.Pow(t, 0.72f));
                    px[x] = new Color(0.035f, 0.085f, 0.105f, alpha);
                }
                _leftScrimTex.SetPixels(px);
                _leftScrimTex.Apply();
            }

            if (_titleStyle != null) return;

            _titleStyle = StylizedUITheme.CreateHeadingLabelStyle(36, StylizedUITheme.CreamText, TextAnchor.MiddleLeft);
            _titleShadowStyle = StylizedUITheme.CreateHeadingLabelStyle(36, new Color(0.02f, 0.05f, 0.06f, 0.85f), TextAnchor.MiddleLeft);

            _itemNormalStyle = StylizedUITheme.CreateBodyLabelStyle(20, new Color(0.663f, 0.812f, 0.784f, 0.68f), TextAnchor.MiddleLeft);
            _itemHoverStyle = StylizedUITheme.CreateHeadingLabelStyle(21, StylizedUITheme.GoldAccent, TextAnchor.MiddleLeft);

            _continueMetaStyle = StylizedUITheme.CreateBodyLabelStyle(14, new Color(0.965f, 0.937f, 0.851f, 0.62f), TextAnchor.MiddleLeft);
            _descStyle = StylizedUITheme.CreateBodyLabelStyle(14, new Color(0.965f, 0.937f, 0.851f, 0.72f), TextAnchor.UpperLeft, wordWrap: true);
            _footerStyle = StylizedUITheme.CreateBodyLabelStyle(13, new Color(0.663f, 0.812f, 0.784f, 0.78f), TextAnchor.MiddleLeft);
            _keycapStyle = StylizedUITheme.CreateKeycapStyle(11);
        }
    }
}
