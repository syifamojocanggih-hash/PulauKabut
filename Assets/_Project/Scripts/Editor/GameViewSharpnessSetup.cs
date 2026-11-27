using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PulauKabut.Editor
{
    /// <summary>
    /// Memastikan opsi "Low Resolution Aspect Ratios" di Game View selalu nonaktif
    /// pada layar Windows dengan Display Scaling (mis. 125%/150%) agar font UI dan 3D
    /// selalu dirender tajam di resolusi penuh, bukan di-upscale buram.
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewSharpnessSetup
    {
        private static readonly Type GameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
        private static readonly PropertyInfo LowResProp = GameViewType?.GetProperty(
            "lowResolutionForAspectRatios",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        static GameViewSharpnessSetup()
        {
            EditorApplication.delayCall += EnforceSharpGameViews;
            EditorApplication.playModeStateChanged += _ => EnforceSharpGameViews();
        }

        public static void EnforceSharpGameViews()
        {
            if (GameViewType == null || LowResProp == null) return;

            UnityEngine.Object[] views = Resources.FindObjectsOfTypeAll(GameViewType);
            for (int i = 0; i < views.Length; i++)
            {
                if (views[i] == null) continue;
                try
                {
                    if ((bool)LowResProp.GetValue(views[i]))
                    {
                        LowResProp.SetValue(views[i], false);
                        if (views[i] is EditorWindow ew)
                        {
                            ew.Repaint();
                        }
                    }
                }
                catch
                {
                    // Abaikan jika properti tidak tersedia di versi Editor tertentu
                }
            }
        }
    }
}
