using System.Collections.Generic;
using PulauKabut.Core;
using PulauKabut.Dialog;
using PulauKabut.Lantern;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PulauKabut.UI
{
    /// <summary>
    /// Minimap oktagon bergaya GTA di pojok kiri atas (bingkai emas + tema HUD Lengkap):
    /// - Menampilkan peta top-down nyata (pulau, dermaga, rumah, air rawa) lewat kamera ortografis ringan 256x256 tanpa kabut.
    /// - Berputar real-time mengikuti arah hadap kamera/pemain (panah pemain selalu menunjuk ke depan/atas!).
    /// - Badge Utara [U] dan semua titik (Misi, NPC, Pecahan Cahaya, Tungku) ikut berputar sinkron.
    /// </summary>
    public class MinimapHUD : MonoBehaviour
    {
        [Header("Pengaturan Minimap GTA")]
        [Tooltip("Ukuran diameter oktagon minimap dalam piksel.")]
        [Range(110f, 220f)]
        [SerializeField] private float mapSize = 148f;

        [Tooltip("Radius cakupan dunia (meter) dari pusat ke tepi peta.")]
        [Min(18f)]
        [SerializeField] private float worldRadius = 38f;

        [Tooltip("Transform pemain (kosong = otomatis cari PlayerController saat Start).")]
        [SerializeField] private Transform playerTransform;

        [SerializeField] private bool showMinimap = true;

        private readonly List<Transform> _npcs = new List<Transform>(8);
        private readonly List<Transform> _shards = new List<Transform>(12);
        private readonly List<Transform> _braziers = new List<Transform>(8);

        private static readonly int FogOfWarTexId = Shader.PropertyToID("_FogOfWarTex");
        private static readonly int PlayerPosId = Shader.PropertyToID("_PlayerPos");
        private static readonly int WorldBoundsId = Shader.PropertyToID("_WorldBounds");

        private const int FowSize = 128;
        private static readonly Vector4 WorldBounds = new Vector4(-90f, -140f, 200f, 260f);

        private Transform _cameraTransform;
        private Camera _minimapCam;
        private RenderTexture _minimapRT;
        private bool _savedFogState;

        private Texture2D _fowTex;
        private Color32[] _fowPixels;
        private float _fowStampTimer;
        private Vector2 _lastStampXZ = new Vector2(9999f, 9999f);

        private Texture2D _octFrameTex;
        private Texture2D _octMaskTex;
        private Texture2D _dotTex;
        private Texture2D _arrowTex;
        private Texture2D _coneTex;
        private Material _maskMat;
        private GUIStyle _northBadgeStyle;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var pc = FindAnyObjectByType<Player.PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            // Cache seluruh penanda sekali saat Start (0 FindObjectOfType di Update)
            foreach (var npc in FindObjectsByType<NpcInteractable>()) _npcs.Add(npc.transform);
            foreach (var shard in FindObjectsByType<LightShardPickup>()) _shards.Add(shard.transform);
            foreach (var brazier in FindObjectsByType<LanternBrazier>()) _braziers.Add(brazier.transform);

            CreateMinimapCamera();
            InitFogOfWar();
            if (playerTransform != null)
            {
                StampExploredArea(playerTransform.position, 13f);
            }
        }

        private void InitFogOfWar()
        {
            _fowTex = new Texture2D(FowSize, FowSize, TextureFormat.Alpha8, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _fowPixels = new Color32[FowSize * FowSize];
            // Awalnya 100% tertutup (alpha = 0) -> baru terbuka setelah dijelajahi!
            _fowTex.SetPixels32(_fowPixels);
            _fowTex.Apply(false);
        }

        private void StampExploredArea(Vector3 worldPos, float revealRadiusMeters)
        {
            if (_fowTex == null || _fowPixels == null) return;

            float u = (worldPos.x - WorldBounds.x) / WorldBounds.z;
            float v = (worldPos.z - WorldBounds.y) / WorldBounds.w;
            int cx = Mathf.RoundToInt(u * (FowSize - 1));
            int cy = Mathf.RoundToInt(v * (FowSize - 1));
            int rx = Mathf.CeilToInt((revealRadiusMeters / WorldBounds.z) * FowSize);
            int ry = Mathf.CeilToInt((revealRadiusMeters / WorldBounds.w) * FowSize);

            int minX = Mathf.Max(0, cx - rx);
            int maxX = Mathf.Min(FowSize - 1, cx + rx);
            int minY = Mathf.Max(0, cy - ry);
            int maxY = Mathf.Min(FowSize - 1, cy + ry);

            bool dirty = false;
            for (int y = minY; y <= maxY; y++)
            {
                float dy = (y - cy) / (float)Mathf.Max(1, ry);
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x - cx) / (float)Mathf.Max(1, rx);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > 1f) continue;

                    // Tepi halus agar batas area terjelajah terlihat alami di peta
                    byte val = (byte)Mathf.RoundToInt(Mathf.Clamp01((1f - d) * 2.5f) * 255f);
                    int idx = y * FowSize + x;
                    if (val > _fowPixels[idx].a)
                    {
                        _fowPixels[idx].a = val;
                        dirty = true;
                    }
                }
            }

            if (dirty)
            {
                _fowTex.SetPixels32(_fowPixels);
                _fowTex.Apply(false);
            }
        }

        private bool IsExplored(Vector3 worldPos)
        {
            if (_fowPixels == null) return true;
            float u = (worldPos.x - WorldBounds.x) / WorldBounds.z;
            float v = (worldPos.z - WorldBounds.y) / WorldBounds.w;
            int x = Mathf.Clamp(Mathf.RoundToInt(u * (FowSize - 1)), 0, FowSize - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(v * (FowSize - 1)), 0, FowSize - 1);
            return _fowPixels[y * FowSize + x].a > 90;
        }

        private void CreateMinimapCamera()
        {
            _minimapRT = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
            {
                name = "RT_Minimap_GTA",
                filterMode = FilterMode.Bilinear,
                antiAliasing = 1
            };
            _minimapRT.Create();

            GameObject camObj = new GameObject("MinimapCamera_GTA");
            camObj.transform.SetParent(transform, false);
            _minimapCam = camObj.AddComponent<Camera>();
            _minimapCam.orthographic = true;
            _minimapCam.orthographicSize = worldRadius;
            _minimapCam.nearClipPlane = 1f;
            _minimapCam.farClipPlane = 160f;
            _minimapCam.clearFlags = CameraClearFlags.SolidColor;
            _minimapCam.backgroundColor = new Color(0.08f, 0.21f, 0.25f, 1f);
            _minimapCam.targetTexture = _minimapRT;
            _minimapCam.depth = -10f;
            // Sembunyikan layer Player (6) dari kamera minimap karena kita pakai panah emas di tengah
            _minimapCam.cullingMask = ~(1 << 6);

            // Matikan Post-Processing & Shadow pada kamera minimap agar tetap 150+ FPS!
            var urpData = _minimapCam.GetUniversalAdditionalCameraData();
            if (urpData != null)
            {
                urpData.renderPostProcessing = false;
                urpData.renderShadows = false;
                urpData.antialiasing = AntialiasingMode.None;
            }
        }

        private void LateUpdate()
        {
            if (_minimapCam == null || playerTransform == null) return;

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            float camYaw = _cameraTransform != null ? _cameraTransform.eulerAngles.y : playerTransform.eulerAngles.y;
            Vector3 p = playerTransform.position;

            // Kamera minimap selalu tepat di atas pemain & berputar mengikuti arah hadap kamera (gaya GTA!)
            _minimapCam.transform.position = new Vector3(p.x, p.y + 65f, p.z);
            _minimapCam.transform.rotation = Quaternion.Euler(90f, camYaw, 0f);
            _minimapCam.orthographicSize = worldRadius;

            // Buka peta (Fog of War) secara bertahap hanya pada area yang sudah dijelajahi pemain
            _fowStampTimer += Time.deltaTime;
            if (_fowStampTimer >= 0.15f)
            {
                _fowStampTimer = 0f;
                Vector2 curXZ = new Vector2(p.x, p.z);
                if (Vector2.SqrMagnitude(curXZ - _lastStampXZ) > 1.5f || (LanternController.ActiveLantern != null && LanternController.ActiveLantern.IsLit))
                {
                    _lastStampXZ = curXZ;
                    float revealR = (LanternController.ActiveLantern != null && LanternController.ActiveLantern.IsLit) ? 20f : 12f;
                    StampExploredArea(p, revealR);
                }
            }
        }

        // Matikan kabut sementara HANYA saat kamera minimap merender agar peta tetap jelas terbaca!
        private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            if (cam == _minimapCam)
            {
                _savedFogState = RenderSettings.fog;
                RenderSettings.fog = false;
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            if (cam == _minimapCam)
            {
                RenderSettings.fog = _savedFogState;
            }
        }

        private void OnDestroy()
        {
            if (_minimapCam != null) Destroy(_minimapCam.gameObject);
            if (_minimapRT != null) { _minimapRT.Release(); Destroy(_minimapRT); }
            if (_fowTex != null) Destroy(_fowTex);
            if (_octFrameTex != null) Destroy(_octFrameTex);
            if (_octMaskTex != null) Destroy(_octMaskTex);
            if (_dotTex != null) Destroy(_dotTex);
            if (_arrowTex != null) Destroy(_arrowTex);
            if (_coneTex != null) Destroy(_coneTex);
            if (_maskMat != null) Destroy(_maskMat);
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
            float innerRadius = (mapSize * 0.5f) - 10f;

            float camYaw = _cameraTransform != null ? _cameraTransform.eulerAngles.y : playerTransform.eulerAngles.y;
            float yawRad = camYaw * Mathf.Deg2Rad;
            float cosY = Mathf.Cos(yawRad);
            float sinY = Mathf.Sin(yawRad);

            // 1. Render peta live top-down di dalam oktagon (tertutup Fog of War sampai dijelajahi!)
            if (_minimapRT != null && Event.current.type == EventType.Repaint)
            {
                Rect innerRect = new Rect(mapRect.x + 4f, mapRect.y + 4f, mapRect.width - 8f, mapRect.height - 8f);
                if (_maskMat != null)
                {
                    Vector3 p = playerTransform.position;
                    _maskMat.SetTexture(FogOfWarTexId, _fowTex);
                    _maskMat.SetVector(PlayerPosId, new Vector4(p.x, p.z, worldRadius, yawRad));
                    _maskMat.SetVector(WorldBoundsId, WorldBounds);
                    Graphics.DrawTexture(innerRect, _minimapRT, _maskMat);
                }
                else
                {
                    GUI.DrawTexture(innerRect, _minimapRT);
                }
            }

            // 2. Overlay bingkai oktagon emas + tint teal tipis + cincin radar halus
            GUI.DrawTexture(mapRect, _octFrameTex);

            Vector3 playerPos = playerTransform.position;

            // 3. Tungku Pelita (hanya tampil jika wilayahnya sudah dijelajahi)
            for (int i = 0; i < _braziers.Count; i++)
            {
                Transform t = _braziers[i];
                if (t == null || !t.gameObject.activeInHierarchy || !IsExplored(t.position)) continue;
                DrawRotatedBlip(t.position, playerPos, cosY, sinY, center, innerRadius, 7f, new Color(1f, 0.58f, 0.22f, 0.98f), clampToEdge: false);
            }

            // 4. NPC Warga (hanya tampil jika wilayahnya sudah dijelajahi)
            for (int i = 0; i < _npcs.Count; i++)
            {
                Transform t = _npcs[i];
                if (t == null || !t.gameObject.activeInHierarchy || !IsExplored(t.position)) continue;
                DrawRotatedBlip(t.position, playerPos, cosY, sinY, center, innerRadius, 8f, StylizedUITheme.MintAccent, clampToEdge: false);
            }

            // 5. Pecahan Cahaya (hanya tampil jika wilayahnya sudah dijelajahi)
            for (int i = 0; i < _shards.Count; i++)
            {
                Transform t = _shards[i];
                if (t == null || !t.gameObject.activeInHierarchy || !IsExplored(t.position)) continue;
                DrawRotatedBlip(t.position, playerPos, cosY, sinY, center, innerRadius, 8f, new Color(1f, 0.95f, 0.45f, 0.98f), clampToEdge: false);
            }

            // 6. Target Misi Aktif (emas berkedip, menempel di tepi oktagon jika jauh sebagai kompas GTA!)
            if (ObjectiveGuide.Instance != null && ObjectiveGuide.Instance.CurrentTarget != null)
            {
                float pulse = 0.85f + Mathf.Sin(Time.unscaledTime * 5f) * 0.15f;
                DrawRotatedBlip(
                    ObjectiveGuide.Instance.CurrentTarget.position,
                    playerPos, cosY, sinY, center, innerRadius - 2f,
                    11f * pulse,
                    StylizedUITheme.GoldAccent,
                    clampToEdge: true);
            }

            // 7. Sorotan cahaya senter selalu menunjuk KE ATAS (depan arah pandangmu!)
            bool isLanternLit = LanternController.ActiveLantern != null && LanternController.ActiveLantern.IsLit;
            float coneSize = isLanternLit ? (innerRadius * 1.75f) : (innerRadius * 1.20f);
            Color prevColor = GUI.color;
            GUI.color = isLanternLit
                ? new Color(1f, 0.86f, 0.42f, 0.40f)
                : new Color(0.75f, 0.92f, 1f, 0.22f);
            GUI.DrawTexture(new Rect(center.x - coneSize * 0.5f, center.y - coneSize * 0.5f, coneSize, coneSize), _coneTex);

            // 8. Panah Emas Karakter di tengah (selalu menghadap depan/atas seperti GTA!)
            GUI.color = StylizedUITheme.GoldAccent;
            GUI.DrawTexture(new Rect(center.x - 10f, center.y - 10f, 20f, 20f), _arrowTex);
            GUI.color = prevColor;

            // 9. Badge Utara [U] ikut berputar mengelilingi tepi oktagon (gaya kompas GTA!)
            if (_northBadgeStyle == null)
            {
                _northBadgeStyle = StylizedUITheme.CreateKeycapStyle(11);
            }
            float northX = -sinY * (mapSize * 0.5f - 2f);
            float northY = -cosY * (mapSize * 0.5f - 2f);
            GUI.Box(new Rect(center.x + northX - 12f, center.y + northY - 10f, 24f, 20f), "U", _northBadgeStyle);
        }

        private void DrawRotatedBlip(Vector3 targetPos, Vector3 playerPos, float cosY, float sinY,
            Vector2 center, float innerRadius, float size, Color color, bool clampToEdge)
        {
            float dx = targetPos.x - playerPos.x;
            float dz = targetPos.z - playerPos.z;

            // Rotasi koordinat dunia ke ruang kamera (depan kamera = atas layar!)
            float rx = dx * cosY - dz * sinY;
            float rz = dx * sinY + dz * cosY;

            Vector2 offset = new Vector2(rx / worldRadius, -rz / worldRadius) * innerRadius;
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
            if (_octFrameTex == null)
            {
                const int s = 192;
                float half = s * 0.5f;
                float outerR = half - 1.5f;
                float borderInnerR = half - 6.5f;
                float ring1R = half * 0.62f;
                float ring2R = half * 0.30f;

                _octFrameTex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                _octMaskTex = new Texture2D(s, s, TextureFormat.Alpha8, false) { filterMode = FilterMode.Bilinear };
                Color[] framePx = new Color[s * s];
                Color[] maskPx = new Color[s * s];

                for (int y = 0; y < s; y++)
                {
                    float v = y / (float)(s - 1);
                    for (int x = 0; x < s; x++)
                    {
                        float px = Mathf.Abs((x + 0.5f) - half);
                        float py = Mathf.Abs((y + 0.5f) - half);
                        // Jarak oktagon beraturan
                        float octD = Mathf.Max(Mathf.Max(px, py), (px + py) * 0.70710678f);
                        float circR = Mathf.Sqrt(px * px + py * py);

                        if (octD > outerR)
                        {
                            framePx[y * s + x] = Color.clear;
                            maskPx[y * s + x] = new Color(0, 0, 0, 0);
                        }
                        else if (octD > borderInnerR)
                        {
                            float aa = Mathf.Clamp01(outerR - octD);
                            Color gold = Color.Lerp(new Color(0.60f, 0.37f, 0.14f, 1f), new Color(1f, 0.80f, 0.34f, 1f), v);
                            gold.a *= aa;
                            framePx[y * s + x] = gold;
                            maskPx[y * s + x] = new Color(1, 1, 1, 0);
                        }
                        else
                        {
                            maskPx[y * s + x] = new Color(1, 1, 1, 1);
                            // Tint teal tipis + garis radar halus di atas peta live GTA
                            Color c = new Color(0.06f, 0.17f, 0.21f, 0.28f);
                            bool onRing = Mathf.Abs(circR - ring1R) < 0.9f || Mathf.Abs(circR - ring2R) < 0.9f;
                            bool onAxis = (px < 0.7f || py < 0.7f) && octD < borderInnerR - 4f;
                            if (onRing || onAxis)
                            {
                                c = new Color(0.49f, 0.95f, 0.85f, 0.28f);
                            }
                            framePx[y * s + x] = c;
                        }
                    }
                }
                _octFrameTex.SetPixels(framePx);
                _octFrameTex.Apply();
                _octMaskTex.SetPixels(maskPx);
                _octMaskTex.Apply();

                Shader maskShader = Resources.Load<Shader>("Shaders/UI_OctagonMinimap");
                if (maskShader == null) maskShader = Shader.Find("PulauKabut/UI_OctagonMinimap");
                if (maskShader != null)
                {
                    _maskMat = new Material(maskShader);
                }
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
                const int aSize = 28;
                _arrowTex = new Texture2D(aSize, aSize, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[aSize * aSize];
                for (int y = 0; y < aSize; y++)
                {
                    for (int x = 0; x < aSize; x++)
                    {
                        float nx = (x / (float)(aSize - 1)) * 2f - 1f;
                        float ny = (y / (float)(aSize - 1)) * 2f - 1f;
                        bool inside = ny <= 0.88f && ny >= -0.68f && Mathf.Abs(nx) <= (0.88f - ny) * 0.52f;
                        bool notch = ny < -0.22f && Mathf.Abs(nx) < (-0.22f - ny) * 0.82f;
                        pixels[y * aSize + x] = (inside && !notch) ? Color.white : Color.clear;
                    }
                }
                _arrowTex.SetPixels(pixels);
                _arrowTex.Apply();
            }

            if (_coneTex == null)
            {
                const int cSize = 128;
                _coneTex = new Texture2D(cSize, cSize, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                float half = cSize * 0.5f;
                float maxR = half - 2f;
                const float halfConeRad = 34f * Mathf.Deg2Rad;

                Color[] pixels = new Color[cSize * cSize];
                for (int y = 0; y < cSize; y++)
                {
                    for (int x = 0; x < cSize; x++)
                    {
                        float dx = (x + 0.5f) - half;
                        float dy = (y + 0.5f) - half;
                        float r = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dy <= 0f || r < 3f || r > maxR) { pixels[y * cSize + x] = Color.clear; continue; }
                        float angle = Mathf.Atan2(Mathf.Abs(dx), dy);
                        if (angle > halfConeRad) { pixels[y * cSize + x] = Color.clear; continue; }
                        float radialFade = Mathf.Pow(1f - (r / maxR), 1.35f);
                        float edgeFade = Mathf.Clamp01((halfConeRad - angle) / (7f * Mathf.Deg2Rad));
                        pixels[y * cSize + x] = new Color(1f, 1f, 1f, radialFade * edgeFade);
                    }
                }
                _coneTex.SetPixels(pixels);
                _coneTex.Apply();
            }
        }
    }
}
