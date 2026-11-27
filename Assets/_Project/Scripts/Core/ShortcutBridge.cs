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
        [SerializeField] private float extendDuration = 1.15f;

        private bool _isActivated;
        private bool _isAnimating;
        private float _extendProgress;
        private Vector3 _fullLocalScale = Vector3.one;

        public string PromptText => promptText;
        public bool CanInteract => !_isActivated;

        private void Awake()
        {
            if (bridgePivot != null)
            {
                _fullLocalScale = bridgePivot.localScale;
                bridgePivot.localRotation = Quaternion.identity;
                bridgePivot.localScale = new Vector3(_fullLocalScale.x, _fullLocalScale.y, 0.001f);
                bridgePivot.gameObject.SetActive(false);
            }

            if (bridgeWalkCollider != null)
            {
                bridgeWalkCollider.enabled = false;
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_isActivated)
            {
                return;
            }

            _isActivated = true;
            _isAnimating = true;
            _extendProgress = 0f;

            if (bridgePivot != null)
            {
                bridgePivot.gameObject.SetActive(true);
            }

            GameAudioManager.Instance?.PlayBrazierLit();
            ObjectiveGuide.Instance?.ShowUnlockNotification(unlockBannerMessage);
        }

        private void Update()
        {
            if (!_isAnimating || bridgePivot == null)
            {
                return;
            }

            _extendProgress = Mathf.Clamp01(_extendProgress + Time.deltaTime / Mathf.Max(0.2f, extendDuration));
            float smooth = Mathf.SmoothStep(0.02f, 1f, _extendProgress);
            bridgePivot.localScale = new Vector3(_fullLocalScale.x, _fullLocalScale.y, _fullLocalScale.z * smooth);

            if (_extendProgress >= 1f)
            {
                bridgePivot.localScale = _fullLocalScale;
                _isAnimating = false;

                if (bridgeWalkCollider != null)
                {
                    bridgeWalkCollider.enabled = true;
                }
            }
        }
    }
}
