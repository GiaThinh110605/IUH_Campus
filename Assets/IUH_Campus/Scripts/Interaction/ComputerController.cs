using UnityEngine;
using IUHCampus.Core;
using IUHCampus.UI;

namespace IUHCampus.Interaction
{
    [RequireComponent(typeof(WorldObjectID))]
    public class ComputerController : MonoBehaviour, IInteractable
    {
        [Header("Terminal Configuration")]
        public string terminalTitle = "MÁY TRẠM PHÒNG THỰC HÀNH CNTT - IUH";
        [TextArea(4, 10)]
        public string terminalContent = "HỆ ĐIỀU HÀNH: IUH Lab OS v4.2\nTRẠNG THÁI: Trực tuyến (Sẵn sàng)\nNGƯỜI DÙNG: Sinh viên Đại học Công nghiệp TP.HCM\n\n[1] Mở IDE lập trình Unity / C#\n[2] Kiểm tra kết nối mạng nội bộ IUH\n[3] Đăng xuất phiên làm việc";

        public bool isBeingUsed = false;
        private WorldObjectID m_WorldID;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (m_WorldID == null) m_WorldID = GetComponent<WorldObjectID>();
        }

        public bool CanInteract()
        {
            return true;
        }

        public string GetInteractionText()
        {
            return "[E] Sử dụng máy tính";
        }

        public void Interact(GameObject source)
        {
            EnsureInitialized();
            isBeingUsed = true;
            Debug.Log($"[ComputerController] Đang sử dụng máy trạm (ID: {m_WorldID?.objectId})");
            SimplePanelUI.ShowPanel(terminalTitle, terminalContent);
            GameEvents.TriggerObjectInteracted(m_WorldID?.objectId, source);
        }
    }
}
