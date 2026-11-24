using System.IO;
using PulauKabut.Lantern;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupLanternAndFog
{
    private const string SettingsPath = "Assets/_Project/Prefabs/Player/LanternSettings_Default.asset";
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";
    private const string MatFolder = "Assets/_Project/Prefabs/Player/Materials";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        if (!Directory.Exists(MatFolder))
            Directory.CreateDirectory(MatFolder);

        // 1. Buat aset LanternSettings_Default
        LanternSettings settings = AssetDatabase.LoadAssetAtPath<LanternSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<LanternSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        // Material lentera + platform bayangan (padat emas vs bayangan biru redup)
        Material matFrame    = GetOrCreateMat("MAT_Lantern_Frame",    "#4A3728", 0f, false);
        Material matCore     = GetOrCreateMat("MAT_Lantern_Core",     "#FFE28A", 0.1f, true);
        Material matRevealed = GetOrCreateMat("MAT_FogPlatform_Lit",  "#F5C869", 0.05f, true);
        Material matShadow   = GetOrCreateMat("MAT_FogPlatform_Dim",  "#3A4E63", 0f, false);

        // 2. Pasang LanternController + Visual Lentera di PF_Player
        GameObject player = GameObject.Find("PF_Player");
        if (player == null)
            throw new System.Exception("PF_Player tidak ditemukan di scene aktif.");

        Transform visuals = player.transform.Find("Visuals");
        Transform parentForLantern = visuals != null ? visuals : player.transform;

        Transform oldLantern = parentForLantern.Find("Player_Lantern");
        if (oldLantern != null) Object.DestroyImmediate(oldLantern.gameObject);

        GameObject lanternObj = new GameObject("Player_Lantern");
        lanternObj.transform.SetParent(parentForLantern, false);
        // Posisi di tangan kanan karakter (terlihat di Third-Person, dan tetap berada di dekat pemain di First-Person)
        lanternObj.transform.localPosition = new Vector3(0.46f, 0.86f, 0.28f);

        // Visual rangka + inti lentera chunky low-poly (tanpa collider)
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "Lantern_Cap";
        cap.transform.SetParent(lanternObj.transform, false);
        cap.transform.localPosition = new Vector3(0f, 0.14f, 0f);
        cap.transform.localScale = new Vector3(0.20f, 0.06f, 0.20f);
        Object.DestroyImmediate(cap.GetComponent<Collider>());
        cap.GetComponent<MeshRenderer>().sharedMaterial = matFrame;

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
        core.name = "Lantern_Core";
        core.transform.SetParent(lanternObj.transform, false);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = new Vector3(0.15f, 0.22f, 0.15f);
        Object.DestroyImmediate(core.GetComponent<Collider>());
        MeshRenderer coreRenderer = core.GetComponent<MeshRenderer>();
        coreRenderer.sharedMaterial = matCore;

        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.name = "Lantern_Bottom";
        bottom.transform.SetParent(lanternObj.transform, false);
        bottom.transform.localPosition = new Vector3(0f, -0.14f, 0f);
        bottom.transform.localScale = new Vector3(0.20f, 0.06f, 0.20f);
        Object.DestroyImmediate(bottom.GetComponent<Collider>());
        bottom.GetComponent<MeshRenderer>().sharedMaterial = matFrame;

        // Point Light tanpa shadow (hemat GPU 4 GB VRAM!)
        Light light = lanternObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = settings.LightColor;
        light.range = settings.LightRange;
        light.intensity = 0f;
        light.shadows = LightShadows.None;

        SetLayerRecursively(lanternObj, player.layer);

        LanternController lanternCtrl = player.GetComponent<LanternController>() ?? player.AddComponent<LanternController>();
        SerializedObject ctrlSo = new SerializedObject(lanternCtrl);
        ctrlSo.FindProperty("settings").objectReferenceValue = settings;
        ctrlSo.FindProperty("lanternPoint").objectReferenceValue = lanternObj.transform;
        ctrlSo.FindProperty("lanternLight").objectReferenceValue = light;
        ctrlSo.FindProperty("lanternCoreRenderer").objectReferenceValue = coreRenderer;
        ctrlSo.ApplyModifiedPropertiesWithoutUndo();

        // Simpan ke PF_Player.prefab
        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        // 3. Buat 3 Platform Bayangan (FogRevealable) contoh di sebelah kanan jalan masuk agar langsung bisa diuji!
        GameObject oldDemo = GameObject.Find("--- DEMO PLATFORM BAYANGAN (TEKAN F) ---");
        if (oldDemo != null) Object.DestroyImmediate(oldDemo);

        GameObject demoRoot = new GameObject("--- DEMO PLATFORM BAYANGAN (TEKAN F) ---");
        Vector3[] positions = new Vector3[]
        {
            new Vector3(5.5f, 3.6f, -43.5f),
            new Vector3(8.5f, 4.6f, -41.0f),
            new Vector3(11.5f, 5.6f, -38.5f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat.name = $"ShadowPlatform_{i + 1:00}";
            plat.transform.SetParent(demoRoot.transform);
            plat.transform.position = positions[i];
            plat.transform.localScale = new Vector3(2.8f, 0.45f, 2.8f);
            plat.GetComponent<MeshRenderer>().sharedMaterial = matShadow;

            FogRevealable rev = plat.AddComponent<FogRevealable>();
            SerializedObject revSo = new SerializedObject(rev);
            revSo.FindProperty("targetCollider").objectReferenceValue = plat.GetComponent<Collider>();
            revSo.FindProperty("targetRenderer").objectReferenceValue = plat.GetComponent<MeshRenderer>();
            revSo.FindProperty("revealedMaterial").objectReferenceValue = matRevealed;
            revSo.FindProperty("shadowMaterial").objectReferenceValue = matShadow;
            revSo.FindProperty("extraRevealMargin").floatValue = 1.0f;
            revSo.ApplyModifiedPropertiesWithoutUndo();
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: LanternController (F), LanternSettings, dan 3 contoh FogRevealable terpasang di PF_Player & scene!";
    }

    private static Material GetOrCreateMat(string name, string hex, float smoothness, bool emissive)
    {
        string path = $"{MatFolder}/{name}.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            m = new Material(s);
            AssetDatabase.CreateAsset(m, path);
        }
        ColorUtility.TryParseHtmlString(hex, out Color c);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        if (emissive && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * 1.4f);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
