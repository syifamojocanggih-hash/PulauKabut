using PulauKabut.Dialog;
using PulauKabut.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CleanupCameraZonesAndDialogCam
{
    private const string SettingsPath = "Assets/_Project/Prefabs/Player/PlayerMovementSettings_Default.asset";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        // 1. Hapus PlotZone_LagoonBridge agar tidak mengganggu saat berjalan di jembatan
        GameObject bridgeZone = GameObject.Find("PlotZone_LagoonBridge");
        if (bridgeZone != null)
        {
            Object.DestroyImmediate(bridgeZone);
        }

        // 2. Naikkan FirstPersonHideDistance ke 1.1 m agar kepala tidak pernah menutupi lensa
        PlayerMovementSettings settings = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(SettingsPath);
        if (settings != null)
        {
            SerializedObject sSo = new SerializedObject(settings);
            sSo.FindProperty("firstPersonHideDistance").floatValue = 1.1f;
            sSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        // 3. Pasang kamera obrolan sinematik khusus untuk NPC Mbah Sauh
        GameObject npc = GameObject.Find("NPC_MbahSauh");
        if (npc != null)
        {
            Transform oldCam = npc.transform.Find("CM_Dialog_MbahSauh");
            if (oldCam != null) Object.DestroyImmediate(oldCam.gameObject);

            GameObject camObj = new GameObject("CM_Dialog_MbahSauh");
            camObj.transform.SetParent(npc.transform, false);
            // Posisi serong kanan-depan Mbah Sauh menatap wajah Mbah Sauh
            camObj.transform.localPosition = new Vector3(1.4f, 1.85f, 3.2f);

            CinemachineCamera vcam = camObj.AddComponent<CinemachineCamera>();
            vcam.Priority.Enabled = true;
            vcam.Priority.Value = 0;
            vcam.Lens.FieldOfView = 45f;

            Transform head = npc.transform.Find("Head");
            vcam.Target.TrackingTarget = head != null ? head : npc.transform;
            vcam.Target.LookAtTarget = head != null ? head : npc.transform;
            vcam.Target.CustomLookAtTarget = true;

            CinemachineRotationComposer composer = camObj.AddComponent<CinemachineRotationComposer>();
            composer.Damping = new Vector2(0.5f, 0.5f);

            NpcInteractable interact = npc.GetComponent<NpcInteractable>();
            if (interact != null)
            {
                SerializedObject npcSo = new SerializedObject(interact);
                npcSo.FindProperty("dialogCamera").objectReferenceValue = vcam;
                npcSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: PlotZone_LagoonBridge dihapus, FirstPersonHideDistance=1.1m, dan kamera dialog Mbah Sauh aktif!";
    }
}
