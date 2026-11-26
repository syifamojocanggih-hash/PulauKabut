using PulauKabut.Lantern;
using UnityEngine;
using UnityEngine.Audio;

namespace PulauKabut.Core
{
    /// <summary>
    /// Pengelola Audio Dasar Pulau Kabut (Ambience rawa/pantai, SFX Lentera, Pecahan Cahaya, & Tungku):
    /// mendukung AudioMixerGroup opsional serta lapisan volume statis (MusicVol / SFXVol / AmbientVol)
    /// tanpa alokasi memori di Update.
    /// </summary>
    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager Instance { get; private set; }

        [Header("Mixer Opsional (Jika Dibuat di Editor)")]
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;

        [Header("Klip Audio")]
        [SerializeField] private AudioClip ambientLoopClip;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip milestoneClip;
        [SerializeField] private AudioClip lanternOnClip;
        [SerializeField] private AudioClip lanternOffClip;
        [SerializeField] private AudioClip brazierLitClip;
        [SerializeField] private AudioClip uiClickClip;

        [Header("Pengaturan Volume (0..1)")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.75f;
        [Range(0f, 1f)] [SerializeField] private float ambientVolume = 0.35f;

        private AudioSource _ambientSource;
        private AudioSource _sfxSource;
        private int _lastHubStage = -1;

        public float SfxVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        public float AmbientVolume
        {
            get => ambientVolume;
            set
            {
                ambientVolume = Mathf.Clamp01(value);
                if (_ambientSource != null) _ambientSource.volume = ambientVolume;
            }
        }

        public AudioSource AmbientSource => _ambientSource;
        public AudioSource SfxSource => _sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.spatialBlend = 0f;
            _ambientSource.volume = ambientVolume;
            if (ambientMixerGroup != null) _ambientSource.outputAudioMixerGroup = ambientMixerGroup;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.volume = sfxVolume;
            if (sfxMixerGroup != null) _sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        private void Start()
        {
            if (ambientLoopClip != null)
            {
                _ambientSource.clip = ambientLoopClip;
                _ambientSource.Play();
            }

            if (ShardManager.Instance != null)
            {
                _lastHubStage = ShardManager.Instance.CurrentHubStage;
                ShardManager.Instance.OnShardCollected += HandleShardCollected;
                ShardManager.Instance.OnMemoryFragmentCollected += HandleMemoryCollected;
            }

            if (LanternController.ActiveLantern != null)
            {
                LanternController.ActiveLantern.OnLanternToggled += HandleLanternToggled;
            }
        }

        private void OnDestroy()
        {
            if (ShardManager.Instance != null)
            {
                ShardManager.Instance.OnShardCollected -= HandleShardCollected;
                ShardManager.Instance.OnMemoryFragmentCollected -= HandleMemoryCollected;
            }

            if (LanternController.ActiveLantern != null)
            {
                LanternController.ActiveLantern.OnLanternToggled -= HandleLanternToggled;
            }

            if (Instance == this) Instance = null;
        }

        private void HandleShardCollected(int collected, int total, string shardId)
        {
            if (shardId == "RESTORED") return;

            int newStage = ShardManager.Instance != null ? ShardManager.Instance.CurrentHubStage : _lastHubStage;
            if (_lastHubStage >= 0 && newStage > _lastHubStage && milestoneClip != null)
            {
                PlayOneShotVaried(milestoneClip, 0.9f, 1.0f);
            }
            else
            {
                PlayPickup();
            }
            _lastHubStage = newStage;
        }

        private void HandleMemoryCollected(int collected, int total, string fragmentId)
        {
            PlayPickup();
        }

        private void HandleLanternToggled(bool isLit)
        {
            AudioClip clip = isLit ? lanternOnClip : lanternOffClip;
            if (clip != null)
            {
                PlayOneShotVaried(clip, 0.85f, isLit ? 1.03f : 0.96f);
            }
        }

        public void PlayPickup()
        {
            if (pickupClip != null)
            {
                PlayOneShotVaried(pickupClip, 0.85f, Random.Range(0.93f, 1.08f));
            }
        }

        public void PlayBrazierLit()
        {
            if (brazierLitClip != null)
            {
                PlayOneShotVaried(brazierLitClip, 0.95f, Random.Range(0.96f, 1.04f));
            }
        }

        public void PlayUiClick()
        {
            if (uiClickClip != null)
            {
                PlayOneShotVaried(uiClickClip, 0.65f, 1.0f);
            }
        }

        private void PlayOneShotVaried(AudioClip clip, float gainScale, float pitch)
        {
            if (_sfxSource == null || clip == null || sfxVolume <= 0.001f) return;
            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip, sfxVolume * gainScale);
        }
    }
}
