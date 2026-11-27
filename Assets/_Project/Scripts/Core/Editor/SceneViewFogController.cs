using UnityEditor;

namespace PulauKabut.Core.Editor
{
    /// <summary>
    /// Menonaktifkan tampilan kabut (Fog) di jendela Scene View saat mode Edit agar
    /// pemeriksaan dan penataan wilayah terlihat jelas, lalu mengaktifkannya kembali saat Play Mode.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneViewFogController
    {
        static SceneViewFogController()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += SyncSceneViewFog;
            EditorApplication.hierarchyChanged += SyncSceneViewFog;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode ||
                state == PlayModeStateChange.EnteredEditMode)
            {
                SyncSceneViewFog();
            }
        }

        private static void SyncSceneViewFog()
        {
            bool enableFog = EditorApplication.isPlayingOrWillChangePlaymode;
            foreach (SceneView view in SceneView.sceneViews)
            {
                if (view != null && view.sceneViewState != null && view.sceneViewState.showFog != enableFog)
                {
                    view.sceneViewState.showFog = enableFog;
                    view.Repaint();
                }
            }
        }
    }
}
