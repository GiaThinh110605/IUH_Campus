using System.IO;
using UnityEngine;
using UnityEditor;

namespace IUHCampus.Editor
{
    public static class IUHTextureGenerator
    {
        private const string TexturePath = "Assets/IUH_Campus/Textures/";

        public static void GenerateAllTextures()
        {
            if (!Directory.Exists(TexturePath))
            {
                Directory.CreateDirectory(TexturePath);
            }

            GenerateCurtainWallSign();
            GenerateTopLogoCrest();
            GenerateVietnamFlag();
            GenerateIUHFlag();
            GenerateYouthUnionFlag();
            GenerateRoadArrow();
            GenerateGraniteTexture();
            GenerateGrassTexture();
            GenerateFlowerTexture();
            GenerateBuildingGSign();
            GenerateWeatheredWallTexture();
            GenerateConcreteSeamsTexture();
            GenerateAsphaltRoadTexture();
            GenerateDrainGrateTexture();
            GenerateManholeCoverTexture();
            GenerateDirectionalSignTexture();
            GenerateNoticeBoardTexture();
            GenerateACLouversTexture();
            GenerateWindowBlindsTexture();
            GenerateTreeBarkTexture();
            GenerateWayfindingSignTexture();
            GenerateFireExtinguisherTexture();
            GenerateAccessControlTexture();
            GenerateUrbanHouseTexture();

            AssetDatabase.Refresh();
            Debug.Log("[IUH] All textures generated successfully in " + TexturePath);
        }

