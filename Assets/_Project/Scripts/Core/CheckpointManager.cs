using System;
using PulauKabut.Player;
using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Menyimpan titik Checkpoint terakhir dan mengembalikan pemain dengan aman
    /// jika jatuh ke jurang atau air dalam.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [Header("Notifikasi Checkpoint")]
        [SerializeField] private float toastDuration = 2.2f;

        private Vector3 _checkpointPos;
        private Quaternion _checkpointRot = Quaternion.identity;
        private bool _hasCheckpoint;
        private string _lastCheckpointName = "Dermaga";
        private float _toastTimer;
        private GUIStyle _toastStyle;

        public bool HasCheckpoint => _hasCheckpoint;
        public Vector3 CheckpointPosition => _checkpointPos;
        public Quaternion CheckpointRotation => _checkpointRot;
        public string LastCheckpointName => _lastCheckpointName;

        public event Action<Vector3, Quaternion, string> OnCheckpointUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Jadikan posisi awal pemain sebagai checkpoint pertama secara otomatis
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null && !_hasCheckpoint)
            {
                _checkpointPos = player.transform.position;
                _checkpointRot = player.transform.rotation;
                _hasCheckpoint = true;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
            }
        }

        /// <summary>Menyimpan lokasi checkpoint baru dan menampilkan pesan singkat di layar.</summary>
        public void SetCheckpoint(Vector3 position, Quaternion rotation, string checkpointName)
        {
            _checkpointPos = position;
            _checkpointRot = rotation;
            _hasCheckpoint = true;
            _lastCheckpointName = string.IsNullOrEmpty(checkpointName) ? "Dermaga" : checkpointName;
            _toastTimer = toastDuration;
            OnCheckpointUpdated?.Invoke(_checkpointPos, _checkpointRot, _lastCheckpointName);
        }

        /// <summary>Memulihkan checkpoint dari data tersimpan tanpa memicu toast ulang.</summary>
        public void RestoreCheckpoint(Vector3 position, Quaternion rotation, string checkpointName)
        {
            _checkpointPos = position;
            _checkpointRot = rotation;
            _hasCheckpoint = true;
            if (!string.IsNullOrEmpty(checkpointName))
            {
                _lastCheckpointName = checkpointName;
            }
        }

        /// <summary>Memindahkan pemain kembali ke titik checkpoint terakhir dengan aman.</summary>
        public void RespawnPlayer(PlayerController player)
        {
            if (!_hasCheckpoint || player == null)
            {
                return;
            }

            // Matikan CharacterController sesaat saat memindah posisi agar tidak tertahan collider
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.SetPositionAndRotation(_checkpointPos + Vector3.up * 0.25f, _checkpointRot);

            if (cc != null) cc.enabled = true;
        }

        private void OnGUI()
        {
            if (_toastTimer <= 0f || Time.timeScale <= 0f)
            {
                return;
            }

            if (_toastStyle == null)
            {
                _toastStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(14, 14, 6, 6)
                };
                _toastStyle.normal.textColor = new Color(0.75f, 1f, 0.82f);
            }

            const float w = 310f;
            const float h = 36f;
            Rect rect = new Rect((Screen.width - w) * 0.5f, 62f, w, h);
            string label = string.IsNullOrEmpty(_lastCheckpointName)
                ? "Checkpoint Tersimpan"
                : $"Checkpoint: {_lastCheckpointName}";
            GUI.Box(rect, label, _toastStyle);
        }
    }
}
