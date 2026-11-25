using System;
using PulauKabut.Player;
using PulauKabut.UI;
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
        private GUIStyle _nameplateStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _bodyShadowStyle;
        private GUIStyle _keycapStyle;
        private GUIStyle _footerLabelStyle;
        private GUIStyle _arrowStyle;

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

            // Panel Dialog Chamfered Deep-Teal + Bingkai Emas (gaya HUD Lengkap)
            float w = Mathf.Min(780f, Screen.width - 64f);
            const float h = 162f;
            Rect panel = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 24f, w, h);
            GUI.Box(panel, GUIContent.none, _panelStyle);

            // Nameplate Speaker melayang di tepi kiri atas kotak dialog
            string speaker = _currentData.SpeakerName;
            Vector2 spSize = _nameplateStyle.CalcSize(new GUIContent(speaker));
            float npW = Mathf.Max(130f, spSize.x + 28f);
            Rect nameplateRect = new Rect(panel.x + 22f, panel.y - 18f, npW, 34f);
            GUI.Box(nameplateRect, speaker, _nameplateStyle);

            // Isi teks percakapan (krem hangat + drop shadow tipis)
            string lineText = _currentData.Lines[_currentLineIndex];
            Rect bodyRect = new Rect(panel.x + 28f, panel.y + 30f, panel.width - 56f, 82f);
            GUI.Label(new Rect(bodyRect.x + 1f, bodyRect.y + 2f, bodyRect.width, bodyRect.height), lineText, _bodyShadowStyle);
            GUI.Label(bodyRect, lineText, _bodyStyle);

            // 4 ikon berlian mint berkedip lembut di kiri bawah (persis Hud.dc.html!)
            Color prevCol = GUI.color;
            for (int i = 0; i < 4; i++)
            {
                float a = 0.45f + 0.55f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.6f - i * 0.55f));
                GUI.color = new Color(StylizedUITheme.MintAccent.r, StylizedUITheme.MintAccent.g, StylizedUITheme.MintAccent.b, a);
                GUI.DrawTexture(new Rect(panel.x + 28f + i * 22f, panel.yMax - 36f, 14f, 18f), StylizedUITheme.DiamondTex);
            }
            GUI.color = prevCol;

            bool isLast = _currentLineIndex >= _currentData.Lines.Length - 1;
            string actionLabel = isLast ? "Selesai" : "Lanjut";
            float rightEdge = panel.xMax - 24f;
            float btnY = panel.yMax - 42f;

            // Panah mint memantul halus di ujung kanan
            float bounce = Mathf.Sin(Time.unscaledTime * 5.5f) * 3f;
            Rect arrowRect = new Rect(rightEdge - 16f + bounce, btnY + 3f, 18f, 24f);
            GUI.Label(arrowRect, "►", _arrowStyle);

            Rect actRect = new Rect(rightEdge - 82f, btnY + 2f, 64f, 26f);
            GUI.Label(actRect, actionLabel, _footerLabelStyle);

            Rect spaceRect = new Rect(actRect.x - 66f, btnY + 2f, 58f, 25f);
            GUI.Box(spaceRect, "SPASI", _keycapStyle);

            Rect orRect = new Rect(spaceRect.x - 38f, btnY + 2f, 36f, 25f);
            GUI.Label(orRect, "atau", _footerLabelStyle);

            Rect eRect = new Rect(orRect.x - 32f, btnY + 2f, 28f, 25f);
            GUI.Box(eRect, "E", _keycapStyle);
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null) return;

            _panelStyle = StylizedUITheme.CreateGoldPanelStyle(14, TextAnchor.UpperLeft, 18, 14);
            _nameplateStyle = StylizedUITheme.CreateKeycapStyle(16);

            _bodyStyle = StylizedUITheme.CreateBodyLabelStyle(18, StylizedUITheme.CreamText, TextAnchor.UpperLeft, wordWrap: true);
            _bodyShadowStyle = StylizedUITheme.CreateBodyLabelStyle(18, new Color(0.03f, 0.08f, 0.10f, 0.85f), TextAnchor.UpperLeft, wordWrap: true);

            _keycapStyle = StylizedUITheme.CreateKeycapStyle(13);
            _footerLabelStyle = StylizedUITheme.CreateBodyLabelStyle(14, StylizedUITheme.SeafoamText, TextAnchor.MiddleCenter);
            _arrowStyle = StylizedUITheme.CreateHeadingLabelStyle(15, StylizedUITheme.MintAccent, TextAnchor.MiddleCenter);
        }
    }
}
