using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryDialogueUI : MonoBehaviour
    {
        RectTransform safeRoot;
        Text speakerText;
        Text body;
        Text objective;
        Text caption;
        Button dialogue;
        IReadOnlyList<string> lines;
        Action finished;
        int index;
        float remaining;
        Coroutine captionRoutine;
        UnityEngine.Rect lastSafeArea;
        Vector2Int lastScreenSize;
        public bool IsBusy => lines != null;

        public static IUHStoryDialogueUI Create()
        {
            var go = new GameObject("STORY_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            return go.AddComponent<IUHStoryDialogueUI>();
        }

        void Awake()
        {
            safeRoot = Rect("SafeArea", transform, Vector2.zero, Vector2.one);
            ApplySafeArea();
            var panel = Rect("Dialogue", safeRoot, new Vector2(.15f, .67f), new Vector2(.85f, .91f));
            panel.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .75f);
            dialogue = panel.gameObject.AddComponent<Button>();
            dialogue.onClick.AddListener(Advance);
            speakerText = Label("Speaker", panel, 34, new Vector2(.03f, .70f), new Vector2(.97f, .98f));
            speakerText.color = new Color(25f / 255, 242f / 255, 1);
            body = Label("Line", panel, 34, new Vector2(.03f, .06f), new Vector2(.97f, .70f));
            objective = Label("Objective", safeRoot, 30, new Vector2(.025f, .92f), new Vector2(.75f, .99f));
            caption = Label("Caption", safeRoot, 56, new Vector2(.1f, .36f), new Vector2(.9f, .64f));
            caption.alignment = TextAnchor.MiddleCenter;
            dialogue.gameObject.SetActive(false);
            caption.gameObject.SetActive(false);
        }

        void ApplySafeArea()
        {
            if (Screen.width == 0 || Screen.height == 0)
                return;
            var area = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (area == lastSafeArea && size == lastScreenSize)
                return;
            lastSafeArea = area;
            lastScreenSize = size;
            safeRoot.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
        }

        internal static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        internal static Text Label(string name, Transform parent, int size, Vector2 min, Vector2 max)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = Color.white;
            text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        internal Button CreateSkipButton(Action skip)
        {
            var rect = Rect("Skip", safeRoot, new Vector2(.78f, .32f), new Vector2(.97f, .41f));
            rect.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .8f);
            var button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => skip());
            var label = Label("Label", rect, 34, Vector2.zero, Vector2.one);
            label.text = "Bỏ qua";
            label.alignment = TextAnchor.MiddleCenter;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            return button;
        }

        public void ShowLines(string speaker, IReadOnlyList<string> text, Action onFinished)
        {
            lines = text;
            index = 0;
            finished = onFinished;
            speakerText.text = speaker;
            if (lines == null || lines.Count == 0)
            {
                Finish();
                return;
            }

            dialogue.gameObject.SetActive(true);
            Display();
        }

        void Display()
        {
            body.text = lines[index];
            remaining = Mathf.Max(4, .06f * body.text.Length);
        }

        void Advance()
        {
            if (!IsBusy)
                return;
            if (++index == lines.Count)
                Finish();
            else
                Display();
        }

        void Finish()
        {
            lines = null;
            dialogue.gameObject.SetActive(false);
            var callback = finished;
            finished = null;
            callback?.Invoke();
        }

        void Update()
        {
            ApplySafeArea();
            if (!IsBusy)
                return;
            remaining -= Time.unscaledDeltaTime;
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard=Keyboard.current;
            pressed=keyboard!=null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
            pressed=Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space);
#endif
            if (pressed || remaining <= 0)
                Advance();
        }

        public void SetObjective(string text)
        {
            objective.text = text;
            objective.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void ShowCaption(string text, float seconds)
        {
            if (captionRoutine != null)
                StopCoroutine(captionRoutine);
            captionRoutine = StartCoroutine(Caption(text, seconds));
        }

        IEnumerator Caption(string text, float seconds)
        {
            caption.text = text;
            caption.gameObject.SetActive(true);
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                caption.color = new Color(1, 1, 1, Mathf.Clamp01(seconds - t));
                yield return null;
            }

            caption.gameObject.SetActive(false);
            captionRoutine = null;
        }
    }
}
