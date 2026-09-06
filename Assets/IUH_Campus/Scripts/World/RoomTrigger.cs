using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.World
{
    [RequireComponent(typeof(Collider))]
    public class RoomTrigger : MonoBehaviour
    {
        [Header("Room Identity")]
        public string buildingName = "Tòa G";
        public int floorNumber = 2;
        public string roomId = "G-201";
        public string roomDisplayName = "Phòng học G-201";

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (PlayerLocationTracker.Instance != null)
                {
                    PlayerLocationTracker.Instance.UpdateLocation(buildingName, floorNumber, roomDisplayName);
                }
                GameEvents.TriggerRoomEntered(buildingName, roomId);
                GameEvents.TriggerFloorChanged(buildingName, floorNumber);
            }
        }
    }
}
