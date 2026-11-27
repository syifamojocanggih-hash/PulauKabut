using System;
using System.Collections.Generic;
using System.Text;
using PulauKabut.UI;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Mengelola progres koleksi Pecahan Cahaya (utama), Serpihan Ingatan (opsional),
    /// tahap pemulihan Hub Desa Pelita (0-5), serta menampilkan HUD penghitung ringan.
    /// </summary>
    public class ShardManager : MonoBehaviour
    {
        public static ShardManager Instance { get; private set; }

        [Header("Target Progres")]
        [Tooltip("Total Pecahan Cahaya utama (4 per wilayah x 5 wilayah = 20, atau 24 termasuk rangkaian akhir).")]
        [Min(1)]
        [SerializeField] private int totalLightShards = 24;

        [Tooltip("Jumlah Pecahan Cahaya per wilayah untuk menghitung tahap pemulihan Desa Pelita (0-5).")]
        [Min(1)]
        [SerializeField] private int shardsPerRegion = 4;

        [Tooltip("Total Serpihan Ingatan opsional di seluruh dunia.")]
        [Min(0)]
        [SerializeField] private int totalMemoryFragments = 15;

        [Header("Tampilan HUD")]
        [SerializeField] private bool showCounterHud = true;

        private readonly HashSet<string> _collectedShards = new HashSet<string>();
        private readonly HashSet<string> _collectedMemories = new HashSet<string>();
        private readonly StringBuilder _hudBuilder = new StringBuilder(96);
        private string _cachedHudText = string.Empty;
        private GUIStyle _hudStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _goldNumStyle;
        private GUIStyle _mintNumStyle;
        private string _shardNumText = "0 / 24";
        private string _memNumText = "0 / 15";
        private string _stageNumText = "Tahap 0/5";

        public int CollectedShardCount => _collectedShards.Count;
        public int TotalLightShards => totalLightShards;
        public int CollectedMemoryCount => _collectedMemories.Count;
        public int TotalMemoryFragments => totalMemoryFragments;

        /// <summary>Tahap visual Desa Pelita saat ini (0 sampai 5).</summary>
        public int CurrentHubStage => Mathf.Clamp(CollectedShardCount / Mathf.Max(1, shardsPerRegion), 0, 5);

        public event Action<int, int, string> OnShardCollected;
        public event Action<int, int, string> OnMemoryFragmentCollected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            RebuildHudText();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Mencatat Pecahan Cahaya yang baru diambil. Mengembalikan false jika ID sudah pernah diambil.</summary>
        public bool CollectLightShard(string shardId)
        {
            if (string.IsNullOrEmpty(shardId) || !_collectedShards.Add(shardId))
            {
                return false;
            }

            RebuildHudText();
            OnShardCollected?.Invoke(CollectedShardCount, totalLightShards, shardId);
            return true;
        }

        /// <summary>Mencatat Serpihan Ingatan opsional yang baru diambil.</summary>
        public bool CollectMemoryFragment(string fragmentId)
        {
            if (string.IsNullOrEmpty(fragmentId) || !_collectedMemories.Add(fragmentId))
            {
                return false;
            }

            RebuildHudText();
            OnMemoryFragmentCollected?.Invoke(CollectedMemoryCount, totalMemoryFragments, fragmentId);
            return true;
        }

        public bool HasCollectedShard(string shardId) => _collectedShards.Contains(shardId);
        public bool HasCollectedMemory(string fragmentId) => _collectedMemories.Contains(fragmentId);

        /// <summary>Menyalin daftar ID yang sudah dikumpulkan ke list tujuan untuk SaveSystem.</summary>
        public void CopyCollectedIds(List<string> shardsOut, List<string> memoriesOut)
        {
            if (shardsOut != null)
            {
                shardsOut.Clear();
                foreach (string id in _collectedShards) shardsOut.Add(id);
            }
            if (memoriesOut != null)
            {
                memoriesOut.Clear();
                foreach (string id in _collectedMemories) memoriesOut.Add(id);
            }
        }

        /// <summary>Memulihkan daftar ID dari SaveSystem tanpa memicu ulang animasi popup satu per satu.</summary>
        public void RestoreCollectedIds(List<string> savedShards, List<string> savedMemories)
        {
            _collectedShards.Clear();
            _collectedMemories.Clear();
            if (savedShards != null)
            {
                for (int i = 0; i < savedShards.Count; i++)
                {
                    if (!string.IsNullOrEmpty(savedShards[i])) _collectedShards.Add(savedShards[i]);
                }
            }
            if (savedMemories != null)
            {
                for (int i = 0; i < savedMemories.Count; i++)
                {
                    if (!string.IsNullOrEmpty(savedMemories[i])) _collectedMemories.Add(savedMemories[i]);
                }
            }
            RebuildHudText();
            if (CollectedShardCount > 0)
            {
                OnShardCollected?.Invoke(CollectedShardCount, totalLightShards, "RESTORED");
            }
        }

        private void RebuildHudText()
        {
            _shardNumText = $"{CollectedShardCount} / {totalLightShards}";
            _memNumText = $"{CollectedMemoryCount} / {totalMemoryFragments}";
            _stageNumText = $"Tahap {CurrentHubStage}/5";
            _cachedHudText = _shardNumText;
        }

        private void OnGUI()
        {
            if (!showCounterHud || Time.timeScale <= 0f)
            {
                return;
            }

            if (_hudStyle == null)
            {
                _hudStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 14, 10);
                _labelStyle = StylizedUITheme.CreateBodyLabelStyle(13, StylizedUITheme.SeafoamText, TextAnchor.MiddleLeft);
                _goldNumStyle = StylizedUITheme.CreateHeadingLabelStyle(15, StylizedUITheme.GoldAccent, TextAnchor.MiddleRight);
                _mintNumStyle = StylizedUITheme.CreateHeadingLabelStyle(15, StylizedUITheme.MintAccent, TextAnchor.MiddleRight);
            }

            // Kartu status persis Hud.dc.html (diberi ruang 32px di bawah minimap untuk badge [M] Peta!)
            Rect card = new Rect(14f, 190f, 232f, 152f);
            GUI.Box(card, GUIContent.none, _hudStyle);

            float ix = card.x + 14f;
            float tx = card.x + 42f;
            float rx = card.x + 14f;
            float rw = card.width - 28f;
            Color prev = GUI.color;
            Color sepCol = new Color(0.02f, 0.08f, 0.10f, 0.55f);

            // Baris 1: Pecahan Cahaya
            GUI.DrawTexture(new Rect(ix, card.y + 15f, 20f, 20f), StylizedUITheme.PentagonIconTex);
            GUI.Label(new Rect(tx, card.y + 13f, 115f, 24f), "Pecahan Cahaya", _labelStyle);
            GUI.Label(new Rect(rx, card.y + 13f, rw, 24f), _shardNumText, _goldNumStyle);

            GUI.color = sepCol;
            GUI.DrawTexture(new Rect(ix, card.y + 44f, rw, 2f), StylizedUITheme.SolidTex);
            GUI.color = prev;

            // Baris 2: Serpihan Ingatan
            GUI.DrawTexture(new Rect(ix, card.y + 54f, 20f, 20f), StylizedUITheme.HexagonIconTex);
            GUI.Label(new Rect(tx, card.y + 52f, 115f, 24f), "Serpihan Ingatan", _labelStyle);
            GUI.Label(new Rect(rx, card.y + 52f, rw, 24f), _goldNumTextOrMem(_memNumText), _goldNumStyle);

            GUI.color = sepCol;
            GUI.DrawTexture(new Rect(ix, card.y + 83f, rw, 2f), StylizedUITheme.SolidTex);
            GUI.color = prev;

            // Baris 3: Desa Pelita + 5 berlian tahap
            GUI.DrawTexture(new Rect(ix, card.y + 93f, 20f, 20f), StylizedUITheme.HouseIconTex);
            GUI.Label(new Rect(tx, card.y + 91f, 100f, 24f), "Desa Pelita", _labelStyle);
            GUI.Label(new Rect(rx, card.y + 91f, rw, 24f), _stageNumText, _goldNumStyle);

            int stage = CurrentHubStage;
            for (int i = 0; i < 5; i++)
            {
                GUI.color = i < stage
                    ? StylizedUITheme.GoldAccent
                    : new Color(0.965f, 0.937f, 0.851f, 0.25f);
                GUI.DrawTexture(new Rect(tx + i * 20f, card.y + 122f, 12f, 12f), StylizedUITheme.DiamondTex);
            }
            GUI.color = prev;
        }

        private static string _goldNumTextOrMem(string s) => s;
    }
}
