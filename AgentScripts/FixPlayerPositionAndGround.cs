using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixPlayerPositionAndGround
{
    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        // 1. Kembalikan skala Y Island_SandBase & Island_GrassMain ke 1.0 (XZ tetap 2.85x = 400m!)
        // supaya permukaan rumput tetap tepat di y = 2.60 (tidak menenggelamkan jalan, pagar, pondok, dan pohon!)
        GameObject root = GameObject.Find("--- PULAU KABUT (~400x400m TEST) ---")
                          ?? GameObject.Find("--- PULAU KABUT (~140x140m STYLIZED) ---");
        if (root != null)
        {
            Transform terrain = root.transform.Find("01_Terrain_And_Water");
            if (terrain != null)
            {
                Transform sand = terrain.Find("Island_SandBase");
                if (sand != null) sand.localScale = new Vector3(2.85f, 1.0f, 2.85f);

                Transform grass = terrain.Find("Island_GrassMain");
                if (grass != null) grass.localScale = new Vector3(2.85f, 1.0f, 2.85f);
            }
        }

        // 2. Letakkan PF_Player di jalan masuk selatan (di atas tanah y = 2.75, menghadap utara ke pondok & Menara Lentera)
        GameObject player = GameObject.Find("PF_Player");
        if (player == null)
            throw new System.Exception("PF_Player tidak ditemukan.");

        player.transform.position = new Vector3(0f, 2.75f, -45f);
        player.transform.rotation = Quaternion.identity;

        // Pastikan Main Camera juga langsung menghadap karakter saat di Editor
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0f, 5.0f, -50.5f);
            Camera.main.transform.LookAt(new Vector3(0f, 4.2f, -40f));
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Permukaan tanah diratakan di y=2.60 (400x400m) dan PF_Player diposisikan di jalan masuk (0, 2.75, -45)!";
    }
}
