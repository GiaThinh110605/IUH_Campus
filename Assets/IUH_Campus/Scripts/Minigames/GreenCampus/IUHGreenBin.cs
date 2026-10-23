using UnityEngine;

namespace IUHCampus.Minigames.GreenCampus
{
    public class IUHGreenBin : MonoBehaviour
    {
        public WasteCategory category;
        public string Label => CategoryLabel(category);
        public static string CategoryLabel(WasteCategory category)
        {
            switch (category)
            {
                case WasteCategory.Recyclable: return "TÁI CHẾ";
                case WasteCategory.Organic: return "HỮU CƠ";
                default: return "CÒN LẠI";
            }
        }
    }
}
