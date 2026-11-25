using UnityEngine;

namespace PulauKabut.Lantern
{
    /// <summary>
    /// Menyimpan seluruh parameter sistem Lentera dan respons Kabut agar mudah disetel di Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "LanternSettings", menuName = "Pulau Kabut/Lantern Settings")]
    public class LanternSettings : ScriptableObject
    {
        [Header("Cahaya Lentera")]
        [Tooltip("Radius jangkauan lentera untuk menyingkap platform bayangan (FogRevealable) dalam meter.")]
        [Min(1f)]
        [SerializeField] private float revealRadius = 12f;

        [Tooltip("Intensitas Point Light lentera saat menyala penuh.")]
        [Min(0f)]
        [SerializeField] private float lightIntensity = 5f;

        [Tooltip("Jarak jangkau Point Light lentera (meter).")]
        [Min(1f)]
        [SerializeField] private float lightRange = 16f;

        [Tooltip("Warna cahaya hangat lentera.")]
        [SerializeField] private Color lightColor = new Color(1f, 0.81f, 0.44f);

        [Tooltip("Kecepatan transisi nyala/padam cahaya lentera.")]
        [Min(0.5f)]
        [SerializeField] private float fadeSpeed = 8f;

        [Tooltip("Status awal lentera saat scene dimulai.")]
        [SerializeField] private bool startLit = false;

        [Header("Daya / Batasan Nyala Lentera")]
        [Tooltip("Aktifkan batas durasi nyala lentera agar pemain perlu mengelola kapan lentera dinyalakan.")]
        [SerializeField] private bool enableDurationLimit = true;

        [Tooltip("Durasi maksimum lentera menyala penuh (detik) sebelum padam otomatis.")]
        [Min(2f)]
        [SerializeField] private float maxChargeDuration = 18f;

        [Tooltip("Jeda (detik) setelah lentera padam sebelum daya mulai terisi kembali.")]
        [Min(0f)]
        [SerializeField] private float rechargeDelay = 1.2f;

        [Tooltip("Waktu (detik) untuk mengisi daya lentera dari 0% hingga 100% saat padam.")]
        [Min(1f)]
        [SerializeField] private float rechargeDuration = 10f;

        [Tooltip("Minimum daya (0-1) agar lentera bisa dinyalakan kembali setelah padam/habis.")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float minChargeToRelight = 0.2f;

        [Tooltip("Ambang daya rendah (0-1) saat cahaya lentera mulai berkedip sebagai peringatan akan padam.")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float lowChargeFlickerThreshold = 0.25f;

        [Header("Respons Kabut Pulau")]
        [Tooltip("Aktifkan pergeseran jarak kabut otomatis saat lentera menyala atau padam.")]
        [SerializeField] private bool controlSceneFog = true;

        [Tooltip("Jarak mulai kabut saat lentera PADAM (meter). Semakin kecil semakin pekat.")]
        [Min(0f)]
        [SerializeField] private float unlitFogStart = 4f;

        [Tooltip("Jarak akhir kabut tebal saat lentera PADAM (meter). Membatasi jarak pandang agar tidak terbuka semua.")]
        [Min(5f)]
        [SerializeField] private float unlitFogEnd = 24f;

        [Tooltip("Jarak mulai kabut saat lentera MENYALA (meter).")]
        [Min(0f)]
        [SerializeField] private float litFogStart = 12f;

        [Tooltip("Jarak akhir kabut saat lentera MENYALA (meter). Jarak pandang tetap terbatas oleh kabut jauh.")]
        [Min(10f)]
        [SerializeField] private float litFogEnd = 52f;

        [Tooltip("Kecepatan transisi mundur/majunya kabut.")]
        [Min(0.2f)]
        [SerializeField] private float fogTransitionSpeed = 3.5f;

        public float RevealRadius => revealRadius;
        public float LightIntensity => lightIntensity;
        public float LightRange => lightRange;
        public Color LightColor => lightColor;
        public float FadeSpeed => fadeSpeed;
        public bool StartLit => startLit;
        public bool EnableDurationLimit => enableDurationLimit;
        public float MaxChargeDuration => maxChargeDuration;
        public float RechargeDelay => rechargeDelay;
        public float RechargeDuration => rechargeDuration;
        public float MinChargeToRelight => minChargeToRelight;
        public float LowChargeFlickerThreshold => lowChargeFlickerThreshold;
        public bool ControlSceneFog => controlSceneFog;
        public float UnlitFogStart => unlitFogStart;
        public float UnlitFogEnd => unlitFogEnd;
        public float LitFogStart => litFogStart;
        public float LitFogEnd => litFogEnd;
        public float FogTransitionSpeed => fogTransitionSpeed;
    }
}
