using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Kontrak universal untuk semua objek yang bisa diinteraksikan dengan tombol E
    /// (NPC, tiang suar, totem puzzle, Pecahan Cahaya, dan Altar Pelita).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Teks petunjuk singkat di layar, misalnya "Bicara dengan Mbah Sauh".</summary>
        string PromptText { get; }

        /// <summary>True jika objek saat ini sedang bisa diinteraksikan.</summary>
        bool CanInteract { get; }

        /// <summary>Aksi yang dijalankan saat pemain menekan tombol Interaksi (E).</summary>
        void Interact(GameObject interactor);
    }
}
