using PulauKabut.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupTahap6
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu sebelum setup Tahap 6.");

        GameObject player = GameObject.Find("PF_Player");
        if (player == null)
            throw new System.Exception("PF_Player tidak ditemukan di scene aktif.");

        PlayerDebugOverlay overlay = player.GetComponent<PlayerDebugOverlay>() ?? player.AddComponent<PlayerDebugOverlay>();
        SerializedObject so = new SerializedObject(overlay);
        so.FindProperty("showOverlay").boolValue = true;
        so.FindProperty("playerController").objectReferenceValue = player.GetComponent<PlayerController>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Simpan ke Prefab PF_Player.prefab (otomatis terpropagasi ke Sandbox_Player & Sandbox_Environment)
        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: PlayerDebugOverlay terpasang pada PF_Player dan disimpan ke PF_Player.prefab!";
    }
}
