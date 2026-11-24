using System.Text;
using UnityEngine;

namespace PulauKabut.UI
{
    /// <summary>
    /// Petunjuk arah tujuan aktif di bagian atas tengah layar:
    /// menampilkan judul tujuan, panah arah 8-penjuru relatif terhadap kamera, dan jarak dalam meter.
    /// </summary>
    public class ObjectiveGuide : MonoBehaviour
    {
        private const float RefreshInterval = 0.1f;

        public static ObjectiveGuide Instance { get; private set; }

        [Header("Tujuan Awal")]
        [Tooltip("Teks tujuan yang tampil di bagian atas layar.")]
        [SerializeField] private string objectiveTitle = "Bicara dengan Mbah Sauh";

        [Tooltip("Transform target lokasi tujuan di dunia.")]
        [SerializeField] private Transform targetTransform;

        [Tooltip("Transform pemain sebagai titik ukur jarak (kosong = otomatis cari Main Camera).")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("Tampilkan bar petunjuk arah di bagian atas tengah layar.")]
        [SerializeField] private bool showGuide = true;

        private readonly StringBuilder _builder = new StringBuilder(96);
        private string _cachedText = string.Empty;
        private Transform _cameraTransform;
        private GUIStyle _guideStyle;
        private float _timer;

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
            RebuildGuideText();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!showGuide || targetTransform == null)
            {
                return;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer >= RefreshInterval)
            {
                _timer = 0f;
                RebuildGuideText();
            }
        }

        /// <summary>Mengubah tujuan petunjuk arah secara dinamis dari dialog, puzzle, atau area trigger.</summary>
        public void SetObjective(string title, Transform newTarget)
        {
            objectiveTitle = title;
            targetTransform = newTarget;
            showGuide = newTarget != null;
            RebuildGuideText();
        }

        private void RebuildGuideText()
        {
            if (targetTransform == null)
            {
                _cachedText = string.Empty;
                return;
            }

            Transform origin = playerTransform != null ? playerTransform : _cameraTransform;
            if (origin == null) return;

            Vector3 toTarget = targetTransform.position - origin.position;
            float distance = toTarget.magnitude;

            // Hitung sudut mendatar relatif terhadap arah hadap kamera agar panah selalu akurat
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            Vector3 camForward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
            camForward.y = 0f;

            string arrow = "↑";
            if (flatToTarget.sqrMagnitude > 0.01f && camForward.sqrMagnitude > 0.01f)
            {
                float signedAngle = Vector3.SignedAngle(camForward.normalized, flatToTarget.normalized, Vector3.up);
                arrow = GetDirectionArrow(signedAngle);
            }

            _builder.Clear();
            _builder.Append(arrow).Append("  ").Append(objectiveTitle).Append("  (").Append(Mathf.RoundToInt(distance)).Append(" m)");
            _cachedText = _builder.ToString();
        }

        private static string GetDirectionArrow(float angleDeg)
        {
            if (angleDeg >= -22.5f && angleDeg < 22.5f) return "↑";
            if (angleDeg >= 22.5f && angleDeg < 67.5f) return "↗";
            if (angleDeg >= 67.5f && angleDeg < 112.5f) return "→";
            if (angleDeg >= 112.5f && angleDeg < 157.5f) return "↘";
            if (angleDeg >= 157.5f || angleDeg < -157.5f) return "↓";
            if (angleDeg >= -157.5f && angleDeg < -112.5f) return "↙";
            if (angleDeg >= -112.5f && angleDeg < -67.5f) return "←";
            return "↖";
        }

        private void OnGUI()
        {
            if (!showGuide || string.IsNullOrEmpty(_cachedText) || Time.timeScale <= 0f)
            {
                return;
            }

            if (_guideStyle == null)
            {
                _guideStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(16, 16, 8, 8)
                };
                _guideStyle.normal.textColor = new Color(1f, 0.94f, 0.68f);
            }

            const float w = 440f;
            const float h = 40f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, 14f, w, h);
            GUI.Box(rect, _cachedText, _guideStyle);
        }
    }
}
