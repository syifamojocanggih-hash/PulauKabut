using System.Collections.Generic;
using PulauKabut.Core;
using PulauKabut.Dialog;
using PulauKabut.Lantern;
using UnityEngine;

namespace PulauKabut.UI
{
    /// <summary>
    /// Minimap berbentuk bulat di pojok kiri atas layar (ringan, tanpa kamera kedua, 0 GC di Update).
    /// Menampilkan posisi pemain (panah arah), target misi aktif (ikon emas berkedip),
    /// NPC warga (biru muda), Pecahan Cahaya (kuning), dan Tungku Pelita (oranye).
    /// </summary>
    public class MinimapHUD : MonoBehaviour
    {
        [Header("Pengaturan Minimap Bulat")]
        [Tooltip("Diameter minimap bulat dalam piksel.")]
        [Range(100f, 220f)]
        [SerializeField] private float mapSize = 144f;

        [Tooltip("Jarak dunia (meter) yang dicakup dari pusat hingga tepi lingkaran minimap.")]
        [Min(15f)]
        [SerializeField] private float worldRadius = 52f;

        [Tooltip("Transform pemain (kosong = otomatis cari PlayerController saat Start).")]
        [SerializeField] private Transform playerTransform;

        [SerializeField] private bool showMinimap = true;

        private readonly List<Transform> _npcs = new List<Transform>(8);
        private readonly List<Transform> _shards = new List<Transform>(12);
        private readonly List<Transform> _braziers = new List<Transform>(8);

        private Texture2D _circleBgTex;
        private Texture2D _dotTex;
        private Texture2D _arrowTex;
        private GUIStyle _compassStyle;

        private void Start()
        {
            if (playerTransform == null)
            {
                var pc = FindAnyObjectByType<Player.PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }

            // Cache semua penanda sekali saja saat Start (nol FindObjectOfType di Update!)
            foreach (var npc in FindObjectsByType<NpcInteractable>())
            {
                _npcs.Add(npc.transform);
            }

            foreach (var shard in FindObjectsByType<LightShardPickup>())
            {
                _shards.Add(shard.transform);
            }

            foreach (var brazier in FindObjectsByType<LanternBrazier>())
            {
                _braziers.Add(brazier.transform);
            }
        }

        private void OnDestroy()
        {
            if (_circleBgTex != null) Destroy(_circleBgTex);
            if (_dotTex != null) Destroy(_dotTex);
            if (_arrowTex != null) Destroy(_arrowTex);
        }

        private void OnGUI()
        {
            if (!showMinimap || playerTransform == null || Time.timeScale <= 0f)
            {
                return;
            }

            EnsureTextures();

            const float margin = 14f;
            Rect mapRect = new Rect(margin, margin, mapSize, mapSize);
            Vector2 center = mapRect.center;
            float innerRadius = (mapSize * 0.5f) - 8f;

            // 1. Latar lingkaran minimap + bingkai
            GUI.DrawTexture(mapRect, _circleBgTex);

            Vector3 playerPos = playerTransform.position;

            // 2. Tungku Pelita (oranye hangat)
            for (int i = 0; i < _braziers.Count; i++)
            {
                Transform t = _braziers[i];
                if (t == null || !t.gameObject.activeInHierarchy) continue;
                DrawWorldBlip(t.position, playerPos, center, innerRadius, 6f, new Color(1f, 0.58f, 0.22f, 0.95f), clampToEdge: false);
            }

            // 3. NPC Warga (biru muda)
            for (int i = 0; i < _npcs.Count; i++)
            {
                Transform t = _npcs[i];
                if (t == null || !t.gameObject.activeInHierarchy) continue;
                DrawWorldBlip(t.position, playerPos, center, innerRadius, 7f, new Color(0.42f, 0.88f, 1f, 0.95f), clampToEdge: false);
            }

            // 4. Pecahan Cahaya yang belum diambil (kuning cerah)
            for (int i = 0; i < _shards.Count; i++)
            {
                Transform t = _shards[i];
                if (t == null || !t.gameObject.activeInHierarchy) continue;
                DrawWorldBlip(t.position, playerPos, center, innerRadius, 7f, new Color(1f, 0.95f, 0.45f, 0.95f), clampToEdge: false);
            }

            // 5. Target Misi Aktif (emas berkedip, menempel di tepi lingkaran jika jauh sebagai kompas)
            if (ObjectiveGuide.Instance != null && ObjectiveGuide.Instance.CurrentTarget != null)
            {
                float pulse = 0.85f + Mathf.Sin(Time.unscaledTime * 5f) * 0.15f;
                DrawWorldBlip(
                    ObjectiveGuide.Instance.CurrentTarget.position,
                    playerPos, center, innerRadius - 2f,
                    10f * pulse,
                    new Color(1f, 0.82f, 0.2f, 1f),
                    clampToEdge: true);
            }

            // 6. Panah Arah Pemain di tengah lingkaran (berputar sesuai arah hadap pemain)
            Matrix4x4 prevMatrix = GUI.matrix;
            float yaw = playerTransform.eulerAngles.y;
            GUIUtility.RotateAroundPivot(yaw, center);
            Color prevColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(center.x - 8f, center.y - 8f, 16f, 16f), _arrowTex);
            GUI.color = prevColor;
            GUI.matrix = prevMatrix;

