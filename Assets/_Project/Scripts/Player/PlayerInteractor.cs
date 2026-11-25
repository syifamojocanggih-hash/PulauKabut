using PulauKabut.Core;
using PulauKabut.Dialog;
using PulauKabut.UI;
using UnityEngine;

namespace PulauKabut.Player
{
    /// <summary>
    /// Mendeteksi objek IInteractable terdekat di sekitar pemain (tanpa alokasi GC di Update),
    /// menampilkan prompt [E] di layar, dan memicu Interact() saat tombol E ditekan.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        private const int MaxHits = 16;

        [Header("Deteksi Interaksi")]
        [Tooltip("Jarak jangkau interaksi dari posisi pemain (meter).")]
        [Min(0.5f)]
        [SerializeField] private float interactRadius = 2.8f;

        [Tooltip("Layer objek yang bisa diinteraksikan (jangan sertakan layer Player).")]
        [SerializeField] private LayerMask interactMask = ~0;

        private readonly Collider[] _hitBuffer = new Collider[MaxHits];
        private readonly GUIContent _promptContent = new GUIContent();
        private PlayerInputActions _input;
        private IInteractable _currentTarget;
        private GUIStyle _pillStyle;
        private GUIStyle _badgeTextStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _shadowLabelStyle;

        /// <summary>Objek interaktif terdekat yang sedang disorot pemain.</summary>
        public IInteractable CurrentTarget => _currentTarget;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input ??= new PlayerInputActions();
            _input.Player.Enable();
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
        }

        private void OnDestroy()
        {
            _input?.Dispose();
        }

        private void Update()
        {
            // Jangan deteksi interaksi baru jika game dijeda atau sedang dalam percakapan dialog
            if (Time.timeScale <= 0f || (DialogManager.Instance != null && DialogManager.Instance.IsOpen))
            {
                _currentTarget = null;
                return;
            }

            FindNearestInteractable();

            if (_currentTarget != null && _input.Player.Interact.WasPressedThisFrame())
            {
                _currentTarget.Interact(gameObject);
            }
        }

        private void FindNearestInteractable()
        {
            Vector3 center = transform.position + Vector3.up * 0.9f;
            int hitCount = Physics.OverlapSphereNonAlloc(center, interactRadius, _hitBuffer, interactMask, QueryTriggerInteraction.Collide);

            IInteractable nearest = null;
            float bestDistSqr = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null) continue;

                IInteractable interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract) continue;

                float distSqr = (col.transform.position - center).sqrMagnitude;
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    nearest = interactable;
                }
            }

            _currentTarget = nearest;
        }

        private void OnGUI()
        {
            if (_currentTarget == null || Time.timeScale <= 0f)
            {
                return;
            }

            if (DialogManager.Instance != null && DialogManager.Instance.IsOpen)
            {
                return;
            }

            EnsureStyles();

            string promptText = _currentTarget.PromptText;
            if (string.IsNullOrEmpty(promptText))
            {
                return;
            }

            _promptContent.text = promptText;
            Vector2 textSize = _labelStyle.CalcSize(_promptContent);

            // Lebar otomatis mengikuti panjang teks (tidak pernah terpotong!)
            const float h = 44f;
            const float badgeSize = 28f;
            const float leftPad = 12f;
            const float gap = 10f;
            const float rightPad = 20f;
            float w = Mathf.Max(180f, leftPad + badgeSize + gap + textSize.x + rightPad);

            Rect pillRect = new Rect((Screen.width - w) * 0.5f, Screen.height - 132f, w, h);
            GUI.Box(pillRect, GUIContent.none, _pillStyle);

            // Keycap emas chamfered tombol [E] di sisi kiri (gaya HUD Lengkap)
            Rect badgeRect = new Rect(pillRect.x + leftPad, pillRect.y + (h - badgeSize) * 0.5f, badgeSize, badgeSize);
            GUI.Box(badgeRect, "E", _badgeTextStyle);

            // Teks aksi krem + drop shadow tipis agar tajam di atas kabut
            Rect textRect = new Rect(badgeRect.xMax + gap, pillRect.y, textSize.x + 6f, h);
            GUI.Label(new Rect(textRect.x + 1f, textRect.y + 1f, textRect.width, textRect.height), _promptContent, _shadowLabelStyle);
            GUI.Label(textRect, _promptContent, _labelStyle);
        }

        private void EnsureStyles()
        {
            if (_pillStyle == null)
            {
                _pillStyle = StylizedUITheme.CreatePillStyle(14, TextAnchor.MiddleCenter, 16, 8);
            }

            if (_badgeTextStyle == null)
            {
                _badgeTextStyle = StylizedUITheme.CreateKeycapStyle(14);
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    wordWrap = false
                };
                _labelStyle.normal.textColor = StylizedUITheme.CreamText;
            }

            if (_shadowLabelStyle == null)
            {
                _shadowLabelStyle = new GUIStyle(_labelStyle);
                _shadowLabelStyle.normal.textColor = new Color(0.03f, 0.08f, 0.10f, 0.88f);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.35f, 1f, 0.65f, 0.4f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, interactRadius);
        }
#endif
    }
}
