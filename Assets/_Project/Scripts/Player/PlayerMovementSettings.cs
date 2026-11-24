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
        [Tooltip("Kecepatan jalan normal pemain dalam meter per detik.")]
        [Min(0.1f)]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("Kecepatan lari cepat (Sprint) saat menahan tombol Shift (m/s).")]
        [Min(0.1f)]
        [SerializeField] private float sprintSpeed = 7.8f;

        [Tooltip("Waktu yang dibutuhkan dari diam hingga kecepatan penuh (detik).")]
        [Min(0.01f)]
        [SerializeField] private float accelerationTime = 0.15f;

        [Tooltip("Waktu yang dibutuhkan dari kecepatan penuh hingga berhenti total (detik).")]
        [Min(0.01f)]
        [SerializeField] private float decelerationTime = 0.08f;

        [Tooltip("Kecepatan putar visual karakter menghadap arah gerak (derajat per detik).")]
        [Min(60f)]
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Lompat")]
        [Tooltip("Tinggi lompat maksimum saat tombol lompat ditahan penuh (meter).")]
        [Min(0.1f)]
        [SerializeField] private float jumpHeight = 1.6f;

        [Tooltip("Pengali kecepatan vertikal saat tombol lompat dilepas lebih awal (0-1). Semakin kecil, lompat singkat makin rendah.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.5f;

        [Tooltip("Tenggang waktu masih boleh melompat sesaat setelah meninggalkan tepi pijakan (detik).")]
        [Min(0f)]
        [SerializeField] private float coyoteTime = 0.12f;

        [Tooltip("Tenggang waktu tombol lompat yang ditekan sesaat sebelum mendarat tetap dieksekusi (detik).")]
        [Min(0f)]
        [SerializeField] private float jumpBufferTime = 0.10f;

        [Header("Glide (Melayang di Udara)")]
        [Tooltip("Aktifkan kemampuan melayang saat menahan tombol lompat ketika sedang jatuh.")]
        [SerializeField] private bool enableGlide = true;

        [Tooltip("Kecepatan jatuh maksimum saat melayang (nilai negatif kecil, m/s).")]
        [SerializeField] private float glideFallSpeed = -2.2f;

        [Tooltip("Kecepatan gerak mendatar saat melayang di udara (m/s).")]
        [Min(0.5f)]
        [SerializeField] private float glideMoveSpeed = 6.5f;

        [Tooltip("Waktu penyesuaian arah saat melayang di udara (detik).")]
        [Min(0.01f)]
        [SerializeField] private float glideAccelerationTime = 0.25f;

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

        [Header("Kamera (POV 1 & Transisi POV 2)")]
        [Tooltip("Sensitivitas gerak mouse untuk menoleh (derajat per piksel).")]
        [Range(0.02f, 1f)]
        [SerializeField] private float lookSensitivity = 0.15f;

        [Tooltip("Kehalusan damping rotasi kamera (semakin besar semakin cepat mengikuti, 0 = tanpa damping).")]
        [Min(0f)]
        [SerializeField] private float lookSmoothing = 16f;

        [Tooltip("Batas sudut menunduk maksimum kamera (derajat, nilai negatif).")]
        [Range(-89f, 0f)]
        [SerializeField] private float minPitch = -75f;

        [Tooltip("Batas sudut mendongak maksimum kamera (derajat, nilai positif).")]
        [Range(0f, 89f)]
        [SerializeField] private float maxPitch = 80f;

        [Tooltip("Balik arah gerak vertikal mouse jika dicentang.")]
        [SerializeField] private bool invertY = false;

        [Tooltip("Jika jarak kamera aktif ke titik mata lebih dekat dari nilai ini, visual karakter disembunyikan agar tidak menghalangi pandangan POV 1.")]
        [Min(0.1f)]
        [SerializeField] private float firstPersonHideDistance = 0.65f;

        [Tooltip("Jarak kamera di belakang karakter saat mode Third-Person (meter).")]
        [Range(2f, 10f)]
        [SerializeField] private float thirdPersonDistance = 5f;

        [Tooltip("Jika dicentang, game dimulai dari pandangan mata (First-Person). Jika tidak, mulai dari belakang karakter (Third-Person). Tekan V untuk berganti.")]
        [SerializeField] private bool startInFirstPerson = false;

        // Properti baca-saja agar nilai aset tidak berubah tidak sengaja saat Play Mode
        public float MoveSpeed => moveSpeed;
        public float SprintSpeed => sprintSpeed;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;
        public float RotationSpeed => rotationSpeed;
        public float JumpHeight => jumpHeight;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public bool EnableGlide => enableGlide;
        public float GlideFallSpeed => glideFallSpeed;
        public float GlideMoveSpeed => glideMoveSpeed;
        public float GlideAccelerationTime => glideAccelerationTime;
        public float Gravity => gravity;
        public float GroundedStickForce => groundedStickForce;
        public float MaxFallSpeed => maxFallSpeed;
        public LayerMask GroundMask => groundMask;
        public float GroundCheckDistance => groundCheckDistance;
        public float SlopeLimit => slopeLimit;
        public float StepOffset => stepOffset;
        public float LookSensitivity => lookSensitivity;
        public float LookSmoothing => lookSmoothing;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
        public bool InvertY => invertY;
        public float FirstPersonHideDistance => firstPersonHideDistance;
        public float ThirdPersonDistance => thirdPersonDistance;
        public bool StartInFirstPerson => startInFirstPerson;
    }
}
