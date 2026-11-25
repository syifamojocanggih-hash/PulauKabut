using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixPeakPathAndPlotCam
{
    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        GameObject root = GameObject.Find("--- PULAU KABUT (~400x400m TEST) ---")
                          ?? GameObject.Find("--- PULAU KABUT (~140x140m STYLIZED) ---");
        if (root == null) throw new System.Exception("Root pulau tidak ditemukan.");

        Transform cliffs = root.transform.Find("02_Terraced_Cliffs_And_Ramps");
        if (cliffs != null)
        {
            // 1. Sambungkan tanjakan dari dataran utama (y=2.6) masuk mantap ke atas Cliff_NorthMid_Grass (y=7.95)
            ConnectRamp(
                cliffs,
                "Ramp_MainToNorthMid",
                new Vector3(-13.5f, 2.55f, 3.0f),
                new Vector3(-11.0f, 8.00f, 19.5f),
                5.0f
            );

            // 2. Sambungkan tanjakan dari atas Cliff_NorthMid_Grass (y=7.9) masuk mantap ke atas puncak Cliff_NorthPeak_Grass (y=13.8)
            ConnectRamp(
                cliffs,
                "Ramp_NorthMidToPeak",
                new Vector3(-13.0f, 7.85f, 25.5f),
                new Vector3(-2.5f, 13.82f, 31.0f),
                4.8f
            );

            // 3. Pastikan tanjakan ke Tebing Timur juga menancap mantap di atas Cliff_East_Grass (y=6.65)
            ConnectRamp(
                cliffs,
                "Ramp_MainToEast",
                new Vector3(11.5f, 2.55f, -5.5f),
                new Vector3(22.5f, 6.70f, 2.5f),
                4.6f
            );
        }

        // 4. Pindahkan CM_POV2_PlotZone_CampHub ke ruang terbuka di antara 2 pondok (bebas halangan atap)
        GameObject plotCam = GameObject.Find("CM_POV2_PlotZone_CampHub");
        if (plotCam != null)
        {
            plotCam.transform.position = new Vector3(0f, 9.5f, -34f);
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Jalur tanjakan menuju Teras Utara, Puncak Menara Lentera, dan Tebing Timur sudah disambungkan mulus + kamera plot dipindah ke ruang terbuka!";
    }

    /// <summary>
    /// Memposisikan dan memutar ramp tepat dari titik bawah (start) hingga menancap di titik atas (end).
    /// </summary>
    private static void ConnectRamp(Transform parent, string rampName, Vector3 start, Vector3 end, float width)
    {
        Transform t = parent.Find(rampName);
        if (t == null) return;

        Vector3 diff = end - start;
        float length = diff.magnitude + 1.2f; // sedikit lebih panjang agar ujungnya tertanam rapi tanpa celah
        Vector3 mid = (start + end) * 0.5f - Vector3.up * 0.30f; // permukaan atas ramp tepat di garis start->end

        t.position = mid;
        t.rotation = Quaternion.LookRotation(diff.normalized, Vector3.up);
        t.localScale = new Vector3(width, 0.65f, length);
    }
}
