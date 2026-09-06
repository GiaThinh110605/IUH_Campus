using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.World
{
    public class PlayerLocationTracker : MonoBehaviour
    {
        public static PlayerLocationTracker Instance { get; private set; }

        [Header("Current Status")]
        public string currentBuilding = "Khuôn viên IUH";
        public int currentFloor = 1;
        public string currentRoom = "Sân trường";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            UpdateLocation(currentBuilding, currentFloor, currentRoom);
        }

        public void UpdateLocation(string building, int floor, string room)
        {
            bool changed = (building != currentBuilding) || (floor != currentFloor) || (room != currentRoom);
            currentBuilding = building;
            currentFloor = floor;
            currentRoom = room;

            if (changed)
            {
                string desc = GetFullLocationString();
                GameEvents.TriggerLocationChanged(desc);
                GameEvents.TriggerFloorChanged(building, floor);
                GameEvents.TriggerRoomEntered(building, room);
                Debug.Log($"[LocationTracker] Vị trí cập nhật: {desc}");
            }
        }

        public string GetFullLocationString()
        {
            if (string.IsNullOrEmpty(currentRoom) || currentRoom == "Sân trường")
            {
                return $"Khuôn viên IUH | {currentBuilding}";
            }
            return $"Khuôn viên IUH | {currentBuilding} - Tầng {currentFloor} ({currentRoom})";
        }
    }
}
