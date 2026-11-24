using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class UpgradeStylizedIsland
{
    private const string ScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Environment.unity";
    private const string PackPrefabs = "Assets/SimpleNaturePack/Prefabs";
    private const string PackMatPath = "Assets/SimpleNaturePack/Materials/SimpleNaturePack_Texture_01.mat";
    private const string PackTexPath = "Assets/SimpleNaturePack/Textures/NaturePackLite_Texture_01.png";
    private const string VolumeProfilePath = "Assets/_Project/Scenes/Sandbox/StylizedIsland_VolumeProfile.asset";
    private const string MaterialsFolder = "Assets/_Project/Scenes/Sandbox/Materials/StylizedIsland";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
        {
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu sebelum upgrade scene.");
        }

        // Pastikan Sandbox_Environment terbuka
        Scene active = SceneManager.GetActiveScene();
        if (active.path != ScenePath)
        {
            active = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        // 1. Pastikan material SimpleNaturePack sudah kompatibel URP (tidak pink!)
        EnsureSimpleNaturePackUrpMaterial();

        // 2. Harmoniskan palet warna tanah/tebing/air agar menyatu dengan SimpleNaturePack
        TuneBaseMaterials();

        // 3. Cari root pulau
        GameObject root = GameObject.Find("--- PULAU KABUT (~140x140m STYLIZED) ---");
        if (root == null)
        {
            throw new System.Exception("Root pulau tidak ditemukan di Sandbox_Environment.unity.");
        }

        // Hapus vegetasi primitif lama & awan lama jika ada
        DestroyChildIfExists(root.transform, "05_Trees_Bushes_Rocks");
        DestroyChildIfExists(root.transform, "06_Stylized_Clouds");

        // 4. Populasi ulang dengan prefab asli SimpleNaturePack (pohon, semak, rumput, bunga, batu, jamur, tunggul)
        GameObject natureRoot = new GameObject("05_Trees_Bushes_Rocks");
        natureRoot.transform.SetParent(root.transform);
        PopulateWithSimpleNaturePack(natureRoot.transform);

        // 5. Tambahkan awan low-poly melayang di langit agar cakrawala hidup
        GameObject cloudsRoot = new GameObject("06_Stylized_Clouds");
        cloudsRoot.transform.SetParent(root.transform);
        CreateStylizedClouds(cloudsRoot.transform);

        // 6. Tingkatkan lighting, kabut hangat-biru, dan pasang URP Global Volume Post-Processing
        SetupLightingAndPostProcessing();

        // 7. Simpan scene & aset
        EditorSceneManager.MarkSceneDirty(active);
        EditorSceneManager.SaveScene(active);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return "OK: Visual Sandbox_Environment berhasil di-upgrade dengan SimpleNaturePack + URP Post-Processing + Awan Low-Poly!";
    }

    private static void EnsureSimpleNaturePackUrpMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(PackMatPath);
        if (mat == null) return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit != null && mat.shader != urpLit)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PackTexPath);
            mat.shader = urpLit;
            if (tex != null && mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", tex);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(mat);
        }
    }

    private static void TuneBaseMaterials()
    {
        SetMatColor("MAT_Island_Grass",     "#6FC04E", 0.0f);
        SetMatColor("MAT_Island_GrassDark", "#58A641", 0.0f);
        SetMatColor("MAT_Island_Sand",      "#ECD9A3", 0.0f);
        SetMatColor("MAT_Island_Cliff",     "#B6784E", 0.0f);
        SetMatColor("MAT_Island_CliffDark", "#99603B", 0.0f);
        SetMatColor("MAT_Island_Water",     "#39AEC4", 0.25f);
        SetMatColor("MAT_Island_Lagoon",    "#59CDC9", 0.2f);
    }

    private static void SetMatColor(string matName, string hex, float smoothness)
    {
        Material m = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/{matName}.mat");
        if (m == null) return;
        ColorUtility.TryParseHtmlString(hex, out Color c);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(m);
    }

    private static void PopulateWithSimpleNaturePack(Transform parent)
    {
        GameObject[] trees    = LoadPrefabs("Tree_01", "Tree_02", "Tree_03", "Tree_04", "Tree_05");
        GameObject[] bushes   = LoadPrefabs("Bush_01", "Bush_02", "Bush_03");
        GameObject[] grasses  = LoadPrefabs("Grass_01", "Grass_02");
        GameObject[] flowers  = LoadPrefabs("Flowers_01", "Flowers_02");
        GameObject[] rocks    = LoadPrefabs("Rock_01", "Rock_02", "Rock_03", "Rock_04", "Rock_05");
        GameObject[] details  = LoadPrefabs("Stump_01", "Branch_01", "Mushroom_01", "Mushroom_02");

        GameObject treesGroup   = CreateEmpty("Trees", parent);
        GameObject bushesGroup  = CreateEmpty("Bushes", parent);
        GameObject grassGroup   = CreateEmpty("Grass_And_Flowers", parent);
        GameObject rocksGroup   = CreateEmpty("Rocks", parent);
        GameObject detailsGroup = CreateEmpty("Forest_Details", parent);

        System.Random rng = new System.Random(2026);

        // A. Pohon di dataran utama (55 pohon bervariasi)
        int treeCount = 0;
        for (int i = 0; i < 140 && treeCount < 55; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float dist = 12f + (float)rng.NextDouble() * 42f;
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;

            if (IsBlockedOnMainPlain(x, z)) continue;

            GameObject prefab = trees[rng.Next(trees.Length)];
            float scale = 0.85f + (float)rng.NextDouble() * 0.55f;
            SpawnPrefab(prefab, new Vector3(x, 2.55f, z), (float)rng.NextDouble() * 360f, scale, treesGroup.transform, true);
            treeCount++;
        }

        // B. Pohon terkurasi di atas tebing & pulau laguna
        var highlandTrees = new (Vector3 pos, float scale)[]
        {
            (new Vector3(-36.5f, 3.35f, -18.0f), 1.15f),
            (new Vector3(-31.5f, 3.35f, -13.5f), 0.95f),
            (new Vector3(36.5f,  6.60f, 11.5f),  1.15f),
            (new Vector3(38.0f,  6.60f, 2.0f),   1.0f),
            (new Vector3(26.0f,  6.60f, 14.0f),  1.1f),
            (new Vector3(32.0f,  6.60f, -2.5f),  0.9f),
            (new Vector3(-35.5f, 6.20f, 22.0f),  1.2f),
            (new Vector3(-27.0f, 6.20f, 23.0f),  1.0f),
            (new Vector3(-36.0f, 6.20f, 14.0f),  1.05f),
            (new Vector3(-13.5f, 7.85f, 22.0f),  1.15f),
            (new Vector3(14.5f,  7.85f, 21.0f),  1.1f),
            (new Vector3(16.0f,  7.85f, 31.0f),  0.95f),
            (new Vector3(-4.5f,  13.70f, 36.5f), 1.05f),
            (new Vector3(8.5f,   13.70f, 35.0f), 1.0f),
            (new Vector3(5.0f,   13.70f, 26.5f), 0.9f)
        };
        foreach (var ht in highlandTrees)
        {
            SpawnPrefab(trees[rng.Next(trees.Length)], ht.pos, (float)rng.NextDouble() * 360f, ht.scale, treesGroup.transform, true);
        }

        // C. Semak di sepanjang jalan masuk, pondok, dan kaki tebing
        Vector3[] curatedBushes = new Vector3[]
        {
            new Vector3(-5.8f, 2.58f, -47f), new Vector3(6.2f, 2.58f, -45f),
            new Vector3(-5.5f, 2.58f, -37f), new Vector3(6.8f, 2.58f, -32f),
            new Vector3(-13.5f,2.58f, -21f), new Vector3(15.2f,2.58f, -21f),
            new Vector3(-7.5f, 2.58f, -13f), new Vector3(8.5f, 2.58f, -7f),
            new Vector3(-18f,  2.58f, -9f),  new Vector3(-21f, 2.58f, -23f),
            new Vector3(-33f,  3.35f, -19f), new Vector3(23f,  2.58f, -15f),
            new Vector3(33f,   6.60f, 9.5f), new Vector3(-29f, 6.20f, 16f),
            new Vector3(-3f,   7.85f, 16f),  new Vector3(6.8f, 13.70f, 30f),
            new Vector3(-7.5f, 2.58f, -28f), new Vector3(8.2f, 2.58f, -29f),
            new Vector3(-15f,  2.58f, -35f), new Vector3(16f,  2.58f, -36f)
        };
        foreach (Vector3 bp in curatedBushes)
        {
            float s = 0.85f + (float)rng.NextDouble() * 0.5f;
            SpawnPrefab(bushes[rng.Next(bushes.Length)], bp, (float)rng.NextDouble() * 360f, s, bushesGroup.transform, false);
        }

        // D. Rumpun rumput & bunga di seluruh padang rumput (kunci agar tanah tidak terlihat polos/burik!)
        int grassCount = 0;
        for (int i = 0; i < 240 && grassCount < 110; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float dist = 6f + (float)rng.NextDouble() * 47f;
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;

            // Hindari jalan pasir utama dan laguna/tebing
            if (Mathf.Abs(x) < 4.8f && z < -15f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(0f, 28f)) < 25f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(31f, 6f)) < 18f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(-31f, 18f)) < 16f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(-31f, -15f)) < 14f) continue;

            bool isFlower = (rng.NextDouble() < 0.35);
            GameObject[] pool = isFlower ? flowers : grasses;
            float s = 0.9f + (float)rng.NextDouble() * 0.65f;
            SpawnPrefab(pool[rng.Next(pool.Length)], new Vector3(x, 2.58f, z), (float)rng.NextDouble() * 360f, s, grassGroup.transform, false);
            grassCount++;
        }

        // E. Batu low-poly asli di garis pantai, tepi laguna, dan kaki tebing
        for (int i = 0; i < 24; i++)
        {
            float a = (i / 24f) * Mathf.PI * 2f + 0.12f;
            if (Mathf.Sin(a) < -0.92f && Mathf.Abs(Mathf.Cos(a)) < 0.25f) continue;

            float r = 55f + (float)rng.NextDouble() * 9f;
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 1.35f, Mathf.Sin(a) * r);
            float s = 1.0f + (float)rng.NextDouble() * 1.1f;
            SpawnPrefab(rocks[rng.Next(rocks.Length)], p, (float)rng.NextDouble() * 360f, s, rocksGroup.transform, true);
        }

        // F. Detail kecil (tunggul kayu, ranting, jamur) di dekat pondok dan tepi hutan
        Vector3[] detailSpots = new Vector3[]
        {
            new Vector3(-7.2f, 2.58f, -20.5f), new Vector3(9.4f, 2.58f, -22.0f),
            new Vector3(-11.5f,2.58f, -30.0f), new Vector3(12.0f,2.58f, -33.0f),
            new Vector3(-17.5f,2.58f, -12.0f), new Vector3(18.5f,2.58f, -11.5f),
            new Vector3(-4.2f, 2.58f, -41.5f), new Vector3(5.6f, 2.58f, -42.5f),
            new Vector3(-34.5f,3.35f, -15.0f), new Vector3(28.0f,6.60f, 11.0f)
        };
        foreach (Vector3 dp in detailSpots)
        {
            float s = 0.9f + (float)rng.NextDouble() * 0.45f;
            SpawnPrefab(details[rng.Next(details.Length)], dp, (float)rng.NextDouble() * 360f, s, detailsGroup.transform, false);
        }
    }

    private static bool IsBlockedOnMainPlain(float x, float z)
    {
        if (Mathf.Abs(x) < 9.5f && z < -10f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(0f, -22f)) < 16f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(0f, 28f)) < 28f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(31f, 6f)) < 21f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(-31f, 18f)) < 19f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(-31f, -15f)) < 16f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(16.5f, -3.5f)) < 9f) return true;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(-10.5f, 7.5f)) < 10f) return true;
        return false;
    }

    private static void CreateStylizedClouds(Transform parent)
    {
        string matPath = $"{MaterialsFolder}/MAT_Island_Cloud.mat";
        Material cloudMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (cloudMat == null)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            cloudMat = new Material(urpLit);
            cloudMat.SetColor("_BaseColor", new Color(0.98f, 0.99f, 1f));
            cloudMat.SetFloat("_Smoothness", 0f);
            cloudMat.SetFloat("_Metallic", 0f);
            cloudMat.EnableKeyword("_EMISSION");
            cloudMat.SetColor("_EmissionColor", new Color(0.28f, 0.30f, 0.33f));
            AssetDatabase.CreateAsset(cloudMat, matPath);
        }

        Vector3[] cloudCenters = new Vector3[]
        {
            new Vector3(-42f, 38f, 45f),
            new Vector3(36f,  42f, 58f),
            new Vector3(-10f, 46f, 82f),
            new Vector3(62f,  35f, 12f),
            new Vector3(-68f, 36f, -10f),
            new Vector3(18f,  40f, -20f)
        };

        for (int i = 0; i < cloudCenters.Length; i++)
        {
            GameObject cloud = CreateEmpty($"Cloud_{i + 1:00}", parent);
            cloud.transform.position = cloudCenters[i];
            cloud.transform.eulerAngles = new Vector3(0f, i * 37f, 0f);

            // 3 puff kubus lembut membentuk awan chunky stylized
            CreateCloudPuff(cloud.transform, Vector3.zero, new Vector3(11f, 3.2f, 6.2f), cloudMat);
            CreateCloudPuff(cloud.transform, new Vector3(-4.2f, 1.3f, 0.6f), new Vector3(6.5f, 3.6f, 5.2f), cloudMat);
            CreateCloudPuff(cloud.transform, new Vector3(3.8f, 1.0f, -0.5f), new Vector3(7.2f, 2.9f, 4.8f), cloudMat);
        }
    }

    private static void CreateCloudPuff(Transform parent, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Cube);
        puff.transform.SetParent(parent, false);
        puff.transform.localPosition = localPos;
        puff.transform.localScale = scale;
        Object.DestroyImmediate(puff.GetComponent<Collider>());
        MeshRenderer mr = puff.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    private static void SetupLightingAndPostProcessing()
    {
        // 1. Directional Light keemasan lembut
        Light[] lights = Object.FindObjectsByType<Light>();
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.transform.eulerAngles = new Vector3(44f, -35f, 0f);
                l.color = Hex("#FFF4D8");
                l.intensity = 1.3f;
                l.shadowStrength = 0.48f;
            }
        }

        // 2. Ambient Trilight cerah + Fog biru-pastel
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Hex("#9ED8FA");
        RenderSettings.ambientEquatorColor = Hex("#E8F4E2");
        RenderSettings.ambientGroundColor = Hex("#7E916D");

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Hex("#CBEAF7");
        RenderSettings.fogStartDistance = 65f;
        RenderSettings.fogEndDistance = 240f;

        // 3. Global Volume Post-Processing (ACES Tonemapping + Color Adjustments + Bloom lembut + Vignette tipis)
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
        }

        if (!profile.TryGet(out Tonemapping tonemapping))
            tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.overrideState = true;
        tonemapping.mode.value = TonemappingMode.ACES;

        if (!profile.TryGet(out ColorAdjustments colorAdj))
            colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.postExposure.overrideState = true;
        colorAdj.postExposure.value = 0.12f;
        colorAdj.contrast.overrideState = true;
        colorAdj.contrast.value = 10f;
        colorAdj.saturation.overrideState = true;
        colorAdj.saturation.value = 14f;

        if (!profile.TryGet(out Bloom bloom))
            bloom = profile.Add<Bloom>(true);
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.95f;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 0.35f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.65f;

        if (!profile.TryGet(out Vignette vignette))
            vignette = profile.Add<Vignette>(true);
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.15f;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.45f;

        EditorUtility.SetDirty(profile);

        GameObject volObj = GameObject.Find("Global Volume");
        if (volObj == null)
        {
            volObj = new GameObject("Global Volume");
        }
        Volume vol = volObj.GetComponent<Volume>() ?? volObj.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;
        vol.sharedProfile = profile;
        volObj.layer = 0; // Default layer

        // 4. Aktifkan Post-Processing + SMAA di Main Camera
        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#8FD0F7");
            cam.allowHDR = true;

            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
            camData.volumeLayerMask = ~0;
        }
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

    private static void SpawnPrefab(GameObject prefab, Vector3 pos, float rotY, float scale, Transform parent, bool isStatic)
    {
        if (prefab == null) return;
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.transform.position = pos;
        inst.transform.eulerAngles = new Vector3(0f, rotY, 0f);
        inst.transform.localScale = Vector3.one * scale;
        inst.isStatic = isStatic;
    }

    private static void DestroyChildIfExists(Transform parent, string childName)
    {
        Transform t = parent.Find(childName);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }

    private static GameObject CreateEmpty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go;
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
