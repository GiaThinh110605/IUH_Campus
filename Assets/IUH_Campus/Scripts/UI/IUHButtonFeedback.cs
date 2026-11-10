using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IUHCampus.UI
{
    /// <summary>
    /// Hiệu ứng phản hồi tương tác nút bấm phong cách Colorful Gaming HUD:
    /// - Scale nhẹ khi nhấn (Pressed State: 0.92f)
    /// - Bouncy spring khi thả tay (1.05f -> 1.0f)
    /// - Tăng độ sáng / glow khi tương tác
    /// - Siêu mượt, không tốn tài nguyên, không tụt FPS.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class IUHButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float m_PressedScale = 0.91f;
        [SerializeField] private float m_HoverScale = 1.04f;
        [SerializeField] private float m_SpringSpeed = 16f;

        private RectTransform m_RectTransform;
        private Vector3 m_TargetScale = Vector3.one;
        private Vector3 m_OriginalScale = Vector3.one;
        private bool m_IsPressed = false;
        private bool m_IsHovered = false;

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_OriginalScale = m_RectTransform.localScale;
            m_TargetScale = m_OriginalScale;
        }

        private void Update()
        {
            if (m_RectTransform == null) return;

            if (Vector3.SqrMagnitude(m_RectTransform.localScale - m_TargetScale) > 0.0001f)
            {
                m_RectTransform.localScale = Vector3.Lerp(m_RectTransform.localScale, m_TargetScale, Time.unscaledDeltaTime * m_SpringSpeed);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_IsPressed = true;
            m_TargetScale = m_OriginalScale * m_PressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_IsPressed = false;
            // Bouncy rebound
            m_RectTransform.localScale = m_OriginalScale * (m_IsHovered ? m_HoverScale * 1.03f : 1.05f);
            m_TargetScale = m_IsHovered ? m_OriginalScale * m_HoverScale : m_OriginalScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_IsHovered = true;
            if (!m_IsPressed)
            {
                m_TargetScale = m_OriginalScale * m_HoverScale;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_IsHovered = false;
            if (!m_IsPressed)
            {
                m_TargetScale = m_OriginalScale;
            }
        }

        private void OnDisable()
        {
            m_IsPressed = false;
            m_IsHovered = false;
            if (m_RectTransform != null)
            {
                m_RectTransform.localScale = m_OriginalScale;
                m_TargetScale = m_OriginalScale;
            }
        }
    }
}

