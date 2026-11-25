using UnityEngine;

namespace PulauKabut.UI
{
    /// <summary>
    /// Utilitas tema tombol pill bergaya stylized (rounded bevel slate-biru dengan highlight atas,
    /// bayangan bawah, dan bingkai tegas — terinspirasi gaya UI game petualangan low-poly).
    /// Semua tekstur di-cache sekali agar nol alokasi GC per frame.
    /// </summary>
    public static class StylizedUITheme
    {
        private static Texture2D _pillNormalTex;
        private static Texture2D _pillHoverTex;
        private static Texture2D _badgeCircleTex;

        /// <summary>Tekstur 9-slice pill button (slate-blue glossy bevel).</summary>
        public static Texture2D PillNormalTex
        {
            get
            {
                if (_pillNormalTex == null)
                {
                    _pillNormalTex = CreatePillTexture(
                        topColor: new Color(0.57f, 0.69f, 0.74f, 0.96f),
                        midColor: new Color(0.45f, 0.56f, 0.62f, 0.96f),
                        botColor: new Color(0.35f, 0.45f, 0.51f, 0.96f),
                        borderColor: new Color(0.20f, 0.27f, 0.31f, 1f),
                        highlightColor: new Color(0.84f, 0.92f, 0.95f, 0.55f));
                }
                return _pillNormalTex;
            }
        }

        /// <summary>Tekstur 9-slice pill button saat hover/aktif (sedikit lebih terang).</summary>
        public static Texture2D PillHoverTex
        {
            get
            {
                if (_pillHoverTex == null)
                {
                    _pillHoverTex = CreatePillTexture(
                        topColor: new Color(0.66f, 0.78f, 0.82f, 0.98f),
                        midColor: new Color(0.53f, 0.65f, 0.71f, 0.98f),
                        botColor: new Color(0.41f, 0.52f, 0.58f, 0.98f),
                        borderColor: new Color(0.18f, 0.25f, 0.29f, 1f),
                        highlightColor: new Color(0.92f, 0.97f, 1f, 0.68f));
                }
                return _pillHoverTex;
            }
        }

        /// <summary>Tekstur lingkaran badge tombol kunci (misal lingkaran [E]).</summary>
        public static Texture2D BadgeCircleTex
        {
            get
            {
                if (_badgeCircleTex == null)
                {
                    _badgeCircleTex = CreateCircleBadgeTexture(
                        topColor: new Color(0.98f, 0.85f, 0.46f, 1f),
                        botColor: new Color(0.84f, 0.64f, 0.24f, 1f),
                        borderColor: new Color(0.24f, 0.20f, 0.14f, 1f));
                }
                return _badgeCircleTex;
            }
        }

        /// <summary>Membuat GUIStyle tombol pill yang otomatis mendukung 9-slice tanpa pecah.</summary>
        public static GUIStyle CreatePillStyle(int fontSize = 14, TextAnchor alignment = TextAnchor.MiddleCenter, int padH = 18, int padV = 8)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Overflow,
                border = new RectOffset(22, 22, 16, 16),
                padding = new RectOffset(padH, padH, padV, padV)
            };
            style.normal.background = PillNormalTex;
            style.hover.background = PillHoverTex;
            style.active.background = PillHoverTex;
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = new Color(1f, 0.96f, 0.82f);
            return style;
        }

        private static Texture2D CreatePillTexture(Color topColor, Color midColor, Color botColor, Color borderColor, Color highlightColor)
        {
            const int w = 64;
            const int h = 44;
            float radius = h * 0.5f - 1f;
            float halfW = w * 0.5f;
            float halfH = h * 0.5f;
            float innerHalfW = halfW - radius - 1f;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                // y=0 bawah, y=h-1 atas
                float v = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float px = Mathf.Abs((x + 0.5f) - halfW);
                    float py = (y + 0.5f) - halfH;
                    float dx = Mathf.Max(0f, px - innerHalfW);
                    float dist = Mathf.Sqrt(dx * dx + py * py);

                    if (dist > radius)
                    {
                        pixels[y * w + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(radius - dist);
                    if (dist > radius - 2.6f)
                    {
                        Color bc = borderColor;
                        bc.a *= aa;
                        pixels[y * w + x] = bc;
                        continue;
                    }

                    // Gradasi bevel halus dari bawah ke atas
                    Color col = v > 0.55f
                        ? Color.Lerp(midColor, topColor, (v - 0.55f) / 0.45f)
                        : Color.Lerp(botColor, midColor, v / 0.55f);

                    // Garis highlight tipis di tepi atas dalam pill (seperti referensi tombol stylized)
                    if (py > 0f && dist > radius - 5.2f && dist <= radius - 3.0f && v > 0.62f)
                    {
                        col = Color.Lerp(col, highlightColor, 0.65f);
                    }

                    pixels[y * w + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleBadgeTexture(Color topColor, Color botColor, Color borderColor)
        {
            const int size = 36;
            float half = size * 0.5f;
            float rOuter = half - 1f;
            float rBorder = half - 3.2f;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = y / (float)(size - 1);
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) - half;
                    float dy = (y + 0.5f) - half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > rOuter)
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                    else if (dist > rBorder)
                    {
                        Color bc = borderColor;
                        bc.a *= Mathf.Clamp01(rOuter - dist);
                        pixels[y * size + x] = bc;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.Lerp(botColor, topColor, v);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
