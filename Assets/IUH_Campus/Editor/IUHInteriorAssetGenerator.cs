using System.IO;
using UnityEngine;
using UnityEditor;

namespace IUHCampus.Editor
{
    public static class IUHInteriorAssetGenerator
    {
        private const string TexPath = "Assets/IUH_Campus/Textures/";
        private const string MatPath = "Assets/IUH_Campus/Materials/";

        public static void GenerateAllInteriorAssets()
        {
            if (!Directory.Exists(TexPath)) Directory.CreateDirectory(TexPath);
            if (!Directory.Exists(MatPath)) Directory.CreateDirectory(MatPath);

            GenerateBlackMarbleTexture();
            GenerateLightPorcelainTexture();
            GenerateWoodSlatTexture();
            GenerateDashboardTexture();
            GeneratePCScreenTexture();
            GenerateAcademicPoster1();
            GenerateAcademicPoster2();
            GenerateCyanFabricTexture();
            GeneratePerforatedMetalTexture();
            GenerateCeilingGridTexture();

            AssetDatabase.Refresh();

            GenerateInteriorMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[IUH Interior] All interior textures and materials generated successfully!");
        }

        private static void SaveTexture(Texture2D tex, string fileName)
        {
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(TexPath, fileName);
            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        private static void GenerateBlackMarbleTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color baseBlack = new Color(0.08f, 0.09f, 0.10f, 1f);
            Color darkVein = new Color(0.14f, 0.15f, 0.17f, 1f);
            Color lightVein = new Color(0.55f, 0.58f, 0.62f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size * 6.0f;
                    float ny = (float)y / size * 6.0f;
                    float noise1 = Mathf.PerlinNoise(nx, ny);
                    float noise2 = Mathf.PerlinNoise(nx * 2.5f + 1.2f, ny * 2.5f + 3.4f);
                    float vein = Mathf.Sin((nx + ny + noise1 * 2.0f) * 4.0f);

                    Color c = Color.Lerp(baseBlack, darkVein, noise2);
                    if (Mathf.Abs(vein) > 0.96f)
                    {
                        float intensity = (Mathf.Abs(vein) - 0.96f) / 0.04f;
                        c = Color.Lerp(c, lightVein, intensity * 0.65f);
                    }

                    if (x < 3 || x >= size - 3 || y < 3 || y >= size - 3)
                    {
                        c = new Color(0.04f, 0.04f, 0.05f, 1f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Interior_BlackMarbleTile.png");
        }

        private static void GenerateLightPorcelainTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color baseCream = new Color(0.92f, 0.92f, 0.90f, 1f);
            Color tintBeige = new Color(0.88f, 0.87f, 0.84f, 1f);
            Color groutColor = new Color(0.72f, 0.71f, 0.68f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise((float)x / size * 8f, (float)y / size * 8f);
                    Color c = Color.Lerp(baseCream, tintBeige, n * 0.35f);

                    if (x < 3 || x >= size - 3 || y < 3 || y >= size - 3)
                    {
                        c = groutColor;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Interior_LightPorcelainTile.png");
        }

        private static void GenerateWoodSlatTexture()
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color woodBase = new Color(0.82f, 0.68f, 0.50f, 1f);
            Color woodDark = new Color(0.68f, 0.52f, 0.35f, 1f);
            Color woodHighlight = new Color(0.90f, 0.78f, 0.60f, 1f);
            Color slatGroove = new Color(0.25f, 0.18f, 0.12f, 1f);

            int slatWidth = 32;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int slatIndex = x / slatWidth;
                    int xInSlat = x % slatWidth;

                    float woodNoise = Mathf.PerlinNoise((float)slatIndex * 1.5f, (float)y / height * 12f);
                    Color c = Color.Lerp(woodBase, woodDark, woodNoise * 0.7f);

                    float grain = Mathf.Sin((float)y * 0.8f + woodNoise * 5f);
                    if (grain > 0.6f) c = Color.Lerp(c, woodHighlight, 0.25f);

                    if (xInSlat < 2 || xInSlat >= slatWidth - 2)
                    {
                        c = slatGroove;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Interior_WoodSlat.png");
        }

        private static void GenerateDashboardTexture()
        {
            int w = 1024;
            int h = 512;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color bg = new Color(0.04f, 0.07f, 0.14f, 1f);
            Color cardBg = new Color(0.08f, 0.13f, 0.24f, 1f);
            Color cyanAccent = new Color(0.0f, 0.85f, 0.95f, 1f);
            Color blueAccent = new Color(0.18f, 0.50f, 0.95f, 1f);
            Color greenAccent = new Color(0.15f, 0.85f, 0.45f, 1f);
            Color orangeAccent = new Color(0.98f, 0.60f, 0.15f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, bg);
                }
            }

            for (int y = h - 45; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, new Color(0.06f, 0.10f, 0.20f, 1f));
                }
            }

            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, h - 46, cyanAccent);
                tex.SetPixel(x, h - 47, cyanAccent);
            }

