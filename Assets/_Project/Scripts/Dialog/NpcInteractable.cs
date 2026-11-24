using PulauKabut.Core;
using PulauKabut.Player;
using PulauKabut.UI;
using UnityEngine;

namespace PulauKabut.Dialog
{
    /// <summary>
    /// Komponen NPC interaktif (seperti Mbah Sauh atau Penjaga Bakau).
    /// Memutar DialogData saat ditekan E, dan opsional mengarahkan ObjectiveGuide ke tujuan berikutnya.
    /// </summary>
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        [Header("Dialog NPC")]
        [SerializeField] private string promptText = "Bicara dengan Mbah Sauh";
        [SerializeField] private DialogData dialogData;

        [Header("Pembaruan Petunjuk Arah Setelah Dialog (Opsional)")]
        [Tooltip("Tujuan berikutnya yang akan ditunjuk oleh kompas setelah selesai berbicara.")]
        [SerializeField] private Transform nextObjectiveTarget;
        [SerializeField] private string nextObjectiveTitle = "Menuju Menara Lentera di Puncak";

        public string PromptText => promptText;
        public bool CanInteract => dialogData != null && (DialogManager.Instance == null || !DialogManager.Instance.IsOpen);

        public void Interact(GameObject interactor)
        {
            if (DialogManager.Instance == null)
            {
                Debug.LogWarning($"[{nameof(NpcInteractable)}] DialogManager belum ada di scene.", this);
                return;
            }

            PlayerController player = interactor != null ? interactor.GetComponent<PlayerController>() : null;
            DialogManager.Instance.StartDialog(dialogData, player, OnDialogFinished);
        }

        private void OnDialogFinished()
        {
            if (nextObjectiveTarget != null && ObjectiveGuide.Instance != null)
            {
                ObjectiveGuide.Instance.SetObjective(nextObjectiveTitle, nextObjectiveTarget);
            }
        }
    }
}
