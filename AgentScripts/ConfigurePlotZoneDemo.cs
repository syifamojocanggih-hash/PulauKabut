using PulauKabut.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigurePlotZoneDemo
{
    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        GameObject zoneObj = GameObject.Find("PlotZone_CampHub");
        GameObject tower = GameObject.Find("POI_05_LanternTower");
        if (zoneObj == null) return "PlotZone_CampHub tidak ditemukan.";

        ContextCameraZone zone = zoneObj.GetComponent<ContextCameraZone>();
        if (zone == null) return "ContextCameraZone tidak ditemukan.";

        SerializedObject so = new SerializedObject(zone);
        if (tower != null) so.FindProperty("storyFocusTarget").objectReferenceValue = tower.transform;
        so.FindProperty("playOnce").boolValue = true;
        so.FindProperty("autoReturnDuration").floatValue = 3.0f;
        so.FindProperty("pausePlayerDuringShot").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        return "OK: PlotZone_CampHub disetel jadi momen alur otomatis 3 detik menyorot Menara Lentera!";
    }
}
