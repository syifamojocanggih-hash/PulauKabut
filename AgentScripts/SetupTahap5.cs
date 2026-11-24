using PulauKabut.Core;
using PulauKabut.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupTahap5
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";
    private const string EnvScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Environment.unity";
    private const string PlayerScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Player.unity";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu sebelum setup Tahap 5.");

        // 1. Buka Sandbox_Environment.unity dan pastikan CM_POV1_FirstPerson masuk ke dalam PF_Player.prefab
        Scene envScene = EditorSceneManager.OpenScene(EnvScenePath, OpenSceneMode.Single);
        GameObject player = GameObject.Find("PF_Player");
        if (player == null)
            throw new System.Exception("PF_Player tidak ditemukan di Sandbox_Environment.unity.");

        Transform camTarget = player.transform.Find("CameraTarget");

        // Pindahkan CM_POV1_FirstPerson ke dalam PF_Player agar prefab mandiri & siap ditaruh di scene mana pun
        Transform existingChildCam = player.transform.Find("CM_POV1_FirstPerson");
        GameObject externalCam = GameObject.Find("CM_POV1_FirstPerson");
        if (existingChildCam == null)
        {
            if (externalCam != null && externalCam.transform.parent != player.transform)
            {
                externalCam.transform.SetParent(player.transform, false);
                externalCam.transform.localPosition = Vector3.zero;
                externalCam.transform.localRotation = Quaternion.identity;
            }
            else
            {
                GameObject pov1Obj = new GameObject("CM_POV1_FirstPerson");
                pov1Obj.transform.SetParent(player.transform, false);
                CinemachineCamera pov1Cam = pov1Obj.AddComponent<CinemachineCamera>();
                pov1Cam.Priority.Enabled = true;
                pov1Cam.Priority.Value = 10;
                pov1Cam.Target.TrackingTarget = camTarget;
                pov1Cam.Lens.FieldOfView = 68f;
                pov1Cam.Lens.NearClipPlane = 0.08f;
                pov1Obj.AddComponent<CinemachineHardLockToTarget>();
                pov1Obj.AddComponent<CinemachineRotateWithFollowTarget>();
            }
        }

        // Simpan PF_Player.prefab yang sudah mandiri (kapsul/blocky + PlayerController + PlayerCameraController + CM_POV1_FirstPerson)
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);

        // Pastikan GameManager ada di Sandbox_Environment
        EnsureGameManager();
        EditorSceneManager.MarkSceneDirty(envScene);
        EditorSceneManager.SaveScene(envScene);

        // 2. Pasang juga GameManager, CinemachineBrain, dan PF_Player di Sandbox_Player.unity agar langsung bisa diuji!
        if (System.IO.File.Exists(PlayerScenePath))
        {
            Scene playerScene = EditorSceneManager.OpenScene(PlayerScenePath, OpenSceneMode.Single);
            EnsureGameManager();

            if (Camera.main != null)
            {
                CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>() ?? Camera.main.gameObject.AddComponent<CinemachineBrain>();
                brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.0f);
            }

            if (GameObject.Find("PF_Player") == null && prefabAsset != null)
            {
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                GameObject spawn = GameObject.Find("Player_SpawnPoint");
                inst.transform.position = spawn != null ? spawn.transform.position : new Vector3(0f, 1f, 0f);
            }

            EditorSceneManager.MarkSceneDirty(playerScene);
            EditorSceneManager.SaveScene(playerScene);
        }

        // 3. Kembali buka Sandbox_Environment.unity
        EditorSceneManager.OpenScene(EnvScenePath, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();

        return "OK: GameManager & prefab PF_Player mandiri terpasang di Sandbox_Environment dan Sandbox_Player!";
    }

    private static void EnsureGameManager()
    {
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
    }
}
