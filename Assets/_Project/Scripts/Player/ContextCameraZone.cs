using Unity.Cinemachine;
using UnityEngine;

namespace PulauKabut.Player
{
    /// <summary>
    /// Zona pemicu POV 2 (Kamera Plot / Konteks) berbasis Cinemachine 3.
    /// Saat pemain masuk ke zona ini, kamera otomatis berpindah halus ke sudut pandang konteks;
    /// saat keluar, kamera otomatis kembali ke POV 1 (pandangan mata karakter).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ContextCameraZone : MonoBehaviour
    {
        [Header("Kamera POV 2 (Cinemachine 3)")]
        [Tooltip("CinemachineCamera tujuan saat pemain berada di dalam zona ini.")]
        [SerializeField] private CinemachineCamera contextCamera;

        [Tooltip("Prioritas saat aktif (harus lebih tinggi dari POV 1 yang bernilai 10).")]
        [SerializeField] private int activePriority = 20;

        [Tooltip("Prioritas saat tidak aktif (lebih rendah dari POV 1).")]
        [SerializeField] private int inactivePriority = 0;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Awake()
        {
            // Pastikan saat mulai scene kamera konteks dalam keadaan nonaktif
            SetCameraActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out _))
            {
                SetCameraActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out _))
            {
                SetCameraActive(false);
            }
        }

        private void SetCameraActive(bool isActive)
        {
            if (contextCamera == null)
            {
                return;
            }

            contextCamera.Priority.Enabled = true;
            contextCamera.Priority.Value = isActive ? activePriority : inactivePriority;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                return;
            }

            Gizmos.color = new Color(0.15f, 0.85f, 0.95f, 0.18f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = new Color(0.15f, 0.85f, 0.95f, 0.75f);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.color = new Color(0.15f, 0.85f, 0.95f, 0.75f);
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
#endif
    }
}
