using PulauKabut.Lantern;
using PulauKabut.Player;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Rintangan batu pijakan rawa: saat diinjak dan pemain tidak segera berpindah,
    /// batu akan bergetar dan berkelap-kelip lalu jatuh ke air rawa, kemudian naik kembali setelah 5-10 detik.
    /// Nol alokasi memori per frame.
    /// </summary>
    public class CrumblingSteppingStone : MonoBehaviour
    {
        private enum StoneState
        {
            Ready,
            Warning,
            Falling,
            WaitingRespawn,
            Rising
        }

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Referensi (Opsional)")]
        [SerializeField] private Collider standCollider;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private FogRevealable fogRevealable;

        [Header("Waktu Rintangan (Detik)")]
        [Tooltip("Total waktu berdiri sebelum batu jatuh jika pemain tidak segera pergi.")]
        [Min(0.4f)]
        [SerializeField] private float crumbleDelay = 1.65f;

        [Tooltip("Jeda sebelum batu mulai berkelap-kelip setelah terinjak.")]
        [Min(0f)]
        [SerializeField] private float flickerStartDelay = 0.25f;

        [Tooltip("Durasi batu tenggelam sebelum naik kembali ke permukaan (5-10 detik).")]
        [Range(5f, 10f)]
        [SerializeField] private float respawnDelay = 7f;

        [Tooltip("Durasi animasi batu naik kembali ke permukaan.")]
        [Min(0.25f)]
        [SerializeField] private float riseDuration = 1.0f;

        [Header("Gerak & Visual")]
        [SerializeField] private float sinkOffsetOnStep = 0.06f;
        [SerializeField] private float shakeAmplitude = 0.055f;
        [SerializeField] private float fallDistance = 3.2f;
        [SerializeField] private float fallGravity = 18f;
        [SerializeField] private Color flickerColor = new Color(1.0f, 0.34f, 0.16f, 1f);
        [SerializeField] private Color flickerAltColor = new Color(1.0f, 0.92f, 0.56f, 1f);

        private StoneState _state = StoneState.Ready;
        private Vector3 _restLocalPos;
        private float _warningTimer;
        private float _stateTimer;
        private float _fallSpeed;
        private float _fallDepth;
        private PlayerController _player;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _restLocalPos = transform.localPosition;
            _mpb = new MaterialPropertyBlock();

            if (standCollider == null) standCollider = GetComponent<Collider>();
            if (fogRevealable == null) fogRevealable = GetComponent<FogRevealable>();
            if (targetRenderer == null)
            {
                // Cari renderer aktif pertama pada anak (StepStone_TopVisual) atau komponen sendiri
                Renderer[] rends = GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rends.Length; i++)
                {
                    if (rends[i].enabled)
                    {
                        targetRenderer = rends[i];
                        break;
                    }
                }
                if (targetRenderer == null && rends.Length > 0) targetRenderer = rends[0];
            }
        }

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (_player == null)
            {
                _player = FindAnyObjectByType<PlayerController>();
            }

            switch (_state)
            {
                case StoneState.Ready:
                    if (IsPlayerStandingOnStone())
                    {
                        _state = StoneState.Warning;
                        _warningTimer = 0f;
                    }
                    break;

                case StoneState.Warning:
                    UpdateWarning(dt);
                    break;

                case StoneState.Falling:
                    UpdateFalling(dt);
                    break;

                case StoneState.WaitingRespawn:
                    _stateTimer += dt;
                    if (_stateTimer >= respawnDelay)
                    {
                        BeginRising();
                    }
                    break;

                case StoneState.Rising:
                    UpdateRising(dt);
                    break;
            }
        }

        private void UpdateWarning(float dt)
        {
            if (IsPlayerStandingOnStone())
            {
                _warningTimer += dt;
                if (_warningTimer >= crumbleDelay)
                {
                    BeginFalling();
                    return;
                }
            }
            else
            {
                // Jika pemain segera pergi sebelum batas waktu, batu pulih kembali dengan cepat!
                _warningTimer = Mathf.Max(0f, _warningTimer - dt * 2.5f);
                if (_warningTimer <= 0.001f)
                {
                    ResetToReady();
                    return;
                }
            }

            float progress = Mathf.Clamp01(_warningTimer / crumbleDelay);

            // Amblas ringan & bergetar makin kencang mendekati jatuh
            float shakeFreq = Mathf.Lerp(22f, 52f, progress);
            float sx = Mathf.Sin(Time.time * shakeFreq) * shakeAmplitude * progress;
            float sz = Mathf.Cos(Time.time * (shakeFreq * 1.17f)) * shakeAmplitude * progress;
            transform.localPosition = _restLocalPos + new Vector3(sx, -sinkOffsetOnStep * progress, sz);

            // Efek kelap-kelip peringatan (makin cepat saat hampir jatuh)
            if (_warningTimer >= flickerStartDelay && targetRenderer != null)
            {
                float blinkHz = Mathf.Lerp(4.5f, 16f, progress);
                bool flashOn = Mathf.Sin(Time.time * blinkHz * Mathf.PI * 2f) > 0f;
                ApplyFlickerVisual(flashOn);
            }
            else
            {
                ClearFlickerVisual();
            }
        }

        private void BeginFalling()
        {
            _state = StoneState.Falling;
            _fallSpeed = 0f;
            _fallDepth = 0f;
            ClearFlickerVisual();

            if (standCollider != null)
            {
                standCollider.enabled = false;
            }
        }

        private void UpdateFalling(float dt)
        {
            _fallSpeed += fallGravity * dt;
            _fallDepth += _fallSpeed * dt;
            transform.localPosition = _restLocalPos - new Vector3(0f, _fallDepth, 0f);

            if (_fallDepth >= fallDistance)
            {
                _state = StoneState.WaitingRespawn;
                _stateTimer = 0f;
                SetHidden(true);
            }
        }

        private void BeginRising()
        {
            _state = StoneState.Rising;
            _stateTimer = 0f;
            SetHidden(false);

            // Selama naik dari air, collider tetap nonaktif sampai batu benar-benar muncul ke permukaan
            if (standCollider != null)
            {
                standCollider.enabled = false;
            }
        }

        private void UpdateRising(float dt)
        {
            _stateTimer += dt;
            float t = Mathf.Clamp01(_stateTimer / Mathf.Max(0.1f, riseDuration));
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            float depth = Mathf.Lerp(fallDistance, 0f, smooth);
            transform.localPosition = _restLocalPos - new Vector3(0f, depth, 0f);

            // Aktifkan pijakan di 85% ketinggian agar pemain bisa langsung loncat saat hampir rata
            if (t >= 0.85f && standCollider != null && !standCollider.enabled)
            {
                if (fogRevealable == null || !fogRevealable.ForceHidden)
                {
                    standCollider.enabled = true;
                }
            }

            if (t >= 1f)
            {
                ResetToReady();
            }
        }

        private void ResetToReady()
        {
            _state = StoneState.Ready;
            _warningTimer = 0f;
            _stateTimer = 0f;
            transform.localPosition = _restLocalPos;
            ClearFlickerVisual();
            SetHidden(false);

            if (standCollider != null)
            {
                standCollider.enabled = true;
            }
        }

        private bool IsPlayerStandingOnStone()
        {
            if (_player == null || !_player.IsGrounded || standCollider == null || !standCollider.enabled)
            {
                return false;
            }

            Bounds b = standCollider.bounds;
            Vector3 p = _player.transform.position;

            float radius = Mathf.Max(b.extents.x, b.extents.z) + 0.18f;
            float dx = p.x - b.center.x;
            float dz = p.z - b.center.z;
            if ((dx * dx + dz * dz) > radius * radius)
            {
                return false;
            }

            return p.y >= b.max.y - 0.35f && p.y <= b.max.y + 0.45f;
        }

        private void SetHidden(bool hidden)
        {
            if (fogRevealable != null)
            {
                fogRevealable.ForceHidden = hidden;
            }
            else if (targetRenderer != null)
            {
                targetRenderer.enabled = !hidden;
            }
        }

        private void ApplyFlickerVisual(bool flashOn)
        {
            if (targetRenderer == null) return;
            targetRenderer.GetPropertyBlock(_mpb);
            Color c = flashOn ? flickerColor : flickerAltColor;
            _mpb.SetColor(BaseColorId, c);
            _mpb.SetColor(ColorId, c);
            _mpb.SetColor(EmissionColorId, c * (flashOn ? 1.5f : 0.7f));
            targetRenderer.SetPropertyBlock(_mpb);
        }

        private void ClearFlickerVisual()
        {
            if (targetRenderer == null) return;
            _mpb.Clear();
            targetRenderer.SetPropertyBlock(_mpb);
        }
    }
}
