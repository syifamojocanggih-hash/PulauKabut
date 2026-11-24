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

        /// <summary>Radius efektif pengungkap bayangan saat ini dalam meter.</summary>
        public float CurrentRevealRadius => settings != null ? settings.RevealRadius * NormalizedIntensity : 0f;

        /// <summary>Posisi dunia lentera saat ini.</summary>
        public Vector3 LanternPosition => lanternPoint != null ? lanternPoint.position : transform.position + Vector3.up;

        /// <summary>Event dipicu saat status nyala/padam lentera berubah (untuk Audio SFX & UI).</summary>
        public event Action<bool> OnLanternToggled;

        private PlayerInputActions _input;
        private MaterialPropertyBlock _corePropBlock;

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

            // Baca tombol Lantern (F / Gamepad Y) tanpa alokasi memori
            if (_input.Player.Lantern.WasPressedThisFrame())
            {
                ToggleLantern();
            }

            float dt = Time.deltaTime;
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

            IsLit = lit;
            OnLanternToggled?.Invoke(IsLit);
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
            if (lanternLight != null)
            {
                lanternLight.color = settings.LightColor;
                lanternLight.range = settings.LightRange;
                lanternLight.intensity = settings.LightIntensity * NormalizedIntensity;
                lanternLight.enabled = NormalizedIntensity > 0.01f;
            }

            if (lanternCoreRenderer != null)
            {
                // Gunakan MaterialPropertyBlock agar tidak membuat instance material baru di memori
                lanternCoreRenderer.GetPropertyBlock(_corePropBlock);
                Color emissive = settings.LightColor * (NormalizedIntensity * 2.2f);
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
            RenderSettings.fogStartDistance = IsLit ? settings.LitFogStart : settings.UnlitFogStart;
            RenderSettings.fogEndDistance = IsLit ? settings.LitFogEnd : settings.UnlitFogEnd;
        }

        private void UpdateSceneFog(float dt)
        {
            if (!settings.ControlSceneFog)
            {
                return;
            }

            float targetStart = IsLit ? settings.LitFogStart : settings.UnlitFogStart;
            float targetEnd = IsLit ? settings.LitFogEnd : settings.UnlitFogEnd;
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
