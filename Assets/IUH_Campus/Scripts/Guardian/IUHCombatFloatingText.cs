using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Minigames.GreenCampus;

namespace IUHCampus.Guardian
{
    public class IUHCombatFloatingText : MonoBehaviour
    {
        private Text m_Text;
        private CanvasGroup m_CanvasGroup;
        private Vector3 m_Velocity;
        private float m_Lifetime = 1.1f;
        private float m_Age = 0f;

        public static void Spawn(Vector3 worldPos, string content, Color color, float sizeScale = 1.0f)
        {
            GameObject go = new GameObject("FloatingText", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(IUHGreenBillboard), typeof(IUHCombatFloatingText));
            go.transform.position = worldPos + new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.1f, 0.25f), Random.Range(-0.35f, 0.35f));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 60f);
            rt.localScale = Vector3.one * 0.0075f * sizeScale;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            textObj.transform.SetParent(go.transform, false);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var txt = textObj.GetComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;

            var outline = textObj.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var comp = go.GetComponent<IUHCombatFloatingText>();
            comp.Init(txt, go.GetComponent<CanvasGroup>());
        }

        public void Init(Text text, CanvasGroup canvasGroup)
        {
            m_Text = text;
            m_CanvasGroup = canvasGroup;
            m_Velocity = new Vector3(Random.Range(-0.2f, 0.2f), 1.25f, Random.Range(-0.2f, 0.2f));
        }

        private void Update()
        {
            m_Age += Time.deltaTime;
            transform.position += m_Velocity * Time.deltaTime;
            m_Velocity.y = Mathf.Max(0.2f, m_Velocity.y - Time.deltaTime * 0.6f);

            if (m_CanvasGroup != null)
            {
                float progress = m_Age / m_Lifetime;
                if (progress > 0.5f)
                {
                    m_CanvasGroup.alpha = 1.0f - ((progress - 0.5f) / 0.5f);
                }
            }

            if (m_Age >= m_Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}

