using System;
using System.Collections;
using UnityEngine;

namespace PulauKabut.UI
{
    /// <summary>
    /// Efek transisi Fade Out / Fade In layar yang ringan (tanpa overhead saat diam).
    /// Dipakai saat pindah kamera obrolan NPC, respawn checkpoint, maupun perpindahan wilayah.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [Tooltip("Warna layar saat fade (default hitam lembut).")]
        [SerializeField] private Color fadeColor = Color.black;

        private Texture2D _pixelTexture;
        private float _alpha;
        private bool _isFading;

        /// <summary>True jika proses fade sedang berlangsung.</summary>
        public bool IsFading => _isFading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _pixelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _pixelTexture.SetPixel(0, 0, Color.white);
            _pixelTexture.Apply();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_pixelTexture != null) Destroy(_pixelTexture);
        }

        /// <summary>
        /// Melakukan Fade Out -> jalankan aksi di saat gelap penuh -> Fade In kembali.
        /// </summary>
        public void FadeTransition(Action onDarkMidpoint, float fadeOutDuration = 0.25f, float fadeInDuration = 0.28f)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(onDarkMidpoint, fadeOutDuration, fadeInDuration));
        }

        private IEnumerator FadeRoutine(Action onDarkMidpoint, float fadeOutDuration, float fadeInDuration)
        {
            _isFading = true;

            // Fade Out (terang -> gelap)
            yield return AnimateAlpha(1f, fadeOutDuration);

            // Jalankan aksi perpindahan kamera/dialog tepat saat layar gelap penuh
            onDarkMidpoint?.Invoke();

            // Tahan sejenak 1 frame agar Cinemachine memposisikan kamera baru
            yield return null;

            // Fade In (gelap -> terang)
            yield return AnimateAlpha(0f, fadeInDuration);

            _isFading = false;
        }

        private IEnumerator AnimateAlpha(float targetAlpha, float duration)
        {
            if (duration <= 0.001f)
            {
                _alpha = targetAlpha;
                yield break;
            }

            float startAlpha = _alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _alpha = targetAlpha;
        }

        private void OnGUI()
        {
            if (_alpha <= 0.001f || _pixelTexture == null)
            {
                return;
            }

            // Pastikan fade selalu tampil di paling depan di atas elemen GUI lain
            GUI.depth = -1000;
            Color prevColor = GUI.color;
            Color c = fadeColor;
            c.a = _alpha;
            GUI.color = c;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _pixelTexture);
            GUI.color = prevColor;
        }
    }
}
