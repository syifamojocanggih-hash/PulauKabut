using PulauKabut.Lantern;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PulauKabut.Core.Editor
{
    /// <summary>
    /// Tool Editor untuk membangun 1 area Pulau Rahasia yang luas & tersembunyi di perairan barat:
    /// - Petunjuk alam: Perahu Karam tersembunyi di balik karang/pohon tepi barat Desa Pelita + jejak batu kabut (FogRevealable)
    /// - Pulau Rahasia: Pulau besar (1.65x) + dataran tinggi kecil, dikelilingi tebing/pohon agar tersembunyi di kabut
    /// - Shortcut: Jembatan pintas yang bisa diturunkan [E] dari Pulau Rahasia
    /// </summary>
    public static class SecretIslandBuilder
    {
        private const string RootName = "--- PULAU RAHASIA & SHORTCUT ---";
        private const string PrefabSavePath = "Assets/_Project/Prefabs/Props/PF_PulauRahasia_Shortcut.prefab";

        [MenuItem("Pulau Kabut/Bangun Pulau Rahasia & Shortcut")]
        public static void BuildSecretIslandInActiveScene()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            // Tempatkan di perairan terbuka sebelah BARAT Desa Pelita (tepi barat pulau utama ~ X=-38.4, Z=-56.4)
            // Arah lokal +Z root diputar -90 derajat (menghadap Barat / -X dunia) agar menjauh ke laut terbuka tanpa overlap!
            Vector3 anchorPos = new Vector3(-40.5f, 0.2f, -58f);
            GameObject hubObj = GameObject.Find("01_HUB_DesaPelita");
            if (hubObj == null && GameObject.Find("Player_SpawnPoint_UjungPulau") == null)
            {
                // Fallback untuk Sandbox_Environment
                anchorPos = new Vector3(-18f, 0.2f, 0f);
            }

            GameObject root = new GameObject(RootName);
            root.transform.position = anchorPos;
            root.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            Undo.RegisterCreatedObjectUndo(root, "Bangun Pulau Rahasia & Shortcut");

            BuildNaturalClues(root.transform);
            BuildSecretIsland(root.transform);
            BuildShortcutBridge(root.transform);

            PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabSavePath, InteractionMode.AutomatedAction);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        private static void BuildNaturalClues(Transform parent)
        {
            GameObject cluesRoot = new GameObject("01_Petunjuk_Alam");
            cluesRoot.transform.SetParent(parent, false);

            // Rimbunan pohon & batu karang di tepi pantai untuk menyamarkan jalan masuk rahasia
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_01.fbx", cluesRoot.transform,
                "Karang_Penyamaran_01", new Vector3(-3.4f, 0.2f, -1.2f), new Vector3(0f, 35f, 0f), Vector3.one * 1.35f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_02.fbx", cluesRoot.transform,
                "Karang_Penyamaran_02", new Vector3(3.6f, 0.2f, -0.8f), new Vector3(0f, -45f, 0f), Vector3.one * 1.4f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", cluesRoot.transform,
                "Pohon_Penyamaran_01", new Vector3(-4.2f, 0.4f, -2.5f), new Vector3(0f, 12f, 0f), Vector3.one * 1.25f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", cluesRoot.transform,
                "Pohon_Penyamaran_02", new Vector3(4.3f, 0.4f, -2.2f), new Vector3(0f, 95f, 0f), Vector3.one * 1.15f, false);

            // 1. Perahu Karam miring di tepi air menunjuk ke barat (+Z lokal = -X dunia) menuju kabut
            GameObject boatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_FishingBoat_DesaPelita.fbx");
            if (boatPrefab != null)
            {
                GameObject boat = (GameObject)PrefabUtility.InstantiatePrefab(boatPrefab, cluesRoot.transform);
                boat.name = "Landmark_PerahuKaram";
                boat.transform.localPosition = new Vector3(0f, -0.25f, 1.5f);
                boat.transform.localRotation = Quaternion.Euler(-11f, 8f, 16f);
                EnsureCollider<MeshCollider>(boat);
            }

            // 2. Jejak batu loncat melengkung jauh ke dalam kabut (3 batu di tengah butuh Lentera [F]!)
            GameObject stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx");
            GameObject kunangPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Props/PF_Kunang.prefab");
            Material litMat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_Project/Prefabs/Player/Materials/MAT_FogPlatform_Lit.mat");
            Material dimMat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_Project/Prefabs/Player/Materials/MAT_FogPlatform_Dim.mat");

            Vector3[] stoneOffsets =
            {
                new Vector3(0.5f, 0.10f, 6.2f),
                new Vector3(-1.3f, 0.15f, 11.0f),
                new Vector3(0.9f, 0.25f, 16.0f),   // FogRevealable 1
                new Vector3(-0.8f, 0.30f, 21.2f),  // FogRevealable 2
                new Vector3(1.1f, 0.35f, 26.2f),   // FogRevealable 3
                new Vector3(-0.4f, 0.30f, 31.0f)
            };

            for (int i = 0; i < stoneOffsets.Length; i++)
            {
                GameObject stone = stonePrefab != null
                    ? (GameObject)PrefabUtility.InstantiatePrefab(stonePrefab, cluesRoot.transform)
                    : GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                stone.name = $"JejakBatu_{i + 1:00}";
                stone.transform.localPosition = stoneOffsets[i];
                stone.transform.localRotation = Quaternion.Euler(0f, i * 42f, 0f);
                stone.transform.localScale = new Vector3(1.45f, 1f, 1.45f);

                Collider col = EnsureCollider<BoxCollider>(stone);
                Renderer rend = stone.GetComponentInChildren<Renderer>();

                if (i >= 2 && i <= 4)
                {
                    stone.name += "_Kabut";
                    FogRevealable reveal = stone.AddComponent<FogRevealable>();
                    SerializedObject so = new SerializedObject(reveal);
                    so.FindProperty("targetCollider").objectReferenceValue = col;
                    so.FindProperty("targetRenderer").objectReferenceValue = rend;
                    so.FindProperty("revealedMaterial").objectReferenceValue = litMat;
                    so.FindProperty("shadowMaterial").objectReferenceValue = dimMat;
                    so.FindProperty("extraRevealMargin").floatValue = 1.3f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                // Kunang-kunang redup sebagai petunjuk alami di tengah kabut
                if (kunangPrefab != null && (i == 0 || i == 3 || i == 5))
                {
                    GameObject kunang = (GameObject)PrefabUtility.InstantiatePrefab(kunangPrefab, cluesRoot.transform);
                    kunang.name = $"Kunang_Petunjuk_{i + 1:00}";
                    kunang.transform.localPosition = stoneOffsets[i] + new Vector3(0f, 1.15f, 0f);
                }
            }
        }

        private static void BuildSecretIsland(Transform parent)
        {
            GameObject islandRoot = new GameObject("02_Pulau_Rahasia");
            islandRoot.transform.SetParent(parent, false);
            // Berada ~48m di luar pantai barat Desa Pelita sehingga benar-benar tertutup kabut dari map utama
            islandRoot.transform.localPosition = new Vector3(0f, 0f, 48.0f);

            // Daratan utama Pulau Rahasia (diperbesar agar luas ~20m x 19m dan berada di atas permukaan air)
            GameObject islandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozyIsland_Large.fbx");
            if (islandPrefab != null)
            {
                GameObject islandMesh = (GameObject)PrefabUtility.InstantiatePrefab(islandPrefab, islandRoot.transform);
                islandMesh.name = "PulauRahasia_DaratanUtama";
                islandMesh.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                islandMesh.transform.localScale = new Vector3(9.5f, 3.2f, 9.5f);
                EnsureCollider<MeshCollider>(islandMesh);
            }

            // Dataran tinggi kedua di bagian belakang pulau agar eksplorasi bertingkat
            GameObject smallIslandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozyIsland_Small.fbx");
            if (smallIslandPrefab != null)
            {
                GameObject upperTerrace = (GameObject)PrefabUtility.InstantiatePrefab(smallIslandPrefab, islandRoot.transform);
                upperTerrace.name = "PulauRahasia_TerasAtas";
                upperTerrace.transform.localPosition = new Vector3(-2.5f, 3.6f, 6.5f);
                upperTerrace.transform.localScale = new Vector3(6.2f, 2.6f, 6.2f);
                EnsureCollider<MeshCollider>(upperTerrace);
            }

            // Rimbunan pohon & batu mengelilingi sisi luar agar pulau terasa tersembunyi & rimbun
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_01", new Vector3(-9.5f, 0.7f, 2.5f), new Vector3(0f, 25f, 0f), Vector3.one * 1.35f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_02", new Vector3(9.2f, 0.7f, 4.8f), new Vector3(0f, 140f, 0f), Vector3.one * 1.25f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_03", new Vector3(-7.8f, 0.8f, -6.5f), new Vector3(0f, 70f, 0f), Vector3.one * 1.2f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_04", new Vector3(6.8f, 0.7f, -7.2f), new Vector3(0f, 210f, 0f), Vector3.one * 1.1f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_05", new Vector3(-6.2f, 2.7f, 11.2f), new Vector3(0f, 45f, 0f), Vector3.one * 1.3f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_01.fbx", islandRoot.transform,
                "Batu_Rahasia_01", new Vector3(8.5f, 0.6f, -2.5f), new Vector3(0f, 60f, 0f), Vector3.one * 1.4f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_02.fbx", islandRoot.transform,
                "Batu_Rahasia_02", new Vector3(-8.4f, 0.6f, -1.8f), new Vector3(0f, -35f, 0f), Vector3.one * 1.35f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_RedMushroomCluster.fbx", islandRoot.transform,
                "Jamur_Rahasia_01", new Vector3(4.4f, 0.75f, 1.8f), Vector3.zero, Vector3.one * 1.2f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_RedMushroomCluster.fbx", islandRoot.transform,
                "Jamur_Rahasia_02", new Vector3(-4.2f, 2.75f, 7.2f), new Vector3(0f, 85f, 0f), Vector3.one * 1.1f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyBush.fbx", islandRoot.transform,
                "Semak_Rahasia_01", new Vector3(3.2f, 0.7f, -4.8f), Vector3.zero, Vector3.one * 1.3f, false);

            // Tangga batu alami menuju Teras Atas
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Teras_01", new Vector3(0.4f, 1.25f, 3.8f), Vector3.zero, new Vector3(1.5f, 1f, 1.5f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Teras_02", new Vector3(-1.2f, 1.95f, 5.4f), new Vector3(0f, 30f, 0f), new Vector3(1.5f, 1f, 1.5f), true);

            // Tungku Pelita isi ulang energi lentera di Pulau Rahasia
            GameObject brazierObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            brazierObj.name = "Pelita_PulauRahasia";
            brazierObj.transform.SetParent(islandRoot.transform, false);
            brazierObj.transform.localPosition = new Vector3(-4.5f, 1.3f, -2.5f);
            brazierObj.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);

            GameObject flameObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flameObj.name = "Api_Pelita";
            flameObj.transform.SetParent(brazierObj.transform, false);
            flameObj.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            flameObj.transform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
            Object.DestroyImmediate(flameObj.GetComponent<Collider>());

            Light brazierLight = flameObj.AddComponent<Light>();
            brazierLight.type = LightType.Point;
            brazierLight.color = new Color(1f, 0.78f, 0.38f);
            brazierLight.range = 12f;
            brazierLight.intensity = 4.5f;

            LanternBrazier brazier = brazierObj.AddComponent<LanternBrazier>();
            SerializedObject bso = new SerializedObject(brazier);
            bso.FindProperty("startLit").boolValue = false;
            bso.FindProperty("requireLitLantern").boolValue = true;
            bso.FindProperty("brazierName").stringValue = "Pelita Pulau Rahasia";
            bso.FindProperty("flameVisual").objectReferenceValue = flameObj;
            bso.FindProperty("brazierLight").objectReferenceValue = brazierLight;
            bso.ApplyModifiedPropertiesWithoutUndo();

            // Hadiah Eksplorasi: Serpihan Ingatan Rahasia di Teras Atas
            GameObject shardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Props/PF_LightShard.prefab");
            if (shardPrefab != null)
            {
                GameObject shard = (GameObject)PrefabUtility.InstantiatePrefab(shardPrefab, islandRoot.transform);
                shard.name = "Secret_MemoryFragment";
                shard.transform.localPosition = new Vector3(-2.5f, 3.65f, 8.8f);

                LightShardPickup pickup = shard.GetComponent<LightShardPickup>();
                if (pickup != null)
                {
                    SerializedObject pso = new SerializedObject(pickup);
                    pso.FindProperty("collectibleId").stringValue = "W1_Memory_SecretIsland";
                    pso.FindProperty("collectibleType").enumValueIndex = (int)CollectibleType.MemoryFragment;
                    pso.FindProperty("promptText").stringValue = "Ambil Serpihan Ingatan Rahasia";
                    pso.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static void BuildShortcutBridge(Transform parent)
        {
            GameObject shortcutRoot = new GameObject("03_Jembatan_Pintas");
            shortcutRoot.transform.SetParent(parent, false);
            // Dipasang di sisi tepi timur pulau rahasia, mengarah lurus kembali (-Z lokal = +X dunia) ke pantai barat Desa Pelita!
            shortcutRoot.transform.localPosition = new Vector3(7.5f, 0.65f, 37.5f);
            shortcutRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject bridgePivot = new GameObject("Bridge_Pivot");
            bridgePivot.transform.SetParent(shortcutRoot.transform, false);
            bridgePivot.transform.localPosition = Vector3.zero;

            GameObject pierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozyPier.fbx");
            GameObject bridgeMesh;
            if (pierPrefab != null)
            {
                bridgeMesh = (GameObject)PrefabUtility.InstantiatePrefab(pierPrefab, bridgePivot.transform);
                bridgeMesh.name = "Bridge_Mesh";
                bridgeMesh.transform.localPosition = new Vector3(0f, 0f, 16.5f);
                bridgeMesh.transform.localScale = new Vector3(1.25f, 1f, 3.4f);
            }
            else
            {
                bridgeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bridgeMesh.name = "Bridge_Mesh";
                bridgeMesh.transform.SetParent(bridgePivot.transform, false);
                bridgeMesh.transform.localPosition = new Vector3(0f, 0f, 16.5f);
                bridgeMesh.transform.localScale = new Vector3(2.4f, 0.3f, 33f);
            }

            Collider walkCol = EnsureCollider<BoxCollider>(bridgeMesh);

            // Tiang tuas interaktif di sisi Pulau Rahasia
            GameObject leverObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leverObj.name = "Tuas_Jembatan_Pintas";
            leverObj.transform.SetParent(shortcutRoot.transform, false);
            leverObj.transform.localPosition = new Vector3(-1.5f, 0.85f, -0.6f);
            leverObj.transform.localScale = new Vector3(0.4f, 1.4f, 0.4f);

            BoxCollider triggerCol = leverObj.GetComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(3.5f, 2f, 3.5f);

            ShortcutBridge shortcut = leverObj.AddComponent<ShortcutBridge>();
            SerializedObject sso = new SerializedObject(shortcut);
            sso.FindProperty("bridgePivot").objectReferenceValue = bridgePivot.transform;
            sso.FindProperty("bridgeWalkCollider").objectReferenceValue = walkCol;
            sso.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void InstantiateProp(string assetPath, Transform parent, string name,
            Vector3 localPos, Vector3 localEuler, Vector3 scale, bool addCollider)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return;

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            obj.name = name;
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = Quaternion.Euler(localEuler);
            obj.transform.localScale = scale;

            if (addCollider)
            {
                EnsureCollider<MeshCollider>(obj);
            }
        }

        private static Collider EnsureCollider<T>(GameObject target) where T : Collider
        {
            Collider existing = target.GetComponentInChildren<Collider>();
            if (existing != null) return existing;
            return target.AddComponent<T>();
        }
    }
}