            for (int card = 0; card < 4; card++)
            {
                int cx0 = 20 + card * 245;
                int cx1 = cx0 + 230;
                int cy0 = h - 145;
                int cy1 = h - 55;

                for (int y = cy0; y < cy1; y++)
                {
                    for (int x = cx0; x < cx1; x++)
                    {
                        if (x == cx0 || x == cx1 - 1 || y == cy0 || y == cy1 - 1)
                            tex.SetPixel(x, y, cyanAccent * 0.6f);
                        else
                            tex.SetPixel(x, y, cardBg);
                    }
                }
            }

            int lx0 = 20, lx1 = 580, ly0 = 25, ly1 = h - 165;
            for (int y = ly0; y < ly1; y++)
            {
                for (int x = lx0; x < lx1; x++)
                {
                    if (x == lx0 || x == lx1 - 1 || y == ly0 || y == ly1 - 1)
                        tex.SetPixel(x, y, blueAccent * 0.5f);
                    else
                        tex.SetPixel(x, y, cardBg);
                }
            }

            for (int x = lx0 + 10; x < lx1 - 10; x++)
            {
                float t = (float)(x - lx0) / (lx1 - lx0);
                float wave = Mathf.Sin(t * 12f) * 0.25f + Mathf.Cos(t * 5f) * 0.35f + 0.5f;
                int gy = ly0 + 20 + (int)(wave * (ly1 - ly0 - 60));
                for (int y = ly0 + 5; y <= gy; y++)
                {
                    Color fill = Color.Lerp(cardBg, cyanAccent * 0.35f, (float)(y - ly0) / (gy - ly0));
                    tex.SetPixel(x, y, fill);
                }
                tex.SetPixel(x, gy, cyanAccent);
                tex.SetPixel(x, gy + 1, Color.white);
            }

            int rx0 = 605, rx1 = w - 20, ry0 = 190, ry1 = h - 165;
            for (int y = ry0; y < ry1; y++)
            {
                for (int x = rx0; x < rx1; x++)
                {
                    if (x == rx0 || x == rx1 - 1 || y == ry0 || y == ry1 - 1)
                        tex.SetPixel(x, y, greenAccent * 0.5f);
                    else
                        tex.SetPixel(x, y, cardBg);
                }
            }

            for (int b = 0; b < 8; b++)
            {
                int bx0 = rx0 + 20 + b * 45;
                int bx1 = bx0 + 28;
                int barH = 30 + (b * 37) % 110;
                for (int y = ry0 + 10; y < ry0 + 10 + barH; y++)
                {
                    for (int x = bx0; x < bx1; x++)
                    {
                        tex.SetPixel(x, y, (b % 2 == 0) ? greenAccent : orangeAccent);
                    }
                }
            }

            int dx0 = 605, dx1 = w - 20, dy0 = 25, dy1 = 175;
            for (int y = dy0; y < dy1; y++)
            {
                for (int x = dx0; x < dx1; x++)
                {
                    if (x == dx0 || x == dx1 - 1 || y == dy0 || y == dy1 - 1)
                        tex.SetPixel(x, y, orangeAccent * 0.5f);
                    else
                        tex.SetPixel(x, y, cardBg);
                }
            }

