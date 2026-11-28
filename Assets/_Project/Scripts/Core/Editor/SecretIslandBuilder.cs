using PulauKabut.Lantern;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PulauKabut.Core.Editor
{
    /// <summary>
    /// Tool Editor untuk membangun 1 area Pulau Rahasia yang luas, ikonik, & tersembunyi di perairan barat:
    /// - Petunjuk alam & Jembatan Tuas: Celah karang & hutan cemara penyamar, Perahu Karam, + Jembatan Tarik bertuas [E]
    /// - Pulau Rahasia: Daratan luas (~35m x 32m) bertingkat 3 dengan Gapura Batu, Pondok Penjelajah, & Menara Pelita Kuno
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
            // Arah lokal +Z root diputar -90 derajat (menghadap Barat / -X dunia)
            Vector3 anchorPos = new Vector3(-40.5f, 0.2f, -58f);
            GameObject hubObj = GameObject.Find("01_HUB_DesaPelita");
            if (hubObj == null && GameObject.Find("Player_SpawnPoint_UjungPulau") == null)
            {
                anchorPos = new Vector3(-18f, 0.2f, 0f);
            }

            GameObject root = new GameObject(RootName);
            root.transform.position = anchorPos;
            root.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            Undo.RegisterCreatedObjectUndo(root, "Bangun Pulau Rahasia & Shortcut");

            BuildHiddenEntranceAndDrawbridge(root.transform);
            BuildSecretIsland(root.transform);

            PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabSavePath, InteractionMode.AutomatedAction);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        private static void BuildHiddenEntranceAndDrawbridge(Transform parent)
        {
            GameObject entranceRoot = new GameObject("01_Petunjuk_Dan_Jembatan_Tuas");
            entranceRoot.transform.SetParent(parent, false);

            // Rimbunan tebing karang tinggi & pohon cemara rapat di tepi pantai barat untuk menyamarkan celah masuk
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_01.fbx", entranceRoot.transform,
                "Karang_Penyamaran_01", new Vector3(-4.2f, 0.5f, -0.8f), new Vector3(0f, 35f, 0f), Vector3.one * 2.4f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_02.fbx", entranceRoot.transform,
                "Karang_Penyamaran_02", new Vector3(4.5f, 0.5f, -0.5f), new Vector3(0f, -45f, 0f), Vector3.one * 2.6f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_01.fbx", entranceRoot.transform,
                "Karang_Penyamaran_03", new Vector3(-6.8f, 0.6f, 2.5f), new Vector3(0f, 110f, 0f), Vector3.one * 2.1f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", entranceRoot.transform,
                "Pohon_Penyamaran_01", new Vector3(-4.8f, 0.7f, -2.5f), new Vector3(0f, 12f, 0f), Vector3.one * 1.45f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", entranceRoot.transform,
                "Pohon_Penyamaran_02", new Vector3(4.9f, 0.7f, -2.2f), new Vector3(0f, 95f, 0f), Vector3.one * 1.35f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", entranceRoot.transform,
                "Pohon_Penyamaran_03", new Vector3(-6.2f, 0.7f, -4.4f), new Vector3(0f, 45f, 0f), Vector3.one * 1.25f, false);

            // 1. Perahu Karam miring di balik karang menunjuk ke arah jembatan rahasia
            GameObject boatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_FishingBoat_DesaPelita.fbx");
            if (boatPrefab != null)
            {
                GameObject boat = (GameObject)PrefabUtility.InstantiatePrefab(boatPrefab, entranceRoot.transform);
                boat.name = "Landmark_PerahuKaram";
                boat.transform.localPosition = new Vector3(-2.8f, -0.2f, 2.2f);
                boat.transform.localRotation = Quaternion.Euler(-12f, -18f, 18f);
                EnsureCollider<MeshCollider>(boat);
            }

            // 2. Dermaga kecil tempat tuas jembatan rahasia berada
            GameObject pierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozyPier.fbx");
            if (pierPrefab != null)
            {
                GameObject dock = (GameObject)PrefabUtility.InstantiatePrefab(pierPrefab, entranceRoot.transform);
                dock.name = "Dermaga_Tuas_Rahasia";
                dock.transform.localPosition = new Vector3(0f, 1.4f, 1.0f);
                dock.transform.localScale = new Vector3(1.4f, 1f, 0.65f);
                BoxCollider dockCol = dock.AddComponent<BoxCollider>();
                FitBoxColliderToChildren(dock, dockCol);
            }

            // 3. Jembatan Tarik Utama yang awalnya terangkat (-78 derajat) dan turun ke Pulau Rahasia saat tuas ditarik [E]
            GameObject bridgePivot = new GameObject("Jembatan_Rahasia_Pivot");
            bridgePivot.transform.SetParent(entranceRoot.transform, false);
            bridgePivot.transform.localPosition = new Vector3(0f, 1.55f, 3.8f);

            GameObject bridgeMesh;
            if (pierPrefab != null)
            {
                bridgeMesh = (GameObject)PrefabUtility.InstantiatePrefab(pierPrefab, bridgePivot.transform);
                bridgeMesh.name = "Jembatan_Menuju_PulauRahasia";
                bridgeMesh.transform.localPosition = new Vector3(0f, 0f, 21.8f);
                bridgeMesh.transform.localScale = new Vector3(1.4f, 1f, 4.45f);

                BoxCollider fullCol = bridgeMesh.AddComponent<BoxCollider>();
                FitBoxColliderToChildren(bridgeMesh, fullCol);
            }
            else
            {
                bridgeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bridgeMesh.name = "Jembatan_Menuju_PulauRahasia";
                bridgeMesh.transform.SetParent(bridgePivot.transform, false);
                bridgeMesh.transform.localPosition = new Vector3(0f, 0f, 21.8f);
                bridgeMesh.transform.localScale = new Vector3(2.6f, 0.3f, 44f);
            }

            Collider walkCol = EnsureCollider<BoxCollider>(bridgeMesh);

            // 4. Tiang Tuas Interaktif di samping dermaga rahasia
            GameObject leverObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leverObj.name = "Tuas_Jembatan_Rahasia";
            leverObj.transform.SetParent(entranceRoot.transform, false);
            leverObj.transform.localPosition = new Vector3(1.65f, 2.25f, 2.6f);
            leverObj.transform.localScale = new Vector3(0.45f, 1.5f, 0.45f);

            BoxCollider triggerCol = leverObj.GetComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(3.8f, 2.2f, 3.8f);

            ShortcutBridge shortcut = leverObj.AddComponent<ShortcutBridge>();
            SerializedObject sso = new SerializedObject(shortcut);
            sso.FindProperty("bridgePivot").objectReferenceValue = bridgePivot.transform;
            sso.FindProperty("bridgeWalkCollider").objectReferenceValue = walkCol;
            sso.FindProperty("promptText").stringValue = "Tarik Tuas Jembatan Pulau Rahasia";
            sso.FindProperty("unlockBannerMessage").stringValue = "NEW SECRET LOCATION UNLOCKED";
            sso.ApplyModifiedPropertiesWithoutUndo();

            // Kunang-kunang penunjuk lembut di sekitar Perahu Karam & Tuas
            GameObject kunangPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Props/PF_Kunang.prefab");
            if (kunangPrefab != null)
            {
                GameObject k1 = (GameObject)PrefabUtility.InstantiatePrefab(kunangPrefab, entranceRoot.transform);
                k1.name = "Kunang_Petunjuk_Tuas";
                k1.transform.localPosition = new Vector3(1.65f, 3.1f, 2.6f);

                GameObject k2 = (GameObject)PrefabUtility.InstantiatePrefab(kunangPrefab, entranceRoot.transform);
                k2.name = "Kunang_Petunjuk_UjungPulau";
                k2.transform.localPosition = new Vector3(0f, 2.8f, 45.5f);
            }
        }

        private static void BuildSecretIsland(Transform parent)
        {
            GameObject islandRoot = new GameObject("02_Pulau_Rahasia");
            islandRoot.transform.SetParent(parent, false);
            // Berada ~62m di barat Desa Pelita, terhubung oleh jembatan tarik sepanjang ~44m
            islandRoot.transform.localPosition = new Vector3(-4f, 0f, 62.0f);

            // 1. Daratan Utama Luas (~35m x 32m)
            GameObject islandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozyIsland_Large.fbx");
            if (islandPrefab != null)
            {
                GameObject islandMesh = (GameObject)PrefabUtility.InstantiatePrefab(islandPrefab, islandRoot.transform);
                islandMesh.name = "PulauRahasia_DaratanUtama";
                islandMesh.transform.localPosition = new Vector3(0f, 3.0f, 0f);
                islandMesh.transform.localScale = new Vector3(16.5f, 4.0f, 16.5f);
                EnsureCollider<MeshCollider>(islandMesh);
            }

            // 2. Teras Tengah (diperluas untuk area Gapura Batu & Pondok)
            GameObject smallIslandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Models/Environment/SM_CozyIsland_Small.fbx");
            if (smallIslandPrefab != null)
            {
                GameObject midTerrace = (GameObject)PrefabUtility.InstantiatePrefab(smallIslandPrefab, islandRoot.transform);
                midTerrace.name = "PulauRahasia_TerasTengah";
                midTerrace.transform.localPosition = new Vector3(-2.5f, 4.8f, 6.8f);
                midTerrace.transform.localScale = new Vector3(10.2f, 3.0f, 10.2f);
                EnsureCollider<MeshCollider>(midTerrace);

                // 3. Puncak Rahasia tempat Menara Pelita Kuno & Serpihan Ingatan
                GameObject peakTerrace = (GameObject)PrefabUtility.InstantiatePrefab(smallIslandPrefab, islandRoot.transform);
                peakTerrace.name = "PulauRahasia_Puncak";
                peakTerrace.transform.localPosition = new Vector3(-4.5f, 6.7f, 13.5f);
                peakTerrace.transform.localScale = new Vector3(6.6f, 2.6f, 6.6f);
                EnsureCollider<MeshCollider>(peakTerrace);
            }

            // Landmark Keren: Gapura Batu Kuno berdiri kokoh di atas Teras Tengah tepat di ujung tangga
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyStoneArch.fbx", islandRoot.transform,
                "Gapura_PulauRahasia", new Vector3(-2.0f, 3.75f, 0.2f), new Vector3(0f, -8f, 0f), Vector3.one * 1.15f, true);

            // Pondok Penjelajah Tua di sisi kanan daratan utama
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_VillageHut_DesaPelita.fbx", islandRoot.transform,
                "Pondok_Penjelajah_Rahasia", new Vector3(8.5f, 2.3f, -2.5f), new Vector3(0f, -125f, 0f), Vector3.one * 0.95f, true);

            // Menara Pelita Kuno (miniatur) di Puncak Pulau sebagai landmark misterius
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_AltarTower_Pelita.fbx", islandRoot.transform,
                "Menara_Rahasia", new Vector3(-5.2f, 6.0f, 15.8f), new Vector3(0f, 20f, 0f), Vector3.one * 0.62f, true);

            // Tangga batu lebar berundak (4 anak tangga) menuju Teras Tengah melewati Gapura
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Teras_01", new Vector3(-1.5f, 2.05f, -6.2f), new Vector3(0f, 10f, 0f), new Vector3(2.1f, 1.4f, 2.1f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Teras_02", new Vector3(-1.6f, 2.55f, -4.6f), new Vector3(0f, 35f, 0f), new Vector3(2.1f, 1.4f, 2.1f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Teras_03", new Vector3(-1.7f, 3.05f, -3.0f), new Vector3(0f, 65f, 0f), new Vector3(2.1f, 1.4f, 2.1f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Teras_04", new Vector3(-1.8f, 3.55f, -1.4f), new Vector3(0f, 20f, 0f), new Vector3(2.1f, 1.4f, 2.1f), true);

            // Tangga batu lebar berundak (4 anak tangga) menuju Puncak Menara Rahasia
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Puncak_01", new Vector3(-3.2f, 4.10f, 5.0f), new Vector3(0f, -15f, 0f), new Vector3(2.0f, 1.4f, 2.0f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Puncak_02", new Vector3(-3.5f, 4.65f, 6.4f), new Vector3(0f, 25f, 0f), new Vector3(2.0f, 1.4f, 2.0f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Puncak_03", new Vector3(-3.8f, 5.20f, 7.8f), new Vector3(0f, 55f, 0f), new Vector3(2.0f, 1.4f, 2.0f), true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozySteppingStone.fbx", islandRoot.transform,
                "Tangga_Puncak_04", new Vector3(-4.1f, 5.75f, 9.2f), new Vector3(0f, 15f, 0f), new Vector3(2.0f, 1.4f, 2.0f), true);

            // Hutan Cemara & Karang Besar mengelilingi pulau agar rimbun, megah, & terasa tersembunyi
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_01", new Vector3(-12.5f, 2.1f, -3.5f), new Vector3(0f, 25f, 0f), Vector3.one * 1.6f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_02", new Vector3(-13.8f, 2.1f, 4.2f), new Vector3(0f, 85f, 0f), Vector3.one * 1.45f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_03", new Vector3(13.2f, 2.1f, 3.8f), new Vector3(0f, 140f, 0f), Vector3.one * 1.55f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_04", new Vector3(11.4f, 2.1f, -9.2f), new Vector3(0f, 210f, 0f), Vector3.one * 1.35f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_05", new Vector3(-9.8f, 2.1f, -11.5f), new Vector3(0f, 70f, 0f), Vector3.one * 1.4f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_06", new Vector3(-9.2f, 4.0f, 9.8f), new Vector3(0f, 45f, 0f), Vector3.one * 1.5f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_07", new Vector3(4.5f, 4.0f, 10.2f), new Vector3(0f, 175f, 0f), Vector3.one * 1.3f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_08", new Vector3(-9.5f, 5.9f, 16.5f), new Vector3(0f, 35f, 0f), Vector3.one * 1.45f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyPineTree.fbx", islandRoot.transform,
                "Pohon_Rahasia_09", new Vector3(0.5f, 5.9f, 17.2f), new Vector3(0f, 120f, 0f), Vector3.one * 1.35f, false);

            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_01.fbx", islandRoot.transform,
                "Batu_Rahasia_01", new Vector3(13.8f, 2.0f, -4.5f), new Vector3(0f, 60f, 0f), Vector3.one * 2.2f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_02.fbx", islandRoot.transform,
                "Batu_Rahasia_02", new Vector3(-13.2f, 2.0f, -7.8f), new Vector3(0f, -35f, 0f), Vector3.one * 2.3f, true);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyRock_01.fbx", islandRoot.transform,
                "Batu_Rahasia_03", new Vector3(-10.5f, 3.9f, 12.8f), new Vector3(0f, 150f, 0f), Vector3.one * 1.9f, true);

            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_RedMushroomCluster.fbx", islandRoot.transform,
                "Jamur_Rahasia_01", new Vector3(4.8f, 2.3f, -6.2f), Vector3.zero, Vector3.one * 1.35f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_RedMushroomCluster.fbx", islandRoot.transform,
                "Jamur_Rahasia_02", new Vector3(-5.8f, 4.1f, 5.2f), new Vector3(0f, 85f, 0f), Vector3.one * 1.25f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyBush.fbx", islandRoot.transform,
                "Semak_Rahasia_01", new Vector3(5.2f, 2.25f, 2.4f), Vector3.zero, Vector3.one * 1.45f, false);
            InstantiateProp("Assets/_Project/Art/Models/Environment/SM_CozyBush.fbx", islandRoot.transform,
                "Semak_Rahasia_02", new Vector3(-6.2f, 2.25f, -3.4f), new Vector3(0f, 50f, 0f), Vector3.one * 1.4f, false);

            // Kunang-kunang di sekitar Menara & Gapura Rahasia
            GameObject kunangPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Props/PF_Kunang.prefab");
            if (kunangPrefab != null)
            {
                Vector3[] kunangSpots =
                {
                    new Vector3(-1.2f, 4.2f, 1.5f),
                    new Vector3(6.2f, 3.6f, -1.8f),
                    new Vector3(-4.5f, 7.6f, 13.8f)
                };
                for (int k = 0; k < kunangSpots.Length; k++)
                {
                    GameObject kunang = (GameObject)PrefabUtility.InstantiatePrefab(kunangPrefab, islandRoot.transform);
                    kunang.name = $"Kunang_PulauRahasia_{k + 1:00}";
                    kunang.transform.localPosition = kunangSpots[k];
                }
            }

            // Tungku Pelita isi ulang energi lentera di depan Gapura Pulau Rahasia
            GameObject brazierObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            brazierObj.name = "Pelita_PulauRahasia";
            brazierObj.transform.SetParent(islandRoot.transform, false);
            brazierObj.transform.localPosition = new Vector3(3.4f, 2.75f, -2.5f);
            brazierObj.transform.localScale = new Vector3(0.85f, 0.65f, 0.85f);

            GameObject flameObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flameObj.name = "Api_Pelita";
            flameObj.transform.SetParent(brazierObj.transform, false);
            flameObj.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            flameObj.transform.localScale = new Vector3(0.55f, 0.75f, 0.55f);
            Object.DestroyImmediate(flameObj.GetComponent<Collider>());

            Light brazierLight = flameObj.AddComponent<Light>();
            brazierLight.type = LightType.Point;
            brazierLight.color = new Color(1f, 0.78f, 0.38f);
            brazierLight.range = 14f;
            brazierLight.intensity = 4.5f;

            LanternBrazier brazier = brazierObj.AddComponent<LanternBrazier>();
            SerializedObject bso = new SerializedObject(brazier);
            bso.FindProperty("startLit").boolValue = false;
            bso.FindProperty("requireLitLantern").boolValue = true;
            bso.FindProperty("brazierName").stringValue = "Pelita Pulau Rahasia";
            bso.FindProperty("flameVisual").objectReferenceValue = flameObj;
            bso.FindProperty("brazierLight").objectReferenceValue = brazierLight;
            bso.ApplyModifiedPropertiesWithoutUndo();

            // Hadiah Eksplorasi: Serpihan Ingatan Rahasia di Puncak di depan Menara Pelita Kuno
            GameObject shardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Props/PF_LightShard.prefab");
            if (shardPrefab != null)
            {
                GameObject shard = (GameObject)PrefabUtility.InstantiatePrefab(shardPrefab, islandRoot.transform);
                shard.name = "Secret_MemoryFragment";
                shard.transform.localPosition = new Vector3(-4.2f, 7.1f, 12.6f);

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
            if (existing != null)
            {
                if (!(existing is MeshCollider emc && emc.sharedMesh == null))
                {
                    return existing;
                }
                Object.DestroyImmediate(emc);
            }

            if (typeof(T) == typeof(MeshCollider))
            {
                Collider firstCreated = null;
                foreach (MeshFilter mf in target.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null) continue;
                    MeshCollider mc = mf.gameObject.GetComponent<MeshCollider>();
                    if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    if (firstCreated == null) firstCreated = mc;
                }
                if (firstCreated != null) return firstCreated;
            }

            return target.AddComponent<T>();
        }

        private static void FitBoxColliderToChildren(GameObject root, BoxCollider box)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            if (filters.Length == 0) return;

            Bounds b = new Bounds();
            bool hasBounds = false;
            Matrix4x4 rootInv = root.transform.worldToLocalMatrix;

            foreach (MeshFilter mf in filters)
            {
                if (mf.sharedMesh == null || mf.name.StartsWith("Post_")) continue;
                Bounds mb = mf.sharedMesh.bounds;
                Matrix4x4 toRoot = rootInv * mf.transform.localToWorldMatrix;

                for (int ix = -1; ix <= 1; ix += 2)
                for (int iy = -1; iy <= 1; iy += 2)
                for (int iz = -1; iz <= 1; iz += 2)
                {
                    Vector3 corner = mb.center + Vector3.Scale(mb.extents, new Vector3(ix, iy, iz));
                    Vector3 localPt = toRoot.MultiplyPoint3x4(corner);
                    if (!hasBounds) { b = new Bounds(localPt, Vector3.zero); hasBounds = true; }
                    else b.Encapsulate(localPt);
                }
            }

            if (hasBounds)
            {
                box.center = b.center;
                box.size = new Vector3(b.size.x, Mathf.Max(0.35f, b.size.y), b.size.z);
            }
        }
    }
}
