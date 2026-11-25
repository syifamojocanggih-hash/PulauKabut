using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Menggerakkan sekumpulan kunang-kunang (child Transform) secara lembut dan organik
    /// untuk menghidupkan suasana Rawa Kunang dengan nol alokasi memori di Update.
    /// </summary>
    public class FireflySwarm : MonoBehaviour
    {
        [Tooltip("Jarak gerak melayang horizontal (meter).")]
        [SerializeField] private float horizontalAmplitude = 0.85f;

        [Tooltip("Jarak gerak naik-turun vertikal (meter).")]
        [SerializeField] private float verticalAmplitude = 0.45f;

        [Tooltip("Kecepatan dasar gerak melayang kunang-kunang.")]
        [SerializeField] private float moveSpeed = 1.25f;

        private Transform[] _flies;
        private Vector3[] _baseLocalPositions;
        private float[] _phaseOffsets;

        private void Awake()
        {
            int count = transform.childCount;
            _flies = new Transform[count];
            _baseLocalPositions = new Vector3[count];
            _phaseOffsets = new float[count];

            for (int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);
                _flies[i] = child;
                _baseLocalPositions[i] = child.localPosition;
                _phaseOffsets[i] = i * 1.73f;
            }
        }

        private void Update()
        {
            if (_flies == null || _flies.Length == 0)
            {
                return;
            }

            float t = Time.time * moveSpeed;
            for (int i = 0; i < _flies.Length; i++)
            {
                Transform fly = _flies[i];
                if (fly == null) continue;

                float phase = _phaseOffsets[i];
                Vector3 basePos = _baseLocalPositions[i];
                float dx = Mathf.Sin(t + phase) * horizontalAmplitude;
                float dy = Mathf.Sin(t * 1.35f + phase * 1.7f) * verticalAmplitude;
                float dz = Mathf.Cos(t * 0.9f + phase * 0.6f) * horizontalAmplitude;

                fly.localPosition = new Vector3(basePos.x + dx, basePos.y + dy, basePos.z + dz);
            }
        }
    }
}
