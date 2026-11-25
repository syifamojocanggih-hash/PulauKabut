using PulauKabut.Dialog;
using PulauKabut.Player;
using PulauKabut.UI;
using UnityEngine;

namespace PulauKabut.Core
{
    public enum CollectibleType
    {
        LightShard,
        MemoryFragment
    }

    /// <summary>
    /// Objek Pecahan Cahaya atau Serpihan Ingatan yang bisa diambil pemain dengan tombol E.
    /// Dapat memicu adegan kenangan (DialogData) dan memperbarui petunjuk arah setelah diambil.
    /// </summary>
    public class LightShardPickup : MonoBehaviour, IInteractable
    {
        [Header("Identitas Koleksi")]
        [Tooltip("ID unik pecahan (misal: W1_Shard_01). Wajib berbeda untuk setiap objek.")]
        [SerializeField] private string collectibleId = "W1_Shard_01";

        [SerializeField] private CollectibleType collectibleType = CollectibleType.LightShard;

        [SerializeField] private string promptText = "Ambil Pecahan Cahaya";

        [Header("Adegan Kenangan & Arah Berikutnya (Opsional)")]
        [Tooltip("Dialog/narasi adegan kenangan yang langsung diputar saat pecahan diambil.")]
        [SerializeField] private DialogData memoryDialog;

        [Tooltip("Tujuan baru untuk ObjectiveGuide setelah pecahan diambil (opsional).")]
        [SerializeField] private Transform nextObjectiveTarget;
        [SerializeField] private string nextObjectiveTitle = "";

        [Header("Animasi Melayang Ringan")]
        [SerializeField] private float rotateSpeed = 55f;
        [SerializeField] private float bobAmplitude = 0.18f;
        [SerializeField] private float bobFrequency = 2.2f;

        private Vector3 _basePosition;
        private bool _collected;

        public string CollectibleId => collectibleId;
        public string PromptText => promptText;
        public bool CanInteract => !_collected;

        private void Start()
        {
            _basePosition = transform.position;

            // Jika sudah pernah diambil sebelumnya (misal saat kembali ke wilayah lewat Additive Scene), sembunyikan
            if (ShardManager.Instance != null)
            {
                bool alreadyTaken = collectibleType == CollectibleType.LightShard
                    ? ShardManager.Instance.HasCollectedShard(collectibleId)
                    : ShardManager.Instance.HasCollectedMemory(collectibleId);

                if (alreadyTaken)
                {
                    _collected = true;
                    gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (_collected || Time.timeScale <= 0f)
            {
                return;
            }

            // Efek putar & mengapung ringan tanpa alokasi memori
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
            Vector3 pos = _basePosition;
            pos.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = pos;
        }

        public void Interact(GameObject interactor)
        {
            if (_collected)
            {
                return;
            }

            _collected = true;

            if (ShardManager.Instance != null)
            {
                if (collectibleType == CollectibleType.LightShard)
                    ShardManager.Instance.CollectLightShard(collectibleId);
                else
                    ShardManager.Instance.CollectMemoryFragment(collectibleId);
            }

            if (nextObjectiveTarget != null && ObjectiveGuide.Instance != null && !string.IsNullOrEmpty(nextObjectiveTitle))
            {
                ObjectiveGuide.Instance.SetObjective(nextObjectiveTitle, nextObjectiveTarget);
            }

            if (memoryDialog != null && DialogManager.Instance != null)
            {
                PlayerController player = interactor != null ? interactor.GetComponent<PlayerController>() : null;
                DialogManager.Instance.StartDialog(memoryDialog, player);
            }

            gameObject.SetActive(false);
        }
    }
}