            int circleX = (dx0 + dx1) / 2;
            int circleY = (dy0 + dy1) / 2;
            for (int y = dy0 + 10; y < dy1 - 10; y++)
            {
                for (int x = dx0 + 10; x < dx1 - 10; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(circleX, circleY));
                    if (dist >= 38 && dist <= 54)
                    {
                        float angle = Mathf.Atan2(y - circleY, x - circleX);
                        tex.SetPixel(x, y, (angle > 0) ? cyanAccent : greenAccent);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_Interior_DashboardScreen.png");
        }

        private static void GeneratePCScreenTexture()
        {
            int w = 512;
            int h = 288;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color editorBg = new Color(0.11f, 0.12f, 0.14f, 1f);
            Color sidebarBg = new Color(0.18f, 0.19f, 0.21f, 1f);
            Color lineNum = new Color(0.40f, 0.42f, 0.46f, 1f);
            Color textCode = new Color(0.85f, 0.88f, 0.92f, 1f);
            Color kwBlue = new Color(0.35f, 0.65f, 0.95f, 1f);
            Color strOrange = new Color(0.95f, 0.60f, 0.35f, 1f);
            Color funcYellow = new Color(0.92f, 0.85f, 0.40f, 1f);
            Color termBg = new Color(0.07f, 0.08f, 0.09f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, editorBg);
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < 90; x++)
                {
                    tex.SetPixel(x, y, sidebarBg);
                }
            }

            for (int y = 0; y < 65; y++)
            {
                for (int x = 90; x < w; x++)
                {
                    tex.SetPixel(x, y, termBg);
                }
            }

            for (int line = 0; line < 12; line++)
            {
                int ly = h - 35 - line * 14;
                if (ly < 70) break;

                for (int x = 95; x < 110; x++)
                {
                    tex.SetPixel(x, ly, lineNum);
                }

                int curX = 120 + ((line % 3 == 0) ? 0 : 20);
                Color tokenCol = (line % 4 == 0) ? kwBlue : (line % 3 == 0) ? funcYellow : (line % 2 == 0) ? strOrange : textCode;
                int tokenLen = 30 + (line * 17) % 180;
                for (int x = curX; x < curX + tokenLen && x < w - 20; x++)
                {
                    tex.SetPixel(x, ly, tokenCol);
                    tex.SetPixel(x, ly + 1, tokenCol);
                }
            }

            for (int x = 100; x < 350; x++)
            {
                tex.SetPixel(x, 40, new Color(0.2f, 0.85f, 0.35f, 1f));
                tex.SetPixel(x, 25, new Color(0.6f, 0.7f, 0.8f, 1f));
            }

