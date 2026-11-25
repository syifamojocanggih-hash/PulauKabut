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
            _listBuilder.Clear();
            _listBuilder.Append("DAFTAR MISI [J tutup]\n");
            _listBuilder.Append("──────────────────────────────\n");

            if (!string.IsNullOrEmpty(objectiveTitle))
            {
                _listBuilder.Append("► ").Append(objectiveTitle);
                if (targetTransform != null)
                {
                    Transform origin = playerTransform != null ? playerTransform : _cameraTransform;
                    if (origin != null)
                    {
                        int dist = Mathf.RoundToInt(Vector3.Distance(origin.position, targetTransform.position));
                        _listBuilder.Append(" (").Append(dist).Append(" m)");
                    }
                }
                _listBuilder.Append('\n');
            }

            if (_completedMissions.Count > 0)
            {
                _listBuilder.Append("\nSelesai:\n");
                for (int i = 0; i < _completedMissions.Count; i++)
                {
                    _listBuilder.Append("✓ ").Append(_completedMissions[i]).Append('\n');
                }
            }

            _cachedListText = _listBuilder.ToString().TrimEnd('\n');
        }

        private void OnGUI()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            EnsureStyles();

            // 1. Banner notifikasi misi baru (pill stylized auto-width, muncul sekali lalu memudar halus!)
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

            // 2. Panel Daftar Misi (hanya muncul jika dibuka dengan tombol J atau klik [J] Misi)
            if (ShowMissionList && !string.IsNullOrEmpty(_cachedListText))
            {
                const float listW = 340f;
                float listH = Mathf.Clamp(95f + _completedMissions.Count * 22f, 105f, 320f);
                Rect listRect = new Rect(Screen.width - listW - 14f, 48f, listW, listH);
                GUI.Box(listRect, _cachedListText, _listStyle);
            }
        }

        private void EnsureStyles()
        {
            if (_bannerStyle == null)
            {
                _bannerStyle = StylizedUITheme.CreatePillStyle(14, TextAnchor.MiddleCenter, 18, 8);
                _bannerStyle.normal.textColor = new Color(1f, 0.96f, 0.80f);
            }

            if (_listStyle == null)
            {
                _listStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    fontSize = 13,
                    wordWrap = true,
                    padding = new RectOffset(12, 12, 10, 10)
                };
                _listStyle.normal.textColor = new Color(0.95f, 0.97f, 1f);
            }
        }
    }
}
