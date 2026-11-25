using PulauKabut.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupFirstPersonLanternHand
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        Material matSkin  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Player/Materials/MAT_Blocky_PlainLight.mat");
        Material matFrame = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Player/Materials/MAT_Lantern_Frame.mat");
        Material matCore  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Player/Materials/MAT_Lantern_Core.mat");

        using (var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
        {
            GameObject player = scope.prefabContentsRoot;
            Transform camTarget = player.transform.Find("CameraTarget");
            if (camTarget == null)
                throw new System.Exception("CameraTarget tidak ditemukan di PF_Player.prefab.");

            Transform oldHand = camTarget.Find("FirstPerson_LanternHand");
            if (oldHand != null) Object.DestroyImmediate(oldHand.gameObject);

            GameObject handRoot = new GameObject("FirstPerson_LanternHand");
            handRoot.transform.SetParent(camTarget, false);
            handRoot.transform.localPosition = new Vector3(0.36f, -0.68f, 0.42f);
            handRoot.transform.localRotation = Quaternion.identity;

            // Lengan kanan blocky menjulur ke depan-kanan
            CreatePart("FP_Arm", new Vector3(0.04f, -0.10f, -0.18f), new Vector3(14f, -10f, 6f), new Vector3(0.16f, 0.16f, 0.48f), matSkin, handRoot.transform);

            // Lentera chunky di ujung tangan kanan
            GameObject lantern = new GameObject("FP_Lantern");
            lantern.transform.SetParent(handRoot.transform, false);
            lantern.transform.localPosition = new Vector3(0f, 0.02f, 0.08f);

            CreatePart("Cap",    new Vector3(0f,  0.11f, 0f), Vector3.zero, new Vector3(0.16f, 0.045f, 0.16f), matFrame, lantern.transform);
            CreatePart("Core",   Vector3.zero,                Vector3.zero, new Vector3(0.12f, 0.18f,  0.12f), matCore,  lantern.transform);
            CreatePart("Bottom", new Vector3(0f, -0.11f, 0f), Vector3.zero, new Vector3(0.16f, 0.045f, 0.16f), matFrame, lantern.transform);

            SetLayerRecursively(handRoot, player.layer);

            // Hubungkan ke PlayerCameraController
            PlayerCameraController camCtrl = player.GetComponent<PlayerCameraController>();
            if (camCtrl != null)
            {
                SerializedObject so = new SerializedObject(camCtrl);
                so.FindProperty("firstPersonHandRoot").objectReferenceValue = handRoot.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Tangan kanan & lentera First-Person (FirstPerson_LanternHand) terpasang di PF_Player.prefab!";
    }

    private static void CreatePart(string name, Vector3 pos, Vector3 rot, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localEulerAngles = rot;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
