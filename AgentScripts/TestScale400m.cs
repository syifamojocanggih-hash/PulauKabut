using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class TestScale400m
{
    private const string ScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Environment.unity";
    private const string PackPrefabs = "Assets/SimpleNaturePack/Prefabs";
    private const string PackMatPath = "Assets/SimpleNaturePack/Materials/SimpleNaturePack_Texture_01.mat";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        Scene active = SceneManager.GetActiveScene();
        if (active.path != ScenePath)
            active = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Aktifkan GPU Instancing pada material SimpleNaturePack agar skala 400m tetap efisien
        Material packMat = AssetDatabase.LoadAssetAtPath<Material>(PackMatPath);
        if (packMat != null)
        {
            packMat.enableInstancing = true;
            EditorUtility.SetDirty(packMat);
        }

        GameObject root = GameObject.Find("--- PULAU KABUT (~140x140m STYLIZED) ---");
        if (root == null) root = GameObject.Find("--- PULAU KABUT (~400x400m TEST) ---");
        if (root == null) throw new System.Exception("Root pulau tidak ditemukan.");
        root.name = "--- PULAU KABUT (~400x400m TEST) ---";

        // Skala dasar pulau dari ~140m -> ~400m (faktor 2.85x pada sumbu XZ, Y dinaikkan proporsional 1.35x)
        Transform terrain = root.transform.Find("01_Terrain_And_Water");
        if (terrain != null)
        {
            ScaleObj(terrain, "Ocean_Water",      new Vector3(160f, 1f, 160f));
            ScaleObj(terrain, "Island_SandBase",  new Vector3(2.85f, 1.2f, 2.85f));
            ScaleObj(terrain, "Island_GrassMain", new Vector3(2.85f, 1.2f, 2.85f));
        }

        // Hapus stress-test lama jika ada, lalu tambah kepadatan vegetasi realistis untuk area 400x400m
        Transform oldStress = root.transform.Find("07_Scale400m_ExtraVegetation");
        if (oldStress != null) Object.DestroyImmediate(oldStress.gameObject);

        GameObject extraRoot = new GameObject("07_Scale400m_ExtraVegetation");
        extraRoot.transform.SetParent(root.transform);

        GameObject[] trees   = LoadPrefabs("Tree_01", "Tree_02", "Tree_03", "Tree_04", "Tree_05");
        GameObject[] bushes  = LoadPrefabs("Bush_01", "Bush_02", "Bush_03");
        GameObject[] grasses = LoadPrefabs("Grass_01", "Grass_02", "Flowers_01", "Flowers_02");
        GameObject[] rocks   = LoadPrefabs("Rock_01", "Rock_02", "Rock_03", "Rock_04", "Rock_05");

        System.Random rng = new System.Random(400);

        // Tambah 180 pohon di cincin luar (radius 58m .. 162m)
        for (int i = 0; i < 180; i++)
        {
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = 60f + (float)rng.NextDouble() * 100f;
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 2.75f, Mathf.Sin(a) * r);
            float s = 0.9f + (float)rng.NextDouble() * 0.7f;
            Spawn(trees[rng.Next(trees.Length)], p, (float)rng.NextDouble() * 360f, s, extraRoot.transform, true);
        }

        // Tambah 70 semak di cincin luar
        for (int i = 0; i < 70; i++)
        {
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = 55f + (float)rng.NextDouble() * 105f;
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 2.75f, Mathf.Sin(a) * r);
            float s = 0.9f + (float)rng.NextDouble() * 0.6f;
            Spawn(bushes[rng.Next(bushes.Length)], p, (float)rng.NextDouble() * 360f, s, extraRoot.transform, false);
        }

        // Tambah 280 rumput & bunga di cincin luar (matikan CastShadows agar hemat GPU!)
        for (int i = 0; i < 280; i++)
        {
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = 52f + (float)rng.NextDouble() * 108f;
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 2.75f, Mathf.Sin(a) * r);
            float s = 0.9f + (float)rng.NextDouble() * 0.7f;
            Spawn(grasses[rng.Next(grasses.Length)], p, (float)rng.NextDouble() * 360f, s, extraRoot.transform, false);
        }

        // Tambah 40 batu pantai di radius 155m .. 185m
        for (int i = 0; i < 40; i++)
        {
            float a = (i / 40f) * Mathf.PI * 2f;
            float r = 160f + (float)rng.NextDouble() * 22f;
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 1.5f, Mathf.Sin(a) * r);
            float s = 1.4f + (float)rng.NextDouble() * 1.4f;
            Spawn(rocks[rng.Next(rocks.Length)], p, (float)rng.NextDouble() * 360f, s, extraRoot.transform, true);
        }

        // Optimasi Far Clip Plane & Fog untuk skala 400m
        RenderSettings.fogStartDistance = 55f;
        RenderSettings.fogEndDistance = 240f;
        if (Camera.main != null)
        {
            Camera.main.farClipPlane = 270f;
        }

        EditorSceneManager.MarkSceneDirty(active);
        EditorSceneManager.SaveScene(active);
        AssetDatabase.SaveAssets();

        return "OK: Skala 400x400m (total +570 objek vegetasi/batu ekstra + GPU Instancing) siap diuji FPS.";
    }

    private static void ScaleObj(Transform parent, string name, Vector3 scale)
    {
        Transform t = parent.Find(name);
        if (t != null) t.localScale = scale;
    }

    private static GameObject[] LoadPrefabs(params string[] names)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (string n in names)
        {
            GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>($"{PackPrefabs}/{n}.prefab");
            if (p != null) list.Add(p);
        }
        return list.ToArray();
    }

    private static void Spawn(GameObject prefab, Vector3 pos, float rotY, float scale, Transform parent, bool castShadows)
    {
        if (prefab == null) return;
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.transform.position = pos;
        inst.transform.eulerAngles = new Vector3(0f, rotY, 0f);
        inst.transform.localScale = Vector3.one * scale;
        inst.isStatic = true;

        if (!castShadows)
        {
            foreach (MeshRenderer mr in inst.GetComponentsInChildren<MeshRenderer>())
            {
                mr.shadowCastingMode = ShadowCastingMode.Off;
            }
        }
    }
}