            tex.Apply();
            SaveTexture(tex, "T_Interior_ScreenPC.png");
        }

        private static void GenerateAcademicPoster1()
        {
            int w = 512;
            int h = 768;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color bg = new Color(0.96f, 0.97f, 0.98f, 1f);
            Color headerBlue = new Color(0.08f, 0.32f, 0.65f, 1f);
            Color cyanHighlight = new Color(0.0f, 0.72f, 0.85f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, bg);
                }
            }

            for (int y = h - 90; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, headerBlue);
                }
            }

            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, h - 91, cyanHighlight);
                tex.SetPixel(x, h - 92, cyanHighlight);
            }

            for (int s = 0; s < 3; s++)
            {
                int sy0 = 80 + s * 190;
                int sy1 = sy0 + 170;
                for (int y = sy0; y < sy1; y++)
                {
                    for (int x = 30; x < w - 30; x++)
                    {
                        if (x == 30 || x == w - 31 || y == sy0 || y == sy1 - 1)
                            tex.SetPixel(x, y, headerBlue * 0.4f);
                    }
                }

                int cy = (sy0 + sy1) / 2;
                for (int node = 0; node < 3; node++)
                {
                    int nx = 90 + node * 150;
                    for (int dy = -25; dy <= 25; dy++)
                    {
                        for (int dx = -40; dx <= 40; dx++)
                        {
                            if (Mathf.Abs(dx) == 40 || Mathf.Abs(dy) == 25)
                                tex.SetPixel(nx + dx, cy + dy, headerBlue);
                            else
                                tex.SetPixel(nx + dx, cy + dy, new Color(0.90f, 0.94f, 1f, 1f));
                        }
                    }
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < 10 || x >= w - 10 || y < 10 || y >= h - 10)
                        tex.SetPixel(x, y, new Color(0.2f, 0.25f, 0.3f, 1f));
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_Interior_AcademicPoster1.png");
        }

        private static void GenerateAcademicPoster2()
        {
            int w = 512;
            int h = 768;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color bgDark = new Color(0.06f, 0.10f, 0.18f, 1f);
            Color greenCyber = new Color(0.15f, 0.88f, 0.55f, 1f);
            Color blueCyber = new Color(0.18f, 0.60f, 0.98f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, bgDark);
                }
            }

            for (int y = 20; y < h - 20; y += 40)
            {
                for (int x = 20; x < w - 20; x++)
                {
                    tex.SetPixel(x, y, new Color(0.12f, 0.18f, 0.28f, 1f));
                }
            }
            for (int x = 20; x < w - 20; x += 40)
            {
                for (int y = 20; y < h - 20; y++)
                {
                    tex.SetPixel(x, y, new Color(0.12f, 0.18f, 0.28f, 1f));
                }
            }

            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            for (int i = 0; i < 12; i++)
            {
                float ang = i * Mathf.PI * 2f / 12f;
                Vector2 nodePos = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 140f;

                for (float t = 0; t <= 1f; t += 0.01f)
                {
                    Vector2 p = Vector2.Lerp(center, nodePos, t);
                    tex.SetPixel((int)p.x, (int)p.y, blueCyber * 0.6f);
                }

                for (int dy = -12; dy <= 12; dy++)
                {
                    for (int dx = -12; dx <= 12; dx++)
                    {
                        if (dx * dx + dy * dy <= 144)
                            tex.SetPixel((int)nodePos.x + dx, (int)nodePos.y + dy, greenCyber);
                    }
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < 10 || x >= w - 10 || y < 10 || y >= h - 10)
                        tex.SetPixel(x, y, new Color(0.04f, 0.05f, 0.08f, 1f));
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_Interior_AcademicPoster2.png");
        }

        private static void GenerateCyanFabricTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color cyanBase = new Color(0.05f, 0.68f, 0.72f, 1f);
            Color cyanDark = new Color(0.03f, 0.55f, 0.58f, 1f);
            Color cyanLight = new Color(0.15f, 0.78f, 0.82f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool weave = ((x % 4 < 2) && (y % 4 < 2)) || ((x % 4 >= 2) && (y % 4 >= 2));
                    Color c = weave ? cyanLight : cyanDark;
                    float n = Mathf.PerlinNoise((float)x / size * 16f, (float)y / size * 16f);
                    c = Color.Lerp(c, cyanBase, n * 0.4f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Interior_CyanFabric.png");
        }

        private static void GeneratePerforatedMetalTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color metalGray = new Color(0.70f, 0.72f, 0.75f, 1f);
            Color holeDark = new Color(0.18f, 0.19f, 0.21f, 1f);

            int spacing = 16;
            int radius = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int gx = x % spacing - spacing / 2;
                    int gy = y % spacing - spacing / 2;
                    if (gx * gx + gy * gy <= radius * radius)
                    {
                        tex.SetPixel(x, y, holeDark);
                    }
                    else
                    {
                        tex.SetPixel(x, y, metalGray);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Interior_PerforatedMetal.png");
        }

        private static void GenerateCeilingGridTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color tileWhite = new Color(0.95f, 0.95f, 0.96f, 1f);
            Color gridMetal = new Color(0.75f, 0.77f, 0.80f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise((float)x * 0.5f, (float)y * 0.5f) * 0.04f;
                    Color c = tileWhite - new Color(noise, noise, noise, 0f);

                    int gridSpacing = 128;
                    if (x % gridSpacing < 4 || y % gridSpacing < 4)
                    {
                        c = gridMetal;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Interior_CeilingGrid.png");
        }

        public static void GenerateInteriorMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material mMarble = CreateLitMaterial("M_IUH_Int_BlackMarble", litShader, Color.white, 0.88f, 0.12f);
            Texture2D tMarble = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_BlackMarbleTile.png");
            if (tMarble != null) { mMarble.mainTexture = tMarble; mMarble.mainTextureScale = new Vector2(4, 4); }

            Material mPorcelain = CreateLitMaterial("M_IUH_Int_LightTile", litShader, Color.white, 0.72f, 0.05f);
            Texture2D tPorcelain = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_LightPorcelainTile.png");
            if (tPorcelain != null) { mPorcelain.mainTexture = tPorcelain; mPorcelain.mainTextureScale = new Vector2(4, 4); }

            Material mWood = CreateLitMaterial("M_IUH_Int_LightWood", litShader, Color.white, 0.45f, 0.02f);
            Texture2D tWood = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_WoodSlat.png");
            if (tWood != null) { mWood.mainTexture = tWood; mWood.mainTextureScale = new Vector2(2, 2); }

            CreateLitMaterial("M_IUH_Int_SageGreen", litShader, new Color(0.48f, 0.62f, 0.54f, 1f), 0.20f, 0.0f);

            Material mCyan = CreateLitMaterial("M_IUH_Int_CyanSofa", litShader, Color.white, 0.25f, 0.0f);
            Texture2D tCyan = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_CyanFabric.png");
            if (tCyan != null) { mCyan.mainTexture = tCyan; mCyan.mainTextureScale = new Vector2(3, 3); }

            Material mLED = CreateLitMaterial("M_IUH_Int_LEDDisplay", litShader, Color.white, 0.85f, 0.20f);
            Texture2D tLED = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_DashboardScreen.png");
            if (tLED != null)
            {
                mLED.mainTexture = tLED;
                mLED.EnableKeyword("_EMISSION");
                mLED.SetColor("_EmissionColor", new Color(1.2f, 1.2f, 1.2f));
                mLED.SetTexture("_EmissionMap", tLED);
            }

            Material mPC = CreateLitMaterial("M_IUH_Int_ScreenPC", litShader, Color.white, 0.75f, 0.10f);
            Texture2D tPC = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_ScreenPC.png");
            if (tPC != null)
            {
                mPC.mainTexture = tPC;
                mPC.EnableKeyword("_EMISSION");
                mPC.SetColor("_EmissionColor", new Color(1.0f, 1.0f, 1.0f));
                mPC.SetTexture("_EmissionMap", tPC);
            }

            Material mPost1 = CreateLitMaterial("M_IUH_Int_Poster1", litShader, Color.white, 0.35f, 0.0f);
            Texture2D tPost1 = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_AcademicPoster1.png");
            if (tPost1 != null) mPost1.mainTexture = tPost1;

            Material mPost2 = CreateLitMaterial("M_IUH_Int_Poster2", litShader, Color.white, 0.35f, 0.0f);
            Texture2D tPost2 = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_AcademicPoster2.png");
            if (tPost2 != null) mPost2.mainTexture = tPost2;

            CreateTransparentGlass("M_IUH_Int_GlassFrosted", litShader, new Color(0.92f, 0.95f, 0.98f, 0.55f), 0.30f, 0.05f);
            CreateTransparentGlass("M_IUH_Int_GlassClear", litShader, new Color(0.85f, 0.92f, 0.98f, 0.15f), 0.95f, 0.30f);

            Material mMetal = CreateLitMaterial("M_IUH_Int_PerforatedMetal", litShader, Color.white, 0.65f, 0.85f);
            Texture2D tMetal = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_PerforatedMetal.png");
            if (tMetal != null) { mMetal.mainTexture = tMetal; mMetal.mainTextureScale = new Vector2(4, 4); }

            CreateLitMaterial("M_IUH_Int_MatteWhite", litShader, new Color(0.96f, 0.96f, 0.97f, 1f), 0.15f, 0.0f);
            CreateLitMaterial("M_IUH_Int_DarkMullion", litShader, new Color(0.12f, 0.14f, 0.16f, 1f), 0.70f, 0.80f);
            CreateLitMaterial("M_IUH_Int_Chrome", litShader, new Color(0.90f, 0.92f, 0.94f, 1f), 0.95f, 0.95f);
            CreateLitMaterial("M_IUH_Int_NavyChair", litShader, new Color(0.10f, 0.15f, 0.24f, 1f), 0.35f, 0.05f);

            Material mCeil = CreateLitMaterial("M_IUH_Int_CeilingGrid", litShader, Color.white, 0.20f, 0.05f);
            Texture2D tCeil = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Interior_CeilingGrid.png");
            if (tCeil != null) { mCeil.mainTexture = tCeil; mCeil.mainTextureScale = new Vector2(4, 4); }

            CreateLitMaterial("M_IUH_Avatar_Skin", litShader, new Color(0.92f, 0.78f, 0.68f, 1f), 0.35f, 0.0f);
            CreateLitMaterial("M_IUH_Avatar_Jacket", litShader, new Color(0.08f, 0.30f, 0.62f, 1f), 0.30f, 0.0f);
        }

        private static Material CreateLitMaterial(string matName, Shader shader, Color baseColor, float smoothness, float metallic)
        {
            string path = MatPath + matName + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateTransparentGlass(string matName, Shader shader, Color baseColor, float smoothness, float metallic)
        {
            string path = MatPath + matName + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.enableInstancing = true;

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
