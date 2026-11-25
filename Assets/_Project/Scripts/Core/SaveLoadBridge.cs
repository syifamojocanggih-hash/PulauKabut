using System.Collections.Generic;
using PulauKabut.Lantern;
using PulauKabut.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PulauKabut.Core
{
    /// <summary>
    /// Menghubungkan SaveSystem dengan ShardManager, LanternController, dan posisi pemain:
    /// memuat progres otomatis saat scene dimulai dan menyimpan otomatis saat progres bertambah.
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

            ApplySavedProgressIfExists();
        }

        private void OnDestroy()
        {
            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.OnShardCollected -= HandleProgressChanged;
                ShardManager.Instance.OnMemoryFragmentCollected -= HandleProgressChanged;
            }
            if (Instance == this) Instance = null;
        }

        private void HandleProgressChanged(int count, int total, string id)
        {
            SaveNow();
        }

        /// <summary>Menyimpan seluruh progres pemain saat ini ke disk.</summary>
        public void SaveNow()
        {
            GameSaveData data = new GameSaveData
            {
                lastSceneName = SceneManager.GetActiveScene().name,
                hasLantern = LanternController.ActiveLantern != null && LanternController.ActiveLantern.HasLantern
            };

            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.CopyCollectedIds(data.collectedShards, data.collectedMemories);
            }

            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null)
            {
                Vector3 pos = pc.transform.position;
                data.hasCheckpoint = true;
                data.checkpointX = pos.x;
                data.checkpointY = pos.y + 0.15f;
                data.checkpointZ = pos.z;
            }

            SaveSystem.Save(data);
        }

        private void ApplySavedProgressIfExists()
        {
            GameSaveData data = SaveSystem.Load();
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
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null)
                {
                    CharacterController cc = pc.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    pc.transform.position = new Vector3(data.checkpointX, data.checkpointY, data.checkpointZ);
                    if (cc != null) cc.enabled = true;
                }
            }
        }
    }
}
