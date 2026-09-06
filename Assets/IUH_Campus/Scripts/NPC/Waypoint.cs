using UnityEngine;

namespace IUHCampus.NPC
{
    public class Waypoint : MonoBehaviour
    {
        public float waitTime = 2.0f;
        public string waypointName = "WP";

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
