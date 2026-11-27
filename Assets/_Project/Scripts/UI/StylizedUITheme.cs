using UnityEngine;

namespace PulauKabut.UI
{
    /// <summary>
    /// Tema UI Pulau Kabut bergaya chamfered low-poly (Deep Ocean Teal + Bingkai Emas / Mint + Keycap Emas)
    /// mengikuti desain HUD Lengkap. Semua tekstur 9-slice di-cache sekali agar nol alokasi GC per frame.
    /// </summary>
    public static class StylizedUITheme
    {
        public static readonly Color CreamText = new Color(0.965f, 0.937f, 0.851f, 1f);     // #f6efd9
        public static readonly Color SeafoamText = new Color(0.663f, 0.812f, 0.784f, 1f);   // #a9cfc8
        public static readonly Color GoldAccent = new Color(1.000f, 0.824f, 0.416f, 1f);    // #ffd26a
        public static readonly Color MintAccent = new Color(0.490f, 0.953f, 0.847f, 1f);    // #7df3d8
        public static readonly Color KeycapDarkText = new Color(0.169f, 0.094f, 0.024f, 1f);// #2b1806

        private static Texture2D _goldPanelTex;
        private static Texture2D _goldPanelHoverTex;
        private static Texture2D _mintPanelTex;
        private static Texture2D _goldKeycapTex;
        private static Texture2D _mintBadgeTex;
        private static Texture2D _diamondTex;
        private static Texture2D _solidTex;
        private static Texture2D _slantSegTex;
        private static Texture2D _lanternIconTex;
        private static Texture2D _unlockGlowTex;
        private static Texture2D _pentagonIconTex;
        private static Texture2D _hexagonIconTex;
        private static Texture2D _houseIconTex;
        private static Font _headingFont;
        private static Font _bodyFont;
        private static bool _fontsLoaded;

        /// <summary>Font Baloo 2 ExtraBold untuk judul, angka, nameplate, dan keycap (persis HUD Lengkap).</summary>
        public static Font HeadingFont
        {
            get
            {
                EnsureFonts();
                return _headingFont;
            }
        }

        /// <summary>Font Nunito Bold untuk isi chat/dialog, misi, kontrol, dan label tombol (persis HUD Lengkap).</summary>
        public static Font BodyFont
        {
            get
            {
                EnsureFonts();
                return _bodyFont;
            }
        }

        public static Texture2D SolidTex
        {
            get
            {
                if (_solidTex == null)
                {
                    _solidTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _solidTex.SetPixel(0, 0, Color.white);
                    _solidTex.Apply();
                }
                return _solidTex;
            }
        }

        public static Texture2D DiamondTex
        {
            get
            {
                if (_diamondTex == null)
                {
                    const int s = 20;
                    float h = s * 0.5f;
                    _diamondTex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    Color[] px = new Color[s * s];
                    for (int y = 0; y < s; y++)
                    {
                        for (int x = 0; x < s; x++)
                        {
                            float d = Mathf.Abs(x + 0.5f - h) + Mathf.Abs(y + 0.5f - h);
                            float a = Mathf.Clamp01((h - 1.5f) - d);
                            px[y * s + x] = new Color(1f, 1f, 1f, a);
                        }
                    }
                    _diamondTex.SetPixels(px);
                    _diamondTex.Apply();
                }
                return _diamondTex;
            }
        }

        /// <summary>Segmen bar jajar genjang miring (12 segmen bar Lentera persis Hud.dc.html).</summary>
        public static Texture2D SlantSegTex
        {
            get
            {
                if (_slantSegTex == null)
                {
                    const int w = 24;
                    const int h = 18;
                    _slantSegTex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    Color[] px = new Color[w * h];
                    for (int y = 0; y < h; y++)
                    {
                        float v = y / (float)(h - 1);
                        float shift = v * 5.0f; // miring ke kanan atas
                        for (int x = 0; x < w; x++)
                        {
                            float lx = x - shift;
                            bool inside = lx >= 1.5f && lx <= (w - 6.5f) && y >= 1 && y <= h - 2;
                            if (!inside)
                            {
                                px[y * w + x] = Color.clear;
                            }
                            else
                            {
                                // Gradasi lembut terang di atas -> sedikit lebih hangat di bawah
                                float bright = Mathf.Lerp(0.84f, 1.0f, v);
                                px[y * w + x] = new Color(bright, bright, bright, 1f);
                            }
                        }
                    }
                    _slantSegTex.SetPixels(px);
                    _slantSegTex.Apply();
                }
                return _slantSegTex;
            }
        }

