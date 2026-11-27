using PulauKabut.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PulauKabut.UI
{
    /// <summary>
    /// Layar Menu Utama Pulau Kabut bergaya chamfered teal-emas (selaras dengan HUD Lengkap)
    /// dengan menu 4 slot save di bagian LANJUTKAN (slot paling atas = Auto Save di Checkpoint),
    /// skala adaptif layar, dukungan keyboard + mouse, dan diorama pesisir 3D hidup di sisi kanan.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private enum MenuPage
        {
            Main,
            ContinueSlots,
            Options
        }

        [Header("Scene Tujuan")]
        [SerializeField] private string firstLevelScene = "Zone_Mangrove";

        [Header("Kamera & Diorama 3D")]
        [SerializeField] private Transform orbitPivot;
        [SerializeField] private float swayAmplitudeDegrees = 11f;
        [SerializeField] private float swaySpeed = 0.22f;

        private GameSaveData[] _saveSlots = new GameSaveData[SaveSystem.TotalSlots];
        private GameSaveData _autoSave;
        private MenuPage _currentPage = MenuPage.Main;
        private int _selectedIndex;
        private string _activeDescription = string.Empty;
        private float _basePivotYaw;

        private float _masterVolume = 1f;
        private int _qualityIndex = 1;
        private bool _isFullscreen = true;

        private Texture2D _leftScrimTex;
        private Texture2D _lanternIcon;
        private Texture2D _menuBtnNormalTex;
        private Texture2D _menuBtnActiveTex;
        private Texture2D _slotEmptyTex;
        private GUIStyle _titleStyle;
        private GUIStyle _titleShadowStyle;
        private GUIStyle _subtitleBadgeStyle;
        private GUIStyle _btnNormalStyle;
        private GUIStyle _btnActiveStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _slotHoverStyle;
        private GUIStyle _slotEmptyStyle;
        private GUIStyle _slotEmptyTextStyle;
        private GUIStyle _cardHeaderStyle;
        private GUIStyle _cardBodyStyle;
        private GUIStyle _cardValueStyle;
        private GUIStyle _descStyle;
        private GUIStyle _footerStyle;
        private GUIStyle _keycapStyle;
        private GUIStyle _slotBadgeMintStyle;
        private GUIStyle _slotBadgeGoldStyle;
        private GUIStyle _deleteBtnStyle;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshSaveSlots();

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

        private void RefreshSaveSlots()
        {
            _saveSlots = SaveSystem.LoadAllSlots();
            _autoSave = _saveSlots[SaveSystem.AutoSaveSlotIndex];
        }

        private void OnDestroy()
        {
            if (_leftScrimTex != null) { Destroy(_leftScrimTex); _leftScrimTex = null; }
            if (_menuBtnNormalTex != null) { Destroy(_menuBtnNormalTex); _menuBtnNormalTex = null; }
            if (_menuBtnActiveTex != null) { Destroy(_menuBtnActiveTex); _menuBtnActiveTex = null; }
            if (_slotEmptyTex != null) { Destroy(_slotEmptyTex); _slotEmptyTex = null; }
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

            if (_currentPage != MenuPage.Main && Keyboard.current.escapeKey.wasPressedThisFrame)
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
            return _currentPage switch
            {
                MenuPage.ContinueSlots => SaveSystem.TotalSlots + 1, // 4 kotak save + tombol Kembali
                MenuPage.Options => 4, // 3 setting + Kembali
                _ => 4 // LANJUTKAN, MULAI BARU, PENGATURAN, KELUAR
            };
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
            float scrimW = Mathf.Max(540f, vw * 0.50f);
            GUI.DrawTexture(new Rect(0f, 0f, scrimW, vh), _leftScrimTex, ScaleMode.StretchToFill);

            float marginLeft = Mathf.Clamp(vw * 0.052f, 42f, 88f);
            float headerY = _currentPage == MenuPage.ContinueSlots
                ? 28f
                : Mathf.Clamp((vh - 500f) * 0.42f, 48f, 110f);

            // 2. Header Judul dengan ikon Lentera 3D & aura emas
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
            GUI.DrawTexture(new Rect(marginLeft, headerY + 70f, 408f, 2f), StylizedUITheme.SolidTex);
            GUI.color = prevCol;

            // 3. Halaman aktif
            float menuY = headerY + 82f;
            switch (_currentPage)
            {
                case MenuPage.Main:
                    DrawMainPage(marginLeft, menuY, vh);
                    break;
                case MenuPage.ContinueSlots:
                    DrawContinueSlotsPage(marginLeft, menuY, vh);
                    break;
                case MenuPage.Options:
                    DrawOptionsPage(marginLeft, menuY, vh);
                    break;
            }

            // 4. Footer kiri bawah (hanya tombol kembali saat di sub-halaman)
            float footerY = vh - 36f;
            if (_currentPage != MenuPage.Main)
            {
                GUI.Box(new Rect(marginLeft, footerY, 44f, 24f), "ESC", _keycapStyle);
                if (GUI.Button(new Rect(marginLeft + 52f, footerY - 1f, 140f, 26f), "KEMBALI KE MENU", _footerStyle))
                {
                    _currentPage = MenuPage.Main;
                    _selectedIndex = 0;
                }
            }

            GUI.matrix = prevMatrix;
        }

        private void DrawMainPage(float x, float startY, float vh)
        {
            float y = startY;
            const float btnW = 356f;
            const float btnH = 44f;
            const float gap = 10f;
            int idx = 0;
            _activeDescription = string.Empty;

            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "LANJUTKAN",
                "Pilih salah satu dari 4 simpanan perjalanan (termasuk Auto Save di Checkpoint)."))
            {
                RefreshSaveSlots();
                _currentPage = MenuPage.ContinueSlots;
                _selectedIndex = 0;
                return;
            }
            y += btnH + gap;

            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "MULAI BARU",
                "Mulai perjalanan baru dari dermaga Desa Pelita (simpanan manual tetap aman)."))
            {
                SaveSystem.DeleteSave();
                SaveSystem.PendingLoadSlot = -2;
                SceneManager.LoadScene(firstLevelScene);
                return;
            }
            y += btnH + gap;

            if (DrawMenuItem(new Rect(x, y, btnW, btnH), idx++, "PENGATURAN",
                "Atur volume audio, kualitas grafis, dan mode layar penuh."))
            {
                _currentPage = MenuPage.Options;
                _selectedIndex = 0;
                return;
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
            y += btnH + 20f;

            // Kartu Info / Progres Petualangan (menampilkan ringkasan Auto Save / save terbaru jika ada)
            GameSaveData previewSave = _autoSave ?? GetLatestAnySave();
            bool hasSave = previewSave != null;
            float cardH = hasSave ? 164f : 114f;
            float maxCardY = vh - cardH - 50f;
            float cardY = Mathf.Min(y, maxCardY);
            Rect card = new Rect(x, cardY, btnW, cardH);
            GUI.Box(card, GUIContent.none, _cardStyle);

            if (hasSave)
            {
                int shards = previewSave.collectedShards != null ? previewSave.collectedShards.Count : 0;
                int memories = previewSave.collectedMemories != null ? previewSave.collectedMemories.Count : 0;
                int stage = Mathf.Clamp(shards / 4, 0, 5);

                GUI.Label(new Rect(card.x + 18f, card.y + 12f, 260f, 24f), "JEJAK PETUALANGAN", _cardHeaderStyle);
                DrawStatRow(card.x + 18f, card.y + 40f, card.width - 36f, StylizedUITheme.PentagonIconTex, "Pecahan Cahaya", $"{shards} / 24");
                DrawStatRow(card.x + 18f, card.y + 66f, card.width - 36f, StylizedUITheme.HexagonIconTex, "Serpihan Ingatan", $"{memories} / 15");
                DrawStatRow(card.x + 18f, card.y + 92f, card.width - 36f, StylizedUITheme.HouseIconTex, "Desa Pelita", $"Tahap {stage}/5");

                string desc = string.IsNullOrEmpty(_activeDescription)
                    ? $"Checkpoint terakhir: {previewSave.checkpointName}"
                    : _activeDescription;
                GUI.Label(new Rect(card.x + 18f, card.y + 120f, card.width - 36f, 36f), desc, _descStyle);
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

        private void DrawContinueSlotsPage(float x, float startY, float vh)
        {
            GUI.Label(new Rect(x, startY - 4f, 436f, 24f), "LANJUTKAN PERJALANAN (4 SLOT)", _cardHeaderStyle);

            float y = startY + 26f;
            const float boxW = 436f;
            const float boxH = 96f;
            const float gap = 11f;
            const float padX = 20f;

            for (int i = 0; i < SaveSystem.TotalSlots; i++)
            {
                Rect slotRect = new Rect(x, y, boxW, boxH);
                GameSaveData data = _saveSlots[i];
                bool isAuto = i == SaveSystem.AutoSaveSlotIndex;
                bool hasData = data != null;

                bool isHovered = slotRect.Contains(Event.current.mousePosition);
                if (isHovered)
                {
                    _selectedIndex = i;
                }
                bool isSelected = isHovered || _selectedIndex == i;

                // Area tombol hapus di pojok kanan bawah untuk slot manual (Slot 1..3) agar tidak ikut memicu Load
                Rect delRect = new Rect(slotRect.xMax - 74f, slotRect.yMax - 30f, 56f, 21f);
                bool clickDelete = false;
                if (!isAuto && hasData)
                {
                    clickDelete = GUI.Button(delRect, "Hapus", _deleteBtnStyle);
                }

                // Gambar kotak slot: jika kosong buat abu-abu polos + tulisan "Kosong" di tengah
                if (!hasData)
                {
                    GUI.Box(slotRect, GUIContent.none, _slotEmptyStyle);
                    GUI.Label(slotRect, "Kosong", _slotEmptyTextStyle);
                }
                else
                {
                    GUI.Box(slotRect, GUIContent.none, isSelected ? _slotHoverStyle : _cardStyle);

                    // Judul slot & badge kanan atas (dengan jarak lega dari border & baris di bawahnya)
                    string slotTitle = isAuto ? "AUTO SAVE · CHECKPOINT" : $"PERJALANAN {i}";
                    GUI.Label(new Rect(slotRect.x + padX, slotRect.y + 12f, 260f, 22f), slotTitle, _cardHeaderStyle);

                    if (isAuto)
                    {
                        GUI.Box(new Rect(slotRect.xMax - 106f, slotRect.y + 11f, 88f, 22f), "OTOMATIS", _slotBadgeMintStyle);
                    }
                    else
                    {
                        GUI.Box(new Rect(slotRect.xMax - 120f, slotRect.y + 11f, 102f, 22f), data.savedTimestamp, _slotBadgeGoldStyle);
                    }

                    // Detail isi slot dengan spasi vertikal yang lega agar nyaman dibaca
                    int shards = data.collectedShards != null ? data.collectedShards.Count : 0;
                    int memories = data.collectedMemories != null ? data.collectedMemories.Count : 0;
                    int stage = Mathf.Clamp(shards / 4, 0, 5);
                    string cpLabel = string.IsNullOrEmpty(data.checkpointName) ? "Dermaga" : data.checkpointName;
                    string sceneLabel = string.IsNullOrEmpty(data.lastSceneName) ? firstLevelScene : data.lastSceneName;

                    GUI.Label(new Rect(slotRect.x + padX, slotRect.y + 40f, boxW - padX * 2f, 20f),
                        $"Checkpoint: {cpLabel}  ·  {sceneLabel}", _cardBodyStyle);

                    GUI.Label(new Rect(slotRect.x + padX, slotRect.y + 64f, boxW - padX - 110f, 20f),
                        $"Pecahan {shards}/24  ·  Ingatan {memories}/15  ·  Tahap {stage}/5", _descStyle);

                    if (isAuto && !string.IsNullOrEmpty(data.savedTimestamp))
                    {
                        GUI.Label(new Rect(slotRect.xMax - 120f, slotRect.y + 64f, 102f, 20f), data.savedTimestamp, _cardValueStyle);
                    }
                }

                // Gambar ulang tombol Hapus di atas box jika ada, supaya visual & kliknya tepat
                if (!isAuto && hasData)
                {
                    if (GUI.Button(delRect, "Hapus", _deleteBtnStyle) || clickDelete)
                    {
                        SaveSystem.DeleteSlot(i);
                        RefreshSaveSlots();
                        return;
                    }
                }

                bool clickedSlot = !clickDelete
                    && Event.current.type == EventType.MouseDown
                    && Event.current.button == 0
                    && slotRect.Contains(Event.current.mousePosition)
                    && !(!isAuto && hasData && delRect.Contains(Event.current.mousePosition));

                bool confirmedByKey = isSelected && Keyboard.current != null &&
                    (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);

                if (hasData && (clickedSlot || confirmedByKey))
                {
                    if (clickedSlot) Event.current.Use();
                    SaveSystem.PendingLoadSlot = i;
                    string targetScene = string.IsNullOrEmpty(data.lastSceneName) ? firstLevelScene : data.lastSceneName;
                    SceneManager.LoadScene(targetScene);
                    return;
                }

                y += boxH + gap;
            }

            // Tombol Kembali tepat di bawah ke-4 kotak
            float backY = Mathf.Min(y + 4f, vh - 86f);
            if (DrawMenuItem(new Rect(x, backY, 210f, 40f), SaveSystem.TotalSlots, "KEMBALI",
                "Kembali ke Menu Utama."))
            {
                _currentPage = MenuPage.Main;
                _selectedIndex = 0;
            }
        }

        private GameSaveData GetLatestAnySave()
        {
            for (int i = 0; i < _saveSlots.Length; i++)
            {
                if (_saveSlots[i] != null) return _saveSlots[i];
            }
            return null;
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
            y += btnH + gap;

            if (DrawMenuItem(new Rect(x, y, 210f, 40f), idx++, "KEMBALI",
                "Kembali ke Menu Utama."))
            {
                _currentPage = MenuPage.Main;
                _selectedIndex = 0;
                return;
            }
            y += 40f + 16f;

            float cardY = Mathf.Min(y, vh - 138f);
            Rect card = new Rect(x, cardY, btnW, 84f);
            GUI.Box(card, GUIContent.none, _cardStyle);
            GUI.Label(new Rect(card.x + 18f, card.y + 10f, 260f, 24f), "PENGATURAN", _cardHeaderStyle);
            string desc = string.IsNullOrEmpty(_activeDescription)
                ? "Klik opsi di atas atau tekan Esc untuk kembali."
                : _activeDescription;
            GUI.Label(new Rect(card.x + 18f, card.y + 38f, card.width - 36f, 40f), desc, _descStyle);
        }

        private bool DrawMenuItem(Rect rect, int index, string label, string description)
        {
            Vector2 mousePos = Event.current.mousePosition;
            bool isHovered = rect.Contains(mousePos);
            if (isHovered)
            {
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
                _menuBtnNormalTex = CreatePillButtonTex(
                    new Color(0.075f, 0.205f, 0.240f, 0.80f),
                    new Color(0.045f, 0.130f, 0.160f, 0.86f),
                    new Color(0.490f, 0.953f, 0.847f, 0.40f));
            }

            if (_menuBtnActiveTex == null)
            {
                _menuBtnActiveTex = CreatePillButtonTex(
                    new Color(0.130f, 0.335f, 0.380f, 0.97f),
                    new Color(0.075f, 0.195f, 0.235f, 0.98f),
                    StylizedUITheme.GoldAccent);
            }

            if (_slotEmptyTex == null)
            {
                _slotEmptyTex = CreateEmptySlotTex();
            }

            if (_titleStyle != null) return;

            _titleStyle = StylizedUITheme.CreateHeadingLabelStyle(42, StylizedUITheme.CreamText, TextAnchor.MiddleLeft);
            _titleShadowStyle = StylizedUITheme.CreateHeadingLabelStyle(42, new Color(0.02f, 0.05f, 0.06f, 0.92f), TextAnchor.MiddleLeft);
            _subtitleBadgeStyle = StylizedUITheme.CreateMintBadgeStyle(11);

            _btnNormalStyle = StylizedUITheme.CreatePillStyle(17, TextAnchor.MiddleLeft, 24, 8);
            _btnNormalStyle.font = StylizedUITheme.HeadingFont;
            _btnNormalStyle.border = new RectOffset(20, 20, 12, 12);
            _btnNormalStyle.normal.background = _menuBtnNormalTex;
            _btnNormalStyle.hover.background = _menuBtnActiveTex;
            _btnNormalStyle.active.background = _menuBtnActiveTex;
            _btnNormalStyle.normal.textColor = StylizedUITheme.CreamText;

            _btnActiveStyle = StylizedUITheme.CreatePillStyle(18, TextAnchor.MiddleLeft, 36, 8);
            _btnActiveStyle.font = StylizedUITheme.HeadingFont;
            _btnActiveStyle.border = new RectOffset(20, 20, 12, 12);
            _btnActiveStyle.normal.background = _menuBtnActiveTex;
            _btnActiveStyle.hover.background = _menuBtnActiveTex;
            _btnActiveStyle.active.background = _menuBtnActiveTex;
            _btnActiveStyle.normal.textColor = StylizedUITheme.GoldAccent;

            _cardStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 16, 12);
            _slotHoverStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 16, 12);
            _slotHoverStyle.normal.background = StylizedUITheme.GoldPanelHoverTex;

            _slotEmptyStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.MiddleCenter, 16, 12);
            _slotEmptyStyle.normal.background = _slotEmptyTex;
            _slotEmptyTextStyle = StylizedUITheme.CreateHeadingLabelStyle(16, new Color(0.66f, 0.70f, 0.74f, 0.92f), TextAnchor.MiddleCenter);

            _cardHeaderStyle = StylizedUITheme.CreateHeadingLabelStyle(14, StylizedUITheme.GoldAccent, TextAnchor.MiddleLeft);
            _cardBodyStyle = StylizedUITheme.CreateBodyLabelStyle(13, StylizedUITheme.SeafoamText, TextAnchor.MiddleLeft);
            _cardValueStyle = StylizedUITheme.CreateHeadingLabelStyle(13, StylizedUITheme.GoldAccent, TextAnchor.MiddleRight);

            _descStyle = StylizedUITheme.CreateBodyLabelStyle(12, new Color(0.965f, 0.937f, 0.851f, 0.86f), TextAnchor.UpperLeft, wordWrap: true);
            _footerStyle = StylizedUITheme.CreateBodyLabelStyle(12, new Color(0.663f, 0.812f, 0.784f, 0.84f), TextAnchor.MiddleLeft);
            _keycapStyle = StylizedUITheme.CreateKeycapStyle(11);
            _slotBadgeMintStyle = StylizedUITheme.CreateMintBadgeStyle(10);
            _slotBadgeGoldStyle = StylizedUITheme.CreateKeycapStyle(10);

            _deleteBtnStyle = StylizedUITheme.CreatePillStyle(11, TextAnchor.MiddleCenter, 6, 2);
            _deleteBtnStyle.border = new RectOffset(14, 14, 6, 6);
            _deleteBtnStyle.normal.background = _menuBtnNormalTex;
            _deleteBtnStyle.normal.textColor = new Color(1f, 0.62f, 0.56f, 1f);
            _deleteBtnStyle.hover.textColor = Color.white;
        }

        private static Texture2D CreatePillButtonTex(Color topBg, Color botBg, Color borderCol)
        {
            const int w = 128, h = 44;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            Color[] px = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float tl = 13f - ((h - y) + x);
                    float br = 13f - (y + (w - x));
                    float tr = 5f - ((h - y) + (w - x));
                    float bl = 5f - (y + x);
                    float maxCut = Mathf.Max(Mathf.Max(tl, br), Mathf.Max(tr, bl));
                    float dEdge = Mathf.Max(maxCut, Mathf.Max(Mathf.Max(-x + 1f, x - (w - 2f)), Mathf.Max(-y + 1f, y - (h - 2f))));
                    if (dEdge > 0f) { px[y * w + x] = Color.clear; continue; }

                    float aa = Mathf.Clamp01(-dEdge);
                    bool isBorder = dEdge > -2.6f;
                    Color c = isBorder ? borderCol : Color.Lerp(botBg, topBg, v);
                    c.a *= aa;
                    px[y * w + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateEmptySlotTex()
        {
            const int w = 128, h = 96;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            Color[] px = new Color[w * h];
            Color topBg = new Color(0.18f, 0.21f, 0.23f, 0.78f);
            Color botBg = new Color(0.12f, 0.14f, 0.16f, 0.84f);
            Color borderCol = new Color(0.45f, 0.49f, 0.53f, 0.72f);

            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float tl = 22f - ((h - y) + x);
                    float br = 22f - (y + (w - x));
                    float tr = 10f - ((h - y) + (w - x));
                    float bl = 10f - (y + x);
                    float maxCut = Mathf.Max(Mathf.Max(tl, br), Mathf.Max(tr, bl));
                    float dEdge = Mathf.Max(maxCut, Mathf.Max(Mathf.Max(-x + 1f, x - (w - 2f)), Mathf.Max(-y + 1f, y - (h - 2f))));
                    if (dEdge > 0f) { px[y * w + x] = Color.clear; continue; }

                    float aa = Mathf.Clamp01(-dEdge);
                    bool isBorder = dEdge > -3.5f;
                    Color c = isBorder ? borderCol : Color.Lerp(botBg, topBg, v);
                    c.a *= aa;
                    px[y * w + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
