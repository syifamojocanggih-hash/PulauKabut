using System.Collections.Generic;
using PulauKabut.Lantern;
using PulauKabut.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PulauKabut.Core
{
    /// <summary>
    /// Menghubungkan SaveSystem dengan ShardManager, LanternController, dan CheckpointManager:
    /// memuat progres dari slot terpilih saat scene dimulai, serta otomatis memperbarui
    /// Slot 0 (Auto Save) di titik Checkpoint terakhir pemain.
    /// </summary>
    public class SaveLoadBridge : MonoBehaviour
    {
        public static SaveLoadBridge Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.OnShardCollected += HandleProgressChanged;
                ShardManager.Instance.OnMemoryFragmentCollected += HandleProgressChanged;
            }

            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.OnCheckpointUpdated += HandleCheckpointUpdated;
            }

            ApplySavedProgressIfExists();
        }

        private void OnDestroy()
        {
            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.OnShardCollected -= HandleProgressChanged;
                ShardManager.Instance.OnMemoryFragmentCollected -= HandleProgressChanged;
            }

            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.OnCheckpointUpdated -= HandleCheckpointUpdated;
            }

            if (Instance == this) Instance = null;
        }

        private void OnApplicationQuit()
        {
            SaveAutoAtCheckpoint();
        }

        private void HandleProgressChanged(int count, int total, string id)
        {
            SaveAutoAtCheckpoint();
        }

        private void HandleCheckpointUpdated(Vector3 pos, Quaternion rot, string checkpointName)
        {
            SaveAutoAtCheckpoint();
        }

        /// <summary>Menyimpan otomatis ke Slot 0 (Auto Save) dengan posisi spawn tepat di Checkpoint terakhir.</summary>
        public void SaveAutoAtCheckpoint()
        {
            SaveToSlot(SaveSystem.AutoSaveSlotIndex, forceCheckpointPosition: true);
        }

        /// <summary>Menyimpan progres saat ini ke slot tertentu (0 = Auto Save di Checkpoint, 1..3 = Save Manual).</summary>
        public void SaveToSlot(int slotIndex, bool forceCheckpointPosition = true)
        {
            GameSaveData data = BuildCurrentSaveData(slotIndex, forceCheckpointPosition);
            SaveSystem.SaveToSlot(slotIndex, data);
        }

        /// <summary>Kompatibilitas pemanggil lama: memperbarui Auto Save (Slot 0) di posisi Checkpoint.</summary>
        public void SaveNow()
        {
            SaveAutoAtCheckpoint();
        }

        private GameSaveData BuildCurrentSaveData(int slotIndex, bool forceCheckpointPosition)
        {
            bool isAuto = slotIndex == SaveSystem.AutoSaveSlotIndex;
            string cpName = CheckpointManager.Instance != null && !string.IsNullOrEmpty(CheckpointManager.Instance.LastCheckpointName)
                ? CheckpointManager.Instance.LastCheckpointName
                : "Dermaga";

            GameSaveData data = new GameSaveData
            {
                slotIndex = slotIndex,
                isAutoSave = isAuto,
                slotLabel = isAuto ? "AUTO SAVE" : $"Perjalanan {slotIndex}",
                checkpointName = cpName,
                lastSceneName = SceneManager.GetActiveScene().name,
                hasLantern = LanternController.ActiveLantern != null && LanternController.ActiveLantern.HasLantern
            };

            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.CopyCollectedIds(data.collectedShards, data.collectedMemories);
            }

            // Auto Save (Slot 0) SELALU memakai koordinat Checkpoint terakhir agar aman & sesuai titik checkpoint!
            if (forceCheckpointPosition && CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
            {
                Vector3 cpPos = CheckpointManager.Instance.CheckpointPosition;
                data.hasCheckpoint = true;
                data.checkpointX = cpPos.x;
                data.checkpointY = cpPos.y + 0.20f;
                data.checkpointZ = cpPos.z;
            }
            else
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null && (pc.IsGrounded || CheckpointManager.Instance == null || !CheckpointManager.Instance.HasCheckpoint))
                {
                    Vector3 pos = pc.transform.position;
                    data.hasCheckpoint = true;
                    data.checkpointX = pos.x;
                    data.checkpointY = pos.y + 0.15f;
                    data.checkpointZ = pos.z;
                }
                else if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
                {
                    Vector3 cpPos = CheckpointManager.Instance.CheckpointPosition;
                    data.hasCheckpoint = true;
                    data.checkpointX = cpPos.x;
                    data.checkpointY = cpPos.y + 0.20f;
                    data.checkpointZ = cpPos.z;
                }
            }

            return data;
        }

        private void ApplySavedProgressIfExists()
        {
            int targetSlot = SaveSystem.PendingLoadSlot;
            SaveSystem.PendingLoadSlot = -1;

            // Jika masuk via MULAI BARU (-2), jangan muat data lama dan buat snapshot Auto Save awal
            if (targetSlot == -2)
            {
                return;
            }

            GameSaveData data = targetSlot >= 0
                ? SaveSystem.LoadSlot(targetSlot)
                : SaveSystem.LoadSlot(SaveSystem.AutoSaveSlotIndex);
            if (data == null) return;

            if (data.hasLantern && LanternController.ActiveLantern != null)
            {
                LanternController.ActiveLantern.GrantLantern(turnOnImmediately: false);
            }

            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.RestoreCollectedIds(data.collectedShards, data.collectedMemories);

                // Sembunyikan pickup Pecahan / Ingatan yang sudah pernah diambil sebelumnya
                foreach (var pickup in FindObjectsByType<LightShardPickup>())
                {
                    if (ShardManager.Instance.HasCollectedShard(pickup.CollectibleId) ||
                        ShardManager.Instance.HasCollectedMemory(pickup.CollectibleId))
                    {
                        pickup.gameObject.SetActive(false);
                    }
                }
            }

            if (data.hasCheckpoint)
            {
                Vector3 savedPos = new Vector3(data.checkpointX, data.checkpointY, data.checkpointZ);
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null)
                {
                    CharacterController cc = pc.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    pc.transform.position = savedPos;
                    if (cc != null) cc.enabled = true;
                }

                CheckpointManager.Instance?.RestoreCheckpoint(savedPos, Quaternion.identity, data.checkpointName);
            }
        }
    }
}
