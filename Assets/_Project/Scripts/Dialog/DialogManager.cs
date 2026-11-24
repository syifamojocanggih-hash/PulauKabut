using System;
using PulauKabut.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulauKabut.Dialog
{
    /// <summary>
    /// Pengelola tampilan dialog sederhana untuk percakapan NPC dan adegan kenangan.
    /// Menahan gerak pemain selama dialog berlangsung, tekan E atau Spasi untuk melanjutkan.
    /// </summary>
    public class DialogManager : MonoBehaviour
    {
        public static DialogManager Instance { get; private set; }

        /// <summary>True jika kotak dialog sedang terbuka.</summary>
        public bool IsOpen { get; private set; }

        private DialogData _currentData;
        private int _currentLineIndex;
        private PlayerController _activePlayer;
        private Action _onCompleteCallback;
        private bool _justOpenedThisFrame;

        private GUIStyle _panelStyle;
        private GUIStyle _speakerStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _footerStyle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsOpen || Time.timeScale <= 0f)
            {
                return;
            }

            // Abaikan tombol E pada frame yang sama saat dialog baru dibuka
            if (_justOpenedThisFrame)
            {
                _justOpenedThisFrame = false;
                return;
            }

            bool advancePressed = Keyboard.current != null &&
                (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);

            if (advancePressed)
            {
                NextLine();
            }
        }

        /// <summary>Membuka dialog dengan data tertentu dan mengunci gerak pemain sementara.</summary>
        public void StartDialog(DialogData data, PlayerController player = null, Action onComplete = null)
        {
            if (data == null || data.Lines == null || data.Lines.Length == 0)
            {
                return;
            }

            _currentData = data;
            _currentLineIndex = 0;
            _activePlayer = player;
            _onCompleteCallback = onComplete;
            _justOpenedThisFrame = true;
            IsOpen = true;

            if (_activePlayer != null)
            {
                _activePlayer.InputLocked = true;
            }
        }

        private void NextLine()
        {
            _currentLineIndex++;
            if (_currentData == null || _currentLineIndex >= _currentData.Lines.Length)
            {
                CloseDialog();
            }
        }

        private void CloseDialog()
        {
            IsOpen = false;
            _currentData = null;

            if (_activePlayer != null)
            {
                _activePlayer.InputLocked = false;
                _activePlayer = null;
            }

            Action callback = _onCompleteCallback;
            _onCompleteCallback = null;
            callback?.Invoke();
        }

        private void OnGUI()
        {
            if (!IsOpen || _currentData == null || Time.timeScale <= 0f)
            {
                return;
            }

            EnsureStyles();

            float w = Mathf.Min(720f, Screen.width - 80f);
            const float h = 155f;
            Rect panel = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 28f, w, h);
            GUI.Box(panel, GUIContent.none, _panelStyle);

            Rect speakerRect = new Rect(panel.x + 20f, panel.y + 14f, panel.width - 40f, 28f);
            GUI.Label(speakerRect, _currentData.SpeakerName, _speakerStyle);

            string lineText = _currentData.Lines[_currentLineIndex];
            Rect bodyRect = new Rect(panel.x + 20f, panel.y + 46f, panel.width - 40f, 74f);
            GUI.Label(bodyRect, lineText, _bodyStyle);

            string footer = _currentLineIndex < _currentData.Lines.Length - 1
                ? "[E / Spasi] Lanjut  >"
                : "[E / Spasi] Selesai";
            Rect footerRect = new Rect(panel.x + 20f, panel.y + panel.height - 30f, panel.width - 40f, 22f);
            GUI.Label(footerRect, footer, _footerStyle);
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null) return;

            _panelStyle = new GUIStyle(GUI.skin.box);

            _speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };
            _speakerStyle.normal.textColor = new Color(1f, 0.85f, 0.42f);

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = true
            };
            _bodyStyle.normal.textColor = Color.white;

            _footerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleRight
            };
            _footerStyle.normal.textColor = new Color(0.82f, 0.90f, 0.96f);
        }
    }
}
