using PulauKabut.Core;
using PulauKabut.Dialog;
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
        private PlayerInputActions _input;
        private IInteractable _currentTarget;
        private GUIStyle _promptStyle;

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

            if (_promptStyle == null)
            {
                _promptStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(14, 14, 8, 8)
                };
                _promptStyle.normal.textColor = new Color(1f, 0.96f, 0.78f);
            }

            const float w = 340f;
            const float h = 42f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, Screen.height - 135f, w, h);
            GUI.Box(rect, $"[E]  {_currentTarget.PromptText}", _promptStyle);
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
