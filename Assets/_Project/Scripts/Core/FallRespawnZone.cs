using PulauKabut.Player;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Zona bahaya di bawah jurang atau air dalam:
    /// jika pemain jatuh menyentuh zona ini, otomatis dikembalikan ke Checkpoint terakhir.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FallRespawnZone : MonoBehaviour
    {
        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController player))
            {
                CheckpointManager.Instance?.RespawnPlayer(player);
            }
        }
    }
}
