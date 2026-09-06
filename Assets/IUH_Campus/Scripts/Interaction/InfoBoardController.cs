using UnityEngine;
using IUHCampus.Core;
using IUHCampus.UI;

namespace IUHCampus.Interaction
{
    [RequireComponent(typeof(WorldObjectID))]
    public class InfoBoardController : MonoBehaviour, IInteractable
    {
        [Header("Information Board Content")]
        public string boardTitle = "BẢNG TIN ĐẠI HỌC CÔNG NGHIỆP TP.HCM (IUH)";
        [TextArea(4, 10)]
        public string boardContent = "- Thông báo: Tuần lễ sinh hoạt công dân sinh viên đầu khóa.\n- Đăng ký học phần HK1 qua Cổng thông tin sinh viên.\n- Khu vực Nhà G: Văn phòng hỗ trợ học tập & Phòng thực hành CNTT.\n- Lối thoát hiểm & bình PCCC được bố trí tại sảnh chính.";

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
            return "[E] Xem bảng thông tin";
        }

        public void Interact(GameObject source)
        {
            EnsureInitialized();
            Debug.Log($"[InfoBoardController] Đang mở bảng tin (ID: {m_WorldID?.objectId})");
            SimplePanelUI.ShowPanel(boardTitle, boardContent);
            GameEvents.TriggerObjectInteracted(m_WorldID?.objectId, source);
        }
    }
}
