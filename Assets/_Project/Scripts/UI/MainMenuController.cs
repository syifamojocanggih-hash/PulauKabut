using PulauKabut.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PulauKabut.UI
{
    /// <summary>
    /// Layar Menu Utama Pulau Kabut bergaya chamfered teal-emas (HUD Lengkap)
    /// dengan kamera diorama 3D yang mengorbit halus di latar belakang.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Tujuan")]
        [SerializeField] private string firstLevelScene = "Zone_Mangrove";

        [Header("Kamera Orbit Diorama (Opsional)")]
        [SerializeField] private Transform orbitPivot;
        [SerializeField] private float orbitSpeed = 6.5f;

        private GameSaveData _cachedSave;
        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _infoStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _keycapStyle;
        private GUIStyle _btnLabelStyle;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _cachedSave = SaveSystem.Load();
        }

        private void Update()
        {
            if (orbitPivot != null)
            {
                orbitPivot.Rotate(0f, orbitSpeed * Time.deltaTime, 0f, Space.World);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            const float cardW = 390f;
            bool hasSave = _cachedSave != null;
            float cardH = hasSave ? 340f : 276f;
            Rect card = new Rect((Screen.width - cardW) * 0.5f, (Screen.height - cardH) * 0.5f, cardW, cardH);

            GUI.Box(card, GUIContent.none, _cardStyle);

            // Judul PULAU KABUT + subjudul
            GUI.Label(new Rect(card.x, card.y + 22f, card.width, 44f), "PULAU KABUT", _titleStyle);
            GUI.Label(new Rect(card.x, card.y + 64f, card.width, 24f), "Petualangan Lentera di Tengah Kabut", _subtitleStyle);

            // Garis pemisah emas
            Color prev = GUI.color;
            GUI.color = StylizedUITheme.GoldAccent;
            GUI.DrawTexture(new Rect(card.x + 36f, card.y + 98f, card.width - 72f, 2f), StylizedUITheme.SolidTex);
            GUI.color = prev;

            float y = card.y + 118f;
            float btnW = card.width - 72f;
            float btnX = card.x + 36f;
            const float btnH = 44f;

            if (hasSave)
            {
                int shards = _cachedSave.collectedShards != null ? _cachedSave.collectedShards.Count : 0;
                int stage = Mathf.Clamp(shards / 4, 0, 5);
                string saveSummary = $"Progres Tersimpan: {shards}/24 Pecahan  •  Desa Tahap {stage}/5";
                GUI.Label(new Rect(card.x + 20f, y - 6f, card.width - 40f, 22f), saveSummary, _infoStyle);
                y += 24f;

                if (StylizedUITheme.DrawKeyButton(new Rect(btnX, y, btnW, btnH), "▶", "Lanjutkan Petualangan", _btnStyle, _keycapStyle, _btnLabelStyle))
                {
                    string targetScene = string.IsNullOrEmpty(_cachedSave.lastSceneName) ? firstLevelScene : _cachedSave.lastSceneName;
                    SceneManager.LoadScene(targetScene);
                }
                y += btnH + 12f;
            }

            if (StylizedUITheme.DrawKeyButton(new Rect(btnX, y, btnW, btnH), "★", "Mulai Baru", _btnStyle, _keycapStyle, _btnLabelStyle))
            {
                SaveSystem.DeleteSave();
                SceneManager.LoadScene(firstLevelScene);
            }
            y += btnH + 12f;

            if (StylizedUITheme.DrawKeyButton(new Rect(btnX, y, btnW, btnH), "✕", "Keluar", _btnStyle, _keycapStyle, _btnLabelStyle))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void EnsureStyles()
        {
            if (_cardStyle != null) return;
            _cardStyle = StylizedUITheme.CreateGoldPanelStyle(14, TextAnchor.UpperCenter, 18, 16);
            _titleStyle = StylizedUITheme.CreateHeadingLabelStyle(32, StylizedUITheme.GoldAccent, TextAnchor.MiddleCenter);
            _subtitleStyle = StylizedUITheme.CreateBodyLabelStyle(14, StylizedUITheme.SeafoamText, TextAnchor.MiddleCenter);
            _infoStyle = StylizedUITheme.CreateBodyLabelStyle(12, StylizedUITheme.MintAccent, TextAnchor.MiddleCenter);
            _btnStyle = StylizedUITheme.CreatePillStyle(15, TextAnchor.MiddleCenter, 12, 6);
            _keycapStyle = StylizedUITheme.CreateKeycapStyle(14);
            _btnLabelStyle = StylizedUITheme.CreateBodyLabelStyle(15, StylizedUITheme.CreamText, TextAnchor.MiddleLeft);
        }
    }
}
