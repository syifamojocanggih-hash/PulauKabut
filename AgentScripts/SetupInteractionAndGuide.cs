using System.IO;
using PulauKabut.Core;
using PulauKabut.Dialog;
using PulauKabut.Player;
using PulauKabut.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupInteractionAndGuide
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";
    private const string DialogAssetPath = "Assets/_Project/Dialog/Dialog_MbahSauh_Intro.asset";
    private const string MatFolder = "Assets/_Project/Prefabs/NPC/Materials";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        // 1. Pasang PlayerInteractor langsung di dalam PF_Player.prefab
        int playerLayer = LayerMask.NameToLayer("Player");
        using (var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
        {
            GameObject root = scope.prefabContentsRoot;
            PlayerInteractor interactor = root.GetComponent<PlayerInteractor>() ?? root.AddComponent<PlayerInteractor>();
            SerializedObject iso = new SerializedObject(interactor);
            iso.FindProperty("interactRadius").floatValue = 3.0f;
            if (playerLayer >= 0) iso.FindProperty("interactMask").intValue = ~(1 << playerLayer);
            iso.ApplyModifiedPropertiesWithoutUndo();
        }

        // 2. Buat aset DialogData untuk Mbah Sauh
        string dialogDir = Path.GetDirectoryName(DialogAssetPath);
        if (!string.IsNullOrEmpty(dialogDir) && !Directory.Exists(dialogDir))
            Directory.CreateDirectory(dialogDir);

        DialogData dialogAsset = AssetDatabase.LoadAssetAtPath<DialogData>(DialogAssetPath);
        if (dialogAsset == null)
        {
            dialogAsset = ScriptableObject.CreateInstance<DialogData>();
            AssetDatabase.CreateAsset(dialogAsset, DialogAssetPath);
        }

        // 3. Buat placeholder NPC Mbah Sauh (blocky dengan topi lebar hangat) di dekat jalan masuk
        if (!Directory.Exists(MatFolder)) Directory.CreateDirectory(MatFolder);
        Material matHat   = GetOrCreateMat("MAT_MbahSauh_Hat",   "#D99B52");
        Material matShirt = GetOrCreateMat("MAT_MbahSauh_Shirt", "#C95D42");
        Material matSkin  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Player/Materials/MAT_Blocky_PlainLight.mat");
        Material matDark  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Player/Materials/MAT_Blocky_PlainDark.mat");

        GameObject oldNpc = GameObject.Find("NPC_MbahSauh");
        if (oldNpc != null) Object.DestroyImmediate(oldNpc);

        GameObject npc = new GameObject("NPC_MbahSauh");
        npc.transform.position = new Vector3(-2.4f, 2.6f, -39.5f);
        npc.transform.eulerAngles = new Vector3(0f, 135f, 0f);

        CapsuleCollider col = npc.AddComponent<CapsuleCollider>();
        col.height = 1.8f;
        col.radius = 0.5f;
        col.center = new Vector3(0f, 0.9f, 0f);

        CreatePart("Legs",  new Vector3(0f, 0.35f, 0f), new Vector3(0.45f, 0.70f, 0.30f), matDark,  npc.transform);
        CreatePart("Torso", new Vector3(0f, 1.02f, 0f), new Vector3(0.56f, 0.66f, 0.32f), matShirt, npc.transform);
        CreatePart("Head",  new Vector3(0f, 1.56f, 0f), new Vector3(0.42f, 0.42f, 0.42f), matSkin,  npc.transform);
        // Topi lebar nelayan Mbah Sauh
        CreatePart("Hat_Brim", new Vector3(0f, 1.78f, 0f), new Vector3(0.88f, 0.08f, 0.88f), matHat, npc.transform);
        CreatePart("Hat_Top",  new Vector3(0f, 1.90f, 0f), new Vector3(0.46f, 0.18f, 0.46f), matHat, npc.transform);

        GameObject tower = GameObject.Find("POI_05_LanternTower");
        NpcInteractable npcInteract = npc.AddComponent<NpcInteractable>();
        SerializedObject npcSo = new SerializedObject(npcInteract);
        npcSo.FindProperty("promptText").stringValue = "Bicara dengan Mbah Sauh";
        npcSo.FindProperty("dialogData").objectReferenceValue = dialogAsset;
        if (tower != null) npcSo.FindProperty("nextObjectiveTarget").objectReferenceValue = tower.transform;
        npcSo.FindProperty("nextObjectiveTitle").stringValue = "Menuju Menara Lentera di Puncak";
        npcSo.ApplyModifiedPropertiesWithoutUndo();

        // 4. Pasang DialogManager & ObjectiveGuide pada GameManager di scene aktif
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        if (gmObj.GetComponent<DialogManager>() == null)
            gmObj.AddComponent<DialogManager>();

        ObjectiveGuide guide = gmObj.GetComponent<ObjectiveGuide>() ?? gmObj.AddComponent<ObjectiveGuide>();
        GameObject playerObj = GameObject.Find("PF_Player");
        SerializedObject gSo = new SerializedObject(guide);
        gSo.FindProperty("objectiveTitle").stringValue = "Bicara dengan Mbah Sauh";
        gSo.FindProperty("targetTransform").objectReferenceValue = npc.transform;
        if (playerObj != null) gSo.FindProperty("playerTransform").objectReferenceValue = playerObj.transform;
        gSo.FindProperty("showGuide").boolValue = true;
        gSo.ApplyModifiedPropertiesWithoutUndo();

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: PlayerInteractor (E), NPC Mbah Sauh, DialogManager, dan Petunjuk Arah (ObjectiveGuide) siap di scene!";
    }

    private static void CreatePart(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static Material GetOrCreateMat(string name, string hex)
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
}
