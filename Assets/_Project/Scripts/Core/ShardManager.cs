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

        private void RebuildHudText()
        {
            _hudBuilder.Clear();
            _hudBuilder.Append("Pecahan Cahaya : ").Append(CollectedShardCount).Append(" / ").Append(totalLightShards);
            if (totalMemoryFragments > 0)
            {
                _hudBuilder.Append("\nSerpihan Ingatan: ").Append(CollectedMemoryCount).Append(" / ").Append(totalMemoryFragments);
            }
            _hudBuilder.Append("\nDesa Pelita     : Tahap ").Append(CurrentHubStage).Append("/5");
            _cachedHudText = _hudBuilder.ToString();
        }

        private void OnGUI()
        {
            if (!showCounterHud || string.IsNullOrEmpty(_cachedHudText) || Time.timeScale <= 0f)
            {
                return;
            }

            if (_hudStyle == null)
            {
                _hudStyle = StylizedUITheme.CreateGoldPanelStyle(13, TextAnchor.UpperLeft, 14, 10);
                _hudStyle.normal.textColor = StylizedUITheme.CreamText;
            }

            // Ditempatkan rapi tepat di bawah Minimap bulat pojok kiri atas
            GUI.Box(new Rect(14f, 166f, 224f, 74f), _cachedHudText, _hudStyle);
        }
    }
}
