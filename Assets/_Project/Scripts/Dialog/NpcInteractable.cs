using PulauKabut.Core;
using PulauKabut.Lantern;
using PulauKabut.Player;
using PulauKabut.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace PulauKabut.Dialog
{
    /// <summary>
    /// Komponen NPC interaktif (seperti Mbah Sauh atau Penjaga Bakau).
    /// Memutar DialogData saat ditekan E dengan transisi Fade Out/In ke kamera obrolan sinematik,
    /// lalu kembali mulus ke kamera pemain setelah selesai bicara.
    /// </summary>
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        private const int DialogCameraActivePriority = 25;
        private const int DialogCameraInactivePriority = 0;

        [Header("Dialog NPC")]
        [SerializeField] private string promptText = "Bicara dengan Mbah Sauh";
        [SerializeField] private DialogData dialogData;

        [Tooltip("Opsional: kamera Cinemachine 3 khusus sudut obrolan yang aktif otomatis selama dialog berlangsung.")]
        [SerializeField] private CinemachineCamera dialogCamera;

        [Tooltip("Gunakan transisi layar Fade Out / Fade In saat masuk dan keluar obrolan.")]
        [SerializeField] private bool useFadeTransition = true;

        [SerializeField] private float fadeDuration = 0.25f;

        [Header("Pembaruan Petunjuk Arah Setelah Dialog (Opsional)")]
        [Tooltip("Tujuan berikutnya yang akan ditunjuk oleh kompas setelah selesai berbicara.")]
        [SerializeField] private Transform nextObjectiveTarget;
        [SerializeField] private string nextObjectiveTitle = "Menuju Menara Lentera di Puncak";

        [Tooltip("Jika true, NPC ini akan memberikan Lentera kepada pemain setelah selesai bicara (misal Mbah Sauh).")]
        [SerializeField] private bool grantLanternOnDialogComplete = false;

        private PlayerController _currentPlayer;
        private bool _isTransitioning;

        public string PromptText => promptText;
        public bool CanInteract => !_isTransitioning && dialogData != null && (DialogManager.Instance == null || !DialogManager.Instance.IsOpen);

        /// <summary>Mengganti DialogData NPC secara dinamis (misal saat tahap pemulihan Desa Pelita meningkat).</summary>
        public void SetDialogData(DialogData newDialog)
        {
            if (newDialog != null)
            {
                dialogData = newDialog;
            }
        }

        private void Awake()
        {
            SetDialogCameraActive(false);
        }

        public void Interact(GameObject interactor)
        {
            if (DialogManager.Instance == null || _isTransitioning)
            {
                return;
            }

            _currentPlayer = interactor != null ? interactor.GetComponent<PlayerController>() : null;

            if (useFadeTransition && ScreenFader.Instance != null)
            {
                _isTransitioning = true;
                if (_currentPlayer != null) _currentPlayer.InputLocked = true;

                ScreenFader.Instance.FadeTransition(() =>
                {
                    SetDialogCameraActive(true);
                    _isTransitioning = false;
                    DialogManager.Instance.StartDialog(dialogData, _currentPlayer, OnDialogFinished);
                }, fadeDuration, fadeDuration);
            }
            else
            {
                SetDialogCameraActive(true);
                DialogManager.Instance.StartDialog(dialogData, _currentPlayer, OnDialogFinished);
            }
        }

        private void OnDialogFinished()
        {
            if (useFadeTransition && ScreenFader.Instance != null)
            {
                _isTransitioning = true;
                if (_currentPlayer != null) _currentPlayer.InputLocked = true;

                ScreenFader.Instance.FadeTransition(() =>
                {
                    SetDialogCameraActive(false);
                    if (_currentPlayer != null) _currentPlayer.InputLocked = false;
                    _currentPlayer = null;
                    _isTransitioning = false;
                    UpdateNextObjective();
                }, fadeDuration, fadeDuration);
            }
            else
            {
                SetDialogCameraActive(false);
                _currentPlayer = null;
                UpdateNextObjective();
            }
        }

        private void UpdateNextObjective()
        {
            bool shouldGrantLantern = grantLanternOnDialogComplete ||
                gameObject.name.IndexOf("MbahSauh", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (shouldGrantLantern && LanternController.ActiveLantern != null && !LanternController.ActiveLantern.HasLantern)
            {
                LanternController.ActiveLantern.GrantLantern(turnOnImmediately: false);
                ObjectiveGuide.Instance?.ShowUnlockNotification("Lentera Unlocked");
            }
            if (nextObjectiveTarget != null && ObjectiveGuide.Instance != null)
            {
                ObjectiveGuide.Instance.SetObjective(nextObjectiveTitle, nextObjectiveTarget);
            }
        }

        private void SetDialogCameraActive(bool active)
        {
            if (dialogCamera == null)
            {
                return;
            }

            dialogCamera.Priority.Enabled = true;
            dialogCamera.Priority.Value = active ? DialogCameraActivePriority : DialogCameraInactivePriority;
        }
    }
}
