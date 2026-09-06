using UnityEngine;

namespace IUHCampus.Player
{
    public class PlayerState : MonoBehaviour
    {
        public string currentSpawnPointId = "SPAWN_MainCampus";
        public bool movementEnabled = true;
        public bool interactionEnabled = true;

        public Vector3 GetPosition() => transform.position;
        public Quaternion GetRotation() => transform.rotation;

        public void SetPose(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }
}