        private static void SaveTexture(Texture2D tex, string fileName)
        {
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(TexturePath, fileName);
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

        private static void GenerateCurtainWallSign()
        {
            string fullPath = Path.Combine(TexturePath, "T_IUH_Sign_CurtainWall.png");
            if (File.Exists(fullPath))
            {
                AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
                return;
            }

            int width = 1024;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Semi-translucent white/frosted glass backing with dark text/logo
            Color bgColor = new Color(0.95f, 0.97f, 1.0f, 0.88f);
            Color borderBlue = new Color(0.12f, 0.35f, 0.70f, 1.0f);
            Color textDark = new Color(0.08f, 0.15f, 0.30f, 1.0f);
            Color redLogo = new Color(0.85f, 0.12f, 0.12f, 1.0f);
            Color blueLogo = new Color(0.10f, 0.40f, 0.80f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Gradient glass reflection
                    float grad = 0.92f + 0.08f * Mathf.Sin((float)x / width * Mathf.PI);
                    Color c = bgColor * grad;
                    c.a = 0.90f;

                    // Subtle border
                    if (x < 8 || x >= width - 8 || y < 8 || y >= height - 8)
                    {
                        c = borderBlue;
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            // Draw central IUH Logo emblem & stylized flame / lotus
            int logoCx = width / 2 - 180;
            int logoCy = height / 2;
            int logoRadius = 90;

            for (int y = logoCy - logoRadius; y <= logoCy + logoRadius; y++)
            {
                for (int x = logoCx - logoRadius; x <= logoCx + logoRadius; x++)
                {
                    float dx = x - logoCx;
                    float dy = y - logoCy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= logoRadius && dist >= logoRadius - 8)
                    {
                        tex.SetPixel(x, y, borderBlue);
                    }
                    else if (dist < logoRadius - 8)
                    {
                        // Flame petal shape
                        float normY = (y - (logoCy - logoRadius)) / (float)(2 * logoRadius);
                        float petalW = Mathf.Sin(normY * Mathf.PI) * (logoRadius - 16);
                        if (Mathf.Abs(dx) < petalW)
                        {
                            if (dx < 0)
                                tex.SetPixel(x, y, redLogo);
                            else
                                tex.SetPixel(x, y, blueLogo);
                        }
                    }
                }
            }

            // Draw clean block text representations for "IUH"
            DrawBlockLetter(tex, width / 2 - 20, height / 2 + 10, 48, 80, 14, 'I', textDark);
            DrawBlockLetter(tex, width / 2 + 50, height / 2 + 10, 56, 80, 14, 'U', textDark);
            DrawBlockLetter(tex, width / 2 + 130, height / 2 + 10, 56, 80, 14, 'H', textDark);

            // Subtitle banner lines for "INDUSTRIAL UNIVERSITY OF HO CHI MINH CITY"
            for (int y = height / 2 - 60; y <= height / 2 - 45; y++)
            {
                for (int x = width / 2 - 40; x < width / 2 + 320; x++)
                {
                    if (x % 12 > 2)
                        tex.SetPixel(x, y, borderBlue);
                }
            }
            for (int y = height / 2 - 90; y <= height / 2 - 78; y++)
            {
                for (int x = width / 2 - 40; x < width / 2 + 280; x++)
                {
                    if (x % 10 > 2)
                        tex.SetPixel(x, y, textDark);
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Sign_CurtainWall.png");
        }

        private static void GenerateTopLogoCrest()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color blue = new Color(0.10f, 0.40f, 0.85f, 1f);
            Color red = new Color(0.88f, 0.12f, 0.15f, 1f);
            Color white = Color.white;

            int cx = size / 2;
            int cy = size / 2;
            int r = size / 2 - 16;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    if (d > r)
                    {
                        tex.SetPixel(x, y, clear);
                    }
                    else if (d > r - 16)
                    {
                        tex.SetPixel(x, y, blue);
                    }
                    else if (d > r - 32)
                    {
                        tex.SetPixel(x, y, white);
                    }
                    else
                    {
                        // Inner circle
                        float normY = (float)y / size;
                        if (normY > 0.5f)
                        {
                            tex.SetPixel(x, y, new Color(0.95f, 0.98f, 1.0f, 1f));
                        }
                        else
                        {
                            tex.SetPixel(x, y, new Color(0.90f, 0.94f, 0.98f, 1f));
                        }
                    }
                }
            }

            // Draw center emblem
            DrawBlockLetter(tex, cx - 75, cy - 30, 35, 60, 10, 'I', blue);
            DrawBlockLetter(tex, cx - 25, cy - 30, 45, 60, 10, 'U', red);
            DrawBlockLetter(tex, cx + 35, cy - 30, 45, 60, 10, 'H', blue);

            tex.Apply();
            SaveTexture(tex, "T_IUH_Logo_Crest.png");
        }

        private static void GenerateVietnamFlag()
        {
            int w = 512;
            int h = 340;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color red = new Color(0.85f, 0.10f, 0.10f, 1f);
            Color yellow = new Color(1.0f, 0.88f, 0.05f, 1f);

            int cx = w / 2;
            int cy = h / 2;
            float outerR = h * 0.32f;
            float innerR = outerR * (Mathf.Sin(18f * Mathf.Deg2Rad) / Mathf.Sin(54f * Mathf.Deg2Rad));

            // Generate 10 star vertices (alternating outer and inner)
            Vector2[] verts = new Vector2[10];
            for (int i = 0; i < 5; i++)
            {
                float outerAngle = (90f + i * 72f) * Mathf.Deg2Rad;
                verts[i * 2] = new Vector2(cx + outerR * Mathf.Cos(outerAngle), cy + outerR * Mathf.Sin(outerAngle));

                float innerAngle = (90f + 36f + i * 72f) * Mathf.Deg2Rad;
                verts[i * 2 + 1] = new Vector2(cx + innerR * Mathf.Cos(innerAngle), cy + innerR * Mathf.Sin(innerAngle));
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, red);
                }
            }

            // Ray-casting point in polygon test for star
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool inside = false;
                    for (int i = 0, j = 9; i < 10; j = i++)
                    {
                        if (((verts[i].y > y) != (verts[j].y > y)) &&
                            (x < (verts[j].x - verts[i].x) * (y - verts[i].y) / (verts[j].y - verts[i].y) + verts[i].x))
                        {
                            inside = !inside;
                        }
                    }

                    if (inside)
                    {
                        tex.SetPixel(x, y, yellow);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_Flag_Vietnam.png");
        }