        /// <summary>Ikon Lentera kristal bercahaya (persis Hud.dc.html).</summary>
        public static Texture2D LanternIconTex
        {
            get
            {
                if (_lanternIconTex == null)
                {
                    const int s = 32;
                    _lanternIconTex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    Color[] px = new Color[s * s];
                    Color capCol = new Color(0.54f, 0.33f, 0.14f, 1f);
                    Color bodyCol = new Color(1.00f, 0.84f, 0.38f, 1f);
                    Color coreCol = new Color(1.00f, 0.98f, 0.88f, 1f);
                    for (int y = 0; y < s; y++)
                    {
                        float ny = y / (float)(s - 1); // 0 bawah, 1 atas
                        for (int x = 0; x < s; x++)
                        {
                            float nx = (x / (float)(s - 1)) * 2f - 1f; // -1..1
                            // Tutup kayu di atas (0.82..0.96)
                            if (ny >= 0.82f && ny <= 0.96f && Mathf.Abs(nx) <= 0.28f)
                            {
                                px[y * s + x] = capCol;
                            }
                            else if (ny >= 0.08f && ny < 0.84f)
                            {
                                float maxW = ny > 0.56f
                                    ? Mathf.Lerp(0.62f, 0.42f, (ny - 0.56f) / 0.28f)
                                    : Mathf.Lerp(0.04f, 0.62f, (ny - 0.08f) / 0.48f);
                                if (Mathf.Abs(nx) <= maxW)
                                {
                                    bool core = ny > 0.30f && ny < 0.66f && Mathf.Abs(nx) <= maxW * 0.34f;
                                    px[y * s + x] = core ? coreCol : bodyCol;
                                }
                                else px[y * s + x] = Color.clear;
                            }
                            else px[y * s + x] = Color.clear;
                        }
                    }
                    _lanternIconTex.SetPixels(px);
                    _lanternIconTex.Apply();
                }
                return _lanternIconTex;
            }
        }

        /// <summary>Aura cahaya radial emas lembut untuk latar showcase item baru di tengah layar.</summary>
        public static Texture2D UnlockGlowTex
        {
            get
            {
                if (_unlockGlowTex == null)
                {
                    const int s = 128;
                    float h = s * 0.5f;
                    _unlockGlowTex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    Color[] px = new Color[s * s];
                    for (int y = 0; y < s; y++)
                    {
                        for (int x = 0; x < s; x++)
                        {
                            float dx = (x + 0.5f - h) / h;
                            float dy = (y + 0.5f - h) / h;
                            float r = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                            float core = Mathf.Pow(1f - r, 2.2f);
                            float angle = Mathf.Atan2(dy, dx);
                            float rays = 0.85f + 0.15f * Mathf.Cos(angle * 8f);
                            float a = Mathf.Clamp01(core * rays);
                            Color col = Color.Lerp(GoldAccent, new Color(1f, 0.96f, 0.78f, 1f), core);
                            px[y * s + x] = new Color(col.r, col.g, col.b, a * 0.75f);
                        }
                    }
                    _unlockGlowTex.SetPixels(px);
                    _unlockGlowTex.Apply();
                }
                return _unlockGlowTex;
            }
        }

        /// <summary>Ikon kristal segilima mint untuk Pecahan Cahaya.</summary>
        public static Texture2D PentagonIconTex => _pentagonIconTex != null ? _pentagonIconTex : (_pentagonIconTex = CreatePolygonIcon(5, MintAccent));

        /// <summary>Ikon segienam emas untuk Serpihan Ingatan.</summary>
        public static Texture2D HexagonIconTex => _hexagonIconTex != null ? _hexagonIconTex : (_hexagonIconTex = CreatePolygonIcon(6, GoldAccent));

