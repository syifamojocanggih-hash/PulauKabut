using PulauKabut.Dialog;
using PulauKabut.UI;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Mengendalikan perubahan visual dan suasana Hub Desa Pelita secara bertahap (Tahap 0 sampai 5)
    /// setiap kali pemain mengumpulkan 4 Pecahan Cahaya di suatu wilayah.
    /// Nol alokasi memori di Update (berbasis event ShardManager.OnShardCollected).
    /// </summary>
    public class DesaPelitaStageController : MonoBehaviour
    {
        [Header("Objek Visual Per Tahap Pemulihan (Aktif Bertahap)")]
        [Tooltip("Objek yang aktif saat Desa Pelita mencapai Tahap 1/5 (Wilayah 1 selesai).")]
        [SerializeField] private GameObject[] stage1Objects;

        [Tooltip("Objek yang aktif saat Desa Pelita mencapai Tahap 2/5 (Wilayah 2 selesai).")]
        [SerializeField] private GameObject[] stage2Objects;

        [Tooltip("Objek yang aktif saat Desa Pelita mencapai Tahap 3/5 (Wilayah 3 selesai).")]
        [SerializeField] private GameObject[] stage3Objects;

        [Tooltip("Objek yang aktif saat Desa Pelita mencapai Tahap 4/5 (Wilayah 4 selesai).")]
        [SerializeField] private GameObject[] stage4Objects;

        [Tooltip("Objek yang aktif saat Desa Pelita mencapai Tahap 5/5 (Pulau pulih total).")]
        [SerializeField] private GameObject[] stage5Objects;

        [Header("Perubahan Pencahayaan & Dialog NPC")]
        [Tooltip("Directional Light utama yang sedikit menghangat tiap tahap pemulihan.")]
        [SerializeField] private Light sunLight;

        [Tooltip("NPC Mbah Sauh yang dialognya diperbarui saat Tahap 1/5 tercapai.")]
        [SerializeField] private NpcInteractable mbahSauhNpc;

        [Tooltip("Dialog perayaan Mbah Sauh setelah Wilayah 1 selesai (Tahap 1/5).")]
        [SerializeField] private DialogData stage1MbahDialog;

        private int _appliedStage = -1;
        private float _baseSunIntensity = 1.42f;

        private void Start()
        {
            if (sunLight == null)
            {
                foreach (Light l in FindObjectsByType<Light>())
                {
                    if (l.type == LightType.Directional)
                    {
                        sunLight = l;
                        break;
                    }
                }
            }

            if (sunLight != null)
            {
                _baseSunIntensity = sunLight.intensity;
            }

            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.OnShardCollected += HandleShardCollected;
                ApplyStage(ShardManager.Instance.CurrentHubStage, announce: false);
            }
            else
            {
                ApplyStage(0, announce: false);
            }
        }

        private void OnDestroy()
        {
            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.OnShardCollected -= HandleShardCollected;
            }
        }

        private void HandleShardCollected(int collected, int total, string shardId)
        {
            if (ShardManager.Instance == null) return;
            ApplyStage(ShardManager.Instance.CurrentHubStage, announce: true);
        }

        /// <summary>Menerapkan seluruh perubahan visual Desa Pelita untuk tahap 0..5.</summary>
        public void ApplyStage(int stage, bool announce)
        {
            stage = Mathf.Clamp(stage, 0, 5);
            if (stage == _appliedStage) return;

            bool stageUp = _appliedStage >= 0 && stage > _appliedStage;
            _appliedStage = stage;

            SetGroupActive(stage1Objects, stage >= 1);
            SetGroupActive(stage2Objects, stage >= 2);
            SetGroupActive(stage3Objects, stage >= 3);
            SetGroupActive(stage4Objects, stage >= 4);
            SetGroupActive(stage5Objects, stage >= 5);

            // Sedikit hangatkan cahaya matahari seiring desa pulih
            if (sunLight != null)
            {
                sunLight.intensity = _baseSunIntensity + stage * 0.08f;
            }

            // Perbarui dialog Mbah Sauh saat Wilayah 1 selesai (Tahap 1/5)
            if (stage >= 1 && mbahSauhNpc != null && stage1MbahDialog != null)
            {
                mbahSauhNpc.SetDialogData(stage1MbahDialog);
            }

            if (announce && stageUp && ObjectiveGuide.Instance != null)
            {
                Transform target = mbahSauhNpc != null ? mbahSauhNpc.transform : null;
                ObjectiveGuide.Instance.SetObjective($"Desa Pelita Pulih ke Tahap {stage}/5! Temui Mbah Sauh", target);
            }
        }

        private static void SetGroupActive(GameObject[] group, bool active)
        {
            if (group == null) return;
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i] != null)
                {
                    group[i].SetActive(active);
                }
            }
        }
    }
}
