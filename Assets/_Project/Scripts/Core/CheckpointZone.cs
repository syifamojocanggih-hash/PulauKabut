using PulauKabut.Player;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Zona trigger checkpoint otomatis di tiap area. Saat pemain melintas, titik respawn diperbarui.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CheckpointZone : MonoBehaviour
    {
        [Tooltip("Nama area checkpoint yang tampil singkat di layar.")]
        [SerializeField] private string checkpointName = "Dermaga";

        [Tooltip("Titik spawn persis (jika kosong, memakai posisi objek ini).")]
        [SerializeField] private Transform spawnPoint;

        private bool _activated;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_activated) return;

            if (other.TryGetComponent<PlayerController>(out _))
            {
                _activated = true;
                Transform target = spawnPoint != null ? spawnPoint : transform;
                CheckpointManager.Instance?.SetCheckpoint(target.position, target.rotation, checkpointName);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _activated ? new Color(0.3f, 1f, 0.5f, 0.35f) : new Color(0.95f, 0.9f, 0.3f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
#endif
    }
}
