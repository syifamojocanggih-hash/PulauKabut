using PulauKabut.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupThirdAndFirstPov
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        GameObject player = GameObject.Find("PF_Player");
        if (player == null)
            throw new System.Exception("PF_Player tidak ditemukan di scene aktif.");

        Transform camTarget = player.transform.Find("CameraTarget");
        Transform fpObj = player.transform.Find("CM_POV1_FirstPerson");
        CinemachineCamera fpCam = fpObj != null ? fpObj.GetComponent<CinemachineCamera>() : null;

        // Buat atau perbarui CM_POV1_ThirdPerson di dalam PF_Player
        Transform tpTrans = player.transform.Find("CM_POV1_ThirdPerson");
        GameObject tpObj = tpTrans != null ? tpTrans.gameObject : new GameObject("CM_POV1_ThirdPerson");
        if (tpTrans == null)
        {
            tpObj.transform.SetParent(player.transform, false);
        }

        CinemachineCamera tpCam = tpObj.GetComponent<CinemachineCamera>() ?? tpObj.AddComponent<CinemachineCamera>();
        tpCam.Priority.Enabled = true;
        tpCam.Priority.Value = 11; // Default mulai di belakang karakter (Third-Person)
        tpCam.Target.TrackingTarget = camTarget;
        tpCam.Lens.FieldOfView = 60f;
        tpCam.Lens.NearClipPlane = 0.1f;

        CinemachineThirdPersonFollow tpFollow = tpObj.GetComponent<CinemachineThirdPersonFollow>() ?? tpObj.AddComponent<CinemachineThirdPersonFollow>();
        tpFollow.CameraDistance = 5.0f;
        tpFollow.ShoulderOffset = new Vector3(0f, 0.35f, 0f);
        tpFollow.Damping = new Vector3(0.12f, 0.18f, 0.12f);
        tpFollow.CameraSide = 0.5f;

        // Anti-tembus dinding untuk Third-Person
        int playerLayer = LayerMask.NameToLayer("Player");
        CinemachineDeoccluder deoccluder = tpObj.GetComponent<CinemachineDeoccluder>() ?? tpObj.AddComponent<CinemachineDeoccluder>();
        deoccluder.CollideAgainst = playerLayer >= 0 ? ~(1 << playerLayer) : ~0;

        // Hubungkan kedua kamera manual ke PlayerCameraController
        PlayerCameraController camCtrl = player.GetComponent<PlayerCameraController>();
        if (camCtrl != null)
        {
            SerializedObject so = new SerializedObject(camCtrl);
            so.FindProperty("thirdPersonCamera").objectReferenceValue = tpCam;
            so.FindProperty("firstPersonCamera").objectReferenceValue = fpCam;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Perhalus durasi blend kamera di Main Camera menjadi 0.65 detik agar responsif saat tekan V
        if (Camera.main != null && Camera.main.TryGetComponent<CinemachineBrain>(out var brain))
        {
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.65f);
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Kamera manual Third-Person (belakang karakter) + First-Person (mata) dengan toggle V siap di PF_Player.prefab!";
    }
}
