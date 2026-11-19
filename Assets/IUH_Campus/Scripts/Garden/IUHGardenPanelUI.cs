using System;
using System.Collections.Generic;
using IUHCampus.Core;
using IUHCampus.Guardian;
using IUHCampus.Guardian.Match3;
using IUHCampus.Guardian.Story;
using IUHCampus.World;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Garden
{
    [DefaultExecutionOrder(-100)]
    public sealed class IUHGardenPanelUI : MonoBehaviour
    {
        const float OpenDistance = 2f;
        const float CloseDistance = 3f;
        IUHGardenManager manager;
        IUHGardenBedView[] views;
        IUHStoryDialogueUI story;
        Transform player;
        RectTransform safe;
        RectTransform panel;
        Rect lastArea;
        Vector2Int lastSize;
        Font font;
        Text title, status, balance;
        Button plant, water, fertilizer, harvest;
        float refreshAt;
        int nearest = -1;
        CursorLockMode previousLock;
        bool previousCursor;

        public int SelectedBed { get; private set; } = -1;
        public bool IsOpen => SelectedBed >= 0;

        void Start()
        {
            manager = GetComponent<IUHGardenManager>();
            views = GetComponentsInChildren<IUHGardenBedView>();
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            player = student != null ? student.transform : null;
            story = FindFirstObjectByType<IUHStoryDialogueUI>();
            Build();
            manager.OnBedChanged += OnBedChanged;
            foreach (var view in views) OnBedChanged(view.BedIndex);
        }

        void OnDestroy()
        {
            if (manager != null) manager.OnBedChanged -= OnBedChanged;
        }

        void OnDisable()
        {
            if (IsOpen) Close();
        }

        public static int FindNearestBed(Vector3 playerPos, IReadOnlyList<Vector3> beds, float maxDist)
        {
            float best = maxDist * maxDist;
            int index = -1;
            for (int i = 0; i < beds.Count; i++)
            {
                var distance = (playerPos - beds[i]).sqrMagnitude;
                if (distance > best || (index >= 0 && distance == best)) continue;
                index = i;
                best = distance;
            }
            return index;
        }

        public bool CanOpenNow()
        {
            var combat = Match3CombatManager.Instance;
            var shop = IUHUpgradeShopUI.Instance;
            return Time.timeScale > 0 && (combat == null || !combat.BlocksExplorationInput) &&
                (story == null || !story.IsBusy) && (shop == null || !shop.IsOpen) &&
                !IUHConceptPreviewController.IsAnyUIOpen();
        }

        void Update()
        {
            if (player == null || panel == null) return;
            ApplySafeArea();
            var candidate = NearbyBed();
            if (IsOpen)
            {
                var view = FindView(SelectedBed);
                if (view == null || !CanOpenNow() || Vector3.Distance(player.position, view.transform.position) > CloseDistance)
                    Close();
                else if (IUHInputHelper.IsGardenInteractKeyDown())
                    Close();
            }
            else if (candidate >= 0 && CanOpenNow() && IUHInputHelper.IsGardenInteractKeyDown())
            {
                Open(candidate);
            }
            if (candidate == nearest && Time.unscaledTime < refreshAt) return;
            nearest = candidate;
            refreshAt = Time.unscaledTime + .25f;
            var now = manager.Clock();
            foreach (var view in views)
                view.SetPrompt(manager.GetBed(view.BedIndex), now, candidate == view.BedIndex && !IsOpen);
            if (IsOpen) RefreshPanel();
        }

        int NearbyBed()
        {
            float best = OpenDistance * OpenDistance;
            int result = -1;
            foreach (var view in views)
            {
                float distance = (player.position - view.transform.position).sqrMagnitude;
                if (distance > best || (result >= 0 && distance == best)) continue;
                best = distance;
                result = view.BedIndex;
            }
            return result;
        }

        IUHGardenBedView FindView(int index) => Array.Find(views, view => view.BedIndex == index);

        void OnBedChanged(int index)
        {
            FindView(index)?.Refresh(manager.GetBed(index), manager.Clock());
            if (index == SelectedBed) RefreshPanel();
        }

        void Open(int index)
        {
            previousLock = Cursor.lockState;
            previousCursor = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SelectedBed = index;
            panel.gameObject.SetActive(true);
            RefreshPanel();
        }

        void Close()
        {
            SelectedBed = -1;
            panel.gameObject.SetActive(false);
            if (CanOpenNow())
            {
                Cursor.lockState = previousLock;
                Cursor.visible = previousCursor;
            }
        }

        void RefreshPanel()
        {
            var bed = manager.GetBed(SelectedBed);
            int gold = IUHGuardianInventory.Instance != null ? IUHGuardianInventory.Instance.Gold : 0;
            var stage = new[] { "Trống", "Hạt", "Mầm", "Nở" };
            title.text = $"Luống hoa {SelectedBed + 1}/{manager.BedCount}";
            string next = IUHGardenBedView.Status(bed, manager.Clock());
            status.text = stage[(int)bed.Stage] + (bed.Fertilized ? "  ·  Đã bón phân" : "") +
                (next.Length > 0 ? "\n" + next : "");
            balance.text = $"Vàng: {gold}";
            SetButton(plant, bed.CanPlant(), gold >= GardenRules.SeedCost, $"Gieo hạt −{GardenRules.SeedCost}");
            SetButton(water, bed.CanWater(), true, "Tưới");
            SetButton(fertilizer, bed.CanFertilize(), gold >= GardenRules.FertilizerCost, $"Bón phân −{GardenRules.FertilizerCost}");
            int reward = bed.Fertilized ? GardenRules.FertilizedHarvestGold : GardenRules.HarvestGold;
            SetButton(harvest, bed.CanHarvest(), true, $"Hái +{reward} Vàng");
        }

        static void SetButton(Button button, bool visible, bool enabled, string text)
        {
            button.gameObject.SetActive(visible);
            button.interactable = enabled;
            button.GetComponentInChildren<Text>(true).text = enabled ? text : $"Cần {GardenRules.SeedCost} Vàng";
        }

        void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Garden Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;
            safe = Rect("Safe area", root.transform, Vector2.zero, Vector2.one);
            panel = Rect("Flower bed", safe, new Vector2(.27f, .26f), new Vector2(.73f, .74f));
            panel.gameObject.AddComponent<Image>().color = new Color(.035f, .085f, .10f, .96f);
            title = Label("Title", panel, .08f, .78f, .81f, .98f, 34);
            status = Label("Status", panel, .08f, .47f, .92f, .80f, 28);
            balance = Label("Gold", panel, .08f, .04f, .92f, .17f, 24);
            plant = AddButton(panel, .08f, .24f, .48f, .43f, () => manager.TryPlant(SelectedBed));
            water = AddButton(panel, .08f, .24f, .48f, .43f, () => manager.TryWater(SelectedBed));
            harvest = AddButton(panel, .08f, .24f, .48f, .43f, () => manager.TryHarvest(SelectedBed));
            fertilizer = AddButton(panel, .52f, .24f, .94f, .43f, () => manager.TryFertilize(SelectedBed));
            var close = AddButton(panel, .84f, .82f, .98f, .98f, Close);
            close.GetComponentInChildren<Text>().text = "X";
            panel.gameObject.SetActive(false);
        }

        Button AddButton(Transform parent, float x1, float y1, float x2, float y2, UnityEngine.Events.UnityAction click)
        {
            var r = Rect("Action", parent, new Vector2(x1, y1), new Vector2(x2, y2));
            var image = r.gameObject.AddComponent<Image>();
            image.color = new Color(.13f, .35f, .31f);
            var button = r.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);
            Label("Label", r, 0, 0, 1, 1, 24);
            return button;
        }

        Text Label(string name, Transform parent, float x1, float y1, float x2, float y2, int size)
        {
            var r = Rect(name, parent, new Vector2(x1, y1), new Vector2(x2, y2));
            var text = r.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = min;
            r.anchorMax = max;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return r;
        }

        void ApplySafeArea()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            var area = Screen.safeArea;
            if (size.x == 0 || size.y == 0 || (size == lastSize && area == lastArea)) return;
            lastSize = size;
            lastArea = area;
            safe.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            safe.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
        }
    }
}
