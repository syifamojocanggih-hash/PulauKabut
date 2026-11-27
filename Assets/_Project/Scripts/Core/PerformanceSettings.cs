using UnityEngine;

namespace PulauKabut.Core
{
    /// <summary>
    /// Mengunci target framerate default ke 60 FPS agar frame pacing mulus, suhu laptop terjaga,
    /// dan sisa headroom GPU bisa dipakai untuk kualitas visual yang lebih tajam.
    /// </summary>
    public static class PerformanceSettings
    {
        public const int DefaultTargetFps = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnBoot()
        {
            ApplyTargetFrameRate(DefaultTargetFps);
        }

        public static void ApplyTargetFrameRate(int targetFps)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
        }
    }
}