        /// <summary>Ikon rumah krem untuk Desa Pelita.</summary>
        public static Texture2D HouseIconTex
        {
            get
            {
                if (_houseIconTex == null)
                {
                    const int s = 28;
                    _houseIconTex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    Color[] px = new Color[s * s];
                    for (int y = 0; y < s; y++)
                    {
                        float ny = y / (float)(s - 1);
                        for (int x = 0; x < s; x++)
                        {
                            float nx = Mathf.Abs((x / (float)(s - 1)) * 2f - 1f);
                            bool roof = ny >= 0.48f && ny <= 0.94f && nx <= (0.94f - ny) * 1.95f;
                            bool body = ny >= 0.08f && ny < 0.52f && nx <= 0.58f;
                            px[y * s + x] = (roof || body) ? CreamText : Color.clear;
                        }
                    }
                    _houseIconTex.SetPixels(px);
                    _houseIconTex.Apply();
                }
                return _houseIconTex;
            }
        }

        private static Texture2D CreatePolygonIcon(int sides, Color col)
        {
            const int s = 28;
            float half = s * 0.5f;
            float rad = half - 2.5f;
            Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            Color[] px = new Color[s * s];
            float step = Mathf.PI * 2f / sides;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = (x + 0.5f) - half;
                    float dy = (y + 0.5f) - half;
                    float ang = Mathf.Atan2(dx, dy);
                    if (ang < 0f) ang += Mathf.PI * 2f;
                    float sector = (ang % step) - step * 0.5f;
                    float maxR = rad * Mathf.Cos(step * 0.5f) / Mathf.Cos(sector);
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(maxR - r);
                    px[y * s + x] = new Color(col.r, col.g, col.b, col.a * a);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        private static void EnsureFonts()
        {
            if (_fontsLoaded && _headingFont != null && _bodyFont != null) return;
            _headingFont = Resources.Load<Font>("Fonts/Baloo2-Bold");
            _bodyFont = Resources.Load<Font>("Fonts/Nunito-Bold");
            if (_bodyFont == null) _bodyFont = _headingFont;
            if (_headingFont == null) _headingFont = _bodyFont;
            _fontsLoaded = _headingFont != null && _bodyFont != null;
        }

        public static Texture2D GoldPanelTex => _goldPanelTex != null ? _goldPanelTex : (_goldPanelTex = CreateChamferTexture(
            128, 96, 22, 10, 4.2f,
            new Color(0.090f, 0.251f, 0.290f, 0.95f), // #17404a
            new Color(0.047f, 0.133f, 0.161f, 0.97f), // #0c2229
            new Color(1.000f, 0.761f, 0.294f, 1.00f), // #ffc24b
            new Color(0.541f, 0.333f, 0.133f, 1.00f)));// #8a5522

        public static Texture2D GoldPanelHoverTex => _goldPanelHoverTex != null ? _goldPanelHoverTex : (_goldPanelHoverTex = CreateChamferTexture(
            128, 96, 22, 10, 4.2f,
            new Color(0.130f, 0.335f, 0.380f, 0.97f),
            new Color(0.075f, 0.195f, 0.235f, 0.98f),
            new Color(1.000f, 0.860f, 0.450f, 1.00f),
            new Color(0.750f, 0.500f, 0.200f, 1.00f)));

        public static Texture2D MintPanelTex => _mintPanelTex != null ? _mintPanelTex : (_mintPanelTex = CreateChamferTexture(
            128, 96, 20, 8, 3.8f,
            new Color(0.078f, 0.212f, 0.247f, 0.94f), // #14363f
            new Color(0.047f, 0.133f, 0.161f, 0.96f), // #0c2229
            new Color(0.490f, 0.953f, 0.847f, 0.85f), // #7df3d8
            new Color(0.157f, 0.353f, 0.392f, 0.90f)));

        public static Texture2D GoldKeycapTex => _goldKeycapTex != null ? _goldKeycapTex : (_goldKeycapTex = CreateChamferTexture(
            72, 56, 10, 4, 2.2f,
            new Color(1.000f, 0.878f, 0.541f, 1.00f), // #ffe08a
            new Color(1.000f, 0.761f, 0.294f, 1.00f), // #ffc24b
            new Color(0.600f, 0.380f, 0.120f, 1.00f),
            new Color(0.450f, 0.270f, 0.080f, 1.00f)));

        public static Texture2D MintBadgeTex => _mintBadgeTex != null ? _mintBadgeTex : (_mintBadgeTex = CreateChamferTexture(
            72, 56, 10, 4, 2.0f,
            new Color(0.490f, 0.953f, 0.847f, 1.00f),
            new Color(0.320f, 0.800f, 0.720f, 1.00f),
            new Color(0.200f, 0.600f, 0.540f, 1.00f),
            new Color(0.150f, 0.450f, 0.420f, 1.00f)));

        private static FontStyle BodyFontStyle => BodyFont != null ? FontStyle.Normal : FontStyle.Bold;
        private static FontStyle HeadingFontStyle => HeadingFont != null ? FontStyle.Normal : FontStyle.Bold;

        public static GUIStyle CreateGoldPanelStyle(int fontSize = 13, TextAnchor alignment = TextAnchor.UpperLeft, int padH = 14, int padV = 10)
        {
            GUIStyle s = new GUIStyle(GUI.skin.box)
            {
                font = BodyFont,
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = BodyFontStyle,
                wordWrap = true,
                border = new RectOffset(30, 30, 30, 30),
                padding = new RectOffset(padH, padH, padV, padV)
            };
            s.normal.background = GoldPanelTex;
            s.normal.textColor = CreamText;
            return s;
        }

        public static GUIStyle CreateMintPanelStyle(int fontSize = 13, TextAnchor alignment = TextAnchor.UpperLeft, int padH = 14, int padV = 10)
        {
            GUIStyle s = new GUIStyle(GUI.skin.box)
            {
                font = BodyFont,
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = BodyFontStyle,
                wordWrap = true,
                border = new RectOffset(26, 26, 26, 26),
                padding = new RectOffset(padH, padH, padV, padV)
            };
            s.normal.background = MintPanelTex;
            s.normal.textColor = CreamText;
            return s;
        }

        public static GUIStyle CreatePillStyle(int fontSize = 14, TextAnchor alignment = TextAnchor.MiddleCenter, int padH = 14, int padV = 6)
        {
            GUIStyle s = new GUIStyle(GUI.skin.button)
            {
                font = BodyFont,
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = BodyFontStyle,
                wordWrap = false,
                clipping = TextClipping.Overflow,
                border = new RectOffset(30, 30, 26, 26),
                padding = new RectOffset(padH, padH, padV, padV)
            };
            s.normal.background = GoldPanelTex;
            s.hover.background = GoldPanelHoverTex;
            s.active.background = GoldPanelHoverTex;
            s.normal.textColor = CreamText;
            s.hover.textColor = Color.white;
            s.active.textColor = GoldAccent;
            return s;
        }

        public static GUIStyle CreateKeycapStyle(int fontSize = 12)
        {
            GUIStyle s = new GUIStyle(GUI.skin.box)
            {
                font = HeadingFont,
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = HeadingFontStyle,
                border = new RectOffset(14, 14, 12, 12),
                padding = new RectOffset(6, 6, 2, 2)
            };
            s.normal.background = GoldKeycapTex;
            s.normal.textColor = KeycapDarkText;
            return s;
        }

        public static GUIStyle CreateMintBadgeStyle(int fontSize = 12)
        {
            GUIStyle s = new GUIStyle(GUI.skin.box)
            {
                font = HeadingFont,
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = HeadingFontStyle,
                border = new RectOffset(14, 14, 12, 12),
                padding = new RectOffset(6, 6, 2, 2)
            };
            s.normal.background = MintBadgeTex;
            s.normal.textColor = new Color(0.047f, 0.133f, 0.161f, 1f);
            return s;
        }

        public static GUIStyle CreateHeadingLabelStyle(int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label)
            {
                font = HeadingFont,
                fontSize = fontSize,
                fontStyle = HeadingFontStyle,
                alignment = alignment
            };
            s.normal.textColor = color;
            return s;
        }

