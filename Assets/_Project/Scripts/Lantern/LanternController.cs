using System;
using PulauKabut.Player;
using UnityEngine;

namespace PulauKabut.Lantern
{
    /// <summary>
    /// Mengendalikan lentera pemain (tombol F / Gamepad Y):
    /// menyalakan/memadamkan cahaya, menggeser kabut secara halus,
    /// dan menyediakan posisi + radius cahaya untuk objek FogRevealable dan Shader Graph.
    /// </summary>
    public class LanternController : MonoBehaviour
    {
        private static readonly int LanternPosShaderId = Shader.PropertyToID("_LanternPos");
        private static readonly int LanternRadiusShaderId = Shader.PropertyToID("_LanternRadius");
        private static readonly int EmissionColorShaderId = Shader.PropertyToID("_EmissionColor");

        /// <summary>Instance lentera aktif agar FogRevealable tidak perlu FindObjectOfType di Update.</summary>
        public static LanternController ActiveLantern { get; private set; }

        [Header("Referensi")]
        [Tooltip("Aset pengaturan lentera dan kabut (ScriptableObject).")]
        [SerializeField] private LanternSettings settings;

        [Tooltip("Transform posisi lentera pada karakter (di tangan/sisi kanan Visuals).")]
        [SerializeField] private Transform lanternPoint;

        [Tooltip("Point Light lentera (disarankan tanpa shadow agar tetap 60+ FPS di GPU 4 GB VRAM).")]
        [SerializeField] private Light lanternLight;

        [Tooltip("Renderer inti lampu lentera untuk efek pendar emissive (opsional).")]
        [SerializeField] private Renderer lanternCoreRenderer;

        /// <summary>True jika lentera saat ini sedang dinyalakan.</summary>
        public bool IsLit { get; private set; }

        /// <summary>Intensitas ternormalisasi (0 = padam total, 1 = menyala penuh).</summary>
        public float NormalizedIntensity { get; private set; }

        /// <summary>Sisa daya lentera ternormalisasi (0 = habis, 1 = penuh).</summary>
        public float ChargeNormalized { get; private set; } = 1f;

        /// <summary>Sisa durasi nyala lentera dalam detik.</summary>
        public float RemainingSeconds => settings != null ? ChargeNormalized * settings.MaxChargeDuration : 0f;

        /// <summary>True jika batas durasi nyala lentera aktif.</summary>
        public bool HasDurationLimit => settings != null && settings.EnableDurationLimit;

        /// <summary>True jika lentera sedang mengisi daya saat padam.</summary>
        public bool IsRecharging { get; private set; }

        /// <summary>True jika daya cukup untuk menyalakan lentera kembali.</summary>
        public bool CanRelight => settings == null || !settings.EnableDurationLimit || ChargeNormalized >= settings.MinChargeToRelight;

        /// <summary>True jika daya lentera berada di ambang rendah.</summary>
        public bool IsLowCharge => HasDurationLimit && ChargeNormalized <= settings.LowChargeFlickerThreshold;

        /// <summary>Radius efektif pengungkap bayangan saat ini dalam meter.</summary>
        public float CurrentRevealRadius => settings != null ? settings.RevealRadius * NormalizedIntensity : 0f;

        /// <summary>Posisi dunia lentera saat ini.</summary>
        public Vector3 LanternPosition => lanternPoint != null ? lanternPoint.position : transform.position + Vector3.up;

        /// <summary>Event dipicu saat status nyala/padam lentera berubah (untuk Audio SFX & UI).</summary>
        public event Action<bool> OnLanternToggled;

        /// <summary>Event dipicu saat daya lentera habis total dan padam otomatis.</summary>
        public event Action OnLanternDepleted;

        private PlayerInputActions _input;
        private MaterialPropertyBlock _corePropBlock;
        private float _rechargeDelayTimer;
        private float _flickerMultiplier = 1f;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError($"[{nameof(LanternController)}] LanternSettings belum diisi pada '{name}'. Komponen dinonaktifkan.", this);
                enabled = false;
                return;
            }

            ActiveLantern = this;
            _input = new PlayerInputActions();
            _corePropBlock = new MaterialPropertyBlock();

            ChargeNormalized = 1f;
            IsRecharging = false;
            _rechargeDelayTimer = 0f;
            _flickerMultiplier = 1f;

            IsLit = settings.StartLit;
            NormalizedIntensity = IsLit ? 1f : 0f;

