using PulauKabut.Player;
using PulauKabut.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.Core
{
    /// <summary>
    /// Tuas jembatan rahasia (implementasi IInteractable):
    /// Saat diinteraksi [E], pemain harus menekan urutan tombol (E -> W -> B -> S -> A)
    /// sebelum waktu habis, disertai getaran kamera & zoom tergesa-gesa.
    /// Jika waktu habis, tarikan gagal dan pemain harus mengulang dari awal.
    /// </summary>
    public class ShortcutBridge : MonoBehaviour, IInteractable
    {
        [Header("Referensi Jembatan")]
        [Tooltip("Pivot jembatan yang akan memanjang saat diaktifkan.")]
        [SerializeField] private Transform bridgePivot;

        [Tooltip("Collider pijakan jembatan (aktif setelah jembatan terbuka penuh).")]
        [SerializeField] private Collider bridgeWalkCollider;

        [Header("Urutan Aksi Tombol (QTE)")]
        [Tooltip("Jika true, pemain harus menekan urutan tombol untuk menarik tuas jembatan.")]
        [SerializeField] private bool requireKeySequence = true;
        [SerializeField] private Key[] keySequence = new Key[] { Key.E, Key.W, Key.B, Key.S, Key.A };

        [Tooltip("Batas waktu (detik) menyelesaikan urutan tombol sebelum gagal.")]
        [Min(2f)]
        [SerializeField] private float qteTimeLimit = 6.5f;

        [Tooltip("Penalti waktu (detik) jika menekan tombol yang salah.")]
        [Min(0f)]
        [SerializeField] private float wrongKeyPenalty = 0.7f;

        [Header("Efek Kamera Tergesa-gesa")]
        [SerializeField] private float maxShakeAmplitude = 0.14f;
        [SerializeField] private float shakeFrequency = 28f;
        [SerializeField] private float urgencyFovZoom = 5.5f;

        [Header("Animasi & Notifikasi")]
        [SerializeField] private string promptText = "Tarik Tuas Jembatan Pulau Rahasia";
        [SerializeField] private string unlockTagText = "LOKASI RAHASIA";
        [SerializeField] private string unlockBannerMessage = "NEW SECRET LOCATION UNLOCKED";
        [SerializeField] private string unlockSubtitle = "Jembatan menuju Pulau Rahasia telah terbuka!";
        [SerializeField] private float extendDuration = 1.15f;

        private bool _isActivated;
        private bool _isAnimating;
        private bool _isPullingSequence;
        private bool _justStartedThisFrame;
        private int _stepIndex;
        private float _timeRemaining;
        private float _wrongKeyTimer;
        private float _failBannerTimer;
        private float _extendProgress;
        private Vector3 _fullLocalScale = Vector3.one;
        private PlayerController _activePlayer;

        private Camera _mainCam;
        private float _originalFov = 60f;
        private bool _hasSavedFov;
        private Vector3 _lastShakeOffset;

        private GUIStyle _qtePanelStyle;
        private GUIStyle _qteTitleStyle;
        private GUIStyle _qteSubStyle;
        private GUIStyle _qteFailStyle;
        private GUIStyle _qteKeyNormalStyle;
        private GUIStyle _qteKeyDoneStyle;
        private GUIStyle _qteKeyActiveStyle;

        public string PromptText => promptText;
        public bool CanInteract => !_isActivated && !_isPullingSequence;

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

        private void OnDisable()
        {
            RestoreCameraImmediate();
        }

        public void Interact(GameObject interactor)
        {
            if (_isActivated || _isPullingSequence)
            {
                return;
            }

            if (!requireKeySequence || keySequence == null || keySequence.Length == 0)
            {
                ActivateBridge();
                return;
            }

            _isPullingSequence = true;
            _justStartedThisFrame = true;
            _stepIndex = 0;
            _timeRemaining = qteTimeLimit;
            _wrongKeyTimer = 0f;
            _failBannerTimer = 0f;

            _mainCam = Camera.main;
            if (_mainCam != null && !_hasSavedFov)
            {
                _originalFov = _mainCam.fieldOfView;
                _hasSavedFov = true;
            }

            _activePlayer = interactor != null ? interactor.GetComponent<PlayerController>() : null;
            if (_activePlayer != null)
            {
                _activePlayer.InputLocked = true;
            }
        }

        private void Update()
        {
            if (_failBannerTimer > 0f)
            {
                _failBannerTimer = Mathf.Max(0f, _failBannerTimer - Time.unscaledDeltaTime);
            }

            if (_isPullingSequence)
            {
                HandleSequenceInput();
            }

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

        private void LateUpdate()
        {
            if (_mainCam == null) return;

            if (_isPullingSequence)
            {
                // Hitung tingkat kepanikan (0 di awal -> 1 saat hampir habis waktu)
                float urgency = 1f - Mathf.Clamp01(_timeRemaining / Mathf.Max(0.5f, qteTimeLimit));
                float amp = Mathf.Lerp(maxShakeAmplitude * 0.28f, maxShakeAmplitude, urgency);
                if (_wrongKeyTimer > 0f) amp += maxShakeAmplitude * 0.65f;

                float t = Time.unscaledTime * shakeFrequency;
                float sx = (Mathf.PerlinNoise(t, 1.7f) - 0.5f) * 2f * amp;
                float sy = (Mathf.PerlinNoise(3.1f, t) - 0.5f) * 2f * amp;

                // Terapkan offset getaran relatif terhadap posisi kamera frame ini
                _lastShakeOffset = _mainCam.transform.right * sx + _mainCam.transform.up * sy;
                _mainCam.transform.position += _lastShakeOffset;

                if (_hasSavedFov)
                {
                    float pulse = Mathf.Sin(Time.unscaledTime * (7f + urgency * 10f)) * (0.4f + urgency * 0.9f);
                    float targetFov = _originalFov - (urgency * urgencyFovZoom) + pulse;
                    _mainCam.fieldOfView = Mathf.Lerp(_mainCam.fieldOfView, targetFov, Time.unscaledDeltaTime * 10f);
                }
            }
            else if (_hasSavedFov)
            {
                _mainCam.fieldOfView = Mathf.Lerp(_mainCam.fieldOfView, _originalFov, Time.unscaledDeltaTime * 8f);
                if (Mathf.Abs(_mainCam.fieldOfView - _originalFov) < 0.05f)
                {
                    _mainCam.fieldOfView = _originalFov;
                    _hasSavedFov = false;
                }
            }
        }

        private void HandleSequenceInput()
        {
            if (_wrongKeyTimer > 0f)
            {
                _wrongKeyTimer = Mathf.Max(0f, _wrongKeyTimer - Time.unscaledDeltaTime);
            }

            _timeRemaining -= Time.unscaledDeltaTime;
            if (_timeRemaining <= 0f)
            {
                FailSequence();
                return;
            }

            if (_justStartedThisFrame)
            {
                _justStartedThisFrame = false;
                return;
            }

            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                CancelSequence();
                return;
            }

            Key expected = keySequence[_stepIndex];
            if (kb[expected].wasPressedThisFrame)
            {
                _wrongKeyTimer = 0f;
                GameAudioManager.Instance?.PlayUiClick();
                _stepIndex++;

                if (_stepIndex >= keySequence.Length)
                {
                    FinishSequence();
                }
                return;
            }

            if (kb.anyKey.wasPressedThisFrame)
            {
                _wrongKeyTimer = 0.65f;
                _timeRemaining = Mathf.Max(0.15f, _timeRemaining - wrongKeyPenalty);
            }
        }

        private void FailSequence()
        {
            _isPullingSequence = false;
            _failBannerTimer = 2.4f;
            if (_activePlayer != null)
            {
                _activePlayer.InputLocked = false;
                _activePlayer = null;
            }
        }

        private void CancelSequence()
        {
            _isPullingSequence = false;
            if (_activePlayer != null)
            {
                _activePlayer.InputLocked = false;
                _activePlayer = null;
            }
        }

        private void FinishSequence()
        {
            _isPullingSequence = false;
            if (_activePlayer != null)
            {
                _activePlayer.InputLocked = false;
                _activePlayer = null;
            }
            ActivateBridge();
        }

        private void RestoreCameraImmediate()
        {
            if (_mainCam != null && _hasSavedFov)
            {
                _mainCam.fieldOfView = _originalFov;
                _hasSavedFov = false;
            }
        }

        private void ActivateBridge()
        {
            _isActivated = true;
            _isAnimating = true;
            _extendProgress = 0f;

            if (bridgePivot != null)
            {
                bridgePivot.gameObject.SetActive(true);
            }

            GameAudioManager.Instance?.PlayBrazierLit();
            ObjectiveGuide.Instance?.ShowUnlockNotification(
                unlockBannerMessage,
                unlockSubtitle,
                icon: null,
                duration: 0f,
                tagText: unlockTagText);
        }

        private void OnGUI()
        {
            if (Time.timeScale <= 0f) return;

            if (_failBannerTimer > 0f)
            {
                EnsureQteStyles();
                float alpha = _failBannerTimer < 0.5f ? Mathf.Clamp01(_failBannerTimer / 0.5f) : 1f;
                Color prev = GUI.color;
                GUI.color = new Color(1f, 0.45f, 0.40f, alpha);
                Rect failRect = new Rect((Screen.width - 390f) * 0.5f, Screen.height - 130f, 390f, 44f);
                GUI.Box(failRect, "GAGAL! Terlalu lambat menarik tuas, coba lagi!", _qteFailStyle);
                GUI.color = prev;
            }

            if (!_isPullingSequence || keySequence == null || keySequence.Length == 0)
            {
                return;
            }

            EnsureQteStyles();

            int count = keySequence.Length;
            const float keySize = 44f;
            const float gap = 14f;
            float rowW = count * keySize + (count - 1) * gap;
            float panelW = Mathf.Max(360f, rowW + 64f);
            const float panelH = 144f;

            // Sedikit getaran visual juga pada panel QTE saat waktu hampir habis
            float urgency = 1f - Mathf.Clamp01(_timeRemaining / Mathf.Max(0.5f, qteTimeLimit));
            float uiJitterX = urgency > 0.45f ? Mathf.Sin(Time.unscaledTime * 48f) * (urgency * 4f) : 0f;
            Rect panel = new Rect((Screen.width - panelW) * 0.5f + uiJitterX, Screen.height - panelH - 52f, panelW, panelH);
            GUI.Box(panel, GUIContent.none, _qtePanelStyle);

            string titleText = urgency > 0.55f ? "CEPAT! TARIK TUAS JEMBATAN!" : "TARIK TUAS JEMBATAN RAHASIA";
            GUI.Label(new Rect(panel.x, panel.y + 10f, panel.width, 24f), titleText, _qteTitleStyle);

            // Progress bar waktu mundur (Emas -> Oranye -> Merah)
            float barW = panel.width - 56f;
            Rect barBg = new Rect(panel.x + 28f, panel.y + 37f, barW, 8f);
            Color prevCol = GUI.color;
            GUI.color = new Color(0.08f, 0.16f, 0.18f, 0.85f);
            GUI.DrawTexture(barBg, StylizedUITheme.SolidTex);

            float fillRatio = Mathf.Clamp01(_timeRemaining / Mathf.Max(0.1f, qteTimeLimit));
            GUI.color = fillRatio > 0.5f
                ? StylizedUITheme.GoldAccent
                : Color.Lerp(new Color(1f, 0.25f, 0.22f, 1f), new Color(1f, 0.62f, 0.20f, 1f), fillRatio * 2f);
            GUI.DrawTexture(new Rect(barBg.x, barBg.y, barW * fillRatio, barBg.height), StylizedUITheme.SolidTex);

            float startX = panel.x + (panel.width - rowW) * 0.5f;
            float keyY = panel.y + 54f;

            for (int i = 0; i < count; i++)
            {
                Rect kr = new Rect(startX + i * (keySize + gap), keyY, keySize, keySize);
                string keyLabel = keySequence[i].ToString().ToUpperInvariant();

                if (i < _stepIndex)
                {
                    GUI.color = StylizedUITheme.MintAccent;
                    GUI.Box(kr, keyLabel, _qteKeyDoneStyle);
                }
                else if (i == _stepIndex)
                {
                    float pulse = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 10f);
                    GUI.color = _wrongKeyTimer > 0f
                        ? new Color(1f, 0.35f, 0.30f, 1f)
                        : new Color(1f, 0.92f * pulse, 0.50f, 1f);
                    GUI.Box(kr, keyLabel, _qteKeyActiveStyle);
                }
                else
                {
                    GUI.color = new Color(0.75f, 0.85f, 0.85f, 0.48f);
                    GUI.Box(kr, keyLabel, _qteKeyNormalStyle);
                }
            }

            GUI.color = prevCol;

            string expectedLabel = keySequence[_stepIndex].ToString().ToUpperInvariant();
            string sub = _wrongKeyTimer > 0f
                ? $"Salah tombol! (-{wrongKeyPenalty:0.0}s) Tekan [{expectedLabel}]"
                : $"Tekan [{expectedLabel}] cepat!  ·  {_timeRemaining:0.0} dtk  ·  [Esc] Batal";
            GUI.Label(new Rect(panel.x + 12f, panel.yMax - 30f, panel.width - 24f, 22f), sub, _qteSubStyle);
        }

        private void EnsureQteStyles()
        {
            if (_qtePanelStyle != null) return;
            _qtePanelStyle = StylizedUITheme.CreateGoldPanelStyle(14, TextAnchor.UpperCenter, 16, 12);
            _qteTitleStyle = StylizedUITheme.CreateHeadingLabelStyle(15, StylizedUITheme.GoldAccent, TextAnchor.MiddleCenter);
            _qteSubStyle = StylizedUITheme.CreateBodyLabelStyle(13, StylizedUITheme.CreamText, TextAnchor.MiddleCenter);
            _qteFailStyle = StylizedUITheme.CreatePillStyle(14, TextAnchor.MiddleCenter, 16, 8);
            _qteFailStyle.normal.textColor = new Color(1f, 0.72f, 0.68f, 1f);
            _qteKeyNormalStyle = StylizedUITheme.CreateKeycapStyle(17);
            _qteKeyActiveStyle = StylizedUITheme.CreateKeycapStyle(19);
            _qteKeyDoneStyle = StylizedUITheme.CreateKeycapStyle(17);
        }
    }
}
