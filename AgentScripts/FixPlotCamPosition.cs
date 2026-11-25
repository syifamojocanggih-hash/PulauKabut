using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixPlotCamPosition
{
    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        GameObject plotCam = GameObject.Find("CM_POV2_PlotZone_CampHub");
        if (plotCam != null)
        {
            // Posisikan di tengah-atas jalan (ruang terbuka di antara 2 pondok, bebas halangan atap!)
            plotCam.transform.position = new Vector3(0f, 9.5f, -34f);
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        return "OK: CM_POV2_PlotZone_CampHub dipindah ke ruang terbuka (0, 9.5, -34) bebas halangan pondok!";
    }
}
