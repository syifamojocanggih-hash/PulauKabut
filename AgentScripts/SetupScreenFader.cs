using PulauKabut.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupScreenFader
{
    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
            throw new System.Exception("GameManager tidak ditemukan.");

        if (gmObj.GetComponent<ScreenFader>() == null)
            gmObj.AddComponent<ScreenFader>();

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: ScreenFader terpasang pada GameManager untuk transisi Fade Out/In obrolan!";
    }
}
