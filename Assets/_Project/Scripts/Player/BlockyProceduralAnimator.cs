using PulauKabut.Lantern;
using UnityEngine;

namespace PulauKabut.Player
{
    /// <summary>
    /// Animasi prosedural ringan untuk karakter blocky sementara (gaya Roblox):
    /// mengayunkan lengan dan kaki saat berjalan, pose lompat di udara, napas saat diam,
    /// serta mengangkat lengan kanan saat lentera menyala.
    /// </summary>
    public class BlockyProceduralAnimator : MonoBehaviour
    {
        [Header("Referensi Sistem")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LanternController lanternController;

        [Header("Sendi / Bagian Tubuh")]
        [Tooltip("Root rig visual yang ikut memantul halus saat melangkah.")]
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform leftLegPivot;
        [SerializeField] private Transform rightLegPivot;
        [SerializeField] private Transform leftArmPivot;
        [SerializeField] private Transform rightArmPivot;
        [SerializeField] private Transform headPivot;

        [Header("Parameter Gerak")]
        [Tooltip("Frekuensi ayunan langkah saat berjalan penuh.")]
        [SerializeField] private float walkFrequency = 10.5f;

        [Tooltip("Sudut ayunan maksimum kaki (derajat).")]
        [SerializeField] private float legSwingAngle = 42f;

        [Tooltip("Sudut ayunan maksimum lengan (derajat).")]
        [SerializeField] private float armSwingAngle = 36f;

        [Tooltip("Tinggi pantulan tubuh saat melangkah (meter).")]
        [SerializeField] private float walkBobAmount = 0.045f;

        [Tooltip("Sudut angkat lengan kanan saat memegang lentera menyala (derajat, nilai negatif = ke depan).")]
        [SerializeField] private float lanternHoldAngle = -68f;

        [Tooltip("Kecepatan transisi pose antar kondisi (diam, jalan, lompat, lentera).")]
        [SerializeField] private float poseLerpSpeed = 14f;

        private float _walkCycle;
        private float _idleTime;
        private Vector3 _initialBodyLocalPos;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (lanternController == null) lanternController = GetComponent<LanternController>();
            if (bodyRoot != null) _initialBodyLocalPos = bodyRoot.localPosition;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || playerController == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            float speed = playerController.HorizontalSpeed;
            bool isGrounded = playerController.IsGrounded;
            bool isHoldingLitLantern = lanternController != null && lanternController.IsLit;

            // Normalisasi kecepatan gerak (0 = diam, 1 = lari penuh)
            float speedFactor = Mathf.Clamp01(speed / 5f);
            float blend = 1f - Mathf.Exp(-poseLerpSpeed * dt);

            float targetLeftLegX = 0f;
            float targetRightLegX = 0f;
            float targetLeftArmX = 0f;
            float targetRightArmX = 0f;
            float targetLeftArmZ = 0f;
            float targetRightArmZ = 0f;
            float targetBodyY = _initialBodyLocalPos.y;
            float targetBodyPitch = 0f;

            if (playerController.IsGliding)
            {
                // Pose melayang (glide): kedua lengan terentang lebar tertiup angin, tubuh sedikit condong maju
                _idleTime += dt * 5f;
                float sway = Mathf.Sin(_idleTime) * 4f;
                targetLeftLegX = 16f;
                targetRightLegX = 10f;
                targetLeftArmX = -10f;
                targetRightArmX = -10f;
                targetLeftArmZ = -78f + sway;
                targetRightArmZ = 78f - sway;
                targetBodyPitch = 14f;
            }
            else if (!isGrounded)
            {
                // Pose di udara (lompat / jatuh)
                bool isRising = playerController.Velocity.y > 0f;
                targetLeftLegX = isRising ? -26f : 18f;
                targetRightLegX = isRising ? 22f : -14f;
                targetLeftArmX = isRising ? -38f : -18f;
                targetRightArmX = isRising ? -38f : -18f;
                targetLeftArmZ = -18f;
                targetRightArmZ = 18f;
                targetBodyPitch = isRising ? -4f : 5f;
            }
            else if (speedFactor > 0.05f)
            {
                // Pose berjalan / berlari: ayunan kaki dan tangan berlawanan arah
                _walkCycle += dt * walkFrequency * Mathf.Lerp(0.6f, 1.15f, speedFactor);
                float sinWave = Mathf.Sin(_walkCycle);

                targetLeftLegX = sinWave * legSwingAngle * speedFactor;
                targetRightLegX = -sinWave * legSwingAngle * speedFactor;

                targetLeftArmX = -sinWave * armSwingAngle * speedFactor;
                targetRightArmX = sinWave * armSwingAngle * speedFactor;

                // Pantulan tubuh halus 2x per siklus langkah
                targetBodyY = _initialBodyLocalPos.y + Mathf.Abs(sinWave) * walkBobAmount * speedFactor;
                targetBodyPitch = 6f * speedFactor;
            }
            else
            {
                // Pose diam: napas halus
                _idleTime += dt * 2.2f;
                float breath = Mathf.Sin(_idleTime);
                targetBodyY = _initialBodyLocalPos.y + breath * 0.014f;
                targetLeftArmZ = -3f - breath * 1.5f;
                targetRightArmZ = 3f + breath * 1.5f;
            }

            // Jika lentera menyala, lengan kanan diangkat mantap ke depan membawa lentera
            if (isHoldingLitLantern)
            {
                targetRightArmX = lanternHoldAngle + Mathf.Sin(_walkCycle) * 4f * speedFactor;
                targetRightArmZ = 4f;
            }

            ApplyPivotRotation(leftLegPivot,  targetLeftLegX,  0f, blend);
            ApplyPivotRotation(rightLegPivot, targetRightLegX, 0f, blend);
            ApplyPivotRotation(leftArmPivot,  targetLeftArmX,  targetLeftArmZ, blend);
            ApplyPivotRotation(rightArmPivot, targetRightArmX, targetRightArmZ, blend);

            if (bodyRoot != null)
            {
                Vector3 pos = bodyRoot.localPosition;
                pos.y = Mathf.Lerp(pos.y, targetBodyY, blend);
                bodyRoot.localPosition = pos;

                Quaternion targetBodyRot = Quaternion.Euler(targetBodyPitch, 0f, 0f);
                bodyRoot.localRotation = Quaternion.Slerp(bodyRoot.localRotation, targetBodyRot, blend);
            }

            if (headPivot != null)
            {
                float headNod = isGrounded ? Mathf.Sin(_walkCycle * 2f) * 1.8f * speedFactor : 0f;
                headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, Quaternion.Euler(headNod, 0f, 0f), blend);
            }
        }

        private static void ApplyPivotRotation(Transform pivot, float angleX, float angleZ, float t)
        {
            if (pivot == null)
            {
                return;
            }

            Quaternion target = Quaternion.Euler(angleX, 0f, angleZ);
            pivot.localRotation = Quaternion.Slerp(pivot.localRotation, target, t);
        }
    }
}