            // 7. Penanda arah Utara ("U") di puncak bingkai lingkaran
            GUI.Label(new Rect(center.x - 10f, mapRect.y + 3f, 20f, 16f), "U", _compassStyle);
        }

        private void DrawWorldBlip(Vector3 targetPos, Vector3 playerPos, Vector2 center, float innerRadius, float size, Color color, bool clampToEdge)
        {
            float dx = targetPos.x - playerPos.x;
            float dz = targetPos.z - playerPos.z;

            // Utara (+Z) ke atas (-Y di layar GUI)
            Vector2 offset = new Vector2(dx / worldRadius, -dz / worldRadius) * innerRadius;
            float dist = offset.magnitude;

            if (dist > innerRadius)
            {
                if (!clampToEdge) return;
                offset = offset.normalized * innerRadius;
            }

            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x + offset.x - size * 0.5f, center.y + offset.y - size * 0.5f, size, size), _dotTex);
            GUI.color = prev;
        }

        private void EnsureTextures()
        {
            if (_circleBgTex == null)
            {
                const int texSize = 192;
                _circleBgTex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
                _circleBgTex.filterMode = FilterMode.Bilinear;
                float half = texSize * 0.5f;
                float outerR = half - 1f;
                float borderInnerR = half - 5f;
                float ring1R = half * 0.62f;
                float ring2R = half * 0.30f;

                Color[] pixels = new Color[texSize * texSize];
                for (int y = 0; y < texSize; y++)
                {
                    for (int x = 0; x < texSize; x++)
                    {
                        float px = x - half + 0.5f;
                        float py = y - half + 0.5f;
                        float r = Mathf.Sqrt(px * px + py * py);

                        if (r > outerR)
                        {
                            pixels[y * texSize + x] = Color.clear;
                        }
                        else if (r > borderInnerR)
                        {
                            // Bingkai luar kuningan hangat
                            float aa = Mathf.Clamp01(outerR - r);
                            pixels[y * texSize + x] = new Color(0.84f, 0.72f, 0.44f, 0.95f * aa);
                        }
                        else
                        {
                            // Latar dalam biru-abu pesisir + garis radar halus
                            Color c = new Color(0.09f, 0.16f, 0.21f, 0.78f);
                            bool onRing = Mathf.Abs(r - ring1R) < 0.9f || Mathf.Abs(r - ring2R) < 0.9f;
                            bool onAxis = (Mathf.Abs(px) < 0.7f || Mathf.Abs(py) < 0.7f) && r < borderInnerR - 4f;
                            if (onRing || onAxis)
                            {
                                c = Color.Lerp(c, new Color(0.55f, 0.74f, 0.80f, 0.45f), 0.32f);
                            }
                            pixels[y * texSize + x] = c;
                        }
                    }
                }
                _circleBgTex.SetPixels(pixels);
                _circleBgTex.Apply();
            }

            if (_dotTex == null)
            {
                const int dSize = 24;
                _dotTex = new Texture2D(dSize, dSize, TextureFormat.RGBA32, false);
                float half = dSize * 0.5f;
                Color[] pixels = new Color[dSize * dSize];
                for (int y = 0; y < dSize; y++)
                {
                    for (int x = 0; x < dSize; x++)
                    {
                        float r = Mathf.Sqrt((x - half + 0.5f) * (x - half + 0.5f) + (y - half + 0.5f) * (y - half + 0.5f));
                        float alpha = Mathf.Clamp01((half - 1.5f) - r);
                        pixels[y * dSize + x] = new Color(1f, 1f, 1f, alpha);
                    }
                }
                _dotTex.SetPixels(pixels);
                _dotTex.Apply();
            }

            if (_arrowTex == null)
            {
                const int aSize = 24;
                _arrowTex = new Texture2D(aSize, aSize, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[aSize * aSize];
                for (int y = 0; y < aSize; y++)
                {
                    for (int x = 0; x < aSize; x++)
                    {
                        // Segitiga panah menunjuk ke atas (y tinggi di Texture2D = atas)
                        float nx = (x / (float)(aSize - 1)) * 2f - 1f;
                        float ny = (y / (float)(aSize - 1)) * 2f - 1f;
                        bool inside = ny <= 0.85f && ny >= -0.65f && Mathf.Abs(nx) <= (0.85f - ny) * 0.52f;
                        bool notch = ny < -0.25f && Mathf.Abs(nx) < (-0.25f - ny) * 0.8f;
                        pixels[y * aSize + x] = (inside && !notch) ? Color.white : Color.clear;
                    }
                }
                _arrowTex.SetPixels(pixels);
                _arrowTex.Apply();
            }

            if (_compassStyle == null)
            {
                _compassStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };
                _compassStyle.normal.textColor = new Color(1f, 0.92f, 0.65f);
            }
        }
    }
}
