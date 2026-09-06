using System.IO;
using UnityEngine;
using UnityEditor;

namespace IUHCampus.Editor
{
    public static class IUHMaterialGenerator
    {
        private const string MatPath = "Assets/IUH_Campus/Materials/";
        private const string TexPath = "Assets/IUH_Campus/Textures/";

        public static void GenerateAllMaterials()
        {
            if (!Directory.Exists(MatPath))
            {
                Directory.CreateDirectory(MatPath);
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                litShader = Shader.Find("Standard");
            }

            // 1. Structural Concrete / White Facade Frame
            CreateLitMaterial("M_IUH_WhiteFacade", litShader, new Color(0.93f, 0.94f, 0.96f, 1f), 0.25f, 0.05f);

            // 2. Pastel Sky-Blue Facade Fin
            CreateLitMaterial("M_IUH_BlueFins", litShader, new Color(0.48f, 0.68f, 0.86f, 1f), 0.40f, 0.10f);

            // 3. Pale Blue Facade Wall Panels
            CreateLitMaterial("M_IUH_BlueFacade", litShader, new Color(0.72f, 0.82f, 0.92f, 1f), 0.30f, 0.05f);

            // 4. Dark Metal (Frames, Louver Trims, Canopy Frame)
            CreateLitMaterial("M_IUH_DarkMetal", litShader, new Color(0.15f, 0.17f, 0.20f, 1f), 0.65f, 0.85f);

            // 5. Light Metal / Chrome (Lettering, Handrails, Flagpoles)
            CreateLitMaterial("M_IUH_LightMetal", litShader, new Color(0.85f, 0.88f, 0.92f, 1f), 0.85f, 0.90f);

            // 6. Architectural Reflective Blue-Gray Glass (Opaque/Reflective for Windows)
            CreateLitMaterial("M_IUH_Glass_Reflective", litShader, new Color(0.18f, 0.32f, 0.45f, 1f), 0.95f, 0.45f);

            // 7. Transparent Entrance Glass
            CreateTransparentGlass("M_IUH_Glass_CurtainWall", litShader, new Color(0.75f, 0.88f, 0.96f, 0.35f), 0.95f, 0.2f);

            // 8. Billboard Sign Material
            Material signMat = CreateLitMaterial("M_IUH_Sign_CurtainWall", litShader, Color.white, 0.5f, 0.0f);
            Texture2D signTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Sign_CurtainWall.png");
            if (signTex != null)
            {
                signMat.mainTexture = signTex;
                signMat.SetColor("_BaseColor", Color.white);
            }

            // 9. Crest Logo Material
            Material crestMat = CreateTransparentCutout("M_IUH_Logo_Crest", litShader);
            Texture2D crestTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Logo_Crest.png");
            if (crestTex != null)
            {
                crestMat.mainTexture = crestTex;
            }

            // 10. Granite Steps & Entrance Podium
            Material graniteMat = CreateLitMaterial("M_IUH_Granite_Stairs", litShader, new Color(0.35f, 0.38f, 0.42f, 1f), 0.60f, 0.05f);
            Texture2D graniteTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Granite_Noise.png");
            if (graniteTex != null)
            {
                graniteMat.mainTexture = graniteTex;
                graniteMat.mainTextureScale = new Vector2(4, 4);
            }

            // 11. Asphalt Road
            CreateLitMaterial("M_IUH_Asphalt_Road", litShader, new Color(0.28f, 0.30f, 0.33f, 1f), 0.15f, 0.0f);

            // 12. Concrete Sidewalk & Curb
            CreateLitMaterial("M_IUH_Concrete_Ground", litShader, new Color(0.78f, 0.80f, 0.82f, 1f), 0.20f, 0.0f);

            // 13. Grass Island
            Material grassMat = CreateLitMaterial("M_IUH_Grass", litShader, new Color(0.25f, 0.55f, 0.18f, 1f), 0.10f, 0.0f);
            Texture2D grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Grass_Noise.png");
            if (grassTex != null)
            {
                grassMat.mainTexture = grassTex;
                grassMat.mainTextureScale = new Vector2(8, 8);
            }

            // 14. Flowers (Yellow/Orange)
            Material flowerMat = CreateLitMaterial("M_IUH_Flower_Orange", litShader, Color.white, 0.2f, 0.0f);
            Texture2D flowerTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Flowers_YellowOrange.png");
            if (flowerTex != null)
            {
                flowerMat.mainTexture = flowerTex;
            }

            // 15. White Flower Pot / Planter
            CreateLitMaterial("M_IUH_FlowerPot_White", litShader, new Color(0.92f, 0.94f, 0.95f, 1f), 0.40f, 0.0f);

            // 16. Foliage / Tree Leaves
            CreateLitMaterial("M_IUH_Foliage_Tree", litShader, new Color(0.18f, 0.42f, 0.12f, 1f), 0.15f, 0.0f);

            // 17. Monument Stone (White Sculpted Rock)
            CreateLitMaterial("M_IUH_Monument_Stone", litShader, new Color(0.90f, 0.92f, 0.93f, 1f), 0.35f, 0.02f);

            // 18. Monument 3D Letters ("IUH" Light Gold/Silver Polish)
            CreateLitMaterial("M_IUH_Monument_Letter", litShader, new Color(0.85f, 0.87f, 0.90f, 1f), 0.90f, 0.85f);

            // 19. Vietnam National Flag
            Material flagVnMat = CreateLitMaterial("M_IUH_Flag_Vietnam", litShader, Color.white, 0.2f, 0.0f);
            Texture2D flagVnTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Flag_Vietnam.png");
            if (flagVnTex != null)
            {
                flagVnMat.mainTexture = flagVnTex;
            }

            // 20. IUH Flag
            Material flagIuhMat = CreateLitMaterial("M_IUH_Flag_IUH", litShader, Color.white, 0.2f, 0.0f);
            Texture2D flagIuhTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Flag_IUH.png");
            if (flagIuhTex != null)
            {
                flagIuhMat.mainTexture = flagIuhTex;
            }

            // 20b. Youth Union Flag (Cờ Đoàn Thanh Niên / Cờ Truyền Thống)
            Material flagYouthMat = CreateLitMaterial("M_IUH_Flag_YouthUnion", litShader, Color.white, 0.2f, 0.0f);
            Texture2D flagYouthTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Flag_YouthUnion.png");
            if (flagYouthTex != null)
            {
                flagYouthMat.mainTexture = flagYouthTex;
            }

            // 21. Road Direction Arrow
            Material arrowMat = CreateTransparentCutout("M_IUH_Road_Arrow", litShader);
            Texture2D arrowTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Road_Arrow_Yellow.png");
            if (arrowTex != null)
            {
                arrowMat.mainTexture = arrowTex;
            }

            // 22. Background Buildings
            CreateLitMaterial("M_IUH_Background_Building", litShader, new Color(0.82f, 0.85f, 0.88f, 1f), 0.25f, 0.05f);

            // ================= NEW MATERIALS FOR RIGHT-SIDE EXPANSION =================

            // 23. Turquoise / Emerald Green Glass (Signature Green Tower)
            CreateLitMaterial("M_IUH_GreenGlass", litShader, new Color(0.12f, 0.52f, 0.42f, 1f), 0.92f, 0.40f);

            // 24. Weathered Terracotta / Red Metal Roof (Rear Academic Block)
            CreateLitMaterial("M_IUH_RedMetalRoof", litShader, new Color(0.72f, 0.18f, 0.12f, 1f), 0.35f, 0.15f);

            // 25. Pale Sage-Green / Khaki Corrugated Metal Roof (Low-Rise Academic Wing)
            CreateLitMaterial("M_IUH_PaleGreenRoof", litShader, new Color(0.68f, 0.74f, 0.65f, 1f), 0.30f, 0.10f);

            // 26. Dark Slate Grey Pyramidal Metal Roof (Tower Crown)
            CreateLitMaterial("M_IUH_TowerPyramidRoof", litShader, new Color(0.24f, 0.28f, 0.30f, 1f), 0.45f, 0.30f);

            // 27. Motorbike Paint (Red, Blue, Dark Metallic)
            CreateLitMaterial("M_IUH_Motorbike_Red", litShader, new Color(0.85f, 0.15f, 0.12f, 1f), 0.80f, 0.20f);
            CreateLitMaterial("M_IUH_Motorbike_Dark", litShader, new Color(0.15f, 0.15f, 0.18f, 1f), 0.70f, 0.60f);

            // 28. Blue Tarp / Student Event Canopy Shelter
            CreateLitMaterial("M_IUH_BlueTarp", litShader, new Color(0.12f, 0.38f, 0.78f, 1f), 0.20f, 0.0f);

            // 29. Dense Tropical Shade Foliage (Mature campus trees)
            Material matureFoliageMat = CreateLitMaterial("M_IUH_MatureFoliage", litShader, new Color(0.12f, 0.36f, 0.08f, 1f), 0.15f, 0.0f);
            if (grassTex != null)
            {
                matureFoliageMat.mainTexture = grassTex;
                matureFoliageMat.mainTextureScale = new Vector2(6, 6);
            }

            // 30. Rooftop Mechanical / Water Tank Metal
            CreateLitMaterial("M_IUH_RooftopMetal", litShader, new Color(0.80f, 0.82f, 0.85f, 1f), 0.80f, 0.85f);

            // ================= DEDICATED MAIN BUILDING MATERIALS =================
            CreateLitMaterial("M_IUH_Main_WhiteFacade", litShader, new Color(0.94f, 0.95f, 0.97f, 1f), 0.30f, 0.05f);
            CreateLitMaterial("M_IUH_Main_LightBlueFin", litShader, new Color(0.52f, 0.72f, 0.88f, 1f), 0.45f, 0.10f);
            CreateTransparentGlass("M_IUH_Main_Glass", litShader, new Color(0.70f, 0.85f, 0.95f, 0.40f), 0.95f, 0.15f);
            CreateLitMaterial("M_IUH_Main_Glass_Reflective", litShader, new Color(0.18f, 0.32f, 0.45f, 1f), 0.95f, 0.45f);
            CreateLitMaterial("M_IUH_Main_DarkMetal", litShader, new Color(0.12f, 0.14f, 0.16f, 1f), 0.70f, 0.85f);
            CreateLitMaterial("M_IUH_Main_WhiteMetal", litShader, new Color(0.92f, 0.94f, 0.96f, 1f), 0.80f, 0.85f);
            CreateLitMaterial("M_IUH_Main_Concrete", litShader, new Color(0.76f, 0.78f, 0.80f, 1f), 0.20f, 0.0f);
            CreateLitMaterial("M_IUH_Main_Stone", litShader, new Color(0.30f, 0.33f, 0.36f, 1f), 0.65f, 0.05f);
            CreateLitMaterial("M_IUH_Main_Roof", litShader, new Color(0.18f, 0.20f, 0.22f, 1f), 0.50f, 0.30f);
            CreateLitMaterial("M_IUH_Main_Grass", litShader, new Color(0.25f, 0.55f, 0.18f, 1f), 0.10f, 0.0f);

            // ================= RIGHT LARGE ACADEMIC BUILDING MATERIALS =================
            CreateLitMaterial("M_IUH_Academic_White", litShader, new Color(0.95f, 0.96f, 0.97f, 1f), 0.25f, 0.05f);
            CreateLitMaterial("M_IUH_Academic_Glass", litShader, new Color(0.12f, 0.48f, 0.52f, 1f), 0.92f, 0.40f);
            CreateLitMaterial("M_IUH_Academic_Turquoise", litShader, new Color(0.40f, 0.72f, 0.76f, 1f), 0.50f, 0.10f);
            CreateLitMaterial("M_IUH_Academic_DarkFrame", litShader, new Color(0.18f, 0.22f, 0.25f, 1f), 0.65f, 0.80f);
            CreateLitMaterial("M_IUH_Academic_Concrete", litShader, new Color(0.78f, 0.80f, 0.82f, 1f), 0.20f, 0.0f);

            Material topSignMat = CreateTransparentCutout("M_IUH_Academic_TopSign", litShader);
            Texture2D topSignTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Academic_TopSign.png");
            if (topSignTex != null)
            {
                topSignMat.mainTexture = topSignTex;
            }

            // ================= ENTRANCE GATE MATERIALS =================

            // 31. Gate Stone Pillar (Granite-look beige stone for entrance pillars)
            CreateLitMaterial("M_IUH_Gate_Stone", litShader, new Color(0.88f, 0.86f, 0.82f, 1f), 0.35f, 0.02f);

            // 32. Blue Metal Railing / Fence
            CreateLitMaterial("M_IUH_Gate_BlueRailing", litShader, new Color(0.10f, 0.28f, 0.62f, 1f), 0.65f, 0.80f);

            // 33. Welcome Banner (red/gold horizontal overhead banner)
            Material bannerMat = CreateLitMaterial("M_IUH_Gate_WelcomeBanner", litShader, new Color(0.82f, 0.08f, 0.08f, 1f), 0.30f, 0.0f);
            Texture2D bannerTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_WelcomeBanner.png");
            if (bannerTex != null) { bannerMat.mainTexture = bannerTex; }

            // 34. Security Booth / Guard House
            CreateLitMaterial("M_IUH_Gate_Booth", litShader, new Color(0.92f, 0.94f, 0.96f, 1f), 0.28f, 0.05f);

            // 35. Booth Roof (same blue shade as railings)
            CreateLitMaterial("M_IUH_Gate_BoothRoof", litShader, new Color(0.10f, 0.28f, 0.62f, 1f), 0.45f, 0.10f);

            // 36. Colorful Event Flag (Orange/Gold - festive pennant-style decorative flag)
            CreateLitMaterial("M_IUH_Gate_EventFlag", litShader, new Color(0.96f, 0.58f, 0.08f, 1f), 0.25f, 0.0f);

            // 37. Gate Gate asphalt approach road
            CreateLitMaterial("M_IUH_Gate_Road", litShader, new Color(0.22f, 0.24f, 0.26f, 1f), 0.12f, 0.0f);

            // 38. Sidewalk curb along entrance
            CreateLitMaterial("M_IUH_Gate_Curb", litShader, new Color(0.85f, 0.87f, 0.88f, 1f), 0.20f, 0.0f);

            // 39. Gold lettering on stone monument  
            CreateLitMaterial("M_IUH_Gate_GoldText", litShader, new Color(0.92f, 0.75f, 0.18f, 1f), 0.90f, 0.85f);

            // ================= RIGHT CLUSTER (G - I - C - PODIUM) MATERIALS =================
            // Building G (Nha G - KTX Nu) - Weathered warm off-white PBR
            Material gWallMat = CreateLitMaterial("M_IUH_G_Wall", litShader, new Color(0.92f, 0.91f, 0.88f, 1f), 0.25f, 0.02f);
            Texture2D wallTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Wall_Weathered.png");
            if (wallTex != null)
            {
                gWallMat.mainTexture = wallTex;
                gWallMat.mainTextureScale = new Vector2(8, 12);
            }

            CreateLitMaterial("M_IUH_G_WindowGlass", litShader, new Color(0.35f, 0.52f, 0.54f, 1f), 0.94f, 0.20f);
            CreateLitMaterial("M_IUH_G_Railings", litShader, new Color(0.28f, 0.30f, 0.32f, 1f), 0.55f, 0.50f);
            CreateLitMaterial("M_IUH_G_Roof", litShader, new Color(0.85f, 0.83f, 0.80f, 1f), 0.30f, 0.05f);
            CreateLitMaterial("M_IUH_G_RoofEdge", litShader, new Color(0.78f, 0.27f, 0.16f, 1f), 0.35f, 0.10f);
            
            Material gSignMat = CreateLitMaterial("M_IUH_G_Sign", litShader, Color.white, 0.40f, 0.0f);
            Texture2D gSignTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Sign_BuildingG.png");
            if (gSignTex != null)
            {
                gSignMat.mainTexture = gSignTex;
            }

            // Building I (Nha I - KTX Nam)
            Material iWallMat = CreateLitMaterial("M_IUH_I_Wall", litShader, new Color(0.91f, 0.90f, 0.88f, 1f), 0.24f, 0.02f);
            if (wallTex != null)
            {
                iWallMat.mainTexture = wallTex;
                iWallMat.mainTextureScale = new Vector2(8, 14);
            }
            CreateLitMaterial("M_IUH_I_Roof", litShader, new Color(0.76f, 0.24f, 0.16f, 1f), 0.35f, 0.15f);
            CreateLitMaterial("M_IUH_I_Glass", litShader, new Color(0.18f, 0.26f, 0.30f, 1f), 0.92f, 0.25f);
            CreateLitMaterial("M_IUH_I_Railings", litShader, new Color(0.25f, 0.27f, 0.29f, 1f), 0.50f, 0.50f);

            // Building C (Nha C)
            Material cWallMat = CreateLitMaterial("M_IUH_C_Wall", litShader, new Color(0.93f, 0.94f, 0.94f, 1f), 0.25f, 0.02f);
            if (wallTex != null)
            {
                cWallMat.mainTexture = wallTex;
                cWallMat.mainTextureScale = new Vector2(6, 8);
            }
            CreateLitMaterial("M_IUH_C_AccentMint", litShader, new Color(0.35f, 0.68f, 0.54f, 1f), 0.40f, 0.05f);
            CreateLitMaterial("M_IUH_C_GreenGlass", litShader, new Color(0.16f, 0.46f, 0.40f, 1f), 0.94f, 0.25f);
            CreateLitMaterial("M_IUH_C_Roof", litShader, new Color(0.34f, 0.52f, 0.44f, 1f), 0.30f, 0.05f);

            // Low Podium Front of G
            CreateLitMaterial("M_IUH_Podium_Roof", litShader, new Color(0.48f, 0.54f, 0.58f, 1f), 0.35f, 0.20f);
            CreateLitMaterial("M_IUH_Podium_Glazing", litShader, new Color(0.12f, 0.18f, 0.22f, 1f), 0.94f, 0.30f);
            CreateLitMaterial("M_IUH_Podium_Turquoise", litShader, new Color(0.20f, 0.65f, 0.62f, 1f), 0.50f, 0.10f);

            // Ground Surfaces (Asphalt, Concrete Seams, Curbs)
            Material roadMat = CreateLitMaterial("M_IUH_Asphalt_Road", litShader, new Color(0.32f, 0.33f, 0.35f, 1f), 0.16f, 0.0f);
            Texture2D asphaltTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Asphalt_Cracks.png");
            if (asphaltTex != null)
            {
                roadMat.mainTexture = asphaltTex;
                roadMat.mainTextureScale = new Vector2(4, 4);
            }

            Material groundMat = CreateLitMaterial("M_IUH_Concrete_Ground", litShader, new Color(0.82f, 0.83f, 0.85f, 1f), 0.22f, 0.0f);
            Texture2D concreteTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Concrete_Seams.png");
            if (concreteTex != null)
            {
                groundMat.mainTexture = concreteTex;
                groundMat.mainTextureScale = new Vector2(8, 8);
            }

            Material curbMat = CreateLitMaterial("M_IUH_Concrete_Curb", litShader, new Color(0.75f, 0.76f, 0.78f, 1f), 0.25f, 0.02f);
            if (concreteTex != null)
            {
                curbMat.mainTexture = concreteTex;
                curbMat.mainTextureScale = new Vector2(2, 1);
            }

            // Drainage Grates & Manholes
            Material drainMat = CreateLitMaterial("M_IUH_Drain_Grate", litShader, Color.white, 0.60f, 0.85f);
            Texture2D drainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Drain_Grate.png");
            if (drainTex != null) { drainMat.mainTexture = drainTex; }

            Material manholeMat = CreateLitMaterial("M_IUH_Manhole_Cover", litShader, Color.white, 0.55f, 0.80f);
            Texture2D manholeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Manhole_Cover.png");
            if (manholeTex != null) { manholeMat.mainTexture = manholeTex; }

            // Directional & Notice Signage
            Material dirSignMat = CreateLitMaterial("M_IUH_Sign_Directional", litShader, Color.white, 0.35f, 0.05f);
            Texture2D dirSignTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Sign_Directional.png");
            if (dirSignTex != null) { dirSignMat.mainTexture = dirSignTex; }

            Material noticeMat = CreateLitMaterial("M_IUH_Sign_NoticeBoard", litShader, Color.white, 0.25f, 0.0f);
            Texture2D noticeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Sign_NoticeBoard.png");
            if (noticeTex != null) { noticeMat.mainTexture = noticeTex; }

            // MEP & AC Units
            Material acLouversMat = CreateLitMaterial("M_IUH_AC_Louvers", litShader, Color.white, 0.40f, 0.30f);
            Texture2D acTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_AC_Louvers.png");
            if (acTex != null) { acLouversMat.mainTexture = acTex; }
            CreateLitMaterial("M_IUH_AC_Unit", litShader, new Color(0.88f, 0.89f, 0.90f, 1f), 0.35f, 0.15f);
            CreateLitMaterial("M_IUH_PVC_Pipe", litShader, new Color(0.55f, 0.58f, 0.62f, 1f), 0.45f, 0.05f); // Grey PVC drain downspout

            // Facade Window Variations (Blinds & Dark interior)
            Material blindsMat = CreateLitMaterial("M_IUH_Window_Blinds", litShader, Color.white, 0.30f, 0.05f);
            Texture2D blindsTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Window_Blinds.png");
            if (blindsTex != null) { blindsMat.mainTexture = blindsTex; }
            CreateLitMaterial("M_IUH_Window_Dark", litShader, new Color(0.08f, 0.10f, 0.12f, 1f), 0.92f, 0.15f);

            // Tree Bark
            Material barkMat = CreateLitMaterial("M_IUH_Tree_Bark", litShader, Color.white, 0.15f, 0.0f);
            Texture2D barkTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Tree_Bark.png");
            if (barkTex != null) { barkMat.mainTexture = barkTex; }

            // Campus Props (Benches, Soil, Pits)
            Material benchMat = CreateLitMaterial("M_IUH_Bench_Granite", litShader, new Color(0.86f, 0.85f, 0.82f, 1f), 0.50f, 0.05f);
            Texture2D graniteBenchTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Granite_Noise.png");
            if (graniteBenchTex != null)
            {
                benchMat.mainTexture = graniteBenchTex;
                benchMat.mainTextureScale = new Vector2(3, 3);
            }

            Material matureFoliage = CreateLitMaterial("M_IUH_MatureFoliage", litShader, new Color(0.20f, 0.45f, 0.16f, 1f), 0.20f, 0.0f);
            Texture2D foliageGrassTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_Grass_Noise.png");
            if (foliageGrassTex != null)
            {
                matureFoliage.mainTexture = foliageGrassTex;
                matureFoliage.mainTextureScale = new Vector2(4, 4);
            }

            CreateLitMaterial("M_IUH_Bench_Wood", litShader, new Color(0.52f, 0.32f, 0.18f, 1f), 0.35f, 0.0f);
            CreateLitMaterial("M_IUH_Planter_Soil", litShader, new Color(0.24f, 0.20f, 0.16f, 1f), 0.10f, 0.0f);
            CreateLitMaterial("M_IUH_FireCabinet", litShader, new Color(0.82f, 0.12f, 0.10f, 1f), 0.55f, 0.20f);

            // Motorbikes (8 Colors)
            CreateLitMaterial("M_IUH_Motorbike_Red", litShader, new Color(0.85f, 0.15f, 0.12f, 1f), 0.80f, 0.25f);
            CreateLitMaterial("M_IUH_Motorbike_Dark", litShader, new Color(0.14f, 0.15f, 0.18f, 1f), 0.70f, 0.60f);
            CreateLitMaterial("M_IUH_Motorbike_Blue", litShader, new Color(0.12f, 0.38f, 0.78f, 1f), 0.75f, 0.30f);
            CreateLitMaterial("M_IUH_Motorbike_White", litShader, new Color(0.92f, 0.92f, 0.94f, 1f), 0.80f, 0.20f);
            CreateLitMaterial("M_IUH_Motorbike_Silver", litShader, new Color(0.72f, 0.74f, 0.77f, 1f), 0.85f, 0.75f);
            CreateLitMaterial("M_IUH_Motorbike_Teal", litShader, new Color(0.15f, 0.62f, 0.58f, 1f), 0.75f, 0.25f);
            CreateLitMaterial("M_IUH_Motorbike_Yellow", litShader, new Color(0.92f, 0.75f, 0.15f, 1f), 0.75f, 0.25f);
            CreateLitMaterial("M_IUH_Motorbike_Orange", litShader, new Color(0.90f, 0.42f, 0.12f, 1f), 0.75f, 0.25f);
            CreateLitMaterial("M_IUH_Helmet_Red", litShader, new Color(0.82f, 0.12f, 0.12f, 1f), 0.85f, 0.10f);
            CreateLitMaterial("M_IUH_Helmet_Blue", litShader, new Color(0.15f, 0.35f, 0.80f, 1f), 0.85f, 0.10f);
            CreateLitMaterial("M_IUH_Helmet_White", litShader, new Color(0.92f, 0.92f, 0.94f, 1f), 0.85f, 0.10f);

            // Student NPCs
            CreateLitMaterial("M_IUH_Student_WhiteShirt", litShader, new Color(0.94f, 0.95f, 0.96f, 1f), 0.15f, 0.0f);
            CreateLitMaterial("M_IUH_Student_BluePolo", litShader, new Color(0.15f, 0.38f, 0.72f, 1f), 0.15f, 0.0f);
            CreateLitMaterial("M_IUH_Student_Jeans", litShader, new Color(0.20f, 0.32f, 0.52f, 1f), 0.18f, 0.0f);
            CreateLitMaterial("M_IUH_Student_DarkPants", litShader, new Color(0.15f, 0.16f, 0.18f, 1f), 0.18f, 0.0f);
            CreateLitMaterial("M_IUH_Student_Skin", litShader, new Color(0.88f, 0.72f, 0.60f, 1f), 0.20f, 0.0f);
            CreateLitMaterial("M_IUH_Student_Hair", litShader, new Color(0.12f, 0.10f, 0.08f, 1f), 0.15f, 0.0f);

            // Clothes / Laundry props on balconies
            CreateLitMaterial("M_IUH_Cloth_Red", litShader, new Color(0.82f, 0.22f, 0.22f, 1f), 0.10f, 0.0f);
            CreateLitMaterial("M_IUH_Cloth_Blue", litShader, new Color(0.20f, 0.45f, 0.85f, 1f), 0.10f, 0.0f);
            CreateLitMaterial("M_IUH_Cloth_Yellow", litShader, new Color(0.92f, 0.82f, 0.25f, 1f), 0.10f, 0.0f);
            CreateLitMaterial("M_IUH_Cloth_White", litShader, new Color(0.95f, 0.95f, 0.95f, 1f), 0.10f, 0.0f);

            // Rooftop props & AC units
            CreateLitMaterial("M_IUH_WaterTank_Black", litShader, new Color(0.13f, 0.13f, 0.14f, 1f), 0.60f, 0.70f);
            CreateLitMaterial("M_IUH_WaterTank_Stainless", litShader, new Color(0.83f, 0.85f, 0.87f, 1f), 0.80f, 0.85f);
            CreateLitMaterial("M_IUH_Parking_Line", litShader, new Color(0.96f, 0.82f, 0.25f, 1f), 0.20f, 0.0f);

            // Wayfinding, Fire Extinguisher, Access Control, Urban Background
            Material wayfindingMat = CreateLitMaterial("M_IUH_Sign_Wayfinding", litShader, Color.white, 0.35f, 0.05f);
            Texture2D wayfindingTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Sign_Wayfinding.png");
            if (wayfindingTex != null) { wayfindingMat.mainTexture = wayfindingTex; }

            Material fireExtMat = CreateLitMaterial("M_IUH_Fire_Extinguisher", litShader, Color.white, 0.45f, 0.10f);
            Texture2D fireExtTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Fire_Extinguisher.png");
            if (fireExtTex != null) { fireExtMat.mainTexture = fireExtTex; }

            Material accessControlMat = CreateLitMaterial("M_IUH_Access_Control", litShader, Color.white, 0.40f, 0.20f);
            Texture2D accessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Access_Control.png");
            if (accessTex != null) { accessControlMat.mainTexture = accessTex; }

            Material urbanHouseMat = CreateLitMaterial("M_IUH_Urban_House", litShader, Color.white, 0.25f, 0.02f);
            Texture2D urbanTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath + "T_IUH_Urban_House.png");
            if (urbanTex != null) { urbanHouseMat.mainTexture = urbanTex; urbanHouseMat.mainTextureScale = new Vector2(2, 2); }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[IUH] All materials generated successfully in " + MatPath);
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

        private static Material CreateTransparentCutout(string matName, Shader shader)
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
