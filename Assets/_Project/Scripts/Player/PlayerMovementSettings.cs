using UnityEngine;

namespace PulauKabut.Player
{
    /// <summary>
    /// Menyimpan seluruh parameter gerak pemain agar mudah disetel di Inspector tanpa mengubah kode.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Pulau Kabut/Player Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("Gerak")]
        [Tooltip("Kecepatan lari maksimum pemain dalam meter per detik.")]
        [Min(0.1f)]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("Waktu yang dibutuhkan dari diam hingga kecepatan penuh (detik).")]
        [Min(0.01f)]
        [SerializeField] private float accelerationTime = 0.15f;

        [Tooltip("Waktu yang dibutuhkan dari kecepatan penuh hingga berhenti total (detik).")]
        [Min(0.01f)]
        [SerializeField] private float decelerationTime = 0.08f;

        [Tooltip("Kecepatan putar visual karakter menghadap arah gerak (derajat per detik).")]
        [Min(60f)]
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Gravitasi")]
        [Tooltip("Percepatan gravitasi ke bawah (nilai negatif, m/s^2).")]
        [SerializeField] private float gravity = -25f;

        [Tooltip("Gaya tekan kecil ke bawah saat di tanah agar karakter tetap menempel di tanjakan/turunan.")]
        [SerializeField] private float groundedStickForce = -2f;

        [Tooltip("Batas kecepatan jatuh maksimum (nilai negatif, m/s).")]
        [SerializeField] private float maxFallSpeed = -40f;

        [Header("Deteksi Tanah")]
        [Tooltip("Layer yang dianggap sebagai tanah. Jangan sertakan layer Player agar tidak mendeteksi diri sendiri.")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Jarak tambahan ke bawah dari kaki karakter untuk mendeteksi tanah (meter).")]
        [Min(0.01f)]
        [SerializeField] private float groundCheckDistance = 0.15f;

        [Header("Lereng & Tangga")]
        [Tooltip("Sudut kemiringan maksimum yang masih dapat dipijak karakter (derajat).")]
        [Range(0f, 89f)]
        [SerializeField] private float slopeLimit = 50f;

        [Tooltip("Tinggi anak tangga maksimum yang dapat dinaiki otomatis oleh CharacterController (meter).")]
        [Min(0f)]
        [SerializeField] private float stepOffset = 0.3f;

        // Properti baca-saja agar nilai aset tidak berubah tidak sengaja saat Play Mode
        public float MoveSpeed => moveSpeed;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;
        public float RotationSpeed => rotationSpeed;
        public float Gravity => gravity;
        public float GroundedStickForce => groundedStickForce;
        public float MaxFallSpeed => maxFallSpeed;
        public LayerMask GroundMask => groundMask;
        public float GroundCheckDistance => groundCheckDistance;
        public float SlopeLimit => slopeLimit;
        public float StepOffset => stepOffset;
    }
}
