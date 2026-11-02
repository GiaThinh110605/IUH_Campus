using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IUHCampus.Guardian.Match3
{
    public class Match3TileView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private Image m_IconImage;
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private RectTransform m_RectTransform;

        private BoardCoord m_Coord;
        private TileType m_Type;
        private Action<BoardCoord, Vector2> m_OnSwipeAction;
        private Action<BoardCoord> m_OnClickAction;
        private Vector2 m_PointerDownPosition;
        private bool m_IsDragging;

        private const float SWIPE_THRESHOLD = 30f;

        public BoardCoord Coord => m_Coord;
        public TileType Type => m_Type;
        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }

        private void Awake()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
            if (m_IconImage == null) m_IconImage = GetComponent<Image>();
        }

        public void Setup(BoardCoord coord, TileType type, Sprite iconSprite, Sprite bgSprite, Action<BoardCoord, Vector2> onSwipe, Action<BoardCoord> onClick)
        {
            EnsureComponents();
            m_Coord = coord;
            m_Type = type;
            m_Coord = coord;
            m_Type = type;
            m_OnSwipeAction = onSwipe;
            m_OnClickAction = onClick;

            if (m_IconImage != null)
            {
                m_IconImage.sprite = iconSprite;
                m_IconImage.color = Color.white;
            }

            if (m_BackgroundImage != null && bgSprite != null)
            {
                m_BackgroundImage.sprite = bgSprite;
            }

            transform.localScale = Vector3.one;
        }

        public void UpdateCoord(BoardCoord newCoord)
        {
            m_Coord = newCoord;
        }

        public void SetVisualType(TileType type, Sprite iconSprite)
        {
            m_Type = type;
            if (m_IconImage != null)
            {
                m_IconImage.sprite = iconSprite;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_PointerDownPosition = eventData.position;
            m_IsDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_IsDragging) return;

            Vector2 delta = eventData.position - m_PointerDownPosition;
            if (delta.magnitude >= SWIPE_THRESHOLD)
            {
                m_IsDragging = false;
                Vector2 direction = GetCardinalDirection(delta);
                m_OnSwipeAction?.Invoke(m_Coord, direction);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_IsDragging)
            {
                Vector2 delta = eventData.position - m_PointerDownPosition;
                if (delta.magnitude < SWIPE_THRESHOLD)
                {
                    m_OnClickAction?.Invoke(m_Coord);
                }
                m_IsDragging = false;
            }
        }

        private Vector2 GetCardinalDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? Vector2.right : Vector2.left;
            }
            return delta.y > 0 ? Vector2.up : Vector2.down;
        }

        public IEnumerator AnimateMoveTo(Vector2 targetAnchoredPos, float duration)
        {
            Vector2 startPos = m_RectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3);
                m_RectTransform.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, ease);
                yield return null;
            }

            m_RectTransform.anchoredPosition = targetAnchoredPos;
        }

        public IEnumerator AnimatePopAndFade(float duration)
        {
            Vector3 startScale = Vector3.one;
            Vector3 popScale = Vector3.one * 1.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, popScale, t);
                if (m_IconImage != null)
                {
                    Color c = m_IconImage.color;
                    c.a = 1f - t;
                    m_IconImage.color = c;
                }
                yield return null;
            }

            gameObject.SetActive(false);
        }

        public IEnumerator AnimateSpawn(Vector2 targetPos, float duration)
        {
            gameObject.SetActive(true);
            m_RectTransform.anchoredPosition = targetPos + new Vector2(0, 50f);
            transform.localScale = Vector3.zero;
            if (m_IconImage != null) m_IconImage.color = Color.white;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3);
                m_RectTransform.anchoredPosition = Vector2.Lerp(m_RectTransform.anchoredPosition, targetPos, ease);
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ease);
                yield return null;
            }

            m_RectTransform.anchoredPosition = targetPos;
            transform.localScale = Vector3.one;
        }
    }
}

