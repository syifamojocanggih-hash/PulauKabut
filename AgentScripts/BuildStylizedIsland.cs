using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class BuildStylizedIsland
{
    private const string ScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Environment.unity";
    private const string MaterialsFolder = "Assets/_Project/Scenes/Sandbox/Materials/StylizedIsland";
    private const string MeshesFolder = "Assets/_Project/Scenes/Sandbox/Meshes/StylizedIsland";

    public static string Run()
    {
        EnsureFolder(Path.GetDirectoryName(ScenePath));
        EnsureFolder(MaterialsFolder);
        EnsureFolder(MeshesFolder);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Material Matte Stylized (Smoothness = 0, palet cerah ala Nearsight)
        Material matWater      = CreateStylizedMaterial("MAT_Island_Water",      Hex("#47B8C8"), 0.15f);
        Material matLagoon     = CreateStylizedMaterial("MAT_Island_Lagoon",     Hex("#63CFCC"), 0.12f);
        Material matSand       = CreateStylizedMaterial("MAT_Island_Sand",       Hex("#E8D39B"), 0.0f);
        Material matGrass      = CreateStylizedMaterial("MAT_Island_Grass",      Hex("#7BC950"), 0.0f);
        Material matGrassDark  = CreateStylizedMaterial("MAT_Island_GrassDark",  Hex("#5FAE42"), 0.0f);
        Material matCliff      = CreateStylizedMaterial("MAT_Island_Cliff",      Hex("#B57449"), 0.0f);
        Material matCliffDark  = CreateStylizedMaterial("MAT_Island_CliffDark",  Hex("#965C38"), 0.0f);
        Material matTrunk      = CreateStylizedMaterial("MAT_Island_Trunk",      Hex("#7A4E32"), 0.0f);
        Material matLeafA      = CreateStylizedMaterial("MAT_Island_LeafA",      Hex("#3F9142"), 0.0f);
        Material matLeafB      = CreateStylizedMaterial("MAT_Island_LeafB",      Hex("#56AD4B"), 0.0f);
        Material matBush       = CreateStylizedMaterial("MAT_Island_Bush",       Hex("#4FA348"), 0.0f);
        Material matRock       = CreateStylizedMaterial("MAT_Island_Rock",       Hex("#9BA3A8"), 0.0f);
        Material matWood       = CreateStylizedMaterial("MAT_Island_Wood",       Hex("#A96E42"), 0.0f);
        Material matRoof       = CreateStylizedMaterial("MAT_Island_Roof",       Hex("#D96B43"), 0.0f);
        Material matSignBoard  = CreateStylizedMaterial("MAT_Island_SignBoard",  Hex("#F2E5C6"), 0.0f);
        Material matRope       = CreateStylizedMaterial("MAT_Island_Rope",       Hex("#DEC89B"), 0.0f);
        Material matFlowerY    = CreateStylizedMaterial("MAT_Island_FlowerY",    Hex("#FFD84D"), 0.0f);
        Material matFlowerP    = CreateStylizedMaterial("MAT_Island_FlowerP",    Hex("#FF7EA8"), 0.0f);
        Material matLantern    = CreateStylizedMaterial("MAT_Island_Lantern",    Hex("#FFE58F"), 0.1f, true);
        Material matPlayerRef  = CreateStylizedMaterial("MAT_Player_ScaleRef",   Hex("#4A8FE7"), 0.0f);

        // 2. Mesh Low-Poly Flat-Shaded skala ~140x140 m
        Mesh meshSandBase     = SaveMesh("MESH_Island_SandBase",     BuildChunkyPrism(18, 68f, 75f, 1.8f, 11, 0.13f));
        Mesh meshGrassMain    = SaveMesh("MESH_Island_GrassMain",    BuildChunkyPrism(18, 58f, 62f, 2.2f, 22, 0.11f));

        // Tebing Utara (2 tingkat: teras tengah + puncak menara lentera)
        Mesh meshNorthMid     = SaveMesh("MESH_Cliff_NorthMid",      BuildChunkyPrism(10, 22f, 26f, 6.4f, 31, 0.13f));
        Mesh meshNorthMidTop  = SaveMesh("MESH_Cliff_NorthMidTop",   BuildChunkyPrism(10, 22.4f, 22.4f, 0.65f, 31, 0.13f));
        Mesh meshNorthPeak    = SaveMesh("MESH_Cliff_NorthPeak",     BuildChunkyPrism(9,  12.5f, 15.5f, 6.2f, 32, 0.14f));
        Mesh meshNorthPeakTop = SaveMesh("MESH_Cliff_NorthPeakTop",  BuildChunkyPrism(9,  12.8f, 12.8f, 0.6f, 32, 0.14f));

        // Tebing Timur (dataran tinggi bertingkat)
        Mesh meshEastBase     = SaveMesh("MESH_Cliff_EastBase",      BuildChunkyPrism(9,  16f, 19f, 5.2f, 41, 0.14f));
        Mesh meshEastTop      = SaveMesh("MESH_Cliff_EastTop",       BuildChunkyPrism(9,  16.4f, 16.4f, 0.6f, 41, 0.14f));

        // Tebing Barat Laut (latar belakang laguna mangrove)
        Mesh meshWestBase     = SaveMesh("MESH_Cliff_WestBase",      BuildChunkyPrism(8,  14f, 17f, 4.8f, 51, 0.14f));
        Mesh meshWestTop      = SaveMesh("MESH_Cliff_WestTop",       BuildChunkyPrism(8,  14.4f, 14.4f, 0.6f, 51, 0.14f));

        // Pulau kecil di dalam laguna barat & pulau batu lepas pantai
        Mesh meshIslet        = SaveMesh("MESH_Island_LagoonIslet",  BuildChunkyPrism(8,  7.5f, 9.0f, 1.5f, 61, 0.12f));
        Mesh meshSeaStack     = SaveMesh("MESH_Island_SeaStack",     BuildChunkyPrism(7,  4.5f, 7.2f, 7.5f, 65, 0.18f));

        // Mesh vegetasi & prop
        Mesh meshCone         = SaveMesh("MESH_LowPoly_Cone",        BuildFlatCone(6, 1.0f, 1.85f));
        Mesh meshRock         = SaveMesh("MESH_LowPoly_Rock",        BuildFlatRock(77));
        Mesh meshBush         = SaveMesh("MESH_LowPoly_Bush",        BuildFlatIcosahedron(0.9f));
        Mesh meshCylinder     = SaveMesh("MESH_LowPoly_Cylinder",    BuildChunkyPrism(6, 0.35f, 0.44f, 1.0f, 88, 0.0f));

        // 3. Root Hierarchy rapi berdasarkan zona/POI
        GameObject root = new GameObject("--- PULAU KABUT (~140x140m STYLIZED) ---");
        GameObject terrainRoot = CreateEmpty("01_Terrain_And_Water", root.transform);
        GameObject cliffsRoot  = CreateEmpty("02_Terraced_Cliffs_And_Ramps", root.transform);
        GameObject pathsRoot   = CreateEmpty("03_Paths_And_Bridges", root.transform);
        GameObject poiRoot     = CreateEmpty("04_POIs_And_Landmarks", root.transform);
        GameObject natureRoot  = CreateEmpty("05_Trees_Bushes_Rocks", root.transform);

        // 4. Lautan luas (700 x 700 m)
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "Ocean_Water";
        ocean.transform.SetParent(terrainRoot.transform);
        ocean.transform.position = Vector3.zero;
        ocean.transform.localScale = new Vector3(70f, 1f, 70f);
        ocean.GetComponent<MeshRenderer>().sharedMaterial = matWater;
        ocean.isStatic = true;

        // 5. Pantai Pasir & Daratan Rumput Utama (permukaan rumput tepat di y = 2.6)
        CreateMeshObj("Island_SandBase", meshSandBase, matSand, new Vector3(0f, 0.55f, 0f), Vector3.zero, Vector3.one, terrainRoot.transform, true);
        CreateMeshObj("Island_GrassMain", meshGrassMain, matGrass, new Vector3(0f, 1.50f, 0f), Vector3.zero, Vector3.one, terrainRoot.transform, true);

        // 6. Tebing Bertingkat (semua menancap mantap ke dalam tanah y = 1.5)
        // Tebing Utara Level 1 (permukaan di y = 7.7)
        CreateMeshObj("Cliff_NorthMid_Base", meshNorthMid, matCliff, new Vector3(0f, 4.2f, 28f), new Vector3(0f, 12f, 0f), Vector3.one, cliffsRoot.transform, true);
        CreateMeshObj("Cliff_NorthMid_Grass", meshNorthMidTop, matGrass, new Vector3(0f, 7.6f, 28f), new Vector3(0f, 12f, 0f), Vector3.one, cliffsRoot.transform, true);

        // Tebing Utara Level 2 / Puncak (permukaan di y = 13.5)
        CreateMeshObj("Cliff_NorthPeak_Base", meshNorthPeak, matCliffDark, new Vector3(2f, 10.2f, 33f), new Vector3(0f, -15f, 0f), Vector3.one, cliffsRoot.transform, true);
        CreateMeshObj("Cliff_NorthPeak_Grass", meshNorthPeakTop, matGrassDark, new Vector3(2f, 13.45f, 33f), new Vector3(0f, -15f, 0f), Vector3.one, cliffsRoot.transform, true);

        // Tebing Timur (permukaan di y = 6.4)
        CreateMeshObj("Cliff_East_Base", meshEastBase, matCliff, new Vector3(31f, 3.6f, 6f), new Vector3(0f, 32f, 0f), Vector3.one, cliffsRoot.transform, true);
        CreateMeshObj("Cliff_East_Grass", meshEastTop, matGrassDark, new Vector3(31f, 6.35f, 6f), new Vector3(0f, 32f, 0f), Vector3.one, cliffsRoot.transform, true);

        // Tebing Barat Laut (permukaan di y = 6.0)
        CreateMeshObj("Cliff_West_Base", meshWestBase, matCliffDark, new Vector3(-31f, 3.4f, 18f), new Vector3(0f, -24f, 0f), Vector3.one, cliffsRoot.transform, true);
        CreateMeshObj("Cliff_West_Grass", meshWestTop, matGrass, new Vector3(-31f, 5.95f, 18f), new Vector3(0f, -24f, 0f), Vector3.one, cliffsRoot.transform, true);

        // Tanjakan penghubung antar tingkat (landai ~18-20 derajat, nyaman didaki pemain)
        CreateRamp("Ramp_MainToEast",      new Vector3(16.5f, 4.35f, -3.5f), new Vector3(-18f, 58f, 0f),  new Vector3(4.5f, 0.6f, 13.5f), matSand, cliffsRoot.transform);
        CreateRamp("Ramp_MainToNorthMid",  new Vector3(-10.5f, 5.05f, 7.5f), new Vector3(-20f, 22f, 0f),  new Vector3(4.8f, 0.6f, 15.5f), matSand, cliffsRoot.transform);
        CreateRamp("Ramp_NorthMidToPeak",  new Vector3(-9.5f, 10.45f, 27.5f),new Vector3(-22f, 76f, 0f),  new Vector3(4.2f, 0.6f, 15.0f), matSand, cliffsRoot.transform);
        CreateRamp("Ramp_MainToWestCliff", new Vector3(-24.5f, 4.25f, 3.5f), new Vector3(-18f, -20f, 0f), new Vector3(4.2f, 0.6f, 12.5f), matSand, cliffsRoot.transform);

        // 7. POI 03: Laguna Mangrove Dangkal di sisi Barat (kolam toska + pulau kecil + jembatan kayu)
        GameObject lagoonPool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lagoonPool.name = "POI_03_LagoonWater";
        lagoonPool.transform.SetParent(terrainRoot.transform);
        lagoonPool.transform.position = new Vector3(-31f, 2.58f, -15f);
        lagoonPool.transform.localScale = new Vector3(26f, 0.08f, 22f);
        lagoonPool.GetComponent<MeshRenderer>().sharedMaterial = matLagoon;
        lagoonPool.isStatic = true;

        CreateMeshObj("POI_03_LagoonIslet", meshIslet, matGrass, new Vector3(-34f, 2.65f, -16f), new Vector3(0f, 25f, 0f), Vector3.one, terrainRoot.transform, true);
        CreateWoodenBridge("Bridge_To_LagoonIslet", new Vector3(-19.5f, 2.85f, -15.5f), new Vector3(-28.0f, 3.25f, -15.8f), matWood, pathsRoot.transform);

        // 8. Jalan Setapak Pasir menghubungkan seluruh POI
        CreatePathStrip("Path_PierToHub", new Vector3[]
        {
            new Vector3(0f, 2.62f, -54f),
            new Vector3(0.5f, 2.62f, -46f),
            new Vector3(-1.0f, 2.62f, -38f),
            new Vector3(0.8f, 2.62f, -30f),
            new Vector3(0f, 2.62f, -22f)
        }, 4.6f, matSand, pathsRoot.transform);

        CreatePathStrip("Path_HubToEast", new Vector3[]
        {
            new Vector3(4f, 2.62f, -18f),
            new Vector3(9f, 2.62f, -12f),
            new Vector3(13f, 2.62f, -7f)
        }, 3.8f, matSand, pathsRoot.transform);

        CreatePathStrip("Path_HubToWest", new Vector3[]
        {
            new Vector3(-5f, 2.62f, -19f),
            new Vector3(-12f, 2.62f, -17f),
            new Vector3(-18f, 2.62f, -15.5f)
        }, 3.8f, matSand, pathsRoot.transform);

        CreatePathStrip("Path_HubToNorthRamp", new Vector3[]
        {
            new Vector3(-1f, 2.62f, -15f),
            new Vector3(-5f, 2.62f, -7f),
            new Vector3(-11f, 2.62f, 0f)
        }, 4.0f, matSand, pathsRoot.transform);

        // 9. POI Landmarks & Props
        // POI 01: Dermaga Awal di Selatan
        CreateWoodenPier(new Vector3(0f, 1.45f, -66f), matWood, poiRoot.transform);
        CreateRopeFenceLine(new Vector3(-4.5f, 2.6f, -52f), new Vector3(-4.2f, 2.6f, -34f), 6, matWood, matRope, poiRoot.transform, "Fence_EntranceLeft");
        CreateRopeFenceLine(new Vector3(4.5f, 2.6f, -52f),  new Vector3(4.8f, 2.6f, -40f),  4, matWood, matRope, poiRoot.transform, "Fence_EntranceRight");
        CreateSignboard(new Vector3(5.2f, 2.6f, -36.5f), -28f, matWood, matSignBoard, poiRoot.transform, "Signboard_Welcome");

        // POI 02: Pos Perkemahan / Hub NPC di Tengah-Selatan
        CreateStylizedHut("POI_02_Hut_A", new Vector3(-9.5f, 2.6f, -24.5f), 28f, matWood, matSignBoard, matRoof, poiRoot.transform);
        CreateStylizedHut("POI_02_Hut_B", new Vector3(11.5f, 2.6f, -25.5f), -35f, matWood, matSignBoard, matRoof, poiRoot.transform);
        CreateLanternPost("POI_02_HubLantern", new Vector3(3.2f, 2.6f, -21.5f), 0f, matWood, matLantern, poiRoot.transform);

        // POI 04: Gardu Lentera di Dataran Tinggi Timur
        CreateLanternPost("POI_04_EastLantern", new Vector3(29f, 6.65f, 8f), -45f, matWood, matLantern, poiRoot.transform);
        CreateSignboard(new Vector3(25.5f, 6.65f, 4.5f), 40f, matWood, matSignBoard, poiRoot.transform, "Signboard_EastLookout");

        // POI 05: Menara Lentera Utama di Puncak Utara (terlihat dari seluruh pulau!)
        CreateLighthouseTower(new Vector3(2f, 13.75f, 33f), matSignBoard, matWood, matRoof, matLantern, poiRoot.transform);

        // Pulau batu lepas pantai agar laut tidak terasa kosong
        CreateMeshObj("SeaStack_SW", meshSeaStack, matCliffDark, new Vector3(-86f, 1.5f, -55f), new Vector3(0f, 15f, 0f), new Vector3(1.4f, 1.2f, 1.4f), terrainRoot.transform, false);
        CreateMeshObj("SeaStack_SE", meshSeaStack, matCliff,     new Vector3(88f, 1.0f, -48f),  new Vector3(0f, 60f, 0f), new Vector3(1.1f, 0.9f, 1.1f), terrainRoot.transform, false);
        CreateMeshObj("SeaStack_NE", meshSeaStack, matCliffDark, new Vector3(78f, 2.0f, 72f),   new Vector3(0f, 110f, 0f),new Vector3(1.8f, 1.5f, 1.6f), terrainRoot.transform, false);
        CreateMeshObj("SeaStack_NW", meshSeaStack, matCliff,     new Vector3(-82f, 1.5f, 68f),  new Vector3(0f, 40f, 0f), new Vector3(1.5f, 1.3f, 1.5f), terrainRoot.transform, false);

        // 10. Vegetasi Prosedural Rapi (Pohon, Semak, Batu) dengan jarak aman dari jalan & tebing
        PopulateNature(meshCylinder, meshCone, meshBush, meshRock, matTrunk, matLeafA, matLeafB, matBush, matFlowerY, matFlowerP, matRock, natureRoot.transform);

        // 11. Spawn Point + Kapsul Pembanding Skala Pemain (tinggi 1.8m)
        GameObject spawn = new GameObject("Player_SpawnPoint");
        spawn.transform.position = new Vector3(0f, 3.6f, -48f);

        GameObject playerRef = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerRef.name = "Player_ScaleReference_1_8m";
        playerRef.transform.position = new Vector3(0f, 3.5f, -44f);
        playerRef.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f); // tinggi total = 1.8 meter
        playerRef.GetComponent<MeshRenderer>().sharedMaterial = matPlayerRef;

        // 12. Lighting, Kabut Pastel, dan Kamera
        ConfigureStylizedLightingAndCamera();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return $"OK: {ScenePath} (Scale ~140x140m)";
    }

    private static void PopulateNature(
        Mesh trunkMesh, Mesh coneMesh, Mesh bushMesh, Mesh rockMesh,
        Material matTrunk, Material matLeafA, Material matLeafB,
        Material matBush, Material matFlowerY, Material matFlowerP, Material matRock,
        Transform parent)
    {
        System.Random rng = new System.Random(2026);
        int treeIdx = 0;

        // Pohon di dataran utama (radius 16..52 m, menghindari koridor jalan tengah, laguna, dan tapak tebing)
        for (int i = 0; i < 95 && treeIdx < 52; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float dist = 12f + (float)rng.NextDouble() * 41f;
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;

            // Hindari jalan utama selatan-tengah
            if (Mathf.Abs(x) < 9.5f && z < -10f) continue;
            // Hindari area pos NPC hub
            if (Vector2.Distance(new Vector2(x, z), new Vector2(0f, -22f)) < 16f) continue;
            // Hindari tapak tebing utara, timur, barat laut, dan laguna barat
            if (Vector2.Distance(new Vector2(x, z), new Vector2(0f, 28f)) < 28f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(31f, 6f)) < 21f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(-31f, 18f)) < 19f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(-31f, -15f)) < 16f) continue;
            // Hindari tanjakan
            if (Vector2.Distance(new Vector2(x, z), new Vector2(16.5f, -3.5f)) < 9f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(-10.5f, 7.5f)) < 10f) continue;

            float scale = 1.05f + (float)rng.NextDouble() * 0.65f;
            treeIdx++;
            CreateStylizedTree($"Tree_Main_{treeIdx:00}", new Vector3(x, 2.6f, z), scale, trunkMesh, coneMesh, matTrunk, (treeIdx % 2 == 0) ? matLeafB : matLeafA, parent);
        }

        // Pohon di atas tebing & pulau laguna (posisi terkurasi)
        var curatedTrees = new (Vector3 pos, float scale, bool alt)[]
        {
            // Pulau Laguna Barat
            (new Vector3(-36.5f, 3.35f, -18.0f), 1.35f, false),
            (new Vector3(-31.5f, 3.35f, -13.5f), 1.05f, true),
            // Tebing Timur (y = 6.65)
            (new Vector3(36.5f, 6.65f, 11.5f), 1.3f, false),
            (new Vector3(38.0f, 6.65f, 2.0f),  1.15f, true),
            (new Vector3(26.0f, 6.65f, 14.0f), 1.25f, false),
            (new Vector3(32.0f, 6.65f, -2.5f), 1.0f, true),
            // Tebing Barat Laut (y = 6.25)
            (new Vector3(-35.5f, 6.25f, 22.0f), 1.4f, false),
            (new Vector3(-27.0f, 6.25f, 23.0f), 1.15f, true),
            (new Vector3(-36.0f, 6.25f, 14.0f), 1.2f, true),
            // Tebing Utara Teras Tengah (y = 7.9)
            (new Vector3(-13.5f, 7.9f, 22.0f), 1.3f, false),
            (new Vector3(14.5f,  7.9f, 21.0f), 1.25f, true),
            (new Vector3(16.0f,  7.9f, 31.0f), 1.1f, false),
            // Puncak Utara (y = 13.75)
            (new Vector3(-4.5f, 13.75f, 36.5f), 1.25f, false),
            (new Vector3(8.5f,  13.75f, 35.0f), 1.15f, true),
            (new Vector3(5.0f,  13.75f, 26.5f), 1.0f, false)
        };
        for (int i = 0; i < curatedTrees.Length; i++)
        {
            var t = curatedTrees[i];
            CreateStylizedTree($"Tree_Highland_{i + 1:00}", t.pos, t.scale, trunkMesh, coneMesh, matTrunk, t.alt ? matLeafB : matLeafA, parent);
        }

        // Semak berbunga di sepanjang jalan dan tepi area
        Vector3[] bushes = new Vector3[]
        {
            new Vector3(-6.2f, 2.6f, -46f), new Vector3(6.5f, 2.6f, -44f),
            new Vector3(-5.8f, 2.6f, -36f), new Vector3(7.2f, 2.6f, -31f),
            new Vector3(-14f,  2.6f, -21f), new Vector3(15f,  2.6f, -20f),
            new Vector3(-8f,   2.6f, -12f), new Vector3(9f,   2.6f, -6f),
            new Vector3(-18f,  2.6f, -8f),  new Vector3(-22f, 2.6f, -24f),
            new Vector3(-33f,  3.35f,-19f), new Vector3(24f,  2.6f, -16f),
            new Vector3(33f,   6.65f, 9.5f),new Vector3(-29f, 6.25f, 16f),
            new Vector3(-3f,   7.9f, 16f),  new Vector3(7f,   13.75f, 30f)
        };
        for (int i = 0; i < bushes.Length; i++)
        {
            CreateBushCluster($"Bush_{i + 1:00}", bushes[i], bushMesh, matBush, (i % 2 == 0) ? matFlowerY : matFlowerP, parent);
        }

        // Batu chunky di garis pantai & kaki tebing
        for (int i = 0; i < 20; i++)
        {
            float a = (i / 20f) * Mathf.PI * 2f + 0.15f;
            // Jangan taruh batu persis di depan dermaga selatan
            if (Mathf.Sin(a) < -0.92f && Mathf.Abs(Mathf.Cos(a)) < 0.25f) continue;

            float r = 56f + (float)rng.NextDouble() * 8f;
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 1.35f, Mathf.Sin(a) * r);
            Vector3 s = new Vector3(1.8f + (float)rng.NextDouble() * 2.2f, 1.2f + (float)rng.NextDouble() * 1.4f, 1.8f + (float)rng.NextDouble() * 2.0f);
            CreateMeshObj($"Rock_Shore_{i + 1:00}", rockMesh, matRock, p, new Vector3(0f, (float)rng.NextDouble() * 360f, 0f), s, parent, true);
        }
    }

    private static void ConfigureStylizedLightingAndCamera()
    {
        Light[] lights = Object.FindObjectsByType<Light>();
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.transform.eulerAngles = new Vector3(42f, -32f, 0f);
                l.color = Hex("#FFF5DC");
                l.intensity = 1.25f;
                l.shadowStrength = 0.52f;
            }
        }

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Hex("#A9DCFA");
        RenderSettings.ambientEquatorColor = Hex("#E3F0DC");
        RenderSettings.ambientGroundColor = Hex("#7F8C6D");

        // Kabut disesuaikan dengan skala 140m (mulai di 55m agar dekat tetap jernih, jauh berkabut lembut)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Hex("#CBEAF7");
        RenderSettings.fogStartDistance = 55f;
        RenderSettings.fogEndDistance = 230f;

        // Main Camera di belakang kapsul pembanding skala, menghadap ke seluruh pulau & menara lentera di kejauhan
        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#9FD8F7");
            cam.farClipPlane = 600f;
            cam.transform.position = new Vector3(0.8f, 6.2f, -52.5f);
            cam.transform.LookAt(new Vector3(0f, 5.5f, 10f));
        }

        // Scene View menghadap keseluruhan pulau dari sudut udara 3/4
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.LookAt(new Vector3(0f, 5f, 0f), Quaternion.Euler(32f, 22f, 0f), 115f);
            SceneView.lastActiveSceneView.Repaint();
        }
    }

    private static void CreateRamp(string name, Vector3 pos, Vector3 euler, Vector3 scale, Material mat, Transform parent)
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = name;
        ramp.transform.SetParent(parent);
        ramp.transform.position = pos;
        ramp.transform.eulerAngles = euler;
        ramp.transform.localScale = scale;
        ramp.GetComponent<MeshRenderer>().sharedMaterial = mat;
        ramp.isStatic = true;
    }

    private static void CreatePathStrip(string name, Vector3[] points, float width, Material mat, Transform parent)
    {
        GameObject holder = CreateEmpty(name, parent);
        int idx = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];
            int steps = Mathf.Max(2, Mathf.CeilToInt(Vector3.Distance(a, b) / (width * 0.72f)));
            for (int s = 0; s <= steps; s++)
            {
                if (i > 0 && s == 0) continue;
                Vector3 p = Vector3.Lerp(a, b, (float)s / steps);
                GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                patch.name = $"Patch_{++idx:00}";
                patch.transform.SetParent(holder.transform);
                patch.transform.position = p;
                patch.transform.localScale = new Vector3(width, 0.06f, width * 1.1f);
                patch.GetComponent<MeshRenderer>().sharedMaterial = mat;
                Object.DestroyImmediate(patch.GetComponent<Collider>());
                patch.isStatic = true;
            }
        }
    }

    private static void CreateWoodenBridge(string name, Vector3 start, Vector3 end, Material matWood, Transform parent)
    {
        GameObject bridge = CreateEmpty(name, parent);
        int planks = 10;
        Vector3 dir = (end - start).normalized;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        for (int i = 0; i <= planks; i++)
        {
            float t = (float)i / planks;
            Vector3 p = Vector3.Lerp(start, end, t);
            p.y += Mathf.Sin(t * Mathf.PI) * 0.45f; // sedikit melengkung manis

            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = $"BridgePlank_{i + 1:00}";
            plank.transform.SetParent(bridge.transform);
            plank.transform.position = p;
            plank.transform.rotation = rot;
            plank.transform.localScale = new Vector3(3.2f, 0.16f, 0.75f);
            plank.GetComponent<MeshRenderer>().sharedMaterial = matWood;
            plank.isStatic = true;
        }
    }

    private static void CreateStylizedHut(string name, Vector3 pos, float rotY, Material matWood, Material matWall, Material matRoof, Transform parent)
    {
        GameObject hut = CreateEmpty(name, parent);
        hut.transform.position = pos;
        hut.transform.eulerAngles = new Vector3(0f, rotY, 0f);

        // Badan rumah
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Hut_Body";
        body.transform.SetParent(hut.transform);
        body.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        body.transform.localScale = new Vector3(4.2f, 3.0f, 4.2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = matWall;
        body.isStatic = true;

        // Pintu kayu
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Hut_Door";
        door.transform.SetParent(hut.transform);
        door.transform.localPosition = new Vector3(0f, 1.05f, 2.12f);
        door.transform.localScale = new Vector3(1.1f, 2.1f, 0.12f);
        door.GetComponent<MeshRenderer>().sharedMaterial = matWood;
        door.isStatic = true;

        // Atap miring chunky (2 papan besar)
        GameObject roofL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofL.name = "Hut_Roof_L";
        roofL.transform.SetParent(hut.transform);
        roofL.transform.localPosition = new Vector3(-1.35f, 3.7f, 0f);
        roofL.transform.localEulerAngles = new Vector3(0f, 0f, 32f);
        roofL.transform.localScale = new Vector3(3.4f, 0.28f, 5.0f);
        roofL.GetComponent<MeshRenderer>().sharedMaterial = matRoof;
        roofL.isStatic = true;

        GameObject roofR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofR.name = "Hut_Roof_R";
        roofR.transform.SetParent(hut.transform);
        roofR.transform.localPosition = new Vector3(1.35f, 3.7f, 0f);
        roofR.transform.localEulerAngles = new Vector3(0f, 0f, -32f);
        roofR.transform.localScale = new Vector3(3.4f, 0.28f, 5.0f);
        roofR.GetComponent<MeshRenderer>().sharedMaterial = matRoof;
        roofR.isStatic = true;
    }

    private static void CreateLighthouseTower(Vector3 pos, Material matWall, Material matWood, Material matRoof, Material matLantern, Transform parent)
    {
        GameObject tower = CreateEmpty("POI_05_LanternTower", parent);
        tower.transform.position = pos;

        // Fondasi & badan menara
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "Tower_Base";
        baseObj.transform.SetParent(tower.transform);
        baseObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        baseObj.transform.localScale = new Vector3(5.2f, 0.6f, 5.2f);
        baseObj.GetComponent<MeshRenderer>().sharedMaterial = matWood;
        baseObj.isStatic = true;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Tower_Body";
        body.transform.SetParent(tower.transform);
        body.transform.localPosition = new Vector3(0f, 5.2f, 0f);
        body.transform.localScale = new Vector3(3.6f, 4.6f, 3.6f);
        body.GetComponent<MeshRenderer>().sharedMaterial = matWall;
        body.isStatic = true;

        // Ruang lentera menyala
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lamp.name = "Tower_LanternCore";
        lamp.transform.SetParent(tower.transform);
        lamp.transform.localPosition = new Vector3(0f, 10.6f, 0f);
        lamp.transform.localScale = new Vector3(2.8f, 1.1f, 2.8f);
        lamp.GetComponent<MeshRenderer>().sharedMaterial = matLantern;
        lamp.isStatic = true;

        // Atap menara merah-terakota
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "Tower_Roof";
        roof.transform.SetParent(tower.transform);
        roof.transform.localPosition = new Vector3(0f, 12.1f, 0f);
        roof.transform.localScale = new Vector3(4.2f, 0.55f, 4.2f);
        roof.GetComponent<MeshRenderer>().sharedMaterial = matRoof;
        roof.isStatic = true;
    }

    private static void CreateLanternPost(string name, Vector3 pos, float rotY, Material matWood, Material matLantern, Transform parent)
    {
        GameObject shrine = CreateEmpty(name, parent);
        shrine.transform.position = pos;
        shrine.transform.eulerAngles = new Vector3(0f, rotY, 0f);

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.transform.SetParent(shrine.transform);
        post.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        post.transform.localScale = new Vector3(0.28f, 3.4f, 0.28f);
        post.GetComponent<MeshRenderer>().sharedMaterial = matWood;
        post.isStatic = true;

        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.transform.SetParent(shrine.transform);
        arm.transform.localPosition = new Vector3(0f, 3.2f, -0.55f);
        arm.transform.localScale = new Vector3(0.22f, 0.22f, 1.3f);
        arm.GetComponent<MeshRenderer>().sharedMaterial = matWood;
        arm.isStatic = true;

        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamp.transform.SetParent(shrine.transform);
        lamp.transform.localPosition = new Vector3(0f, 2.75f, -1.0f);
        lamp.transform.localScale = new Vector3(0.45f, 0.55f, 0.45f);
        lamp.GetComponent<MeshRenderer>().sharedMaterial = matLantern;
        lamp.isStatic = true;
    }

    private static void CreateStylizedTree(string name, Vector3 pos, float scale, Mesh trunkMesh, Mesh coneMesh, Material matTrunk, Material matLeaf, Transform parent)
    {
        GameObject tree = new GameObject(name);
        tree.transform.SetParent(parent);
        tree.transform.position = pos;
        tree.transform.localScale = Vector3.one * scale;
        tree.isStatic = true;

        CreateMeshObj("Trunk", trunkMesh, matTrunk, Vector3.up * 1.1f, Vector3.zero, new Vector3(0.85f, 2.2f, 0.85f), tree.transform, true, false);
        CreateMeshObj("Canopy_Bottom", coneMesh, matLeaf, Vector3.up * 2.0f, new Vector3(0f, 10f, 0f), new Vector3(1.85f, 1.35f, 1.85f), tree.transform, false, false);
        CreateMeshObj("Canopy_Mid",    coneMesh, matLeaf, Vector3.up * 3.1f, new Vector3(0f, 35f, 0f), new Vector3(1.45f, 1.22f, 1.45f), tree.transform, false, false);
        CreateMeshObj("Canopy_Top",    coneMesh, matLeaf, Vector3.up * 4.05f, new Vector3(0f, 60f, 0f), new Vector3(0.95f, 1.1f, 0.95f), tree.transform, false, false);
    }

    private static void CreateBushCluster(string name, Vector3 pos, Mesh bushMesh, Material matBush, Material matFlower, Transform parent)
    {
        GameObject cluster = CreateEmpty(name, parent);
        cluster.transform.position = pos;

        CreateMeshObj("Bush_Main", bushMesh, matBush, new Vector3(0f, 0.5f, 0f), Vector3.zero, new Vector3(1.25f, 0.95f, 1.25f), cluster.transform, false, false);
        CreateMeshObj("Bush_Side", bushMesh, matBush, new Vector3(0.75f, 0.4f, 0.3f), new Vector3(0f, 40f, 0f), new Vector3(0.85f, 0.72f, 0.85f), cluster.transform, false, false);
        CreateMeshObj("Flower_Dot", bushMesh, matFlower, new Vector3(0.3f, 0.98f, -0.25f), Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), cluster.transform, false, false);
    }

    private static void CreateWoodenPier(Vector3 pos, Material matWood, Transform parent)
    {
        GameObject pier = CreateEmpty("POI_01_WoodenPier", parent);
        pier.transform.position = pos;

        for (int i = 0; i < 11; i++)
        {
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = $"Plank_{i + 1:00}";
            plank.transform.SetParent(pier.transform);
            plank.transform.localPosition = new Vector3(0f, 0f, i * 1.05f);
            plank.transform.localScale = new Vector3(4.2f, 0.18f, 0.88f);
            plank.GetComponent<MeshRenderer>().sharedMaterial = matWood;
            plank.isStatic = true;
        }

        float[] zOffsets = { 0.5f, 3.8f, 7.0f, 10.2f };
        foreach (float z in zOffsets)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.transform.SetParent(pier.transform);
                post.transform.localPosition = new Vector3(side * 1.9f, -0.55f, z);
                post.transform.localScale = new Vector3(0.28f, 1.4f, 0.28f);
                post.GetComponent<MeshRenderer>().sharedMaterial = matWood;
                post.isStatic = true;
            }
        }
    }

    private static void CreateRopeFenceLine(Vector3 start, Vector3 end, int posts, Material matWood, Material matRope, Transform parent, string name)
    {
        GameObject fence = CreateEmpty(name, parent);
        Vector3 prevPos = Vector3.zero;
        for (int i = 0; i < posts; i++)
        {
            float t = (posts == 1) ? 0f : (float)i / (posts - 1);
            Vector3 p = Vector3.Lerp(start, end, t);

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.SetParent(fence.transform);
            post.transform.position = p + Vector3.up * 0.6f;
            post.transform.localScale = new Vector3(0.2f, 0.62f, 0.2f);
            post.GetComponent<MeshRenderer>().sharedMaterial = matWood;
            post.isStatic = true;

            if (i > 0)
            {
                Vector3 a = prevPos + Vector3.up * 0.92f;
                Vector3 b = p + Vector3.up * 0.92f;
                GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rope.transform.SetParent(fence.transform);
                rope.transform.position = (a + b) * 0.5f;
                rope.transform.up = (b - a).normalized;
                rope.transform.localScale = new Vector3(0.08f, Vector3.Distance(a, b) * 0.5f, 0.08f);
                rope.GetComponent<MeshRenderer>().sharedMaterial = matRope;
                Object.DestroyImmediate(rope.GetComponent<Collider>());
                rope.isStatic = true;
            }
            prevPos = p;
        }
    }

    private static void CreateSignboard(Vector3 pos, float rotY, Material matWood, Material matBoard, Transform parent, string name)
    {
        GameObject sign = CreateEmpty(name, parent);
        sign.transform.position = pos;
        sign.transform.eulerAngles = new Vector3(0f, rotY, 0f);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.transform.SetParent(sign.transform);
            post.transform.localPosition = new Vector3(side * 1.1f, 1.05f, 0f);
            post.transform.localScale = new Vector3(0.2f, 2.1f, 0.2f);
            post.GetComponent<MeshRenderer>().sharedMaterial = matWood;
            post.isStatic = true;
        }

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.transform.SetParent(sign.transform);
        frame.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        frame.transform.localScale = new Vector3(2.6f, 1.4f, 0.16f);
        frame.GetComponent<MeshRenderer>().sharedMaterial = matWood;
        frame.isStatic = true;

        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.transform.SetParent(sign.transform);
        board.transform.localPosition = new Vector3(0f, 1.75f, -0.02f);
        board.transform.localScale = new Vector3(2.35f, 1.15f, 0.17f);
        board.GetComponent<MeshRenderer>().sharedMaterial = matBoard;
        board.isStatic = true;
    }

    private static GameObject CreateMeshObj(string name, Mesh mesh, Material mat, Vector3 pos, Vector3 rot, Vector3 scale, Transform parent, bool addCollider, bool worldPos = true)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        if (worldPos)
        {
            go.transform.position = pos;
            go.transform.eulerAngles = rot;
        }
        else
        {
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = rot;
        }
        go.transform.localScale = scale;
        go.isStatic = true;

        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        if (addCollider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
        return go;
    }

    private static Mesh BuildChunkyPrism(int sides, float topRadius, float bottomRadius, float height, int seed, float jitter)
    {
        System.Random rng = new System.Random(seed);
        Vector3[] topRing = new Vector3[sides];
        Vector3[] botRing = new Vector3[sides];
        float halfH = height * 0.5f;

        for (int i = 0; i < sides; i++)
        {
            float angle = (i / (float)sides) * Mathf.PI * 2f;
            float rMod = 1f + ((float)rng.NextDouble() * 2f - 1f) * jitter;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            topRing[i] = new Vector3(cos * topRadius * rMod, halfH, sin * topRadius * rMod);
            botRing[i] = new Vector3(cos * bottomRadius * rMod, -halfH, sin * bottomRadius * rMod);
        }

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        Vector3 topCenter = new Vector3(0f, halfH, 0f);
        Vector3 botCenter = new Vector3(0f, -halfH, 0f);

        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            AddFlatTri(verts, tris, topCenter, topRing[next], topRing[i]);
            AddFlatTri(verts, tris, botCenter, botRing[i], botRing[next]);
            AddFlatTri(verts, tris, topRing[i], topRing[next], botRing[next]);
            AddFlatTri(verts, tris, topRing[i], botRing[next], botRing[i]);
        }

        Mesh m = new Mesh();
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private static Mesh BuildFlatCone(int sides, float radius, float height)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        Vector3 tip = new Vector3(0f, height, 0f);
        Vector3 baseCenter = Vector3.zero;
        Vector3[] ring = new Vector3[sides];

        for (int i = 0; i < sides; i++)
        {
            float a = (i / (float)sides) * Mathf.PI * 2f;
            ring[i] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
        }

        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            AddFlatTri(verts, tris, tip, ring[next], ring[i]);
            AddFlatTri(verts, tris, baseCenter, ring[i], ring[next]);
        }

        Mesh m = new Mesh();
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private static Mesh BuildFlatIcosahedron(float radius)
    {
        float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
        Vector3[] raw = new Vector3[]
        {
            new Vector3(-1,  t,  0), new Vector3( 1,  t,  0), new Vector3(-1, -t,  0), new Vector3( 1, -t,  0),
            new Vector3( 0, -1,  t), new Vector3( 0,  1,  t), new Vector3( 0, -1, -t), new Vector3( 0,  1, -t),
            new Vector3( t,  0, -1), new Vector3( t,  0,  1), new Vector3(-t,  0, -1), new Vector3(-t,  0,  1)
        };
        for (int i = 0; i < raw.Length; i++) raw[i] = raw[i].normalized * radius;

        int[] idx = new int[]
        {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
            1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
            4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        for (int i = 0; i < idx.Length; i += 3)
            AddFlatTri(verts, tris, raw[idx[i]], raw[idx[i + 1]], raw[idx[i + 2]]);

        Mesh m = new Mesh();
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private static Mesh BuildFlatRock(int seed)
    {
        Mesh baseMesh = BuildFlatIcosahedron(1f);
        Vector3[] verts = baseMesh.vertices;
        System.Random rng = new System.Random(seed);

        Dictionary<Vector3, Vector3> map = new Dictionary<Vector3, Vector3>();
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 key = new Vector3(Mathf.Round(verts[i].x * 100f), Mathf.Round(verts[i].y * 100f), Mathf.Round(verts[i].z * 100f));
            if (!map.TryGetValue(key, out Vector3 displaced))
            {
                float s = 0.78f + (float)rng.NextDouble() * 0.42f;
                displaced = verts[i] * s;
                if (displaced.y < -0.45f) displaced.y = -0.45f;
                map[key] = displaced;
            }
            verts[i] = displaced;
        }
        baseMesh.vertices = verts;
        baseMesh.RecalculateNormals();
        baseMesh.RecalculateBounds();
        return baseMesh;
    }

    private static void AddFlatTri(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 c)
    {
        int start = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
    }

    private static Mesh SaveMesh(string name, Mesh mesh)
    {
        string path = $"{MeshesFolder}/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Material CreateStylizedMaterial(string name, Color color, float smoothness, bool emissive = false)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(urpLit);
            AssetDatabase.CreateAsset(mat, path);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else mat.color = color;

        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);

        if (emissive && mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.8f);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreateEmpty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go;
    }

    private static void EnsureFolder(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