        public static GUIStyle CreateBodyLabelStyle(int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleLeft, bool wordWrap = false)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label)
            {
                font = BodyFont,
                fontSize = fontSize,
                fontStyle = BodyFontStyle,
                alignment = alignment,
                wordWrap = wordWrap
            };
            s.normal.textColor = color;
            return s;
        }

        /// <summary>Menggambar tombol chamfered dengan keycap emas di kiri + label krem di kanan (gaya HUD Lengkap).</summary>
        public static bool DrawKeyButton(Rect rect, string keyLabel, string textLabel, GUIStyle btnStyle, GUIStyle keyStyle, GUIStyle labelStyle)
        {
            bool clicked = GUI.Button(rect, GUIContent.none, btnStyle);
            float keyW = Mathf.Max(22f, keyLabel.Length * 9f + 10f);
            float keyH = rect.height - 10f;
            Rect keyRect = new Rect(rect.x + 8f, rect.y + 5f, keyW, keyH);
            GUI.Box(keyRect, keyLabel, keyStyle);

            Rect txtRect = new Rect(keyRect.xMax + 7f, rect.y, rect.xMax - keyRect.xMax - 12f, rect.height);
            GUI.Label(txtRect, textLabel, labelStyle);
            return clicked;
        }

        private static Texture2D CreateChamferTexture(int w, int h, int bigCut, int smallCut, float borderPx,
            Color topBg, Color botBg, Color borderLight, Color borderDark)
        {
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
                    float u = x / (float)(w - 1);
                    float d = SignedDistChamfer(x + 0.5f, y + 0.5f, w, h, bigCut, smallCut);

                    if (d > 0f)
                    {
                        pixels[y * w + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(-d);
                    if (d > -borderPx)
                    {
                        float diag = Mathf.Abs((u + (1f - v)) - 1f);
                        Color bc = Color.Lerp(borderDark, borderLight, diag * 1.4f);
                        bc.a *= aa;
                        pixels[y * w + x] = bc;
                        continue;
                    }

                    Color bg = Color.Lerp(botBg, topBg, v);
                    // Sheen diagonal halus low-poly (persis linear-gradient 118deg di Hud.dc.html!)
                    if (u * 0.85f + (1f - v) * 0.45f < 0.32f)
                    {
                        bg = Color.Lerp(bg, Color.white, 0.06f);
                    }
                    pixels[y * w + x] = bg;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>Signed distance negatif di dalam poligon chamfered (cut besar di kiri-atas & kanan-bawah, cut kecil di kanan-atas & kiri-bawah).</summary>
        private static float SignedDistChamfer(float x, float y, float w, float h, float bigCut, float smallCut)
        {
            float dLeft = -x;
            float dRight = x - w;
            float dBot = -y;
            float dTop = y - h;

            // Kiri-atas (y tinggi, x kecil): cut = bigCut
            float dTL = ((h - y) + x < bigCut) ? (bigCut - ((h - y) + x)) * 0.7071f : -999f;
            // Kanan-bawah (y kecil, x tinggi): cut = bigCut
            float dBR = (y + (w - x) < bigCut) ? (bigCut - (y + (w - x))) * 0.7071f : -999f;
            // Kanan-atas (y tinggi, x tinggi): cut = smallCut
            float dTR = ((h - y) + (w - x) < smallCut) ? (smallCut - ((h - y) + (w - x))) * 0.7071f : -999f;
            // Kiri-bawah (y kecil, x kecil): cut = smallCut
            float dBL = (y + x < smallCut) ? (smallCut - (y + x)) * 0.7071f : -999f;

            // Exact inward chamfer distance along all 8 edges
            float eTL = (bigCut - ((h - y) + x)) * 0.7071f;
            float eBR = (bigCut - (y + (w - x))) * 0.7071f;
            float eTR = (smallCut - ((h - y) + (w - x))) * 0.7071f;
            float eBL = (smallCut - (y + x)) * 0.7071f;

            return Mathf.Max(
                Mathf.Max(Mathf.Max(dLeft, dRight), Mathf.Max(dBot, dTop)),
                Mathf.Max(Mathf.Max(eTL, eBR), Mathf.Max(eTR, eBL)));
        }
    }
}
