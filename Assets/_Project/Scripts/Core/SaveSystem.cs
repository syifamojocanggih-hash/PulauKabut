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
    /// Sistem Save/Load JSON lokal (tanpa dependensi eksternal, aman untuk Build Windows).
    /// </summary>
    public static class SaveSystem
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "pulaukabut_save.json");

        public static bool HasSaveData()
        {
            return File.Exists(SaveFilePath);
        }

        public static void Save(GameSaveData data)
        {
            if (data == null) return;
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] Gagal menyimpan progres: {ex.Message}");
            }
        }

        public static GameSaveData Load()
        {
            if (!File.Exists(SaveFilePath)) return null;
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] Gagal memuat progres: {ex.Message}");
                return null;
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
        }
    }
}
