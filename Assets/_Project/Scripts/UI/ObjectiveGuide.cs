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
        private string _cachedListText = string.Empty;
        private Transform _cameraTransform;
        private GUIStyle _bannerStyle;
        private GUIStyle _listStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _keyStyle;
        private GUIStyle _subLabelStyle;
        private GUIStyle _missionTextStyle;
        private GUIStyle _distBadgeStyle;
        private GUIStyle _doneTextStyle;
        private string _distText = "0 m";
        private float _bannerTimer;
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

            // 1. Banner notifikasi misi baru (chamfered auto-width, muncul sekali lalu memudar halus!)
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

        private void EnsureStyles()
        {
            if (_bannerStyle == null)
            {
                _bannerStyle = StylizedUITheme.CreatePillStyle(15, TextAnchor.MiddleCenter, 18, 8);
                _bannerStyle.normal.textColor = StylizedUITheme.GoldAccent;
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
