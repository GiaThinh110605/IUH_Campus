#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Editor
{
    public static partial class IUHSharedSceneMenu
    {
        static Sprite rounded, circle;
        static readonly Color Navy = new Color(.055f, .11f, .18f), Paper = new Color(.94f, .96f, .97f), Slate = new Color(.34f, .41f, .47f);

        static RectTransform BuildSixGameMenu(Transform parent)
        {
            rounded = MaskSprite("Rounded", false);
            circle = MaskSprite("Circle", true);

            var panel = Box("PortalMenuPanel", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Paper);
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "Design four game menu");
            var canvas = panel.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;
            panel.gameObject.AddComponent<GraphicRaycaster>();

            var side = Box("Sidebar", panel, Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(276, 0), Navy);
            Label("University", side, "IUH", 44, Color.white, new Vector2(28, -36), new Vector2(220, 58)).fontStyle = FontStyle.Bold;
            Label("Brand", side, "CAMPUS\nADVENTURE", 23, new Color(.46f, .85f, .78f), new Vector2(28, -106), new Vector2(220, 64)).fontStyle = FontStyle.Bold;
            Label("Tagline", side, "Một khuôn viên.\nBốn hành trình để khám phá.", 17, new Color(.68f, .77f, .83f), new Vector2(28, -193), new Vector2(220, 65));

            var selected = Box("Selected", side, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -337), new Vector2(252, -281), new Color(.12f, .28f, .33f));
            Round(selected);
            Label("SelectedText", selected, "TẤT CẢ TRÒ CHƠI    04", 17, Color.white, new Vector2(16, -17), new Vector2(210, 28)).fontStyle = FontStyle.Bold;
            Label("Progress", side, "04 CÓ THỂ CHƠI\nĐẦY ĐỦ 100/100 ĐIỂM", 16, new Color(.62f, .73f, .80f), new Vector2(28, -362), new Vector2(220, 65));

            Label("Heading", panel, "Khám phá theo cách của bạn", 34, Navy, new Vector2(320, -38), new Vector2(980, 52)).fontStyle = FontStyle.Bold;
            Label("Subtitle", panel, "4 mini-game trọng tâm trong cùng khuôn viên IUH. Chọn một hành trình để bắt đầu.", 19, Slate, new Vector2(322, -100), new Vector2(1000, 35));

            var close = MenuButton("CloseBtn", panel, "ĐÓNG  [M]", new Color(.86f, .9f, .92f), Navy);
            TopRight(close, new Vector2(-40, -42), new Vector2(155, 45));

            // Cards Area - 2x2 Grid for 4 games
            var cards = Box("CardsArea", panel, Vector2.zero, Vector2.one, new Vector2(320, 105), new Vector2(-40, -158), Color.clear);
            cards.GetComponent<Image>().raycastTarget = false;

            // 4 Core Minigames
            GameCard(cards, 0, "Grab Campus", "Lấy hàng trà sữa, né & đánh đuổi ma quỷ, giao hàng đến KTX.", "Chuyến xe tâm linh", new Color(.07f, .55f, .38f), true);
            GameCard(cards, 1, "AI Object Hunt", "Quét camera nhận diện đồ vật bằng AI YOLO và chiếu mô hình 3D AR.", "Deep Learning & AR", new Color(.85f, .45f, .12f), true);
            GameCard(cards, 2, "Thợ săn khoảnh khắc", "Tìm góc đẹp, Photo Mode check-in và lưu trữ album sinh viên.", "Photo Hunter", new Color(.08f, .48f, .72f), true);
            GameCard(cards, 3, "Green Campus", "Thu gom rác 3D, trạm phân loại rác AI và đồng bộ điểm lên Adafruit IO.", "Vì một khuôn viên xanh", new Color(.36f, .58f, .12f), true);

            // Direct child names preserve the portal's existing button bindings.
            var footer = Box("Footer", panel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(24, 186), new Vector2(252, 434), Color.clear);
            footer.GetComponent<Image>().raycastTarget = false;
            string[] names = { "BtnFreeRoam", "BtnDorm", "BtnLibrary", "BtnResume" };
            string[] titles = { "Khám phá tự do", "Đến ký túc xá", "Đến thư viện", "Tiếp tục hành trình" };
            for (int i = 0; i < 4; i++)
            {
                var b = MenuButton(names[i], footer, titles[i], new Color(.10f, .18f, .25f), new Color(.85f, .9f, .94f));
                b.anchorMin = b.anchorMax = new Vector2(0, 1);
                b.pivot = new Vector2(0, 1);
                b.anchoredPosition = new Vector2(0, -i * 59);
                b.sizeDelta = new Vector2(228, 47);
            }

            var wardrobe = MenuButton("BtnCustomizationPortal", panel, "Trang phục  [U]", new Color(.13f, .38f, .39f), Color.white);
            wardrobe.anchorMin = wardrobe.anchorMax = Vector2.zero;
            wardrobe.pivot = Vector2.zero;
            wardrobe.anchoredPosition = new Vector2(24, 112);
            wardrobe.sizeDelta = new Vector2(228, 50);

            var note = Label("SidebarNote", panel, "Cùng khám phá IUH", 16, new Color(.58f, .69f, .76f), Vector2.zero, new Vector2(224, 28));
            note.rectTransform.anchorMin = note.rectTransform.anchorMax = Vector2.zero;
            note.rectTransform.pivot = Vector2.zero;
            note.rectTransform.anchoredPosition = new Vector2(28, 55);

            var hint = Label("Controls", panel, "M / Esc: đóng menu     •     1 / 2 / 3 / 4: chọn game     •     U: trang phục", 17, Slate, Vector2.zero, new Vector2(1150, 30));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = Vector2.zero;
            hint.rectTransform.pivot = Vector2.zero;
            hint.rectTransform.anchoredPosition = new Vector2(322, 48);

            return panel;
        }

        static void GameCard(Transform parent, int index, string title, string description, string subtitle, Color accent, bool ready)
        {
            // 2 Columns x 2 Rows Layout
            int column = index % 2, row = index / 2;
            var c = Box("Card_" + index + "_Game" + (index + 1), parent, 
                new Vector2(column * 0.5f, (1 - row) * 0.5f), 
                new Vector2((column + 1) * 0.5f, (2 - row) * 0.5f), 
                new Vector2(column == 0 ? 0 : 10, row == 0 ? 10 : 0), 
                new Vector2(column == 1 ? 0 : -10, row == 0 ? 0 : -10), 
                Color.white);
            Round(c);

            var shadow = c.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.08f, .18f, .25f, .09f);
            shadow.effectDistance = new Vector2(0, -3);

            var click = c.gameObject.AddComponent<Button>();
            click.targetGraphic = c.GetComponent<Image>();
            click.interactable = ready;
            var colors = click.colors;
            colors.highlightedColor = new Color(.94f, .98f, .98f);
            colors.disabledColor = Color.white;
            click.colors = colors;

            var art = Box("Illustration", c, new Vector2(0, 1), Vector2.one, new Vector2(14, -102), new Vector2(-14, -14), Color.Lerp(accent, Color.white, .88f));
            Round(art);
            Illustration(art, index, accent);

            var tag = Label("Status", art, ready ? "CÓ THỂ CHƠI" : "ĐANG PHÁT TRIỂN", 12, accent, new Vector2(12, -10), new Vector2(225, 23));
            tag.fontStyle = FontStyle.Bold;

            Label("Number", art, (index + 1).ToString("00"), 34, new Color(accent.r, accent.g, accent.b, .25f), new Vector2(12, -39), new Vector2(70, 45)).fontStyle = FontStyle.Bold;

            var heading = Label("Title", c, title, 22, Navy, new Vector2(22, -114), new Vector2(400, 32));
            heading.fontStyle = FontStyle.Bold;

            Label("Subtitle", c, subtitle, 15, accent, new Vector2(22, -148), new Vector2(400, 22));
            Label("Desc", c, description, 16, Slate, new Vector2(22, -178), new Vector2(400, 45));

            var bar = Box("BtnBar", c, Vector2.zero, new Vector2(1, 0), new Vector2(20, 16), new Vector2(-20, 58), Color.clear);
            bar.GetComponent<Image>().raycastTarget = false;

            MenuButton("ActionBtn", bar, ready ? "BẮT ĐẦU   [" + (index + 1) + "]   >" : "ĐANG PHÁT TRIỂN", ready ? accent : new Color(.91f, .93f, .95f), ready ? Color.white : new Color(.43f, .49f, .55f)).GetComponent<Button>().interactable = ready;

            foreach (var text in c.GetComponentsInChildren<Text>())
            {
                if (text.transform.parent == c)
                {
                    var r = text.rectTransform;
                    r.anchorMax = new Vector2(1, 1);
                    r.sizeDelta = new Vector2(-44, r.sizeDelta.y);
                }
            }
        }

        static RectTransform MenuButton(string name, Transform parent, string title, Color bg, Color foreground)
        {
            var r = Button(name, parent, title, bg);
            Round(r);
            var t = r.GetComponentInChildren<Text>();
            t.fontSize = 17;
            t.color = foreground;
            return r;
        }

        static void Round(RectTransform r)
        {
            var image = r.GetComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
        }

        static Sprite MaskSprite(string name, bool disk)
        {
            const string folder = "Assets/IUH_Campus/UI/MenuDesign";
            Directory.CreateDirectory(folder);
            string path = folder + "/" + name + ".png";
            if (!File.Exists(path))
            {
                const int n = 64;
                var texture = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var pixels = new Color[n * n];
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        float dx = Mathf.Abs(x - 31.5f), dy = Mathf.Abs(y - 31.5f);
                        float distance = disk ? Mathf.Sqrt(dx * dx + dy * dy) - 30 : Mathf.Sqrt(Mathf.Pow(Mathf.Max(dx - 16, 0), 2) + Mathf.Pow(Mathf.Max(dy - 16, 0), 2)) - 15;
                        pixels[y * n + x] = new Color(1, 1, 1, Mathf.Clamp01(.5f - distance));
                    }
                }
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = disk ? Vector4.zero : new Vector4(20, 20, 20, 20);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void Illustration(RectTransform parent, int game, Color color)
        {
            var icon = Box("Icon", parent, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(-120, -31), new Vector2(-30, 31), color);
            Round(icon);
            icon.GetComponent<Image>().raycastTarget = false;

            if (game == 0) // Grab Campus Motorbike
            {
                icon.GetComponent<Image>().color = Color.clear;
                foreach (float x in new[] { .2f, .8f })
                {
                    var wheel = Box("Wheel", icon, new Vector2(x, 0), new Vector2(x, 0), new Vector2(-13, 0), new Vector2(13, 26), color);
                    wheel.GetComponent<Image>().sprite = circle;
                }
                var frame = Box("BikeBody", icon, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-26, -4), new Vector2(23, 12), color);
                Round(frame);
                Box("Fork", icon, new Vector2(.78f, .3f), new Vector2(.78f, .3f), new Vector2(-3, 0), new Vector2(3, 40), color).localRotation = Quaternion.Euler(0, 0, 18);
            }
            else if (game == 1) // AI Object Hunt & AR
            {
                var text = Label("Symbol", icon, "AI • AR", 22, Color.white, Vector2.zero, Vector2.zero);
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = Vector2.zero;
                text.rectTransform.offsetMax = Vector2.zero;
            }
            else if (game == 2) // Photo Hunter (Camera lens)
            {
                var lens = Box("Lens", icon, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-21, -21), new Vector2(21, 21), Color.white);
                lens.GetComponent<Image>().sprite = circle;
                var inner = Box("LensGlass", lens, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-13, -13), new Vector2(13, 13), Color.Lerp(color, Color.white, .35f));
                inner.GetComponent<Image>().sprite = circle;
                Box("Flash", icon, Vector2.one, Vector2.one, new Vector2(-18, -14), new Vector2(-8, -7), Color.white);
            }
            else if (game == 3) // Green Campus (Leaf & recycle)
            {
                icon.GetComponent<Image>().color = Color.clear;
                var leaf = Box("Leaf", icon, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-23, -27), new Vector2(23, 27), color);
                Round(leaf);
                leaf.localRotation = Quaternion.Euler(0, 0, -30);
                Box("Stem", leaf, new Vector2(.5f, 0), new Vector2(.5f, 1), new Vector2(-2, 5), new Vector2(2, -9), Color.white);
            }

            foreach (var image in parent.GetComponentsInChildren<Image>()) image.raycastTarget = false;
        }
    }
}
#endif
