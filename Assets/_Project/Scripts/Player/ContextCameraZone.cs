using Unity.Cinemachine;
using UnityEngine;

namespace PulauKabut.Player
{
    /// <summary>
    /// Zona pemicu POV 2 (Kamera Alur / Konteks) berbasis Cinemachine 3.
    /// Otomatis menyesuaikan sudut pandang kamera dengan posisi dan tujuan alur saat itu
    /// tanpa pemain perlu mengklik atau mengatur kamera manual.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ContextCameraZone : MonoBehaviour
    {
        [Header("Kamera POV 2 (Cinemachine 3)")]
        [Tooltip("CinemachineCamera tujuan saat pemain memasuki momen alur ini.")]
        [SerializeField] private CinemachineCamera contextCamera;

        [Tooltip("Prioritas saat aktif (harus lebih tinggi dari POV 1 yang bernilai 10).")]
        [SerializeField] private int activePriority = 20;

        [Tooltip("Prioritas saat tidak aktif (lebih rendah dari POV 1).")]
        [SerializeField] private int inactivePriority = 0;

        [Header("Perilaku Alur Otomatis")]
        [Tooltip("Opsional: objek tujuan alur yang ingin disorot kamera (misal Menara Lentera, NPC, atau Papan Petunjuk).")]
        [SerializeField] private Transform storyFocusTarget;

        [Tooltip("Jika dicentang, momen kamera alur ini hanya diputar sekali saat pertama kali pemain masuk.")]
        [SerializeField] private bool playOnce = false;

        [Tooltip("Durasi otomatis kembali ke POV 1 (detik). Isi 0 agar tetap di POV 2 selama pemain masih berada di dalam zona.")]
        [Min(0f)]
        [SerializeField] private float autoReturnDuration = 0f;

        [Tooltip("Jika dicentang, gerak pemain berhenti sejenak selama momen alur berlangsung agar fokus melihat tujuan.")]
        [SerializeField] private bool pausePlayerDuringShot = false;

        [Header("Teks Narasi Sinematik (Opsional)")]
        [Tooltip("Judul singkat lokasi/momen yang tampil saat kamera alur aktif.")]
        [SerializeField] private string captionTitle = "";

        [Tooltip("Kalimat narasi singkat (subtitle) yang tampil di bawah layar selama momen alur berlangsung.")]
        [TextArea(1, 3)]
        [SerializeField] private string captionSubtitle = "";

        private bool _hasPlayed;
        private bool _isActive;
        private float _activeTimer;
        private PlayerController _activePlayer;
        private GUIStyle _captionBoxStyle;
        private GUIStyle _captionTitleStyle;
        private GUIStyle _captionBodyStyle;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Awake()
        {
            if (contextCamera != null && storyFocusTarget != null)
            {
                contextCamera.Target.LookAtTarget = storyFocusTarget;
                contextCamera.Target.CustomLookAtTarget = true;
            }

            SetCameraActive(false);
        }

        private void Update()
        {
            if (!_isActive || autoReturnDuration <= 0f)
            {
                return;
            }

            _activeTimer -= Time.deltaTime;
            if (_activeTimer <= 0f)
            {
                DeactivateZone();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (playOnce && _hasPlayed)
            {
                return;
            }

            if (other.TryGetComponent<PlayerController>(out PlayerController player))
            {
                ActivateZone(player);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Jika memakai autoReturnDuration, biarkan timer yang mengembalikan kamera dengan tenang
            if (autoReturnDuration > 0f)
            {
                return;
            }

            if (other.TryGetComponent<PlayerController>(out _))
            {
                DeactivateZone();
            }
        }

        private void ActivateZone(PlayerController player)
        {
            _hasPlayed = true;
            _isActive = true;
            _activePlayer = player;
            _activeTimer = autoReturnDuration;

            if (pausePlayerDuringShot && _activePlayer != null)
            {
                _activePlayer.InputLocked = true;
            }

            SetCameraActive(true);
        }

        private void DeactivateZone()
        {
            _isActive = false;

            if (pausePlayerDuringShot && _activePlayer != null)
            {
                _activePlayer.InputLocked = false;
            }

            _activePlayer = null;
            SetCameraActive(false);
        }

        private void SetCameraActive(bool isActive)
        {
            if (contextCamera == null)
            {
                return;
            }

            contextCamera.Priority.Enabled = true;
            contextCamera.Priority.Value = isActive ? activePriority : inactivePriority;
        }

        private void OnGUI()
        {
            if (!_isActive || Time.timeScale <= 0f)
            {
                return;
            }

            bool hasTitle = !string.IsNullOrEmpty(captionTitle);
            bool hasSub = !string.IsNullOrEmpty(captionSubtitle);
            if (!hasTitle && !hasSub)
            {
                return;
            }

            if (_captionBoxStyle == null)
            {
                _captionBoxStyle = new GUIStyle(GUI.skin.box);
                _captionTitleStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16,
                    fontStyle = FontStyle.Bold
                };
                _captionTitleStyle.normal.textColor = new Color(1f, 0.88f, 0.48f);

                _captionBodyStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    wordWrap = true
                };
                _captionBodyStyle.normal.textColor = Color.white;
            }

            float w = Mathf.Min(640f, Screen.width - 80f);
            const float h = 84f;
            Rect panel = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 34f, w, h);
            GUI.Box(panel, GUIContent.none, _captionBoxStyle);

            if (hasTitle)
            {
                GUI.Label(new Rect(panel.x + 16f, panel.y + 10f, panel.width - 32f, 26f), captionTitle, _captionTitleStyle);
            }

            if (hasSub)
            {
                float yOffset = hasTitle ? 38f : 18f;
                GUI.Label(new Rect(panel.x + 16f, panel.y + yOffset, panel.width - 32f, 40f), captionSubtitle, _captionBodyStyle);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                return;
            }

            Gizmos.color = new Color(0.15f, 0.85f, 0.95f, 0.18f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = new Color(0.15f, 0.85f, 0.95f, 0.75f);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.color = new Color(0.15f, 0.85f, 0.95f, 0.75f);
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
#endif
    }
}
