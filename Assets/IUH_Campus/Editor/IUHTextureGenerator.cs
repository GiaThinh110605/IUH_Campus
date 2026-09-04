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
    }
}