        private static void GenerateIUHFlag()
        {
            int w = 512;
            int h = 340;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color blue = new Color(0.12f, 0.40f, 0.80f, 1f);
            Color white = Color.white;

            int cx = w / 2;
            int cy = h / 2;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, blue);
                }
            }

            // Draw white circle in middle with "IUH"
            int r = 100;
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= r * r)
                    {
                        tex.SetPixel(x, y, white);
                    }
                }
            }

            DrawBlockLetter(tex, cx - 60, cy - 25, 28, 50, 8, 'I', blue);
            DrawBlockLetter(tex, cx - 20, cy - 25, 36, 50, 8, 'U', blue);
            DrawBlockLetter(tex, cx + 25, cy - 25, 36, 50, 8, 'H', blue);

            tex.Apply();
            SaveTexture(tex, "T_Flag_IUH.png");
        }

        private static void GenerateYouthUnionFlag()
        {
            int w = 512;
            int h = 340;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color red = new Color(0.85f, 0.12f, 0.12f, 1f);
            Color green = new Color(0.10f, 0.55f, 0.25f, 1f);
            Color gold = new Color(1.0f, 0.85f, 0.10f, 1f);

            int cx = w / 2;
            int cy = h / 2;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, (x + y < w + 80) ? red : green);
                }
            }

            // Draw central golden star and Youth emblem ring
            int r = 85;
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= r && dist >= r - 8)
                    {
                        tex.SetPixel(x, y, gold);
                    }
                    else if (dist < r - 8)
                    {
                        tex.SetPixel(x, y, new Color(0.12f, 0.45f, 0.20f, 1f));
                    }
                }
            }

            float outerR = r * 0.65f;
            float innerR = outerR * 0.382f;
            for (int y = cy - (int)outerR; y <= cy + (int)outerR; y++)
            {
                for (int x = cx - (int)outerR; x <= cx + (int)outerR; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float angle = Mathf.Atan2(dy, dx) + Mathf.PI / 2f;
                    if (angle < 0) angle += 2f * Mathf.PI;

                    float armAngle = Mathf.PI * 2f / 5f;
                    float segment = (angle % armAngle) - (armAngle / 2f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    float maxD = innerR / Mathf.Cos(segment + (Mathf.PI / 5f));
                    float peakD = outerR * (1f - Mathf.Abs(segment) / (armAngle / 2f));
                    if (d <= Mathf.Max(maxD, peakD * 0.95f) && d <= outerR)
                    {
                        tex.SetPixel(x, y, gold);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_Flag_YouthUnion.png");
        }

        private static void GenerateRoadArrow()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color yellow = new Color(1f, 0.85f, 0.15f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            for (int y = 100; y < 420; y++)
            {
                for (int x = 120; x < 400; x++)
                {
                    int offset = (int)(Mathf.Abs(y - 260) * 0.75f);
                    if (x >= 200 + offset && x <= 280 + offset)
                    {
                        tex.SetPixel(x, y, yellow);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_Road_Arrow_Yellow.png");
        }

        private static void GenerateGraniteTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color baseC = new Color(0.82f, 0.84f, 0.86f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.12f;
                    float speckle = (Random.value < 0.2f) ? (Random.value * 0.15f - 0.08f) : 0f;
                    Color c = baseC + new Color(noise + speckle, noise + speckle, noise + speckle, 0f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Granite_Noise.png");
        }

        private static void GenerateGrassTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color darkG = new Color(0.18f, 0.48f, 0.15f, 1f);
            Color lightG = new Color(0.32f, 0.65f, 0.20f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.2f, y * 0.2f);
                    tex.SetPixel(x, y, Color.Lerp(darkG, lightG, n));
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Grass_Noise.png");
        }

        private static void GenerateFlowerTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color yellow = new Color(1.0f, 0.82f, 0.1f, 1f);
            Color orange = new Color(0.98f, 0.45f, 0.05f, 1f);
            Color leaf = new Color(0.15f, 0.45f, 0.12f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.3f, y * 0.3f);
                    Color c = (n > 0.45f) ? Color.Lerp(yellow, orange, (n - 0.45f) * 2f) : leaf;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_Flowers_YellowOrange.png");
        }

        private static void DrawBlockLetter(Texture2D tex, int x0, int y0, int w, int h, int thickness, char letter, Color col)
        {
            for (int y = y0; y < y0 + h; y++)
            {
                for (int x = x0; x < x0 + w; x++)
                {
                    bool fill = false;
                    int relX = x - x0;
                    int relY = y - y0;

                    switch (letter)
                    {
                        case 'I':
                            fill = (relX >= (w - thickness) / 2 && relX <= (w + thickness) / 2) ||
                                   (relY < thickness) || (relY >= h - thickness);
                            break;
                        case 'U':
                            fill = (relX < thickness) || (relX >= w - thickness) || (relY < thickness);
                            break;
                        case 'H':
                            fill = (relX < thickness) || (relX >= w - thickness) ||
                                   (relY >= (h - thickness) / 2 && relY <= (h + thickness) / 2);
                            break;
                    }

                    if (fill)
                    {
                        tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static void GenerateBuildingGSign()
        {
            int width = 1024;
            int height = 128;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color bgBlue = new Color(0.02f, 0.18f, 0.48f, 1.0f); // Deep IUH Blue #052E7A
            Color goldAccent = new Color(0.96f, 0.78f, 0.18f, 1.0f); // Gold #F5C72E
            Color textWhite = new Color(0.98f, 0.98f, 0.98f, 1.0f);
            Color emblemRed = new Color(0.85f, 0.12f, 0.12f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Base deep blue
                    Color c = bgBlue;

                    // Gold top and bottom accent stripes
                    if (y < 6 || y >= height - 6)
                    {
                        c = goldAccent;
                    }
                    else if (y == 7 || y == height - 7)
                    {
                        c = new Color(0.7f, 0.55f, 0.1f, 1f);
                    }

                    // Left & right border
                    if (x < 6 || x >= width - 6)
                    {
                        c = goldAccent;
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            // Left side circular emblem
            int emblemCx = 64;
            int emblemCy = height / 2;
            int emblemRadius = 38;

            for (int y = emblemCy - emblemRadius; y <= emblemCy + emblemRadius; y++)
            {
                for (int x = emblemCx - emblemRadius; x <= emblemCx + emblemRadius; x++)
                {
                    float dx = x - emblemCx;
                    float dy = y - emblemCy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= emblemRadius && dist >= emblemRadius - 4)
                    {
                        tex.SetPixel(x, y, goldAccent);
                    }
                    else if (dist < emblemRadius - 4)
                    {
                        if (dx < 0)
                            tex.SetPixel(x, y, emblemRed);
                        else
                            tex.SetPixel(x, y, new Color(0.1f, 0.45f, 0.85f, 1f));
                    }
                }
            }

            // Primary title text banner pattern: "TRƯỜNG ĐẠI HỌC CÔNG NGHIỆP TP. HỒ CHÍ MINH"
            // Stylized high-contrast bold signage bars and glyphs
            int startX = 125;
            int textW = width - 150;
            int textY = height / 2 - 12;
            int textH = 26;

            for (int y = textY; y < textY + textH; y++)
            {
                for (int x = startX; x < startX + textW; x++)
                {
                    // Alternating rhythmic character glyph patterns representing Vietnamese text
                    int glyphIndex = (x - startX) / 16;
                    int glyphOffset = (x - startX) % 16;

                    // Space between words
                    bool isWordSpace = (glyphIndex == 6 || glyphIndex == 13 || glyphIndex == 22 || glyphIndex == 26 || glyphIndex == 30 || glyphIndex == 34);
                    if (!isWordSpace && glyphOffset > 2 && glyphOffset < 14)
                    {
                        // Crossbar or vertical stems
                        bool isStroke = (glyphOffset <= 5 || glyphOffset >= 11 || y <= textY + 4 || y >= textY + textH - 5 || (y >= textY + 11 && y <= textY + 15));
                        if (isStroke)
                        {
                            tex.SetPixel(x, y, goldAccent);
                        }
                    }
                }
            }

            // English subtitle bar: "INDUSTRIAL UNIVERSITY OF HO CHI MINH CITY"
            int subY = textY - 20;
            int subH = 10;
            for (int y = subY; y < subY + subH; y++)
            {
                for (int x = startX + 10; x < startX + textW - 40; x++)
                {
                    int subIndex = (x - startX) / 9;
                    int subOffset = (x - startX) % 9;
                    if (subOffset > 1 && subOffset < 7)
                    {
                        tex.SetPixel(x, y, textWhite);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Sign_BuildingG.png");
        }

        private static void GenerateWeatheredWallTexture()
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color baseWall = new Color(0.92f, 0.91f, 0.88f, 1f); // Warm off-white
            System.Random rnd = new System.Random(42);

            for (int y = 0; y < height; y++)
            {
                float vNorm = (float)y / height;
                // Subtle bottom contact grime gradient & top under-eave shade
                float verticalFactor = 1.0f;
                if (vNorm < 0.15f)
                {
                    verticalFactor -= (0.15f - vNorm) * 0.45f; // Slight contact dirt at base
                }
                else if (vNorm > 0.85f)
                {
                    verticalFactor -= (vNorm - 0.85f) * 0.25f; // Slight shadow under roof eave
                }

                for (int x = 0; x < width; x++)
                {
                    // Fine plaster noise
                    float noise = (float)(rnd.NextDouble() * 0.05f - 0.025f);
                    // Subtle vertical water streak modulation
                    float streak = Mathf.Sin(x * 0.12f) * Mathf.Cos(x * 0.04f + y * 0.01f) * 0.025f;

                    float factor = Mathf.Clamp01(verticalFactor + noise + streak);
                    Color c = new Color(baseWall.r * factor, baseWall.g * factor, baseWall.b * (factor * 0.98f), 1f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Wall_Weathered.png");
        }

        private static void GenerateConcreteSeamsTexture()
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color concreteBase = new Color(0.78f, 0.79f, 0.81f, 1f);
            Color seamDark = new Color(0.42f, 0.43f, 0.45f, 1f);
            System.Random rnd = new System.Random(88);

            int tileSize = 128;

            for (int y = 0; y < height; y++)
            {
                int modY = y % tileSize;
                bool isSeamY = (modY <= 2 || modY >= tileSize - 2);

                for (int x = 0; x < width; x++)
                {
                    int modX = x % tileSize;
                    bool isSeamX = (modX <= 2 || modX >= tileSize - 2);

                    float noise = (float)(rnd.NextDouble() * 0.06f - 0.03f);
                    Color c = concreteBase + new Color(noise, noise, noise, 0f);

                    if (isSeamX || isSeamY)
                    {
                        c = Color.Lerp(c, seamDark, 0.65f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Concrete_Seams.png");
        }

        private static void GenerateAsphaltRoadTexture()
        {
            int width = 512;
            int height = 512;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color asphaltBase = new Color(0.28f, 0.29f, 0.31f, 1f);
            System.Random rnd = new System.Random(133);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Fine aggregate speckling
                    float noise = (float)(rnd.NextDouble() * 0.08f - 0.04f);
                    // Slight tire wear bands
                    float wear = Mathf.Sin(x * 0.03f) * 0.02f;
                    Color c = asphaltBase + new Color(noise + wear, noise + wear, noise + wear, 0f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Asphalt_Cracks.png");
        }

        private static void GenerateDrainGrateTexture()
        {
            int width = 256;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color iron = new Color(0.22f, 0.23f, 0.25f, 1f);
            Color ironHighlight = new Color(0.35f, 0.37f, 0.40f, 1f);
            Color voidBlack = new Color(0.04f, 0.04f, 0.05f, 1f);

            int rim = 16;
            int slotPeriod = 16;

            for (int y = 0; y < height; y++)
            {
                bool inRimY = (y < rim || y >= height - rim);

                for (int x = 0; x < width; x++)
                {
                    bool inRimX = (x < rim || x >= width - rim);

                    if (inRimX || inRimY)
                    {
                        // Outer cast frame
                        tex.SetPixel(x, y, iron);
                    }
                    else
                    {
                        int slotX = (x - rim) % slotPeriod;
                        if (slotX < 7)
                        {
                            // Iron bar
                            float t = (float)slotX / 6f;
                            Color c = Color.Lerp(iron, ironHighlight, Mathf.Sin(t * Mathf.PI));
                            tex.SetPixel(x, y, c);
                        }
                        else
                        {
                            // Drain slot opening (void)
                            tex.SetPixel(x, y, voidBlack);
                        }
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Drain_Grate.png");
        }

        private static void GenerateManholeCoverTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color ironBase = new Color(0.25f, 0.26f, 0.28f, 1f);
            Color ironGrip = new Color(0.38f, 0.40f, 0.43f, 1f);
            Color ironGroove = new Color(0.12f, 0.13f, 0.14f, 1f);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.46f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);

                    if (dist > radius + 4f)
                    {
                        // Surrounding asphalt/concrete rim
                        tex.SetPixel(x, y, ironGroove);
                    }
                    else if (dist > radius - 6f)
                    {
                        // Raised outer rim
                        tex.SetPixel(x, y, ironBase);
                    }
                    else
                    {
                        // Internal tread pattern (radial notches and concentric rings)
                        float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);
                        bool isRing = Mathf.Abs(dist - radius * 0.65f) < 3f || Mathf.Abs(dist - radius * 0.35f) < 3f;
                        bool isRadialSpoke = Mathf.Abs(Mathf.Sin(angle * 12f)) > 0.85f;

                        if (isRing || isRadialSpoke)
                        {
                            tex.SetPixel(x, y, ironGrip);
                        }
                        else
                        {
                            tex.SetPixel(x, y, ironBase);
                        }
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Manhole_Cover.png");
        }

        private static void GenerateDirectionalSignTexture()
        {
            int width = 512;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color headerBlue = new Color(0.08f, 0.32f, 0.68f, 1f);
            Color boardWhite = new Color(0.96f, 0.97f, 0.98f, 1f);
            Color textDark = new Color(0.12f, 0.15f, 0.20f, 1f);
            Color arrowGreen = new Color(0.10f, 0.62f, 0.28f, 1f);

            int headerH = 64;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y >= height - headerH)
                    {
                        tex.SetPixel(x, y, headerBlue);
                    }
                    else
                    {
                        tex.SetPixel(x, y, boardWhite);
                    }

                    // Border
                    if (x < 6 || x >= width - 6 || y < 6 || y >= height - 6)
                    {
                        tex.SetPixel(x, y, headerBlue);
                    }
                }
            }

            // Draw directional text bands & arrow icons
            int[] lineYs = { 150, 105, 60, 20 };
            for (int i = 0; i < lineYs.Length; i++)
            {
                int ly = lineYs[i];
                // Draw arrow indicator on left
                for (int dy = 0; dy < 20; dy++)
                {
                    for (int dx = 0; dx < 24; dx++)
                    {
                        if (dx > 4 && dy > 4 && dy < 16)
                        {
                            tex.SetPixel(24 + dx, ly + dy, arrowGreen);
                        }
                    }
                }

                // Text line blocks
                for (int ty = 0; ty < 16; ty++)
                {
                    for (int tx = 60; tx < width - 40; tx++)
                    {
                        int charIdx = (tx - 60) / 18;
                        int charOff = (tx - 60) % 18;
                        if (charOff > 2 && charOff < 14)
                        {
                            tex.SetPixel(tx, ly + ty, textDark);
                        }
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Sign_Directional.png");
        }

        private static void GenerateNoticeBoardTexture()
        {
            int width = 512;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color corkBacking = new Color(0.85f, 0.76f, 0.62f, 1f);
            Color woodFrame = new Color(0.48f, 0.32f, 0.20f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < 12 || x >= width - 12 || y < 12 || y >= height - 12)
                    {
                        tex.SetPixel(x, y, woodFrame);
                    }
                    else
                    {
                        tex.SetPixel(x, y, corkBacking);
                    }
                }
            }

            // Draw student flyers and notice posters pinned to board
            Color[] flyerColors = {
                new Color(0.98f, 0.98f, 0.98f, 1f), // White official bulletin
                new Color(0.95f, 0.88f, 0.25f, 1f), // Yellow student activity flyer
                new Color(0.25f, 0.65f, 0.92f, 1f), // Blue youth union poster
                new Color(0.92f, 0.35f, 0.35f, 1f), // Red announcement
                new Color(0.96f, 0.96f, 0.96f, 1f)  // White timetable
            };

            int[] flyerXs = { 24, 120, 220, 310, 410 };
            int[] flyerYs = { 30, 45, 25, 40, 35 };
            int[] flyerWs = { 80, 85, 75, 85, 75 };
            int[] flyerHs = { 180, 160, 190, 170, 180 };

            for (int f = 0; f < flyerXs.Length; f++)
            {
                int fx = flyerXs[f];
                int fy = flyerYs[f];
                int fw = flyerWs[f];
                int fh = flyerHs[f];
                Color fc = flyerColors[f];

                for (int py = fy; py < fy + fh; py++)
                {
                    for (int px = fx; px < fx + fw; px++)
                    {
                        if (px < width - 14 && py < height - 14)
                        {
                            // Header bar on flyer
                            if (py > fy + fh - 24)
                            {
                                tex.SetPixel(px, py, Color.Lerp(fc, Color.blue, 0.3f));
                            }
                            else
                            {
                                // Text lines on flyer
                                int lineMod = py % 8;
                                if (lineMod < 3 && px > fx + 6 && px < fx + fw - 6)
                                {
                                    tex.SetPixel(px, py, new Color(0.2f, 0.2f, 0.25f, 1f));
                                }
                                else
                                {
                                    tex.SetPixel(px, py, fc);
                                }
                            }
                        }
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Sign_NoticeBoard.png");
        }

        private static void GenerateACLouversTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color casing = new Color(0.88f, 0.89f, 0.90f, 1f);
            Color darkGrill = new Color(0.18f, 0.20f, 0.22f, 1f);
            Color fanSilhouette = new Color(0.12f, 0.13f, 0.14f, 1f);
            Vector2 fanCenter = new Vector2(size * 0.62f, size * 0.5f);
            float fanRadius = size * 0.36f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Outer border
                    if (x < 10 || x >= size - 10 || y < 10 || y >= size - 10)
                    {
                        tex.SetPixel(x, y, casing);
                    }
                    else
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), fanCenter);
                        if (dist < fanRadius)
                        {
                            // Circular fan circular grill
                            int ring = ((int)dist) % 12;
                            if (ring < 3)
                            {
                                tex.SetPixel(x, y, fanSilhouette);
                            }
                            else
                            {
                                tex.SetPixel(x, y, darkGrill);
                            }
                        }
                        else
                        {
                            // Horizontal side louvers
                            int louver = y % 10;
                            if (louver < 3)
                            {
                                tex.SetPixel(x, y, darkGrill);
                            }
                            else
                            {
                                tex.SetPixel(x, y, casing);
                            }
                        }
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_AC_Louvers.png");
        }

        private static void GenerateWindowBlindsTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color blindLight = new Color(0.92f, 0.90f, 0.85f, 1f);
            Color blindShadow = new Color(0.65f, 0.63f, 0.58f, 1f);

            for (int y = 0; y < size; y++)
            {
                int slatMod = y % 8;
                float slatFactor = (float)slatMod / 7f;
                Color c = Color.Lerp(blindLight, blindShadow, slatFactor);

                for (int x = 0; x < size; x++)
                {
                    // Frame edge
                    if (x < 6 || x >= size - 6 || y < 6 || y >= size - 6)
                    {
                        tex.SetPixel(x, y, new Color(0.3f, 0.32f, 0.35f, 1f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Window_Blinds.png");
        }

        private static void GenerateTreeBarkTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color barkDark = new Color(0.32f, 0.28f, 0.22f, 1f);
            Color barkLight = new Color(0.48f, 0.42f, 0.35f, 1f);
            System.Random rnd = new System.Random(77);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float furrow = Mathf.Sin(x * 0.25f + y * 0.05f) * 0.5f + 0.5f;
                    float noise = (float)(rnd.NextDouble() * 0.15f - 0.075f);
                    Color c = Color.Lerp(barkDark, barkLight, Mathf.Clamp01(furrow + noise));
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            SaveTexture(tex, "T_IUH_Tree_Bark.png");
        }

        private static void GenerateWayfindingSignTexture()
        {
            int width = 512;
            int height = 256;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color navyBlue = new Color(0.05f, 0.16f, 0.35f, 1f);
            Color headerBlue = new Color(0.08f, 0.28f, 0.62f, 1f);
            Color white = Color.white;
            Color yellowAccent = new Color(0.96f, 0.78f, 0.12f, 1f);
            Color border = new Color(0.85f, 0.88f, 0.92f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < 6 || x >= width - 6 || y < 6 || y >= height - 6);
                    if (isBorder)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else if (y > height - 60)
                    {
                        tex.SetPixel(x, y, headerBlue);
                    }
                    else if (y > height - 66)
                    {
                        tex.SetPixel(x, y, yellowAccent);
                    }
                    else
                    {
                        // Directional row stripes
                        int row = (y - 10) / 45;
                        bool isDivider = (y - 10) % 45 == 0;
                        tex.SetPixel(x, y, isDivider ? border : navyBlue);
                    }
                }
            }

            // Draw procedural directional arrows & icons
            for (int r = 0; r < 3; r++)
            {
                int cy = 40 + r * 45;
                int cx = 35;
                for (int dy = -10; dy <= 10; dy++)
                {
                    for (int dx = -10; dx <= 10; dx++)
                    {
                        if (Mathf.Abs(dy) + dx < 8 && dx >= -6)
                        {
                            tex.SetPixel(cx + dx, cy + dy, yellowAccent);
                        }
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Sign_Wayfinding.png");
        }

        private static void GenerateFireExtinguisherTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color red = new Color(0.82f, 0.12f, 0.10f, 1f);
            Color yellow = new Color(0.96f, 0.78f, 0.12f, 1f);
            Color white = Color.white;
            Color dark = new Color(0.12f, 0.12f, 0.12f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 190 && y <= 215)
                    {
                        tex.SetPixel(x, y, yellow); // Warning band
                    }
                    else if (y >= 40 && y <= 180 && x >= 30 && x <= size - 30)
                    {
                        // Instruction label background
                        tex.SetPixel(x, y, white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, red);
                    }
                }
            }

            // Pictogram icons inside white label
            for (int py = 70; py <= 150; py++)
            {
                for (int px = 50; px <= 110; px++)
                {
                    if ((px - 80) * (px - 80) + (py - 110) * (py - 110) < 400)
                    {
                        tex.SetPixel(px, py, red);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Fire_Extinguisher.png");
        }

        private static void GenerateAccessControlTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color casing = new Color(0.12f, 0.13f, 0.15f, 1f);
            Color bezel = new Color(0.25f, 0.26f, 0.28f, 1f);
            Color greenLED = new Color(0.15f, 0.95f, 0.35f, 1f);
            Color sensorPad = new Color(0.18f, 0.20f, 0.22f, 1f);
            Color cardIcon = new Color(0.70f, 0.75f, 0.80f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBezel = (x < 6 || x >= size - 6 || y < 6 || y >= size - 6);
                    if (isBezel)
                    {
                        tex.SetPixel(x, y, bezel);
                    }
                    else if (y >= size - 24 && x >= size / 2 - 8 && x <= size / 2 + 8 && y <= size - 12)
                    {
                        tex.SetPixel(x, y, greenLED); // Indicator LED
                    }
                    else if (y >= 20 && y <= size - 35 && x >= 20 && x <= size - 20)
                    {
                        // RFID sensing surface with card icon
                        bool isCardBorder = (x == 35 || x == size - 35 || y == 40 || y == size - 50);
                        tex.SetPixel(x, y, isCardBorder ? cardIcon : sensorPad);
                    }
                    else
                    {
                        tex.SetPixel(x, y, casing);
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Access_Control.png");
        }

        private static void GenerateUrbanHouseTexture()
        {
            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color pastelYellow = new Color(0.92f, 0.86f, 0.72f, 1f);
            Color pastelMint = new Color(0.78f, 0.85f, 0.80f, 1f);
            Color darkWindow = new Color(0.15f, 0.20f, 0.24f, 1f);
            Color roofRed = new Color(0.72f, 0.22f, 0.16f, 1f);
            Color metalGreen = new Color(0.42f, 0.58f, 0.48f, 1f);
            Color concreteSill = new Color(0.68f, 0.70f, 0.72f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Roof section
                    if (y > size - 70)
                    {
                        int rib = (x / 14) % 2;
                        tex.SetPixel(x, y, (x < size / 2) ? (rib == 0 ? roofRed : roofRed * 0.85f) : metalGreen);
                    }
                    else
                    {
                        Color wallCol = (x < size / 2) ? pastelYellow : pastelMint;
                        // Floor bands
                        if (y % 110 < 8)
                        {
                            tex.SetPixel(x, y, concreteSill);
                        }
                        else
                        {
                            // Window openings
                            int wx = x % 128;
                            int wy = y % 110;
                            if (wx >= 35 && wx <= 95 && wy >= 25 && wy <= 90)
                            {
                                tex.SetPixel(x, y, darkWindow);
                            }
                            else
                            {
                                tex.SetPixel(x, y, wallCol);
                            }
                        }
                    }
                }
            }

            tex.Apply();
            SaveTexture(tex, "T_IUH_Urban_House.png");
        }
    }
}

