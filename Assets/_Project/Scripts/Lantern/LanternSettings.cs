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

        [Header("Respons Kabut Pulau")]
        [Tooltip("Aktifkan pergeseran jarak kabut otomatis saat lentera menyala atau padam.")]
        [SerializeField] private bool controlSceneFog = true;

        [Tooltip("Jarak mulai kabut saat lentera PADAM (meter).")]
        [Min(0f)]
        [SerializeField] private float unlitFogStart = 16f;

        [Tooltip("Jarak akhir kabut tebal saat lentera PADAM (meter).")]
        [Min(5f)]
        [SerializeField] private float unlitFogEnd = 85f;

        [Tooltip("Jarak mulai kabut saat lentera MENYALA (meter).")]
        [Min(0f)]
        [SerializeField] private float litFogStart = 55f;

        [Tooltip("Jarak akhir kabut saat lentera MENYALA (meter).")]
        [Min(10f)]
        [SerializeField] private float litFogEnd = 220f;

        [Tooltip("Kecepatan transisi mundur/majunya kabut.")]
        [Min(0.2f)]
        [SerializeField] private float fogTransitionSpeed = 3.5f;

        public float RevealRadius => revealRadius;
        public float LightIntensity => lightIntensity;
        public float LightRange => lightRange;
        public Color LightColor => lightColor;
        public float FadeSpeed => fadeSpeed;
        public bool StartLit => startLit;
        public bool ControlSceneFog => controlSceneFog;
        public float UnlitFogStart => unlitFogStart;
        public float UnlitFogEnd => unlitFogEnd;
        public float LitFogStart => litFogStart;
        public float LitFogEnd => litFogEnd;
        public float FogTransitionSpeed => fogTransitionSpeed;
    }
}
