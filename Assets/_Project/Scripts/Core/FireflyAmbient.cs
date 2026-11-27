using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Menghidupkan satu unit kunang-kunang (PF_Kunang) yang berdiri sendiri:
    /// terbang melayang lembut, mengepakkan sayap, dan memendarkan cahaya ekor redup hangat
    /// (otomatis nonaktif jika sudah dikendalikan oleh FireflySwarm induk).
    /// </summary>
    public class FireflyAmbient : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Gerak Terbang")]
        [SerializeField] private float horizontalAmplitude = 0.52f;
        [SerializeField] private float verticalAmplitude = 0.26f;
        [SerializeField] private float moveSpeed = 1.15f;

        [Header("Kepakan Sayap")]
        [SerializeField] private float wingFlapSpeed = 46f;
        [SerializeField] private float wingFlapAngle = 30f;

        [Header("Pendar Cahaya Halus")]
        [SerializeField] private Color glowColor = new Color(0.82f, 0.64f, 0.16f, 1f);
        [SerializeField] private float minGlow = 0.35f;
        [SerializeField] private float maxGlow = 0.95f;
        [SerializeField] private float glowPulseSpeed = 2.1f;

        private Vector3 _anchorLocalPos;
        private float _phase;
        private Transform _wingL;
        private Transform _wingR;
        private Quaternion _baseWingLRot;
        private Quaternion _baseWingRRot;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            // Jika berada di dalam FireflySwarm, biarkan FireflySwarm yang menggerakkan agar tidak ganda
            if (GetComponentInParent<FireflySwarm>() != null)
            {
                enabled = false;
                return;
            }

            _anchorLocalPos = transform.localPosition;
            _phase = (transform.position.x * 1.37f + transform.position.z * 2.11f) % 6.283f;
            _mpb = new MaterialPropertyBlock();

            _wingL = FindChildDeep(transform, "Kunang_Wing_L");
            _wingR = FindChildDeep(transform, "Kunang_Wing_R");
            if (_wingL != null) _baseWingLRot = _wingL.localRotation;
            if (_wingR != null) _baseWingRRot = _wingR.localRotation;

            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Update()
        {
            float time = Time.time;
            float t = time * moveSpeed + _phase;

            float dx = Mathf.Sin(t) * horizontalAmplitude;
            float dy = Mathf.Sin(t * 1.42f + 0.8f) * verticalAmplitude;
            float dz = Mathf.Cos(t * 0.85f + 0.4f) * horizontalAmplitude;

            transform.localPosition = new Vector3(_anchorLocalPos.x + dx, _anchorLocalPos.y + dy, _anchorLocalPos.z + dz);

            float vx = Mathf.Cos(t) * horizontalAmplitude;
            float vy = Mathf.Cos(t * 1.42f + 0.8f) * verticalAmplitude * 0.3f;
            float vz = -Mathf.Sin(t * 0.85f + 0.4f) * 0.85f * horizontalAmplitude;
            if (vx * vx + vz * vz > 0.0001f)
            {
                transform.localRotation = Quaternion.LookRotation(new Vector3(vx, vy, vz), Vector3.up);
            }

            float flap = Mathf.Sin(time * wingFlapSpeed + _phase * 2.3f) * wingFlapAngle;
            if (_wingL != null) _wingL.localRotation = _baseWingLRot * Quaternion.AngleAxis(flap, Vector3.forward);
            if (_wingR != null) _wingR.localRotation = _baseWingRRot * Quaternion.AngleAxis(-flap, Vector3.forward);

            // Pendar ekor redup hangat (tidak silau, cukup agar kunang-kunang terlihat hidup)
            if (_renderers != null && _renderers.Length > 0)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(time * glowPulseSpeed + _phase);
                float glow = Mathf.Lerp(minGlow, maxGlow, pulse);
                _mpb.SetColor(EmissionColorId, glowColor * glow);
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null && _renderers[i].name.Contains("Body"))
                    {
                        _renderers[i].SetPropertyBlock(_mpb);
                    }
                }
            }
        }

        private static Transform FindChildDeep(Transform root, string exactName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform c = root.GetChild(i);
                if (c.name == exactName) return c;
                Transform nested = FindChildDeep(c, exactName);
                if (nested != null) return nested;
            }
            return null;
        }
    }
}
