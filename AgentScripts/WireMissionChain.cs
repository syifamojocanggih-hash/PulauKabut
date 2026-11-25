using PulauKabut.Core;
using PulauKabut.Dialog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WireMissionChain
{
    public static string Run()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Keluar dari Play Mode dulu.");

        GameObject npc    = GameObject.Find("NPC_MbahSauh");
        GameObject shard1 = GameObject.Find("Shard_01_ShadowPath");
        GameObject shard2 = GameObject.Find("Shard_02_Lagoon");
        GameObject shard3 = GameObject.Find("Shard_03_TowerPeak");
        GameObject tower  = GameObject.Find("POI_05_LanternTower");

        // Rantai 1 -> 2: Setelah bicara Mbah Sauh, arahkan ke Pecahan 1 (Platform Bayangan)
        if (npc != null && shard1 != null)
        {
            NpcInteractable ni = npc.GetComponent<NpcInteractable>();
            if (ni != null)
            {
                SerializedObject so = new SerializedObject(ni);
                so.FindProperty("nextObjectiveTarget").objectReferenceValue = shard1.transform;
                so.FindProperty("nextObjectiveTitle").stringValue = "Nyalakan Lentera [F] & Ambil Pecahan 1";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Rantai 2 -> 3: Setelah Pecahan 1 diambil, arahkan ke Pecahan 2 (Pulau Laguna Barat)
        if (shard1 != null && shard2 != null)
        {
            SetNext(shard1, shard2.transform, "Seberangi Laguna & Ambil Pecahan 2");
        }

        // Rantai 3 -> 4: Setelah Pecahan 2 diambil, barulah arahkan ke Puncak Menara Lentera (Pecahan 3)
        if (shard2 != null && shard3 != null)
        {
            SetNext(shard2, shard3.transform, "Daki Bukit Menuju Menara Lentera");
        }

        // Rantai 4 -> Selesai: Setelah Pecahan 3 di puncak diambil
        if (shard3 != null && tower != null)
        {
            SetNext(shard3, tower.transform, "Puncak Tercapai! Jelajahi Pulau");
        }

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        return "OK: Rantai misi berurutan tersambung (Mbah Sauh -> Pecahan 1 -> Pecahan 2 -> Puncak Menara)!";
    }

    private static void SetNext(GameObject shardObj, Transform target, string title)
    {
        LightShardPickup pickup = shardObj.GetComponent<LightShardPickup>();
        if (pickup == null) return;

        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("nextObjectiveTarget").objectReferenceValue = target;
        so.FindProperty("nextObjectiveTitle").stringValue = title;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
