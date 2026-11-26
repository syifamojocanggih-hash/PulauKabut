using System;
using PulauKabut.Core;
using UnityEngine;

namespace PulauKabut.Lantern
{
    /// <summary>
    /// Tungku / Tiang Lentera di dunia (implementasi IInteractable):
    /// - Mengisi penuh kembali daya lentera pemain saat berinteraksi [E].
    /// - Jika berupa tungku teka-teki, harus dinyalakan saat lentera pemain MENYALA untuk membuka gerbang/jalur rahasia.
    /// </summary>
    public class LanternBrazier : MonoBehaviour, IInteractable
    {
        [Header("Pengaturan Tungku")]
        [Tooltip("Jika true, tungku sudah menyala sejak awal (berfungsi sebagai titik isi ulang daya lentera).")]
        [SerializeField] private bool startLit = false;

        [Tooltip("Jika true, lentera pemain harus sedang MENYALA untuk menyalakan tungku yang padam.")]
        [SerializeField] private bool requireLitLantern = true;

        [Tooltip("Nama lokasi tungku yang ditampilkan di prompt interaksi.")]
        [SerializeField] private string brazierName = "Tungku Pelita";

        [Header("Visual & Cahaya (Opsional)")]
        [SerializeField] private GameObject flameVisual;
        [SerializeField] private Light brazierLight;
        [SerializeField] private Renderer coreRenderer;
        [SerializeField] private Material litMaterial;
        [SerializeField] private Material unlitMaterial;

        /// <summary>True jika tungku ini sudah menyala.</summary>
        public bool IsLit { get; private set; }

        /// <summary>Event dipicu sekali saat tungku berhasil dinyalakan.</summary>
        public event Action<LanternBrazier> OnBrazierLit;

        private string _promptRefill;
        private string _promptNeedLantern;
        private string _promptLight;

        public bool CanInteract => true;

        public string PromptText
        {
            get
            {
                if (IsLit)
                {
                    return _promptRefill;
                }

                LanternController lantern = LanternController.ActiveLantern;
                if (requireLitLantern && (lantern == null || !lantern.IsLit))
                {
                    return _promptNeedLantern;
                }

                return _promptLight;
            }
        }

        private void Awake()
        {
            _promptRefill = $"Isi Ulang Daya Lentera ({brazierName})";
            _promptNeedLantern = $"Nyalakan Lentera [F] dulu untuk {brazierName}";
            _promptLight = $"Nyalakan {brazierName}";

            IsLit = startLit;
            ApplyVisuals();
        }

        public void Interact(GameObject interactor)
        {
            LanternController lantern = LanternController.ActiveLantern;

            if (IsLit)
            {
                if (lantern != null)
                {
                    lantern.RefillCharge(1f);
                }
                return;
            }

            if (requireLitLantern && (lantern == null || !lantern.IsLit))
            {
                return;
            }

            IsLit = true;
            ApplyVisuals();

            if (lantern != null)
            {
                lantern.RefillCharge(1f);
            }

            GameAudioManager.Instance?.PlayBrazierLit();
            OnBrazierLit?.Invoke(this);
        }

        private void ApplyVisuals()
        {
            if (flameVisual != null)
            {
                flameVisual.SetActive(IsLit);
            }

            if (brazierLight != null)
            {
                brazierLight.enabled = IsLit;
            }

            if (coreRenderer != null)
            {
                Material targetMat = IsLit ? litMaterial : unlitMaterial;
                if (targetMat != null)
                {
                    coreRenderer.sharedMaterial = targetMat;
                }
            }
        }
    }
}
