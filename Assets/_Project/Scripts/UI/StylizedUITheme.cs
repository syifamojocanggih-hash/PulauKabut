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

        public static Texture2D GoldPanelTex => _goldPanelTex != null ? _goldPanelTex : (_goldPanelTex = CreateChamferTexture(
            64, 48, 12, 5, 2.4f,
            new Color(0.090f, 0.251f, 0.290f, 0.94f), // #17404a
            new Color(0.047f, 0.133f, 0.161f, 0.96f), // #0c2229
            new Color(1.000f, 0.761f, 0.294f, 1.00f), // #ffc24b
            new Color(0.541f, 0.333f, 0.133f, 1.00f)));// #8a5522

        public static Texture2D GoldPanelHoverTex => _goldPanelHoverTex != null ? _goldPanelHoverTex : (_goldPanelHoverTex = CreateChamferTexture(
            64, 48, 12, 5, 2.4f,
            new Color(0.130f, 0.335f, 0.380f, 0.97f),
            new Color(0.075f, 0.195f, 0.235f, 0.98f),
            new Color(1.000f, 0.860f, 0.450f, 1.00f),
            new Color(0.750f, 0.500f, 0.200f, 1.00f)));

        public static Texture2D MintPanelTex => _mintPanelTex != null ? _mintPanelTex : (_mintPanelTex = CreateChamferTexture(
            64, 48, 11, 4, 2.2f,
            new Color(0.078f, 0.212f, 0.247f, 0.93f), // #14363f
            new Color(0.047f, 0.133f, 0.161f, 0.95f), // #0c2229
            new Color(0.490f, 0.953f, 0.847f, 0.85f), // #7df3d8
            new Color(0.157f, 0.353f, 0.392f, 0.90f)));

        public static Texture2D GoldKeycapTex => _goldKeycapTex != null ? _goldKeycapTex : (_goldKeycapTex = CreateChamferTexture(
            36, 28, 5, 2, 1.2f,
            new Color(1.000f, 0.878f, 0.541f, 1.00f), // #ffe08a
            new Color(1.000f, 0.761f, 0.294f, 1.00f), // #ffc24b
            new Color(0.600f, 0.380f, 0.120f, 1.00f),
            new Color(0.450f, 0.270f, 0.080f, 1.00f)));

        public static Texture2D MintBadgeTex => _mintBadgeTex != null ? _mintBadgeTex : (_mintBadgeTex = CreateChamferTexture(
            36, 28, 5, 2, 1.0f,
            new Color(0.490f, 0.953f, 0.847f, 1.00f),
            new Color(0.320f, 0.800f, 0.720f, 1.00f),
            new Color(0.200f, 0.600f, 0.540f, 1.00f),
            new Color(0.150f, 0.450f, 0.420f, 1.00f)));

        public static GUIStyle CreateGoldPanelStyle(int fontSize = 13, TextAnchor alignment = TextAnchor.UpperLeft, int padH = 14, int padV = 10)
        {
            GUIStyle s = new GUIStyle(GUI.skin.box)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                border = new RectOffset(16, 16, 16, 16),
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
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                border = new RectOffset(14, 14, 14, 14),
                padding = new RectOffset(padH, padH, padV, padV)
            };
            s.normal.background = MintPanelTex;
            s.normal.textColor = CreamText;
            return s;
        }

        public static GUIStyle CreatePillStyle(int fontSize = 13, TextAnchor alignment = TextAnchor.MiddleCenter, int padH = 14, int padV = 6)
        {
            GUIStyle s = new GUIStyle(GUI.skin.button)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Overflow,
                border = new RectOffset(16, 16, 14, 14),
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(7, 7, 6, 6),
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
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(7, 7, 6, 6),
                padding = new RectOffset(6, 6, 2, 2)
            };
            s.normal.background = MintBadgeTex;
            s.normal.textColor = new Color(0.047f, 0.133f, 0.161f, 1f);
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
