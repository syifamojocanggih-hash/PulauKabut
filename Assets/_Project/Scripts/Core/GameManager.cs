using System;
using PulauKabut.Player;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Status utama jalannya permainan.
    /// </summary>
    public enum GameState
    {
        Playing,
        Paused
    }

    /// <summary>
    /// Pengelola state permainan sederhana (Playing & Paused):
    /// menangani tombol Esc untuk jeda/lanjut, Time.timeScale, dan visibilitas kursor.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>Satu instance aktif di scene saat ini.</summary>
        public static GameManager Instance { get; private set; }

        /// <summary>Status permainan saat ini.</summary>
        public GameState CurrentState { get; private set; } = GameState.Playing;

        /// <summary>True jika game sedang dalam kondisi jeda.</summary>
        public bool IsPaused => CurrentState == GameState.Paused;

        /// <summary>Dipicu setiap kali state berubah (untuk UI pause di tahap berikutnya).</summary>
        public event Action<GameState> OnStateChanged;

        private PlayerInputActions _input;

        private void Awake()
        {
            // Cegah duplikat GameManager di scene yang sama
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[{nameof(GameManager)}] Duplikat GameManager terdeteksi pada '{name}', objek ini dihapus.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _input = new PlayerInputActions();
        }

        private void Start()
        {
            // Mulai permainan dalam keadaan Playing dengan kursor terkunci
            SetState(GameState.Playing);
        }

        private void OnEnable()
        {
            _input?.Player.Enable();
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
        }

        private void OnDestroy()
        {
            // Pastikan Time.timeScale kembali normal saat keluar dari Play Mode / ganti scene
            Time.timeScale = 1f;

            if (Instance == this)
            {
                Instance = null;
            }

            _input?.Dispose();
        }

        private void Update()
        {
            // Baca tombol Pause (Esc / Start) langsung tanpa alokasi memori
            if (_input.Player.Pause.WasPressedThisFrame())
            {
                TogglePause();
            }
        }

        /// <summary>Berpindah antara Playing dan Paused.</summary>
        public void TogglePause()
        {
            SetState(IsPaused ? GameState.Playing : GameState.Paused);
        }

        /// <summary>Mengatur state permainan, Time.timeScale, dan kunci kursor secara konsisten.</summary>
        public void SetState(GameState newState)
        {
            CurrentState = newState;

            bool paused = CurrentState == GameState.Paused;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;

            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
