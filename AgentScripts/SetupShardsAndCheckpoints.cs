using System.IO;
using PulauKabut.Core;
using PulauKabut.Dialog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupShardsAndCheckpoints
{
    private const string ShardPrefabPath = "Assets/_Project/Prefabs/Props/PF_LightShard.prefab";
    private const string DialogFolder = "Assets/_Project/Dialog";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        // 1. Pasang ShardManager & CheckpointManager di GameManager
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
        if (gmObj.GetComponent<ShardManager>() == null) gmObj.AddComponent<ShardManager>();
        if (gmObj.GetComponent<CheckpointManager>() == null) gmObj.AddComponent<CheckpointManager>();

        // 2. Buat DialogData kenangan contoh
        if (!Directory.Exists(DialogFolder)) Directory.CreateDirectory(DialogFolder);
        DialogData mem1 = CreateMemoryDialog("Dialog_Kenangan_01_Perahu", "Kenangan 1 — Perahu Pertama",
            "Tampak bayangan seorang anak kecil mendorong perahu kayu mungil di tepi rawa sambil tertawa riang.",
            "\"Suatu hari nanti, perahu ini akan berlayar jauh!\"");
        DialogData mem2 = CreateMemoryDialog("Dialog_Kenangan_02_Lagu", "Kenangan 2 — Lagu Pengantar Tidur",
            "Sayup-sayup terdengar senandung lembut seorang ibu di tengah gemerisik daun bakau.",
            "Cahaya kecil ini terasa hangat, seperti rumah yang lama dirindukan.");
        DialogData mem3 = CreateMemoryDialog("Dialog_Kenangan_03_Puncak", "Kenangan — Cahaya Pelita",
            "Dari puncak ini, seluruh Pulau Kabut terlihat membentang hingga ujung cakrawala.",
            "Kumpulkan Pecahan Cahaya di setiap wilayah untuk memulihkan Desa Pelita sepenuhnya.");

        // 3. Buat Prefab PF_LightShard (kristal emas bercahaya)
        Material shardMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Player/Materials/MAT_FogPlatform_Lit.mat");
        GameObject tempShard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempShard.name = "PF_LightShard";
        tempShard.transform.localScale = new Vector3(0.55f, 0.85f, 0.55f);
        tempShard.transform.eulerAngles = new Vector3(25f, 45f, 25f);
        Object.DestroyImmediate(tempShard.GetComponent<BoxCollider>());
        SphereCollider trigger = tempShard.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.6f;
        if (shardMat != null) tempShard.GetComponent<MeshRenderer>().sharedMaterial = shardMat;
        tempShard.AddComponent<LightShardPickup>();

        string prefabDir = Path.GetDirectoryName(ShardPrefabPath);
        if (!string.IsNullOrEmpty(prefabDir) && !Directory.Exists(prefabDir))
            Directory.CreateDirectory(prefabDir);

        GameObject shardPrefab = PrefabUtility.SaveAsPrefabAsset(tempShard, ShardPrefabPath);
        Object.DestroyImmediate(tempShard);

        // 4. Pasang 3 contoh Pecahan Cahaya & Checkpoint + FallRespawnZone di scene aktif
        GameObject oldRoot = GameObject.Find("--- SHARDS & CHECKPOINTS ---");
        if (oldRoot != null) Object.DestroyImmediate(oldRoot);
        GameObject root = new GameObject("--- SHARDS & CHECKPOINTS ---");

        PlaceShard(shardPrefab, root.transform, "Shard_01_ShadowPath", "W1_Shard_01", new Vector3(11.5f, 6.7f, -38.5f), mem1);
        PlaceShard(shardPrefab, root.transform, "Shard_02_Lagoon",     "W1_Shard_02", new Vector3(-34.0f, 4.4f, -16.0f), mem2);
        PlaceShard(shardPrefab, root.transform, "Shard_03_TowerPeak",  "W1_Shard_03", new Vector3(2.0f, 14.9f, 28.5f), mem3);

        // Checkpoint di Teras Utara (setelah naik ramp pertama)
        GameObject cpObj = new GameObject("Checkpoint_TerasUtara");
        cpObj.transform.SetParent(root.transform);
        cpObj.transform.position = new Vector3(-10.5f, 8.6f, 20.5f);
        BoxCollider cpCol = cpObj.AddComponent<BoxCollider>();
        cpCol.isTrigger = true;
        cpCol.size = new Vector3(8f, 4f, 8f);
        CheckpointZone cpZone = cpObj.AddComponent<CheckpointZone>();
        SerializedObject cpSo = new SerializedObject(cpZone);
        cpSo.FindProperty("checkpointName").stringValue = "Teras Utara";
        cpSo.ApplyModifiedPropertiesWithoutUndo();

        // FallRespawnZone di bawah permukaan air agar pemain yang jatuh langsung respawn aman
        GameObject fallObj = new GameObject("FallRespawnZone_Ocean");
        fallObj.transform.SetParent(root.transform);
        fallObj.transform.position = new Vector3(0f, -4.5f, 0f);
        BoxCollider fallCol = fallObj.AddComponent<BoxCollider>();
        fallCol.isTrigger = true;
        fallCol.size = new Vector3(1200f, 2f, 1200f);
        fallObj.AddComponent<FallRespawnZone>();

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: ShardManager, CheckpointManager, PF_LightShard.prefab, 3 contoh Pecahan Cahaya, dan FallRespawnZone terpasang!";
    }

    private static void PlaceShard(GameObject prefab, Transform parent, string objName, string id, Vector3 pos, DialogData dialog)
    {
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.name = objName;
        inst.transform.position = pos;

        LightShardPickup pickup = inst.GetComponent<LightShardPickup>();
        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("collectibleId").stringValue = id;
        so.FindProperty("memoryDialog").objectReferenceValue = dialog;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static DialogData CreateMemoryDialog(string fileName, string speaker, params string[] lines)
    {
        string path = $"{DialogFolder}/{fileName}.asset";
        DialogData data = AssetDatabase.LoadAssetAtPath<DialogData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<DialogData>();
            AssetDatabase.CreateAsset(data, path);
        }

        SerializedObject so = new SerializedObject(data);
        so.FindProperty("speakerName").stringValue = speaker;
        SerializedProperty linesProp = so.FindProperty("lines");
        linesProp.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            linesProp.GetArrayElementAtIndex(i).stringValue = lines[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        return data;
    }
}
