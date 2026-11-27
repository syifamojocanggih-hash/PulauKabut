using PulauKabut.UI;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Tuas / tiang jembatan pintas pada Pulau Rahasia (implementasi IInteractable).
    /// Saat diinteraksi [E] dari sisi pulau rahasia, jembatan kayu turun menghubungkan
    /// kembali ke jalur utama sebagai jalan pintas permanen.
    /// </summary>
    public class ShortcutBridge : MonoBehaviour, IInteractable
    {
        [Header("Referensi Jembatan")]
        [Tooltip("Pivot jembatan yang akan berputar turun saat diaktifkan.")]
        [SerializeField] private Transform bridgePivot;

        [Tooltip("Collider pijakan jembatan (aktif setelah jembatan turun).")]
        [SerializeField] private Collider bridgeWalkCollider;

        [Header("Animasi & Teks")]
        [SerializeField] private string promptText = "Turunkan Jembatan Pintas";
        [SerializeField] private string unlockBannerMessage = "Jalur Pintas Terbuka!";
        [SerializeField] private Vector3 raisedLocalEuler = new Vector3(-78f, 0f, 0f);
        [SerializeField] private Vector3 loweredLocalEuler = Vector3.zero;
        [Min(10f)]
        [SerializeField] private float lowerSpeed = 95f;

        private bool _isActivated;
        private bool _isAnimating;
        private Quaternion _targetRotation;

        public string PromptText => promptText;
        public bool CanInteract => !_isActivated;

        private void Awake()
        {
            if (bridgePivot != null)
            {
                bridgePivot.localRotation = Quaternion.Euler(raisedLocalEuler);
            }

            if (bridgeWalkCollider != null)
            {
                bridgeWalkCollider.enabled = false;
            }

            _targetRotation = Quaternion.Euler(loweredLocalEuler);
        }

        public void Interact(GameObject interactor)
        {
            if (_isActivated)
            {
                return;
            }

            _isActivated = true;
            _isAnimating = true;

            GameAudioManager.Instance?.PlayBrazierLit();
            ObjectiveGuide.Instance?.ShowUnlockNotification(unlockBannerMessage);
        }

        private void Update()
        {
            if (!_isAnimating || bridgePivot == null)
            {
                return;
            }

            bridgePivot.localRotation = Quaternion.RotateTowards(
                bridgePivot.localRotation,
                _targetRotation,
                lowerSpeed * Time.deltaTime);

            if (Quaternion.Angle(bridgePivot.localRotation, _targetRotation) <= 0.25f)
            {
                bridgePivot.localRotation = _targetRotation;
                _isAnimating = false;

                if (bridgeWalkCollider != null)
                {
                    bridgeWalkCollider.enabled = true;
                }
            }
        }
    }
}
