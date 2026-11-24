using PulauKabut.Lantern;
using PulauKabut.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupBlockyAnimator
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        // Edit langsung isi prefab asset agar reparenting sendi pivot 100% resmi & bersih (tanpa error Prefab instance)
        using (var editScope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
        {
            GameObject player = editScope.prefabContentsRoot;
            Transform visuals = player.transform.Find("Visuals");
            Transform rig = visuals != null ? visuals.Find("RobloxPlain_Temp") : null;
            if (rig == null)
                throw new System.Exception("RobloxPlain_Temp tidak ditemukan di PF_Player.prefab/Visuals.");

            Transform leftLegPivot  = WrapWithPivot(rig, "Leg_Left",  "Pivot_LegLeft",  new Vector3(-0.14f, 0.66f, 0f));
            Transform rightLegPivot = WrapWithPivot(rig, "Leg_Right", "Pivot_LegRight", new Vector3( 0.14f, 0.66f, 0f));
            Transform leftArmPivot  = WrapWithPivot(rig, "Arm_Left",  "Pivot_ArmLeft",  new Vector3(-0.41f, 1.30f, 0f));
            Transform rightArmPivot = WrapWithPivot(rig, "Arm_Right", "Pivot_ArmRight", new Vector3( 0.41f, 1.30f, 0f));
            Transform headPivot     = WrapWithPivot(rig, "Head",      "Pivot_Head",     new Vector3( 0.00f, 1.36f, 0f));

            ReparentKeepWorld(rig, "Eye_Left", headPivot);
            ReparentKeepWorld(rig, "Eye_Right", headPivot);

            // Pindahkan Player_Lantern ke dalam Pivot_ArmRight supaya ikut terangkat saat lengan kanan naik
            Transform lantern = visuals.Find("Player_Lantern");
            if (lantern == null && rightArmPivot != null) lantern = rightArmPivot.Find("Player_Lantern");
            if (lantern != null && rightArmPivot != null)
            {
                lantern.SetParent(rightArmPivot, false);
                lantern.localPosition = new Vector3(0.02f, -0.58f, 0.16f);
                lantern.localRotation = Quaternion.identity;
            }

            BlockyProceduralAnimator anim = player.GetComponent<BlockyProceduralAnimator>() ?? player.AddComponent<BlockyProceduralAnimator>();
            SerializedObject so = new SerializedObject(anim);
            so.FindProperty("playerController").objectReferenceValue = player.GetComponent<PlayerController>();
            so.FindProperty("lanternController").objectReferenceValue = player.GetComponent<LanternController>();
            so.FindProperty("bodyRoot").objectReferenceValue = rig;
            so.FindProperty("leftLegPivot").objectReferenceValue = leftLegPivot;
            so.FindProperty("rightLegPivot").objectReferenceValue = rightLegPivot;
            so.FindProperty("leftArmPivot").objectReferenceValue = leftArmPivot;
            so.FindProperty("rightArmPivot").objectReferenceValue = rightArmPivot;
            so.FindProperty("headPivot").objectReferenceValue = headPivot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Bersihkan override lama di scene agar instance PF_Player persis mengikuti prefab baru
        GameObject scenePlayer = GameObject.Find("PF_Player");
        if (scenePlayer != null && PrefabUtility.IsPartOfPrefabInstance(scenePlayer))
        {
            PrefabUtility.RevertPrefabInstance(scenePlayer, InteractionMode.AutomatedAction);
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Sendi pivot lengan/kaki/kepala + lentera di tangan kanan + BlockyProceduralAnimator bersih di PF_Player.prefab!";
    }

    private static Transform WrapWithPivot(Transform rig, string partName, string pivotName, Vector3 pivotLocalPos)
    {
        Transform existingPivot = rig.Find(pivotName);
        if (existingPivot != null)
        {
            Transform loosePart = rig.Find(partName);
            if (loosePart != null) loosePart.SetParent(existingPivot, true);
            return existingPivot;
        }

        Transform part = rig.Find(partName);
        if (part == null) return null;

        GameObject pivotObj = new GameObject(pivotName);
        pivotObj.transform.SetParent(rig, false);
        pivotObj.transform.localPosition = pivotLocalPos;
        pivotObj.layer = rig.gameObject.layer;

        part.SetParent(pivotObj.transform, true);
        return pivotObj.transform;
    }

    private static void ReparentKeepWorld(Transform rig, string childName, Transform newParent)
    {
        if (newParent == null) return;
        Transform t = rig.Find(childName);
        if (t != null) t.SetParent(newParent, true);
    }
}
