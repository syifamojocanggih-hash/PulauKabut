using System.IO;
using PulauKabut.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupPlayerInScene
{
    private const string SettingsPath = "Assets/_Project/Prefabs/Player/PlayerMovementSettings_Default.asset";
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

    public static string Run()
    {
        int playerLayer = EnsureLayer("Player");

        // 1. Buat atau muat aset PlayerMovementSettings_Default
        string folder = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        PlayerMovementSettings settings = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        // Pastikan groundMask mengecualikan layer Player
        SerializedObject settingsSo = new SerializedObject(settings);
        SerializedProperty groundMaskProp = settingsSo.FindProperty("groundMask");
        if (groundMaskProp != null && playerLayer >= 0)
        {
            groundMaskProp.intValue = ~(1 << playerLayer);
            settingsSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        // 2. Hapus placeholder statis lama jika ada di scene aktif
        GameObject oldRef = GameObject.Find("Player_ScaleReference_1_8m");
        Vector3 spawnPos = new Vector3(0f, 3.0f, -44f);
        if (oldRef != null)
        {
            spawnPos = new Vector3(oldRef.transform.position.x, 2.7f, oldRef.transform.position.z);
            Object.DestroyImmediate(oldRef);
        }

        // Hapus PF_Player lama di scene jika sudah ada agar tidak dobel
        GameObject existingPlayer = GameObject.Find("PF_Player");
        if (existingPlayer != null)
        {
            Object.DestroyImmediate(existingPlayer);
        }

        // 3. Bangun struktur PF_Player sesuai spesifikasi Tahap 2
        GameObject player = new GameObject("PF_Player");
        player.transform.position = spawnPos;

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 50f;
        cc.stepOffset = 0.3f;

        PlayerController controller = player.AddComponent<PlayerController>();

        // Wadah Visuals (hanya ini yang berputar)
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.SetParent(player.transform, false);
        visuals.transform.localPosition = Vector3.zero;

        Material bodyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Scenes/Sandbox/Materials/StylizedIsland/MAT_Player_ScaleRef.mat");
        Material visorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Scenes/Sandbox/Materials/StylizedIsland/MAT_Island_SignBoard.mat");

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Capsule_Temp";
        capsule.transform.SetParent(visuals.transform, false);
        capsule.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        capsule.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        Object.DestroyImmediate(capsule.GetComponent<Collider>());
        if (bodyMat != null) capsule.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

        // Penanda arah depan (visor) agar putaran karakter terlihat jelas
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "Face_Visor";
        visor.transform.SetParent(visuals.transform, false);
        visor.transform.localPosition = new Vector3(0f, 1.38f, 0.3f);
        visor.transform.localScale = new Vector3(0.36f, 0.16f, 0.22f);
        Object.DestroyImmediate(visor.GetComponent<Collider>());
        if (visorMat != null) visor.GetComponent<MeshRenderer>().sharedMaterial = visorMat;

        // Titik fokus kamera untuk Tahap 4
        GameObject camTarget = new GameObject("CameraTarget");
        camTarget.transform.SetParent(player.transform, false);
        camTarget.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        // Set layer Player ke seluruh hierarki PF_Player
        if (playerLayer >= 0)
        {
            SetLayerRecursively(player, playerLayer);
        }

        // Hubungkan referensi serialized pada PlayerController
        SerializedObject ctrlSo = new SerializedObject(controller);
        ctrlSo.FindProperty("settings").objectReferenceValue = settings;
        ctrlSo.FindProperty("visuals").objectReferenceValue = visuals.transform;
        ctrlSo.ApplyModifiedPropertiesWithoutUndo();

        // 4. Simpan sebagai Prefab PF_Player.prefab
        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        // 5. Posisikan Main Camera sementara agar jelas melihat PF_Player
        if (Camera.main != null)
        {
            Camera.main.transform.position = spawnPos + new Vector3(0f, 4.2f, -8.5f);
            Camera.main.transform.LookAt(spawnPos + new Vector3(0f, 1.5f, 4f));
        }

        // 6. Simpan scene aktif & aset
        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = player;
        return $"OK: PF_Player siap di {activeScene.path} (Layer={playerLayer}) dan prefab disimpan di {PrefabPath}";
    }

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing != -1) return existing;

        Object[] tagAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (tagAssets == null || tagAssets.Length == 0) return -1;

        SerializedObject tagManager = new SerializedObject(tagAssets[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 6; i < layers.arraySize; i++)
        {
            SerializedProperty sp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
        }
        return -1;
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
