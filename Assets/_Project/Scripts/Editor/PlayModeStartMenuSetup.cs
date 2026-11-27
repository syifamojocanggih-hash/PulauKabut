using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PulauKabut.Editor
{
    /// <summary>
    /// Memastikan tombol Play di Unity Editor selalu memulai dari scene MainMenu.unity
    /// agar alur masuk awal game konsisten seperti build rilis, sambil tetap mengizinkan
    /// pengeditan Zone_Mangrove atau scene lain di Scene View.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeStartMenuSetup
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string PrefKey = "PK_PlayFromMainMenu";
        private const string MenuPath = "Pulau Kabut/Selalu Play Dari MainMenu";

        static PlayModeStartMenuSetup()
        {
            ApplyStartScene();
            EditorApplication.delayCall += () => ApplyStartScene();
        }

        [MenuItem(MenuPath, false, 50)]
        private static void TogglePlayFromMainMenu()
        {
            bool next = !EditorPrefs.GetBool(PrefKey, true);
            EditorPrefs.SetBool(PrefKey, next);
            ApplyStartScene();
        }

        [MenuItem(MenuPath, true)]
        private static bool TogglePlayFromMainMenuValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, true));
            return true;
        }

        public static string ApplyStartScene()
        {
            bool enabled = EditorPrefs.GetBool(PrefKey, true);
            if (enabled)
            {
                SceneAsset menuAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
                if (menuAsset != null)
                {
                    EditorSceneManager.playModeStartScene = menuAsset;
                }
            }
            else
            {
                EditorSceneManager.playModeStartScene = null;
            }

            var active = EditorSceneManager.GetActiveScene();
            string startName = EditorSceneManager.playModeStartScene != null ? EditorSceneManager.playModeStartScene.name : "null";
            return $"enabled={enabled} | playModeStartScene={startName} | activeScene={active.name} (dirty={active.isDirty}) | isPlaying={EditorApplication.isPlaying}";
        }
    }
}
