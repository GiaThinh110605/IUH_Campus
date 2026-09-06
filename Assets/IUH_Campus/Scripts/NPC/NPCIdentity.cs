using UnityEngine;

namespace IUHCampus.NPC
{
    public class NPCIdentity : MonoBehaviour
    {
        public string npcId = "NPC_STUDENT_001";
        public string displayName = "Sinh viên IUH";
        public string role = "Sinh viên";
        public string department = "Khoa Công nghệ Thông tin";

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(npcId))
            {
                npcId = "NPC_" + gameObject.name.ToUpper().Replace(" ", "_");
            }
        }
    }
}