            ApplyLightImmediate();
            ApplyFogImmediate();
            PushShaderGlobals();
        }

        private void OnEnable()
        {
            _input ??= new PlayerInputActions();
            _input.Player.Enable();
            ActiveLantern = this;
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
            if (ActiveLantern == this)
            {
                ActiveLantern = null;
            }
        }

        private void OnDestroy()
        {
            if (ActiveLantern == this)
            {
                ActiveLantern = null;
            }
            _input?.Dispose();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            _input ??= new PlayerInputActions();

            // Baca tombol Lantern (F / Gamepad Y) tanpa alokasi memori
            if (_input.Player.Lantern.WasPressedThisFrame())
            {
                ToggleLantern();
            }

            float dt = Time.deltaTime;
            UpdateLanternCharge(dt);
            UpdateLanternIntensity(dt);
            UpdateSceneFog(dt);
            PushShaderGlobals();
        }

        /// <summary>Menyalakan atau memadamkan lentera.</summary>
        public void ToggleLantern()
        {
            SetLit(!IsLit);
        }

        /// <summary>Mengatur status lentera secara langsung (berguna untuk event cerita/dialog).</summary>
        public void SetLit(bool lit)
        {
            if (IsLit == lit)
            {
                return;
            }

            if (lit && !CanRelight)
            {
                return;
            }

            IsLit = lit;
            if (!IsLit)
            {
                _rechargeDelayTimer = settings != null ? settings.RechargeDelay : 1f;
                IsRecharging = false;
            }

            OnLanternToggled?.Invoke(IsLit);
        }

        /// <summary>Mengisi ulang daya lentera secara instan (misal saat menyentuh Checkpoint / Altar).</summary>
        public void RefillCharge(float normalizedAmount = 1f)
        {
            ChargeNormalized = Mathf.Clamp01(ChargeNormalized + normalizedAmount);
            if (ChargeNormalized >= 1f)
            {
                IsRecharging = false;
            }
        }

        private void UpdateLanternCharge(float dt)
        {
            if (!settings.EnableDurationLimit)
            {
                ChargeNormalized = 1f;
                IsRecharging = false;
                _flickerMultiplier = 1f;
                return;
            }

            if (IsLit)
            {
                IsRecharging = false;
                float drainRate = 1f / Mathf.Max(0.1f, settings.MaxChargeDuration);
                ChargeNormalized = Mathf.Max(0f, ChargeNormalized - drainRate * dt);

                if (ChargeNormalized <= settings.LowChargeFlickerThreshold && ChargeNormalized > 0f)
                {
                    // Kedipan halus saat daya hampir habis sebagai peringatan visual
                    float pulse = Mathf.Sin(Time.time * 24f) * 0.5f + 0.5f;
                    _flickerMultiplier = Mathf.Lerp(0.68f, 1f, pulse);
                }
                else
                {
                    _flickerMultiplier = 1f;
                }

                if (ChargeNormalized <= 0f)
                {
                    _flickerMultiplier = 1f;
                    SetLit(false);
                    OnLanternDepleted?.Invoke();
                }
            }
            else
            {
                _flickerMultiplier = 1f;
                if (ChargeNormalized < 1f)
                {
                    if (_rechargeDelayTimer > 0f)
                    {
                        _rechargeDelayTimer = Mathf.Max(0f, _rechargeDelayTimer - dt);
                        IsRecharging = false;
                    }
                    else
                    {
                        IsRecharging = true;
                        float rechargeRate = 1f / Mathf.Max(0.1f, settings.RechargeDuration);
                        ChargeNormalized = Mathf.Min(1f, ChargeNormalized + rechargeRate * dt);
                        if (ChargeNormalized >= 1f)
                        {
                            IsRecharging = false;
                        }
                    }
                }
                else
                {
                    IsRecharging = false;
                }
            }
        }

        private void UpdateLanternIntensity(float dt)
        {
            float target = IsLit ? 1f : 0f;
            float step = 1f - Mathf.Exp(-settings.FadeSpeed * dt);
            NormalizedIntensity = Mathf.MoveTowards(NormalizedIntensity, target, Mathf.Max(step, 0.001f));

            ApplyLightImmediate();
        }

        private void ApplyLightImmediate()
        {
            float effectiveIntensity = NormalizedIntensity * _flickerMultiplier;

            if (lanternLight != null)
            {
                lanternLight.color = settings.LightColor;
                lanternLight.range = settings.LightRange;
                lanternLight.intensity = settings.LightIntensity * effectiveIntensity;
                lanternLight.enabled = effectiveIntensity > 0.01f;
            }

            if (lanternCoreRenderer != null)
            {
                _corePropBlock ??= new MaterialPropertyBlock();

                // Gunakan MaterialPropertyBlock agar tidak membuat instance material baru di memori
                lanternCoreRenderer.GetPropertyBlock(_corePropBlock);
                Color emissive = settings.LightColor * (effectiveIntensity * 2.2f);
                _corePropBlock.SetColor(EmissionColorShaderId, emissive);
                lanternCoreRenderer.SetPropertyBlock(_corePropBlock);
            }
        }

        private void ApplyFogImmediate()
        {
            if (!settings.ControlSceneFog)
            {
                return;
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = Mathf.Lerp(settings.UnlitFogStart, settings.LitFogStart, NormalizedIntensity);
            RenderSettings.fogEndDistance = Mathf.Lerp(settings.UnlitFogEnd, settings.LitFogEnd, NormalizedIntensity);
        }

        private void UpdateSceneFog(float dt)
        {
            if (!settings.ControlSceneFog)
            {
                return;
            }

            float fogFactor = NormalizedIntensity * Mathf.Lerp(0.88f, 1f, _flickerMultiplier);
            float targetStart = Mathf.Lerp(settings.UnlitFogStart, settings.LitFogStart, fogFactor);
            float targetEnd = Mathf.Lerp(settings.UnlitFogEnd, settings.LitFogEnd, fogFactor);
            float t = 1f - Mathf.Exp(-settings.FogTransitionSpeed * dt);

            RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, targetStart, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, targetEnd, t);
        }

        private void PushShaderGlobals()
        {
            Vector3 pos = LanternPosition;
            Shader.SetGlobalVector(LanternPosShaderId, new Vector4(pos.x, pos.y, pos.z, NormalizedIntensity));
            Shader.SetGlobalFloat(LanternRadiusShaderId, CurrentRevealRadius);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (settings == null)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.82f, 0.35f, 0.45f);
            Gizmos.DrawWireSphere(LanternPosition, settings.RevealRadius);
        }
#endif
    }
}
