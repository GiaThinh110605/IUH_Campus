#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;
using IUHCampus.Guardian.Match3;
using IUHCampus.Minigames.GreenCampus;

namespace IUHCampus.Editor
{
    public static class IUHMatch3CombatBuilder
    {
        private const string SCENE_PATH = "Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity";
        private const string TEXTURE_DIR = "Assets/IUH_Campus/Textures/Guardian/";
        private const string MONSTER_MODEL_PATH = "Assets/IUH_Campus/Models/Monsters/Demon.obj";
        private const string MONSTER_MAT_PATH = "Assets/IUH_Campus/Models/Monsters/M_Monster_Atlas.mat";

        [MenuItem("IUH Campus/Guardian/Build Match-3 Combat Arena")]
        public static void BuildCombatArena()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != SCENE_PATH)
            {
                if (System.IO.File.Exists(SCENE_PATH))
                {
                    activeScene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
                }
            }

            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO == null)
            {
                canvasGO = new GameObject("IUH_Minigame_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvasGO.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var cs = canvasGO.GetComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                cs.matchWidthOrHeight = 0.5f;
            }

            var existingArena = canvasGO.transform.Find("CombatArena_Match3");
            if (existingArena != null)
            {
                Undo.DestroyObjectImmediate(existingArena.gameObject);
            }

            GameObject arenaRoot = new GameObject("CombatArena_Match3", typeof(RectTransform));
            arenaRoot.transform.SetParent(canvasGO.transform, false);
            var arenaRect = arenaRoot.GetComponent<RectTransform>();
            arenaRect.anchorMin = Vector2.zero;
            arenaRect.anchorMax = Vector2.one;
            arenaRect.sizeDelta = Vector2.zero;

            Sprite sprSword = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "gem_sword.png");
            Sprite sprShield = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "gem_shield.png");
            Sprite sprEnergy = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "gem_energy.png");
            Sprite sprHeal = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "gem_heal.png");
            Sprite sprTileBg = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "tile_bg.png");
            Sprite sprTileSel = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "tile_selected.png");
            Sprite sprBarBg = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "bar_bg.png");
            Sprite sprHpPlayer = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "bar_hp_player.png");
            Sprite sprHpBoss = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "bar_hp_boss.png");
            Sprite sprBarShield = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "bar_shield.png");
            Sprite sprBarEnergy = AssetDatabase.LoadAssetAtPath<Sprite>(TEXTURE_DIR + "bar_energy.png");

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject boardObj = new GameObject("Match3_Board", typeof(RectTransform), typeof(Image), typeof(Match3BoardView));
            boardObj.transform.SetParent(arenaRoot.transform, false);
            var boardRect = boardObj.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.anchoredPosition = new Vector2(0f, -10f);
            boardRect.sizeDelta = new Vector2(510f, 510f);

            var boardBgImg = boardObj.GetComponent<Image>();
            boardBgImg.color = new Color(0.06f, 0.09f, 0.15f, 0.85f);
            boardBgImg.raycastTarget = false;

            GameObject gridRootObj = new GameObject("GridRoot", typeof(RectTransform));
            gridRootObj.transform.SetParent(boardObj.transform, false);
            var gridRect = gridRootObj.GetComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.sizeDelta = Vector2.zero;

            var boardView = boardObj.GetComponent<Match3BoardView>();
            SerializedObject soBoard = new SerializedObject(boardView);
            SetRef(soBoard, "m_GridRoot", gridRect);
            SetRef(soBoard, "m_SpriteTileBg", sprTileBg);
            SetRef(soBoard, "m_SpriteTileSelected", sprTileSel);
            SetRef(soBoard, "m_SpriteSword", sprSword);
            SetRef(soBoard, "m_SpriteShield", sprShield);
            SetRef(soBoard, "m_SpriteEnergy", sprEnergy);
            SetRef(soBoard, "m_SpriteHeal", sprHeal);
            soBoard.ApplyModifiedProperties();

            var (skillBtn, skillTxt) = CreateButton(boardObj.transform, "BtnSkill", "Nộ: 0/100", new Vector2(0.5f, -0.09f), new Vector2(280, 44), defaultFont);
            var (timerFill, timerTxt) = CreateTimerBar(boardObj.transform, sprBarBg, sprBarEnergy, defaultFont);

            GameObject vicModal = CreateModal(arenaRoot.transform, "VictoryModal", "🏆 CHIẾN THẮNG!", "Bạn đã thanh tẩy hoàn toàn quái vật dữ liệu!\nBảo vệ an toàn cho khuôn viên IUH.", new Color(0.2f, 0.95f, 0.45f), defaultFont, out Button btnRetryVic, out Button btnCloseVic, out Text vicTitleTxt, out Text vicDescTxt, out Text vicRewardsTxt, "TIẾP TỤC KHÁM PHÁ [ĐÓNG]", true);
            GameObject defModal = CreateModal(arenaRoot.transform, "DefeatModal", "💀 THẤT BẠI", "Dữ liệu bị nguyền đã áp đảo lá chắn của bạn!\nHãy thử lại để thanh tẩy khu vực.", new Color(0.95f, 0.2f, 0.3f), defaultFont, out Button btnRetryDef, out Button btnCloseDef, out Text _, out Text defDescTxt, out Text _, "RÚT LUI KHỎI CHIẾN ĐẤU", false);

            var combatMgr = arenaRoot.AddComponent<Match3CombatManager>();

            var playerOverhead = SpawnPlayerOverhead(sprBarBg, sprHpPlayer, sprBarShield, sprBarEnergy, defaultFont);

            var allEncounters = SpawnAllMonsters(combatMgr, sprBarBg, sprHpBoss, defaultFont);
            var firstEncounter = allEncounters.Count > 0 ? allEncounters[0] : null;

            SerializedObject soMgr = new SerializedObject(combatMgr);
            SetRef(soMgr, "m_BoardView", boardView);

            var pDuration = soMgr.FindProperty("m_CombatDuration");
            if (pDuration != null) pDuration.floatValue = 60.0f;
            SetRef(soMgr, "m_TimerText", timerTxt);
            SetRef(soMgr, "m_TimerFill", timerFill);

            SetRef(soMgr, "m_VictoryModal", vicModal);
            SetRef(soMgr, "m_VictoryTitleText", vicTitleTxt);
            SetRef(soMgr, "m_VictoryDescText", vicDescTxt);
            SetRef(soMgr, "m_VictoryRewardsText", vicRewardsTxt);
            SetRef(soMgr, "m_DefeatModal", defModal);
            SetRef(soMgr, "m_DefeatDescText", defDescTxt);

            if (playerOverhead != null)
            {
                SetRef(soMgr, "m_PlayerHpFill", playerOverhead.HpFill);
                SetRef(soMgr, "m_PlayerHpText", playerOverhead.HpText);
                SetRef(soMgr, "m_PlayerShieldFill", playerOverhead.ShieldFill);
                SetRef(soMgr, "m_PlayerShieldText", playerOverhead.ShieldText);
                SetRef(soMgr, "m_PlayerEnergyFill", playerOverhead.EnergyFill);
                SetRef(soMgr, "m_PlayerEnergyText", playerOverhead.EnergyText);
            }

            if (firstEncounter != null)
            {
                SetRef(soMgr, "m_BossHpFill", firstEncounter.BossHpFill);
                SetRef(soMgr, "m_BossHpText", firstEncounter.BossHpText);
                SetRef(soMgr, "m_BossIntentText", firstEncounter.BossIntentText);
            }

            SetRef(soMgr, "m_SkillButton", skillBtn);
            SetRef(soMgr, "m_SkillButtonText", skillTxt);

            SetRef(soMgr, "m_BtnRetryVictory", btnRetryVic);
            SetRef(soMgr, "m_BtnRetryDefeat", btnRetryDef);
            SetRef(soMgr, "m_BtnCloseVictory", btnCloseVic);
            SetRef(soMgr, "m_BtnFleeDefeat", btnCloseDef);

            soMgr.ApplyModifiedProperties();

            boardObj.SetActive(false);
            arenaRoot.SetActive(false);

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();

            Debug.Log("[IUH Guardian] Build complete: Clean overhead UI configured on player and monster!");
        }

        private static void SetRef(SerializedObject so, string propName, UnityEngine.Object obj)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.objectReferenceValue = obj;
            else Debug.LogWarning($"[IUH Guardian] Property {propName} not found on {so.targetObject.GetType()}");
        }

        private static IUHPlayerOverheadHUD SpawnPlayerOverhead(Sprite sprBarBg, Sprite sprHp, Sprite sprShield, Sprite sprEnergy, Font font)
        {
            var playerGO = GameObject.Find("Campus Student") ?? GameObject.Find("IUH_Grab_Motorbike");
            if (playerGO == null) return null;

            var oldHUD = playerGO.transform.Find("PLAYER_OVERHEAD_HUD");
            if (oldHUD != null) Undo.DestroyObjectImmediate(oldHUD.gameObject);

            GameObject hudGO = new GameObject("PLAYER_OVERHEAD_HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(IUHGreenBillboard));
            hudGO.transform.SetParent(playerGO.transform, false);
            hudGO.transform.localPosition = new Vector3(0f, 2.05f, 0f);

            var canvas = hudGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = hudGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120f, 30f);
            rt.localScale = new Vector3(0.004f, 0.004f, 0.004f);

            GameObject cardBg = new GameObject("CardBg", typeof(RectTransform), typeof(Image));
            cardBg.transform.SetParent(hudGO.transform, false);
            var bgRt = cardBg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = cardBg.GetComponent<Image>();
            bgImg.color = new Color(0.04f, 0.06f, 0.10f, 0.70f);
            bgImg.raycastTarget = false;

            CreateText(cardBg.transform, "PlayerTitle", "BẢO VỆ IUH", 9, new Vector2(0.5f, 0.82f), new Vector2(115f, 12f), font, new Color(0.4f, 0.88f, 1f));

            var (hpFill, hpTxt) = CreateMiniBar(cardBg.transform, "HpBar", sprBarBg, sprHp, new Vector2(0.5f, 0.50f), new Vector2(112f, 7f), font, "100/100");
            if (hpTxt != null) hpTxt.fontSize = 7;

            var (shieldFill, shieldTxt) = CreateMiniBar(cardBg.transform, "ShieldBar", sprBarBg, sprShield, new Vector2(0.27f, 0.20f), new Vector2(53f, 5f), font, "");
            if (shieldTxt != null) shieldTxt.fontSize = 6;

            var (energyFill, energyTxt) = CreateMiniBar(cardBg.transform, "EnergyBar", sprBarBg, sprEnergy, new Vector2(0.73f, 0.20f), new Vector2(53f, 5f), font, "0/100");
            if (energyTxt != null) energyTxt.fontSize = 6;

            var overheadComp = hudGO.AddComponent<IUHPlayerOverheadHUD>();
            SerializedObject so = new SerializedObject(overheadComp);
            SetRef(so, "m_HpFill", hpFill);
            SetRef(so, "m_HpText", hpTxt);
            SetRef(so, "m_ShieldFill", shieldFill);
            SetRef(so, "m_ShieldText", shieldTxt);
            SetRef(so, "m_EnergyFill", energyFill);
            SetRef(so, "m_EnergyText", energyTxt);
            so.ApplyModifiedProperties();

            return overheadComp;
        }

        private class MonsterData
        {
            public string id;
            public string name;
            public string modelPath;
            public Vector3 position;
            public float rotationY;
            public Vector3 scale;
            public int hp;
            public int minDmg;
            public int maxDmg;
            public float duration;
            public Color lightColor;
            public float lightIntensity;
            public float lightRange;
            public int rewardGold;
            public string[] rewardItems;
        }

        private static List<IUHMonsterEncounter> SpawnAllMonsters(Match3CombatManager combatMgr, Sprite sprBarBg, Sprite sprHpBoss, Font font)
        {
            var oldSingle = GameObject.Find("DATA_MONSTER_ENCOUNTER");
            if (oldSingle != null) Undo.DestroyObjectImmediate(oldSingle);

            var existingMonsters = Object.FindObjectsByType<IUHMonsterEncounter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var m in existingMonsters)
            {
                Undo.DestroyObjectImmediate(m.gameObject);
            }

            var configs = new MonsterData[]
            {
                new MonsterData
                {
                    id = "MONSTER_01_BLUEDEMON",
                    name = "TIỂU QUỶ BĂNG LAM (DỄ)",
                    modelPath = "Assets/IUH_Campus/Models/Monsters/BlueDemon.obj",
                    position = new Vector3(0.0f, 0.0f, -24.0f),
                    rotationY = 180f,
                    scale = new Vector3(1.35f, 1.35f, 1.35f),
                    hp = 60,
                    minDmg = 8,
                    maxDmg = 12,
                    duration = 45f,
                    lightColor = new Color(0.1f, 0.85f, 1.0f),
                    lightIntensity = 3.5f,
                    lightRange = 6f,
                    rewardGold = 60,
                    rewardItems = new string[] { "Bình Dược Liệu Nano (+50 HP)", "1x Mảnh Tinh Thể Dữ Liệu Lam" }
                },
                new MonsterData
                {
                    id = "MONSTER_02_ORC",
                    name = "CHIẾN BINH QUỶ DỮ LIỆU (VỪA)",
                    modelPath = "Assets/IUH_Campus/Models/Monsters/Orc.obj",
                    position = new Vector3(-18.0f, 0.0f, -8.0f),
                    rotationY = 135f,
                    scale = new Vector3(1.6f, 1.6f, 1.6f),
                    hp = 100,
                    minDmg = 12,
                    maxDmg = 16,
                    duration = 55f,
                    lightColor = new Color(0.2f, 1.0f, 0.4f),
                    lightIntensity = 3.8f,
                    lightRange = 6.5f,
                    rewardGold = 150,
                    rewardItems = new string[] { "Khiên Bảo Hộ Sinh Viên (+30 Giáp)", "Pin Lõi Năng Lượng (+40 Năng Lượng Nộ)", "2x Tinh Thể Lõi Lục" }
                },
                new MonsterData
                {
                    id = "MONSTER_03_PHANTOM",
                    name = "BÓNG MA GLITCH FESTIVAL (KHÁ)",
                    modelPath = "Assets/IUH_Campus/Models/Monsters/BlueDemon.obj",
                    position = new Vector3(18.0f, 0.0f, -6.0f),
                    rotationY = 225f,
                    scale = new Vector3(1.45f, 1.45f, 1.45f),
                    hp = 85,
                    minDmg = 10,
                    maxDmg = 15,
                    duration = 50f,
                    lightColor = new Color(0.85f, 0.25f, 1.0f),
                    lightIntensity = 3.8f,
                    lightRange = 6.5f,
                    rewardGold = 120,
                    rewardItems = new string[] { "Đĩa Dữ Liệu Lễ Hội Festival (Hiếm)", "2x Bình Phục Hồi Nano (+50 HP)", "1x Tinh Thể Tím Huyễn Ảo" }
                },
                new MonsterData
                {
                    id = "MONSTER_04_DEMON_BOSS",
                    name = "ĐẠI MA VƯƠNG HẮC ÁM (ĐẠI BOSS)",
                    modelPath = "Assets/IUH_Campus/Models/Monsters/Demon.obj",
                    position = new Vector3(0.0f, 0.0f, 22.0f),
                    rotationY = 180f,
                    scale = new Vector3(1.85f, 1.85f, 1.85f),
                    hp = 150,
                    minDmg = 16,
                    maxDmg = 22,
                    duration = 70f,
                    lightColor = new Color(1.0f, 0.15f, 0.2f),
                    lightIntensity = 4.5f,
                    lightRange = 8f,
                    rewardGold = 500,
                    rewardItems = new string[] { "⚔️ Kiếm Ánh Sáng Cyber IUH (+15 Công Gạch Kiếm)", "🏆 Huy Hiệu Vàng Người Bảo Vệ IUH (Huyền Thoại)", "💎 3x Lõi Dữ Liệu Tối Thượng" }
                }
            };

            Material monsterMat = AssetDatabase.LoadAssetAtPath<Material>(MONSTER_MAT_PATH);
            var results = new List<IUHMonsterEncounter>();

            foreach (var cfg in configs)
            {
                GameObject monsterRoot = new GameObject(cfg.id);
                monsterRoot.transform.position = cfg.position;
                monsterRoot.transform.rotation = Quaternion.Euler(0f, cfg.rotationY, 0f);

                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cfg.modelPath);
                if (modelPrefab != null)
                {
                    GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, monsterRoot.transform);
                    modelInstance.transform.localPosition = Vector3.zero;
                    modelInstance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    modelInstance.transform.localScale = cfg.scale;

                    if (monsterMat != null)
                    {
                        var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
                        foreach (var rend in renderers) rend.sharedMaterial = monsterMat;
                    }
                }

                var col = monsterRoot.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, 1.2f, 0f);
                col.radius = 1.0f;
                col.height = 2.5f;

                GameObject auraLight = new GameObject("AuraLight");
                auraLight.transform.SetParent(monsterRoot.transform, false);
                auraLight.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                var light = auraLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = cfg.lightColor;
                light.intensity = cfg.lightIntensity;
                light.range = cfg.lightRange;

                GameObject overheadCanvasGO = new GameObject("DATA_CORRUPTION_OVERHEAD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(IUHGreenBillboard));
                overheadCanvasGO.transform.SetParent(monsterRoot.transform, false);
                overheadCanvasGO.transform.localPosition = new Vector3(0f, 5.3f, 0f);

                var canvas = overheadCanvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                var promptRect = overheadCanvasGO.GetComponent<RectTransform>();
                promptRect.sizeDelta = new Vector2(230f, 85f);
                promptRect.localScale = new Vector3(0.0085f, 0.0085f, 0.0085f);

                GameObject cardBg = new GameObject("CardBg", typeof(RectTransform), typeof(Image));
                cardBg.transform.SetParent(overheadCanvasGO.transform, false);
                var cardRt = cardBg.GetComponent<RectTransform>();
                cardRt.anchorMin = Vector2.zero;
                cardRt.anchorMax = Vector2.one;
                cardRt.sizeDelta = Vector2.zero;
                var cardImg = cardBg.GetComponent<Image>();
                cardImg.color = new Color(0.12f, 0.06f, 0.10f, 0.88f);
                cardImg.raycastTarget = false;

                CreateText(cardBg.transform, "BossTitle", $"😈 {cfg.name}", 13, new Vector2(0.5f, 0.82f), new Vector2(220f, 20f), font, cfg.lightColor);

                var (bossHpFill, bossHpTxt) = CreateMiniBar(cardBg.transform, "BossHpBar", sprBarBg, sprHpBoss, new Vector2(0.5f, 0.52f), new Vector2(205f, 13f), font, $"{cfg.hp}/{cfg.hp}");

                Text bossIntentTxt = CreateText(cardBg.transform, "TxtIntent", $"⚔️ Ý định: Tấn công {cfg.minDmg}-{cfg.maxDmg} DMG", 11, new Vector2(0.5f, 0.26f), new Vector2(210f, 18f), font, new Color(1f, 0.85f, 0.3f));

                overheadCanvasGO.SetActive(false);

                var encounter = monsterRoot.AddComponent<IUHMonsterEncounter>();
                SerializedObject soEnc = new SerializedObject(encounter);
                var pName = soEnc.FindProperty("m_MonsterName"); if (pName != null) pName.stringValue = cfg.name;
                var pHp = soEnc.FindProperty("m_MonsterHp"); if (pHp != null) pHp.intValue = cfg.hp;
                var pMinD = soEnc.FindProperty("m_BossMinDmg"); if (pMinD != null) pMinD.intValue = cfg.minDmg;
                var pMaxD = soEnc.FindProperty("m_BossMaxDmg"); if (pMaxD != null) pMaxD.intValue = cfg.maxDmg;
                var pDur = soEnc.FindProperty("m_CombatDuration"); if (pDur != null) pDur.floatValue = cfg.duration;
                var pGold = soEnc.FindProperty("m_RewardGold"); if (pGold != null) pGold.intValue = cfg.rewardGold;
                var pItems = soEnc.FindProperty("m_RewardItems");
                if (pItems != null)
                {
                    pItems.arraySize = cfg.rewardItems.Length;
                    for (int i = 0; i < cfg.rewardItems.Length; i++)
                    {
                        pItems.GetArrayElementAtIndex(i).stringValue = cfg.rewardItems[i];
                    }
                }
                var pRev = soEnc.FindProperty("m_RevealRadius"); if (pRev != null) pRev.floatValue = 16.0f;
                var pTrig = soEnc.FindProperty("m_TriggerRadius"); if (pTrig != null) pTrig.floatValue = 10.0f;
                var pKey = soEnc.FindProperty("m_InteractKey"); if (pKey != null) pKey.enumValueIndex = (int)KeyCode.E;

                SetRef(soEnc, "m_MonsterOverheadCanvas", overheadCanvasGO);
                SetRef(soEnc, "m_BossHpFill", bossHpFill);
                SetRef(soEnc, "m_BossHpText", bossHpTxt);
                SetRef(soEnc, "m_BossIntentText", bossIntentTxt);
                SetRef(soEnc, "m_AuraEffect", auraLight);
                SetRef(soEnc, "m_CombatManager", combatMgr);
                soEnc.ApplyModifiedProperties();

                results.Add(encounter);
            }

            return results;
        }

        private static (Image fill, Text label) CreateMiniBar(Transform parent, string name, Sprite bgSpr, Sprite fillSpr, Vector2 anchor, Vector2 size, Font font, string initText)
        {
            GameObject container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;

            GameObject bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(container.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = bgSpr;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bg.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = fillSpr;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 1f;
            fillImg.raycastTarget = false;

            Text valTxt = CreateText(bg.transform, "Val", initText, 10, new Vector2(0.5f, 0.5f), size, font, Color.white);
            return (fillImg, valTxt);
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchor, Vector2 size, Font font, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;

            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.font = font;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        private static (Button btn, Text txt) CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Font font)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.9f);

            var btn = go.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.3f, 0.7f, 1f);
            cb.pressedColor = new Color(0.15f, 0.45f, 0.75f);
            btn.colors = cb;

            Text txt = CreateText(go.transform, "Text", label, 16, new Vector2(0.5f, 0.5f), size, font, Color.white);
            return (btn, txt);
        }

        private static (Image fill, Text txt) CreateTimerBar(Transform parent, Sprite bgSpr, Sprite fillSpr, Font font)
        {
            GameObject container = new GameObject("TimerContainer", typeof(RectTransform));
            container.transform.SetParent(parent, false);
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1.07f);
            rect.anchorMax = new Vector2(0.5f, 1.07f);
            rect.sizeDelta = new Vector2(340, 36);

            GameObject bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(container.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = bgSpr;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.06f, 0.08f, 0.14f, 0.90f);
            bgImg.raycastTarget = false;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bg.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = fillSpr;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 1f;
            fillImg.color = new Color(0.2f, 0.75f, 1f);
            fillImg.raycastTarget = false;

            Text valTxt = CreateText(bg.transform, "TimerVal", "⏱️ THỜI GIAN: 01:00", 15, new Vector2(0.5f, 0.5f), new Vector2(320, 30), font, Color.white);
            valTxt.fontStyle = FontStyle.Bold;
            return (fillImg, valTxt);
        }

        private static GameObject CreateModal(Transform parent, string name, string title, string desc, Color headerColor, Font font, out Button retryBtn, out Button closeBtn, out Text titleTxt, out Text descTxt, out Text rewardsTxt, string closeText, bool isVictory)
        {
            GameObject modal = new GameObject(name, typeof(RectTransform), typeof(Image));
            modal.transform.SetParent(parent, false);
            var rect = modal.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            var img = modal.GetComponent<Image>();
            img.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);

            GameObject box = new GameObject("Card", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(modal.transform, false);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = isVictory ? new Vector2(540, 430) : new Vector2(500, 340);
            var boxImg = box.GetComponent<Image>();
            boxImg.color = new Color(0.10f, 0.14f, 0.22f);

            if (isVictory)
            {
                titleTxt = CreateText(box.transform, "Title", title, 25, new Vector2(0.5f, 0.88f), new Vector2(500, 40), font, headerColor);
                titleTxt.fontStyle = FontStyle.Bold;

                descTxt = CreateText(box.transform, "Desc", desc, 14, new Vector2(0.5f, 0.77f), new Vector2(480, 36), font, new Color(0.85f, 0.95f, 1f));

                GameObject rewardPanel = new GameObject("RewardPanel", typeof(RectTransform), typeof(Image));
                rewardPanel.transform.SetParent(box.transform, false);
                var rpRect = rewardPanel.GetComponent<RectTransform>();
                rpRect.anchorMin = new Vector2(0.5f, 0.51f);
                rpRect.anchorMax = new Vector2(0.5f, 0.51f);
                rpRect.sizeDelta = new Vector2(480, 125);
                var rpImg = rewardPanel.GetComponent<Image>();
                rpImg.color = new Color(0.05f, 0.08f, 0.14f, 0.95f);
                rpImg.raycastTarget = false;

                CreateText(rewardPanel.transform, "RewardHeader", "🎁 <b>PHẦN THƯỞNG CHIẾN LỢI PHẨM:</b>", 13, new Vector2(0.5f, 0.82f), new Vector2(460, 24), font, new Color(0.98f, 0.92f, 0.55f));

                rewardsTxt = CreateText(rewardPanel.transform, "RewardsContent", "💰 +100 Vàng & Bạc IUH\n✨ Vật phẩm dữ liệu", 13, new Vector2(0.5f, 0.38f), new Vector2(450, 68), font, Color.white);
                rewardsTxt.alignment = TextAnchor.MiddleLeft;

                var (btn1, _) = CreateButton(box.transform, "BtnActionClose", closeText, new Vector2(0.5f, 0.22f), new Vector2(320, 44), font);
                closeBtn = btn1;

                var (btn2, _) = CreateButton(box.transform, "BtnRetry", "🔄 ĐẤU LẠI MÀN NÀY", new Vector2(0.5f, 0.09f), new Vector2(320, 36), font);
                retryBtn = btn2;
            }
            else
            {
                titleTxt = CreateText(box.transform, "Title", title, 32, new Vector2(0.5f, 0.82f), new Vector2(460, 48), font, headerColor);
                descTxt = CreateText(box.transform, "Desc", desc, 17, new Vector2(0.5f, 0.56f), new Vector2(440, 70), font, new Color(0.9f, 0.95f, 1f));
                rewardsTxt = null;

                var (btn1, _) = CreateButton(box.transform, "BtnActionClose", closeText, new Vector2(0.5f, 0.32f), new Vector2(300, 48), font);
                closeBtn = btn1;

                var (btn2, _) = CreateButton(box.transform, "BtnRetry", "🔄 THỬ LẠI TRẬN ĐẤU", new Vector2(0.5f, 0.14f), new Vector2(300, 44), font);
                retryBtn = btn2;
            }

            modal.SetActive(false);
            return modal;
        }
    }
}
#endif
