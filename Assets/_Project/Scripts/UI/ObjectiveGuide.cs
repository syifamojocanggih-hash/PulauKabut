using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.UI
{
    /// <summary>
    /// Mengelola notifikasi misi baru (muncul sekali saja selama beberapa detik di atas layar)
    /// dan panel Daftar Misi (toggle tombol J atau klik tombol UI) yang mencatat riwayat misi selesai & aktif.
    /// </summary>
    public class ObjectiveGuide : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;

        public static ObjectiveGuide Instance { get; private set; }

        [Header("Tujuan Awal")]
        [Tooltip("Teks tujuan awal saat scene dimulai.")]
        [SerializeField] private string objectiveTitle = "Bicara dengan Mbah Sauh di Dermaga";

        [Tooltip("Transform target lokasi tujuan di dunia.")]
        [SerializeField] private Transform targetTransform;

        [Tooltip("Transform pemain sebagai titik ukur jarak (kosong = otomatis cari Main Camera).")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("Aktifkan notifikasi misi sekali muncul saat tujuan baru diterima.")]
        [SerializeField] private bool showGuide = true;

        [Tooltip("Durasi (detik) banner notifikasi misi baru tampil di bagian atas layar sebelum menghilang.")]
        [Min(1.5f)]
        [SerializeField] private float bannerDisplayDuration = 5.0f;

        private readonly List<string> _completedMissions = new List<string>(12);
        private readonly StringBuilder _listBuilder = new StringBuilder(320);

        private string _bannerText = string.Empty;
        private string _unlockTitle = string.Empty;
        private string _unlockSubtitle = string.Empty;
        private string _unlockTagText = "ITEM BARU DIPEROLEH";
        private Texture2D _unlockIcon;
        private Texture2D _cachedLanternIcon;
        private string _cachedListText = string.Empty;
        private Transform _cameraTransform;
        private GUIStyle _bannerStyle;
        private GUIStyle _unlockCardStyle;
        private GUIStyle _unlockTagStyle;
        private GUIStyle _unlockTitleStyle;
        private GUIStyle _unlockTitleShadowStyle;
        private GUIStyle _unlockSubStyle;
        private GUIStyle _unlockHintStyle;
        private GUIStyle _listStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _keyStyle;
        private GUIStyle _subLabelStyle;
        private GUIStyle _missionTextStyle;
        private GUIStyle _distBadgeStyle;
        private GUIStyle _doneTextStyle;
        private string _distText = "0 m";
        private float _bannerTimer;
        private float _unlockBannerTimer;
        private float _unlockElapsed;
        private float _refreshTimer;

        /// <summary>Transform tujuan misi aktif saat ini (dipakai juga oleh MinimapHUD).</summary>
        public Transform CurrentTarget => targetTransform;

        /// <summary>Judul misi aktif saat ini.</summary>
        public string CurrentTitle => objectiveTitle;

        /// <summary>True jika panel Daftar Misi sedang dibuka.</summary>
        public bool ShowMissionList { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (showGuide && !string.IsNullOrEmpty(objectiveTitle))
            {
                TriggerBanner(objectiveTitle);
            }
            RebuildMissionListText();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
            {
                ToggleMissionList();
            }

            if (_bannerTimer > 0f)
            {
                _bannerTimer = Mathf.Max(0f, _bannerTimer - Time.unscaledDeltaTime);
            }

            if (_unlockBannerTimer > 0f)
            {
                _unlockElapsed += Time.unscaledDeltaTime;
                _unlockBannerTimer = Mathf.Max(0f, _unlockBannerTimer - Time.unscaledDeltaTime);

                if (_unlockElapsed > 0.55f && Keyboard.current != null &&
                    (Keyboard.current.fKey.wasPressedThisFrame ||
                     Keyboard.current.eKey.wasPressedThisFrame ||
                     Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    _unlockBannerTimer = 0f;
                }
            }

            if (ShowMissionList)
            {
                _refreshTimer += Time.unscaledDeltaTime;
                if (_refreshTimer >= RefreshInterval)
                {
                    _refreshTimer = 0f;
                    RebuildMissionListText();
                }
            }
        }

        /// <summary>Membuka atau menutup panel Daftar Misi.</summary>
        public void ToggleMissionList()
        {
            ShowMissionList = !ShowMissionList;
            if (ShowMissionList)
            {
                RebuildMissionListText();
            }
        }

        /// <summary>Mengubah tujuan aktif, menandai misi sebelumnya selesai, dan menampilkan banner sekali saja.</summary>
        public void SetObjective(string title, Transform newTarget)
        {
            if (!string.IsNullOrEmpty(objectiveTitle) && objectiveTitle != title && !_completedMissions.Contains(objectiveTitle))
            {
                _completedMissions.Add(objectiveTitle);
            }

            objectiveTitle = title;
            targetTransform = newTarget;
            showGuide = newTarget != null;

            if (!string.IsNullOrEmpty(title))
            {
                TriggerBanner(title);
            }

            RebuildMissionListText();
        }

        private void TriggerBanner(string title)
        {
            _bannerText = "Misi Baru: " + title;
            _bannerTimer = bannerDisplayDuration;
        }

        /// <summary>Menampilkan popup showcase item/lokasi baru di tengah layar beserta ikonnya.</summary>
        public void ShowUnlockNotification(string title, string subtitle = null, Texture2D icon = null, float duration = 0f, string tagText = null)
        {
            if (string.IsNullOrEmpty(title)) return;

            _unlockTitle = title;
            _unlockTagText = string.IsNullOrEmpty(tagText) ? "ITEM BARU DIPEROLEH" : tagText;
            _unlockSubtitle = string.IsNullOrEmpty(subtitle)
                ? "Tekan [F] untuk menyalakan Lentera"
                : subtitle;

            if (icon != null)
            {
                _unlockIcon = icon;
            }
            else if (title.IndexOf("Lentera", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (_cachedLanternIcon == null)
                {
                    _cachedLanternIcon = Resources.Load<Texture2D>("UI/Icon_HandLantern");
                }
                _unlockIcon = _cachedLanternIcon != null ? _cachedLanternIcon : StylizedUITheme.LanternIconTex;
            }
            else if (_unlockTagText.IndexOf("LOKASI", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     title.IndexOf("SECRET LOCATION", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     title.IndexOf("Pulau", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _unlockIcon = StylizedUITheme.IslandIconTex;
            }
            else
            {
                _unlockIcon = StylizedUITheme.PentagonIconTex;
            }

            _unlockElapsed = 0f;
            _unlockBannerTimer = duration > 0f ? duration : Mathf.Max(6.0f, bannerDisplayDuration);
        }

        private void RebuildMissionListText()
        {
            if (targetTransform != null)
            {
                Transform origin = playerTransform != null ? playerTransform : _cameraTransform;
                if (origin != null)
                {
                    int dist = Mathf.RoundToInt(Vector3.Distance(origin.position, targetTransform.position));
                    _distText = $"{dist} m";
                }
            }
            else
            {
                _distText = string.Empty;
            }
        }

        private void OnGUI()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            EnsureStyles();

            // 1a. Showcase popup item baru di TENGAH LAYAR (Lentera Unlocked + gambar lentera!)
            bool hasUnlockBanner = _unlockBannerTimer > 0f && !string.IsNullOrEmpty(_unlockTitle);
            if (hasUnlockBanner)
            {
                DrawUnlockShowcaseCard();
            }

            // 1b. Banner notifikasi misi baru (chamfered auto-width, muncul sekali di atas lalu memudar halus!)
            if (_bannerTimer > 0f && !string.IsNullOrEmpty(_bannerText))
            {
                float alpha = _bannerTimer < 0.8f ? Mathf.Clamp01(_bannerTimer / 0.8f) : 1f;
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                Vector2 textSize = _bannerStyle.CalcSize(new GUIContent(_bannerText));
                float w = Mathf.Clamp(textSize.x + 36f, 260f, Screen.width - 280f);
                const float h = 42f;
                Rect rect = new Rect((Screen.width - w) * 0.5f, 14f, w, h);
                GUI.Box(rect, _bannerText, _bannerStyle);

                GUI.color = prev;
            }

            // 2. Panel DAFTAR MISI gaya Hud.dc.html (header emas + keycap [J] tutup + garis emas + berlian mint + badge jarak)
            if (ShowMissionList)
            {
                const float listW = 320f;
                float listH = Mathf.Clamp(118f + _completedMissions.Count * 24f, 118f, 340f);
                Rect card = new Rect(Screen.width - listW - 14f, 56f, listW, listH);
                GUI.Box(card, GUIContent.none, _listStyle);

                // Header: DAFTAR MISI + [J] tutup
                GUI.Label(new Rect(card.x + 18f, card.y + 12f, 180f, 26f), "DAFTAR MISI", _headerStyle);
                GUI.Box(new Rect(card.xMax - 82f, card.y + 13f, 24f, 22f), "J", _keyStyle);
                GUI.Label(new Rect(card.xMax - 54f, card.y + 13f, 44f, 22f), "tutup", _subLabelStyle);

                // Garis pemisah emas tipis
                Color prev = GUI.color;
                GUI.color = StylizedUITheme.GoldAccent;
                GUI.DrawTexture(new Rect(card.x + 18f, card.y + 42f, card.width - 36f, 2f), StylizedUITheme.SolidTex);

                // Baris Misi Aktif: berlian mint berkedip + judul + badge jarak mint
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
                GUI.color = new Color(StylizedUITheme.MintAccent.r, StylizedUITheme.MintAccent.g, StylizedUITheme.MintAccent.b, 0.55f + 0.45f * pulse);
                GUI.DrawTexture(new Rect(card.x + 18f, card.y + 58f, 14f, 14f), StylizedUITheme.DiamondTex);
                GUI.color = prev;

                float badgeW = string.IsNullOrEmpty(_distText) ? 0f : 56f;
                GUI.Label(new Rect(card.x + 40f, card.y + 52f, card.width - 62f - badgeW, 44f), objectiveTitle, _missionTextStyle);

                if (!string.IsNullOrEmpty(_distText))
                {
                    GUI.Box(new Rect(card.xMax - badgeW - 14f, card.y + 54f, badgeW, 26f), _distText, _distBadgeStyle);
                }

                // Riwayat Misi Selesai
                if (_completedMissions.Count > 0)
                {
                    float y = card.y + 102f;
                    for (int i = 0; i < _completedMissions.Count; i++)
                    {
                        GUI.Label(new Rect(card.x + 18f, y, card.width - 36f, 22f), "✓  " + _completedMissions[i], _doneTextStyle);
                        y += 22f;
                    }
                }
            }
        }

        private void DrawUnlockShowcaseCard()
        {
            float fadeOut = _unlockBannerTimer < 0.7f ? Mathf.Clamp01(_unlockBannerTimer / 0.7f) : 1f;
            float fadeIn = Mathf.Clamp01(_unlockElapsed / 0.22f);
            float alpha = Mathf.Min(fadeIn, fadeOut);
            float pop = Mathf.SmoothStep(0.92f, 1.0f, fadeIn);

            Color prev = GUI.color;

            // Lapisan redup lembut di seluruh layar agar fokus tertuju ke tengah
            GUI.color = new Color(0.02f, 0.07f, 0.09f, 0.48f * alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), StylizedUITheme.SolidTex);

            GUI.color = new Color(1f, 1f, 1f, alpha);

            float cardW = Mathf.Clamp(480f, 360f, Screen.width - 64f) * pop;
            float cardH = Mathf.Clamp(344f, 290f, Screen.height - 64f) * pop;
            Rect card = new Rect((Screen.width - cardW) * 0.5f, (Screen.height - cardH) * 0.5f - 8f, cardW, cardH);

            if (GUI.Button(card, GUIContent.none, _unlockCardStyle) && _unlockElapsed > 0.55f)
            {
                _unlockBannerTimer = 0f;
            }

            // Badge mint di atas (misal: ITEM BARU DIPEROLEH / LOKASI RAHASIA)
            const float tagW = 204f;
            const float tagH = 26f;
            GUI.Box(new Rect(card.x + (card.width - tagW) * 0.5f, card.y + 18f, tagW, tagH), _unlockTagText, _unlockTagStyle);

            // Garis pemisah emas halus
            GUI.color = new Color(StylizedUITheme.GoldAccent.r, StylizedUITheme.GoldAccent.g, StylizedUITheme.GoldAccent.b, 0.55f * alpha);
            GUI.DrawTexture(new Rect(card.x + 36f, card.y + 54f, card.width - 72f, 2f), StylizedUITheme.SolidTex);

            // Aura cahaya radial emas berdenyut di belakang gambar lentera
            float glowPulse = 0.80f + 0.20f * Mathf.Sin(Time.unscaledTime * 3.2f);
            float glowSize = 180f;
            float bobY = Mathf.Sin(Time.unscaledTime * 2.4f) * 4f;
            float iconCenterY = card.y + 136f + bobY;

            GUI.color = new Color(1f, 1f, 1f, glowPulse * alpha);
            GUI.DrawTexture(
                new Rect(card.x + (card.width - glowSize) * 0.5f, iconCenterY - glowSize * 0.5f, glowSize, glowSize),
                StylizedUITheme.UnlockGlowTex,
                ScaleMode.ScaleToFit);

            // Gambar Lentera resolusi tinggi
            if (_unlockIcon != null)
            {
                const float iconSize = 136f;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(
                    new Rect(card.x + (card.width - iconSize) * 0.5f, iconCenterY - iconSize * 0.5f, iconSize, iconSize),
                    _unlockIcon,
                    ScaleMode.ScaleToFit);
            }

            // Judul besar emas + drop shadow (mendukung 2 baris agar tidak terpotong)
            Rect titleRect = new Rect(card.x + 22f, card.y + 206f, card.width - 44f, 52f);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(titleRect.x + 1.5f, titleRect.y + 2f, titleRect.width, titleRect.height), _unlockTitle.ToUpperInvariant(), _unlockTitleShadowStyle);
            GUI.Label(titleRect, _unlockTitle.ToUpperInvariant(), _unlockTitleStyle);

            // Subjudul panduan penggunaan
            Rect subRect = new Rect(card.x + 28f, card.y + 260f, card.width - 56f, 36f);
            GUI.Label(subRect, _unlockSubtitle, _unlockSubStyle);

            // Petunjuk singkat di footer
            Rect hintRect = new Rect(card.x + 24f, card.yMax - 34f, card.width - 48f, 22f);
            GUI.Label(hintRect, "[E / Spasi / Klik] tutup", _unlockHintStyle);

            GUI.color = prev;
        }

        private void EnsureStyles()
        {
            if (_bannerStyle == null)
            {
                _bannerStyle = StylizedUITheme.CreatePillStyle(15, TextAnchor.MiddleCenter, 18, 8);
                _bannerStyle.normal.textColor = StylizedUITheme.GoldAccent;
            }

            if (_unlockCardStyle == null)
            {
                _unlockCardStyle = StylizedUITheme.CreateGoldPanelStyle(14, TextAnchor.UpperCenter, 16, 16);
                _unlockCardStyle.hover.background = StylizedUITheme.GoldPanelTex;
                _unlockCardStyle.active.background = StylizedUITheme.GoldPanelTex;

                _unlockTagStyle = StylizedUITheme.CreateMintBadgeStyle(12);
                _unlockTitleStyle = StylizedUITheme.CreateHeadingLabelStyle(22, StylizedUITheme.GoldAccent, TextAnchor.MiddleCenter);
                _unlockTitleStyle.wordWrap = true;
                _unlockTitleShadowStyle = StylizedUITheme.CreateHeadingLabelStyle(22, new Color(0f, 0f, 0f, 0.55f), TextAnchor.MiddleCenter);
                _unlockTitleShadowStyle.wordWrap = true;
                _unlockSubStyle = StylizedUITheme.CreateBodyLabelStyle(14, StylizedUITheme.CreamText, TextAnchor.MiddleCenter, wordWrap: true);
                _unlockHintStyle = StylizedUITheme.CreateBodyLabelStyle(12, StylizedUITheme.SeafoamText, TextAnchor.MiddleCenter);
            }

            if (_listStyle == null)
            {
                _listStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 16, 12);
                _headerStyle = StylizedUITheme.CreateHeadingLabelStyle(16, StylizedUITheme.GoldAccent, TextAnchor.MiddleLeft);
                _keyStyle = StylizedUITheme.CreateKeycapStyle(12);
                _subLabelStyle = StylizedUITheme.CreateBodyLabelStyle(12, StylizedUITheme.SeafoamText, TextAnchor.MiddleLeft);
                _missionTextStyle = StylizedUITheme.CreateBodyLabelStyle(14, StylizedUITheme.CreamText, TextAnchor.UpperLeft, wordWrap: true);
                _distBadgeStyle = StylizedUITheme.CreateMintBadgeStyle(12);
                _doneTextStyle = StylizedUITheme.CreateBodyLabelStyle(12, StylizedUITheme.SeafoamText, TextAnchor.MiddleLeft);
            }
        }
    }
}
