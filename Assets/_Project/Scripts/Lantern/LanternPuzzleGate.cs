using UnityEngine;

namespace PulauKabut.Lantern
{
    /// <summary>
    /// Gerbang batu teka-teki yang baru terbuka setelah seluruh LanternBrazier yang terhubung dinyalakan.
    /// Bergerak turun secara halus ke dalam tanah tanpa alokasi memori di Update.
    /// </summary>
    public class LanternPuzzleGate : MonoBehaviour
    {
        [Header("Syarat Pembuka Gerbang")]
        [Tooltip("Daftar tungku pelita yang harus dinyalakan semua untuk membuka gerbang ini.")]
        [SerializeField] private LanternBrazier[] requiredBraziers;

        [Header("Animasi Terbuka")]
        [Tooltip("Jarak turun gerbang saat terbuka (meter).")]
        [SerializeField] private float openDropDistance = 6.5f;

        [Tooltip("Kecepatan gerbang turun saat terbuka (meter/detik).")]
        [SerializeField] private float openSpeed = 2.6f;

        public bool IsOpen { get; private set; }

        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private Collider _gateCollider;

        private void Awake()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + Vector3.down * openDropDistance;
            _gateCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            if (requiredBraziers == null) return;
            for (int i = 0; i < requiredBraziers.Length; i++)
            {
                if (requiredBraziers[i] != null)
                {
                    requiredBraziers[i].OnBrazierLit += HandleBrazierLit;
                }
            }
            CheckAllLit();
        }

        private void OnDisable()
        {
            if (requiredBraziers == null) return;
            for (int i = 0; i < requiredBraziers.Length; i++)
            {
                if (requiredBraziers[i] != null)
                {
                    requiredBraziers[i].OnBrazierLit -= HandleBrazierLit;
                }
            }
        }

        private void HandleBrazierLit(LanternBrazier brazier)
        {
            CheckAllLit();
        }

        private void CheckAllLit()
        {
            if (IsOpen || requiredBraziers == null || requiredBraziers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < requiredBraziers.Length; i++)
            {
                if (requiredBraziers[i] == null || !requiredBraziers[i].IsLit)
                {
                    return;
                }
            }

            IsOpen = true;
            if (_gateCollider != null)
            {
                _gateCollider.enabled = false;
            }
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, _openPosition, openSpeed * Time.deltaTime);
        }
    }
}
