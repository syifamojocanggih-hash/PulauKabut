using UnityEngine;

namespace PulauKabut.Lantern
{
    /// <summary>
    /// Komponen untuk platform bayangan atau objek tersembunyi di dalam kabut:
    /// Collider dan tampilan padatnya hanya aktif saat berada dalam jangkauan cahaya Lentera.
    /// </summary>
    public class FogRevealable : MonoBehaviour
    {
        [Header("Komponen Target")]
        [Tooltip("Collider yang hanya aktif saat objek tersingkap cahaya lentera (kosong = otomatis ambil di objek ini).")]
        [SerializeField] private Collider targetCollider;

        [Tooltip("Renderer visual objek (kosong = otomatis ambil di objek ini).")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Material & Perilaku")]
        [Tooltip("Material saat platform padat & tersingkap cahaya lentera.")]
        [SerializeField] private Material revealedMaterial;

        [Tooltip("Material bayangan redup saat tidak tersingkap (jika kosong, objek disembunyikan saat padam).")]
        [SerializeField] private Material shadowMaterial;

        [Tooltip("Tambahan toleransi jarak deteksi khusus objek ini (meter).")]
        [Min(0f)]
        [SerializeField] private float extraRevealMargin = 0.5f;

        [Tooltip("Jika dicentang, objek tetap aktif permanen setelah sekali diterangi lentera.")]
        [SerializeField] private bool stayRevealedOnceLit = false;

        [Tooltip("Jika dicentang (logika real), collider tetap padat/nyata di rawa; cahaya hanya menyingkap visualnya dari samaran kabut.")]
        [SerializeField] private bool keepColliderAlwaysEnabled = false;

        private bool _isRevealed;
        private bool _hasBeenRevealedPermanently;
        private bool _forceHidden;

        /// <summary>True jika objek/platform saat ini aktif dan bisa dipijak.</summary>
        public bool IsRevealed => !_forceHidden && _isRevealed;

        /// <summary>Jika true (mis. batu pijakan sedang jatuh/tenggelam), sembunyikan visual dan collider sementara.</summary>
        public bool ForceHidden
        {
            get => _forceHidden;
            set
            {
                if (_forceHidden == value) return;
                _forceHidden = value;
                ApplyRevealedState(_forceHidden ? false : IsWithinActiveLantern(), force: true);
            }
        }

        private void Awake()
        {
            if (targetCollider == null) targetCollider = GetComponent<Collider>();
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();

            // Mulai dalam kondisi tersembunyi/bayangan (tidak bisa dipijak sebelum lentera menyala)
            ApplyRevealedState(false, force: true);
        }

        private void Update()
        {
            if (_forceHidden)
            {
                return;
            }

            if (_hasBeenRevealedPermanently)
            {
                return;
            }

            bool shouldReveal = IsWithinActiveLantern();
            if (shouldReveal && stayRevealedOnceLit)
            {
                _hasBeenRevealedPermanently = true;
            }

            ApplyRevealedState(shouldReveal, force: false);
        }

        private bool IsWithinActiveLantern()
        {
            LanternController lantern = LanternController.ActiveLantern;
            if (lantern == null || !lantern.IsLit)
            {
                return false;
            }

            float radius = lantern.CurrentRevealRadius + extraRevealMargin;
            if (radius <= 0.05f)
            {
                return false;
            }

            float distSqr = (transform.position - lantern.LanternPosition).sqrMagnitude;
            return distSqr <= radius * radius;
        }

        private void ApplyRevealedState(bool revealed, bool force)
        {
            if (!force && _isRevealed == revealed)
            {
                return;
            }

            _isRevealed = !_forceHidden && revealed;

            if (targetCollider != null)
            {
                targetCollider.enabled = !_forceHidden && (keepColliderAlwaysEnabled || _isRevealed);
            }

            if (targetRenderer != null)
            {
                if (_forceHidden)
                {
                    targetRenderer.enabled = false;
                }
                else if (_isRevealed)
                {
                    targetRenderer.enabled = true;
                    if (revealedMaterial != null)
                    {
                        targetRenderer.sharedMaterial = revealedMaterial;
                    }
                }
                else
                {
                    if (shadowMaterial != null)
                    {
                        targetRenderer.enabled = true;
                        targetRenderer.sharedMaterial = shadowMaterial;
                    }
                    else
                    {
                        targetRenderer.enabled = false;
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _isRevealed
                ? new Color(1f, 0.85f, 0.35f, 0.7f)
                : new Color(0.45f, 0.65f, 1f, 0.45f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
#endif
    }
}
