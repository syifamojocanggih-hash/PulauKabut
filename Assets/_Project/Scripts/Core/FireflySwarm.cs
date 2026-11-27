using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Menggerakkan sekumpulan kunang-kunang (child Transform) secara lembut dan organik,
    /// mengarahkan tubuh mengikuti arah terbang, serta mengepakkan sayap dengan nol alokasi di Update.
    /// </summary>
    public class FireflySwarm : MonoBehaviour
    {
        [Tooltip("Jarak gerak melayang horizontal (meter).")]
        [SerializeField] private float horizontalAmplitude = 0.85f;

        [Tooltip("Jarak gerak naik-turun vertikal (meter).")]
        [SerializeField] private float verticalAmplitude = 0.45f;

        [Tooltip("Kecepatan dasar gerak melayang kunang-kunang.")]
        [SerializeField] private float moveSpeed = 1.25f;

        [Header("Animasi Sayap")]
        [Tooltip("Frekuensi kepakan sayap (rad/detik skala).")]
        [SerializeField] private float wingFlapSpeed = 48f;

        [Tooltip("Sudut maksimal kepakan sayap (derajat).")]
        [SerializeField] private float wingFlapAngle = 32f;

        private Transform[] _flies;
        private Transform[] _wingsL;
        private Transform[] _wingsR;
        private Quaternion[] _baseWingLRot;
        private Quaternion[] _baseWingRRot;
        private Vector3[] _baseLocalPositions;
        private float[] _phaseOffsets;

        private void Awake()
        {
            int count = transform.childCount;
            _flies = new Transform[count];
            _wingsL = new Transform[count];
            _wingsR = new Transform[count];
            _baseWingLRot = new Quaternion[count];
            _baseWingRRot = new Quaternion[count];
            _baseLocalPositions = new Vector3[count];
            _phaseOffsets = new float[count];

            for (int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);
                _flies[i] = child;
                _baseLocalPositions[i] = child.localPosition;
                _phaseOffsets[i] = i * 1.73f;

                Transform wingL = FindWing(child, "Kunang_Wing_L");
                Transform wingR = FindWing(child, "Kunang_Wing_R");
                _wingsL[i] = wingL;
                _wingsR[i] = wingR;
                if (wingL != null) _baseWingLRot[i] = wingL.localRotation;
                if (wingR != null) _baseWingRRot[i] = wingR.localRotation;
            }
        }

        private static Transform FindWing(Transform root, string wingName)
        {
            Transform direct = root.Find(wingName);
            if (direct != null) return direct;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform nested = root.GetChild(i).Find(wingName);
                if (nested != null) return nested;
            }
            return null;
        }

        private void Update()
        {
            if (_flies == null || _flies.Length == 0)
            {
                return;
            }

            float time = Time.time;
            float t = time * moveSpeed;

            for (int i = 0; i < _flies.Length; i++)
            {
                Transform fly = _flies[i];
                if (fly == null) continue;

                float phase = _phaseOffsets[i];
                Vector3 basePos = _baseLocalPositions[i];

                float argX = t + phase;
                float argY = t * 1.35f + phase * 1.7f;
                float argZ = t * 0.9f + phase * 0.6f;

                float dx = Mathf.Sin(argX) * horizontalAmplitude;
                float dy = Mathf.Sin(argY) * verticalAmplitude;
                float dz = Mathf.Cos(argZ) * horizontalAmplitude;

                fly.localPosition = new Vector3(basePos.x + dx, basePos.y + dy, basePos.z + dz);

                // Turunan posisi sebagai vektor arah terbang supaya badan mengarah natural
                float vx = Mathf.Cos(argX) * horizontalAmplitude;
                float vy = Mathf.Cos(argY) * verticalAmplitude * 0.35f;
                float vz = -Mathf.Sin(argZ) * 0.9f * horizontalAmplitude;

                if (vx * vx + vz * vz > 0.0001f)
                {
                    fly.localRotation = Quaternion.LookRotation(new Vector3(vx, vy, vz), Vector3.up);
                }

                float flap = Mathf.Sin(time * wingFlapSpeed + phase * 3.1f) * wingFlapAngle;
                Transform wl = _wingsL[i];
                Transform wr = _wingsR[i];
                if (wl != null)
                {
                    wl.localRotation = _baseWingLRot[i] * Quaternion.AngleAxis(flap, Vector3.forward);
                }
                if (wr != null)
                {
                    wr.localRotation = _baseWingRRot[i] * Quaternion.AngleAxis(-flap, Vector3.forward);
                }
            }
        }
    }
}
