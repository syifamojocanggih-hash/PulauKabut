using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Data progres pemain yang disimpan secara lokal dalam format JSON ringan.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int slotIndex;
        public bool isAutoSave;
        public string slotLabel = "Auto Save";
        public string checkpointName = "Dermaga";
        public string savedTimestamp = string.Empty;

        public bool hasLantern;
        public List<string> collectedShards = new List<string>();
        public List<string> collectedMemories = new List<string>();
        public bool hasCheckpoint;
        public float checkpointX;
        public float checkpointY;
        public float checkpointZ;
        public string lastSceneName = "Zone_Mangrove";
    }

    /// <summary>
    /// Sistem Save/Load JSON lokal 4 slot (Slot 0 = Auto Save di Checkpoint, Slot 1..3 = Save Manual Perjalanan).
    /// </summary>
    public static class SaveSystem
    {
        public const int TotalSlots = 4;
        public const int AutoSaveSlotIndex = 0;

        /// <summary>Slot yang dipilih pemain saat menekan LANJUTKAN di Menu Utama (-1 = pakai Auto Save).</summary>
        public static int PendingLoadSlot { get; set; } = -1;

        private static string LegacySavePath => Path.Combine(Application.persistentDataPath, "pulaukabut_save.json");

        public static string GetSlotPath(int slotIndex)
        {
            slotIndex = Mathf.Clamp(slotIndex, 0, TotalSlots - 1);
            return slotIndex == AutoSaveSlotIndex
                ? Path.Combine(Application.persistentDataPath, "pulaukabut_autosave.json")
                : Path.Combine(Application.persistentDataPath, $"pulaukabut_slot_{slotIndex}.json");
        }

        public static bool HasAnySaveData()
        {
            for (int i = 0; i < TotalSlots; i++)
            {
                if (HasSlotData(i)) return true;
            }
            return false;
        }

        public static bool HasSlotData(int slotIndex)
        {
            if (slotIndex == AutoSaveSlotIndex && File.Exists(LegacySavePath) && !File.Exists(GetSlotPath(AutoSaveSlotIndex)))
            {
                return true;
            }
            return File.Exists(GetSlotPath(slotIndex));
        }

        public static bool HasSaveData() => HasAnySaveData();

        public static void SaveToSlot(int slotIndex, GameSaveData data)
        {
            if (data == null) return;
            slotIndex = Mathf.Clamp(slotIndex, 0, TotalSlots - 1);
            data.slotIndex = slotIndex;
            data.isAutoSave = slotIndex == AutoSaveSlotIndex;
            if (string.IsNullOrEmpty(data.slotLabel))
            {
                data.slotLabel = data.isAutoSave ? "AUTO SAVE" : $"Perjalanan {slotIndex}";
            }
            data.savedTimestamp = DateTime.Now.ToString("dd/MM/yy HH:mm");

            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(GetSlotPath(slotIndex), json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] Gagal menyimpan progres slot {slotIndex}: {ex.Message}");
            }
        }

        public static void Save(GameSaveData data) => SaveToSlot(AutoSaveSlotIndex, data);

        public static GameSaveData LoadSlot(int slotIndex)
        {
            slotIndex = Mathf.Clamp(slotIndex, 0, TotalSlots - 1);
            string path = GetSlotPath(slotIndex);

            // Migrasi otomatis file save tunggal lama ke Slot 0 (Auto Save)
            if (slotIndex == AutoSaveSlotIndex && !File.Exists(path) && File.Exists(LegacySavePath))
            {
                path = LegacySavePath;
            }

            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                if (data != null)
                {
                    data.slotIndex = slotIndex;
                    data.isAutoSave = slotIndex == AutoSaveSlotIndex;
                    if (string.IsNullOrEmpty(data.checkpointName))
                    {
                        data.checkpointName = "Dermaga";
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] Gagal memuat progres slot {slotIndex}: {ex.Message}");
                return null;
            }
        }

        public static GameSaveData[] LoadAllSlots()
        {
            GameSaveData[] result = new GameSaveData[TotalSlots];
            for (int i = 0; i < TotalSlots; i++)
            {
                result[i] = LoadSlot(i);
            }
            return result;
        }

        public static GameSaveData Load()
        {
            int idx = PendingLoadSlot >= 0 && PendingLoadSlot < TotalSlots ? PendingLoadSlot : AutoSaveSlotIndex;
            GameSaveData loaded = LoadSlot(idx);
            if (loaded != null) return loaded;

            // Jika slot default kosong, cek slot lain yang tersedia
            for (int i = 0; i < TotalSlots; i++)
            {
                loaded = LoadSlot(i);
                if (loaded != null) return loaded;
            }
            return null;
        }

        public static void DeleteSlot(int slotIndex)
        {
            slotIndex = Mathf.Clamp(slotIndex, 0, TotalSlots - 1);
            string path = GetSlotPath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (slotIndex == AutoSaveSlotIndex && File.Exists(LegacySavePath))
            {
                File.Delete(LegacySavePath);
            }
        }

        public static void DeleteSave()
        {
            PendingLoadSlot = -1;
            DeleteSlot(AutoSaveSlotIndex);
        }
    }
}
