using PulauKabut.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupDualPovCamera
{
    private const string SettingsPath = "Assets/_Project/Prefabs/Player/PlayerMovementSettings_Default.asset";
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu sebelum setup kamera.");

        PlayerMovementSettings settings = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(SettingsPath);
        GameObject player = GameObject.Find("PF_Player");
        if (player == null || settings == null)
            throw new System.Exception("PF_Player atau PlayerMovementSettings_Default tidak ditemukan.");

        // 1. Pastikan CameraTarget tepat setinggi mata karakter blocky (y = 1.58)
        Transform camTarget = player.transform.Find("CameraTarget");
        if (camTarget == null)
        {
            GameObject ct = new GameObject("CameraTarget");
            ct.transform.SetParent(player.transform, false);
            camTarget = ct.transform;
        }
        camTarget.localPosition = new Vector3(0f, 1.58f, 0f);
        camTarget.localRotation = Quaternion.identity;

        Transform visuals = player.transform.Find("Visuals");

        // 2. Pasang PlayerCameraController pada PF_Player
        PlayerCameraController camCtrl = player.GetComponent<PlayerCameraController>() ?? player.AddComponent<PlayerCameraController>();
        SerializedObject camCtrlSo = new SerializedObject(camCtrl);
        camCtrlSo.FindProperty("settings").objectReferenceValue = settings;
        camCtrlSo.FindProperty("cameraTarget").objectReferenceValue = camTarget;
        if (visuals != null) camCtrlSo.FindProperty("visualsRoot").objectReferenceValue = visuals.gameObject;
        camCtrlSo.FindProperty("lockCursorOnStart").boolValue = true;
        camCtrlSo.ApplyModifiedPropertiesWithoutUndo();

        // Simpan perubahan prefab PF_Player
        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        // 3. Pasang CinemachineBrain di Main Camera (smooth blend 1.0 detik)
        Camera mainCam = Camera.main;
        if (mainCam == null)
            throw new System.Exception("Main Camera tidak ditemukan di scene.");

        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>() ?? mainCam.gameObject.AddComponent<CinemachineBrain>();
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.0f);

        // 4. Buat root kamera Cinemachine agar Hierarchy rapi
        GameObject oldRig = GameObject.Find("--- CAMERAS (2 POV) ---");
        if (oldRig != null) Object.DestroyImmediate(oldRig);

        GameObject camRig = new GameObject("--- CAMERAS (2 POV) ---");

        // 5. POV 1: First-Person murni dari mata karakter (Priority = 10)
        GameObject pov1Obj = new GameObject("CM_POV1_FirstPerson");
        pov1Obj.transform.SetParent(camRig.transform);
        CinemachineCamera pov1Cam = pov1Obj.AddComponent<CinemachineCamera>();
        pov1Cam.Priority.Enabled = true;
        pov1Cam.Priority.Value = 10;
        pov1Cam.Target.TrackingTarget = camTarget;
        pov1Cam.Lens.FieldOfView = 68f;
        pov1Cam.Lens.NearClipPlane = 0.08f;

        pov1Obj.AddComponent<CinemachineHardLockToTarget>();
        pov1Obj.AddComponent<CinemachineRotateWithFollowTarget>();

        int playerLayer = LayerMask.NameToLayer("Player");
        LayerMask collideMask = playerLayer >= 0 ? ~(1 << playerLayer) : ~0;

        // 6. Contoh POV 2 #1: Kamera Plot di area Pos Pondok & Papan Petunjuk (sudut lebar sinematik)
        CreatePlotZone(
            camRig.transform,
            "PlotZone_CampHub",
            new Vector3(0f, 4.5f, -24f),
            new Vector3(22f, 6f, 16f),
            new Vector3(9.5f, 7.5f, -35f),
            camTarget,
            collideMask
        );

        // 7. Contoh POV 2 #2: Kamera Plot di Jembatan Laguna Barat (sudut pandang samping sinematik)
        CreatePlotZone(
            camRig.transform,
            "PlotZone_LagoonBridge",
            new Vector3(-24f, 4.5f, -15.5f),
            new Vector3(13f, 5f, 8f),
            new Vector3(-23.5f, 7.2f, -26.5f),
            camTarget,
            collideMask
        );

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Sistem 2 POV (POV 1 First-Person + 2 contoh POV 2 Kamera Plot Cinemachine 3) siap di scene!";
    }

    private static void CreatePlotZone(
        Transform parent,
        string name,
        Vector3 zoneCenter,
        Vector3 zoneSize,
        Vector3 cameraWorldPos,
        Transform lookAtTarget,
        LayerMask deoccluderMask)
    {
        GameObject zoneObj = new GameObject(name);
        zoneObj.transform.SetParent(parent);
        zoneObj.transform.position = zoneCenter;

        BoxCollider box = zoneObj.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = zoneSize;

        // Kamera POV 2 di dalam zona (Priority = 0 saat nonaktif, naik ke 20 saat pemain masuk)
        GameObject camObj = new GameObject($"CM_POV2_{name}");
        camObj.transform.SetParent(zoneObj.transform);
        camObj.transform.position = cameraWorldPos;

        CinemachineCamera vcam = camObj.AddComponent<CinemachineCamera>();
        vcam.Priority.Enabled = true;
        vcam.Priority.Value = 0;
        vcam.Target.TrackingTarget = lookAtTarget;
        vcam.Lens.FieldOfView = 55f;

        // Rotasi halus selalu menatap karakter pemain
        CinemachineRotationComposer composer = camObj.AddComponent<CinemachineRotationComposer>();
        composer.Damping = new Vector2(0.8f, 0.8f);

        // Anti-tembus dinding agar kamera plot tidak pernah terhalang tembok/pohon
        CinemachineDeoccluder deoccluder = camObj.AddComponent<CinemachineDeoccluder>();
        deoccluder.CollideAgainst = deoccluderMask;

        ContextCameraZone trigger = zoneObj.AddComponent<ContextCameraZone>();
        SerializedObject so = new SerializedObject(trigger);
        so.FindProperty("contextCamera").objectReferenceValue = vcam;
        so.FindProperty("activePriority").intValue = 20;
        so.FindProperty("inactivePriority").intValue = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
