using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PulauKabut.Player.Editor
{
    /// <summary>
    /// Editor utility untuk membangun scene uji Sandbox_Player secara otomatis
    /// lengkap dengan lantai, tanjakan, tangga kecil, jurang kecil, dan rintangan.
    /// </summary>
    public static class SandboxPlayerSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Player.unity";
        private const string MaterialsFolder = "Assets/_Project/Scenes/Sandbox/Materials";

        [MenuItem("Pulau Kabut/Build Sandbox_Player Scene")]
        public static void BuildSandboxScene()
        {
            // Konfirmasi jika scene sudah ada agar tidak menimpa tanpa sengaja
            if (File.Exists(ScenePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Timpa Sandbox_Player?",
                    $"Scene sudah ada di:\n{ScenePath}\n\nApakah Anda ingin membangun ulang scene ini?",
                    "Ya, Bangun Ulang",
                    "Batal"
                );

                if (!overwrite)
                {
                    return;
                }
            }

            // Buat scene baru dengan Main Camera dan Directional Light bawaan
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Siapkan material sederhana URP Lit khusus sandbox agar mudah dibedakan secara visual
            Material groundMat = GetOrCreateMaterial("MAT_Sandbox_Ground", new Color(0.32f, 0.45f, 0.34f));
            Material rampMat = GetOrCreateMaterial("MAT_Sandbox_Ramp", new Color(0.58f, 0.45f, 0.32f));
            Material obstacleMat = GetOrCreateMaterial("MAT_Sandbox_Obstacle", new Color(0.45f, 0.48f, 0.52f));
            Material gapMat = GetOrCreateMaterial("MAT_Sandbox_GapPlatform", new Color(0.75f, 0.52f, 0.28f));

            // Root untuk seluruh geometri lingkungan uji
            GameObject environmentRoot = new GameObject("--- ENVIRONMENT ---");

            // 1. Lantai utama (permukaan atas tepat di y = 0)
            CreateBox("Ground_Main", new Vector3(0f, -0.5f, 0f), new Vector3(44f, 1f, 44f), Vector3.zero, groundMat, environmentRoot.transform);

            // 2. Area Tanjakan (tanjakan landai ~20 derajat dan lebih curam ~32 derajat)
            GameObject rampsRoot = new GameObject("Ramps");
            rampsRoot.transform.SetParent(environmentRoot.transform);
            CreateBox("Ramp_Gentle_20deg", new Vector3(-8f, 0.9f, 8f), new Vector3(4f, 0.4f, 6f), new Vector3(-20f, 0f, 0f), rampMat, rampsRoot.transform);
            CreateBox("Platform_RampGentleTop", new Vector3(-8f, 1.0f, 13.5f), new Vector3(4.5f, 2.0f, 5.5f), Vector3.zero, rampMat, rampsRoot.transform);

            CreateBox("Ramp_Steep_32deg", new Vector3(-14f, 1.35f, 8f), new Vector3(4f, 0.4f, 6f), new Vector3(-32f, 0f, 0f), rampMat, rampsRoot.transform);
            CreateBox("Platform_RampSteepTop", new Vector3(-14f, 1.5f, 13.3f), new Vector3(4.5f, 3.0f, 5.5f), Vector3.zero, rampMat, rampsRoot.transform);

            // 3. Area Tangga Kecil (8 anak tangga, tinggi 0.22 m per anak tangga, cocok untuk uji stepOffset)
            GameObject stairsRoot = new GameObject("Stairs_Small");
            stairsRoot.transform.SetParent(environmentRoot.transform);
            const int stepCount = 8;
            const float stepHeight = 0.22f;
            const float stepDepth = 0.45f;
            const float stepWidth = 4f;
            Vector3 stairsOrigin = new Vector3(0f, 0f, 7f);

            for (int i = 0; i < stepCount; i++)
            {
                float currentHeight = (i + 1) * stepHeight;
                Vector3 pos = stairsOrigin + new Vector3(0f, currentHeight * 0.5f, i * stepDepth);
                Vector3 scale = new Vector3(stepWidth, currentHeight, stepDepth);
                CreateBox($"Step_{i + 1:00}", pos, scale, Vector3.zero, rampMat, stairsRoot.transform);
            }

            float totalStairsHeight = stepCount * stepHeight;
            CreateBox(
                "Platform_StairsTop",
                stairsOrigin + new Vector3(0f, totalStairsHeight * 0.5f, stepCount * stepDepth + 2.25f),
                new Vector3(stepWidth, totalStairsHeight, 5f),
                Vector3.zero,
                rampMat,
                stairsRoot.transform
            );

            // 4. Area Jurang Kecil (untuk uji lompat, coyote time, dan jump buffer)
            GameObject gapRoot = new GameObject("Gap_JumpTest");
            gapRoot.transform.SetParent(environmentRoot.transform);
            // Tanjakan naik menuju platform awal jurang
            CreateBox("Gap_AccessRamp", new Vector3(10f, 0.9f, 5f), new Vector3(4f, 0.4f, 6f), new Vector3(-20f, 0f, 0f), gapMat, gapRoot.transform);
            // Platform awal jurang (permukaan atas di y = 2.0)
            CreateBox("Gap_PlatformStart", new Vector3(10f, 1f, 10.5f), new Vector3(5f, 2f, 5.5f), Vector3.zero, gapMat, gapRoot.transform);
            // Celah jurang selebar 3.2 meter di antara z = 13.25 dan z = 16.45
            CreateBox("Gap_PlatformEnd", new Vector3(10f, 1f, 19.2f), new Vector3(5f, 2f, 5.5f), Vector3.zero, gapMat, gapRoot.transform);

            // 5. Kubus Rintangan & Dinding Uji Collision Kamera
            GameObject obstaclesRoot = new GameObject("Obstacles");
            obstaclesRoot.transform.SetParent(environmentRoot.transform);
            CreateBox("Obstacle_Cube_Small_01", new Vector3(-4f, 0.5f, -4f), new Vector3(1.5f, 1f, 1.5f), Vector3.zero, obstacleMat, obstaclesRoot.transform);
            CreateBox("Obstacle_Cube_Medium_02", new Vector3(3.5f, 0.75f, -3.5f), new Vector3(2f, 1.5f, 2f), Vector3.zero, obstacleMat, obstaclesRoot.transform);
            CreateBox("Obstacle_Cube_Tall_03", new Vector3(7f, 1.5f, -6f), new Vector3(2.5f, 3f, 2.5f), Vector3.zero, obstacleMat, obstaclesRoot.transform);
            CreateBox("Obstacle_LowStep_04", new Vector3(-2f, 0.15f, 3.5f), new Vector3(2.5f, 0.3f, 1.5f), Vector3.zero, obstacleMat, obstaclesRoot.transform);
            // Dinding tinggi untuk menguji kamera third-person agar tidak tembus dinding
            CreateBox("CameraTest_Wall", new Vector3(-9f, 2f, -6f), new Vector3(10f, 4f, 0.8f), Vector3.zero, obstacleMat, obstaclesRoot.transform);

            // 6. Penanda titik spawn pemain
            GameObject spawnPoint = new GameObject("Player_SpawnPoint");
            spawnPoint.transform.position = new Vector3(0f, 1f, 0f);

            // Atur posisi Main Camera sementara agar langsung menghadap area uji
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(0f, 4f, -7f);
                Camera.main.transform.LookAt(new Vector3(0f, 1f, 4f));
            }

            // Pastikan direktori tujuan ada lalu simpan scene
            string directory = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[PulauKabut] Scene Sandbox_Player berhasil dibuat di: {ScenePath}");
        }

        private static GameObject CreateBox(
            string name,
            Vector3 position,
            Vector3 scale,
            Vector3 eulerRotation,
            Material material,
            Transform parent)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = scale;
            box.transform.eulerAngles = eulerRotation;
            box.isStatic = true;

            if (material != null)
            {
                MeshRenderer renderer = box.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }

            return box;
        }

        private static Material GetOrCreateMaterial(string materialName, Color baseColor)
        {
            if (!Directory.Exists(MaterialsFolder))
            {
                Directory.CreateDirectory(MaterialsFolder);
            }

            string assetPath = $"{MaterialsFolder}/{materialName}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                urpLit = Shader.Find("Standard");
            }

            Material mat = new Material(urpLit);
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", baseColor);
            }
            else
            {
                mat.color = baseColor;
            }

            AssetDatabase.CreateAsset(mat, assetPath);
            return mat;
        }
    }
}
