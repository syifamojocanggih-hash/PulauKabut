using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UpdatePlayerVisualRoblox
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";
    private const string MatFolder = "Assets/_Project/Prefabs/Player/Materials";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
        {
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu sebelum mengubah visual PF_Player.");
        }

        if (!Directory.Exists(MatFolder))
            Directory.CreateDirectory(MatFolder);

        // Material polos matte bergaya karakter Roblox klasik (tanpa kilap HD)
        Material matSkin  = CreateMat("MAT_Blocky_PlainLight", "#F0E6D2"); // Kepala & tangan krem polos
        Material matTorso = CreateMat("MAT_Blocky_PlainBlue",  "#4E8FD9"); // Baju biru polos
        Material matLegs  = CreateMat("MAT_Blocky_PlainDark",  "#4C5866"); // Celana abu gelap polos
        Material matEye   = CreateMat("MAT_Blocky_Eye",        "#25282C"); // Titik mata sederhana

        // Cari PF_Player di scene aktif
        GameObject player = GameObject.Find("PF_Player");
        if (player == null)
        {
            throw new System.Exception("PF_Player tidak ditemukan di scene aktif.");
        }

        Transform visuals = player.transform.Find("Visuals");
        if (visuals == null)
        {
            GameObject v = new GameObject("Visuals");
            v.transform.SetParent(player.transform, false);
            visuals = v.transform;
        }

        // Hapus isi Visuals lama (Capsule_Temp & Face_Visor)
        for (int i = visuals.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(visuals.GetChild(i).gameObject);
        }

        // Root model Roblox polos (total tinggi = 1.80 m, kaki menapak tepat di y = 0)
        GameObject rig = new GameObject("RobloxPlain_Temp");
        rig.transform.SetParent(visuals, false);

        // Kaki Kiri & Kanan (y = 0.0 .. 0.66)
        CreatePart("Leg_Left",  new Vector3(-0.14f, 0.33f, 0f), new Vector3(0.24f, 0.66f, 0.28f), matLegs, rig.transform);
        CreatePart("Leg_Right", new Vector3( 0.14f, 0.33f, 0f), new Vector3(0.24f, 0.66f, 0.28f), matLegs, rig.transform);

        // Badan / Torso (y = 0.66 .. 1.32)
        CreatePart("Torso", new Vector3(0f, 0.99f, 0f), new Vector3(0.56f, 0.66f, 0.30f), matTorso, rig.transform);

        // Lengan Kiri & Kanan (y = 0.68 .. 1.30)
        CreatePart("Arm_Left",  new Vector3(-0.41f, 0.99f, 0f), new Vector3(0.22f, 0.62f, 0.24f), matSkin, rig.transform);
        CreatePart("Arm_Right", new Vector3( 0.41f, 0.99f, 0f), new Vector3(0.22f, 0.62f, 0.24f), matSkin, rig.transform);

        // Kepala polos blocky (y = 1.36 .. 1.78)
        GameObject head = CreatePart("Head", new Vector3(0f, 1.57f, 0f), new Vector3(0.42f, 0.42f, 0.42f), matSkin, rig.transform);

        // 2 mata kecil di sisi depan (+Z) agar arah hadap langsung terbaca jelas
        CreatePart("Eye_Left",  new Vector3(-0.09f, 1.60f, 0.215f), new Vector3(0.055f, 0.075f, 0.02f), matEye, rig.transform);
        CreatePart("Eye_Right", new Vector3( 0.09f, 1.60f, 0.215f), new Vector3(0.055f, 0.075f, 0.02f), matEye, rig.transform);

        // Samakan layer semua child dengan layer PF_Player
        SetLayerRecursively(rig, player.layer);

        // Simpan pembaruan ke Prefab PF_Player.prefab & scene aktif
        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Visual PF_Player diganti ke karakter blocky polos ala Roblox (tinggi 1.8m, tanpa collider) & prefab disimpan.";
    }

    private static GameObject CreatePart(string name, Vector3 localPos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localScale = scale;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        part.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return part;
    }

    private static Material CreateMat(string name, string hex)
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
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(m);
        return m;
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
