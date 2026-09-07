using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.NPC
{
    public enum RouteMode
    {
        Loop,
        PingPong,
        OneWay
    }

    public class NPCRoute : MonoBehaviour
    {
        public string routeId = "NPC_Route_01";
        public RouteMode mode = RouteMode.Loop;
        public List<Waypoint> waypoints = new List<Waypoint>();

        private void OnValidate()
        {
            if (waypoints.Count == 0)
            {
                waypoints.AddRange(GetComponentsInChildren<Waypoint>());
            }
        }

        public Waypoint GetWaypoint(int index)
        {
            if (waypoints == null || waypoints.Count == 0) return null;
            if (index < 0 || index >= waypoints.Count) return null;
            return waypoints[index];
        }

        public int GetNextIndex(int currentIndex, ref bool forward)
        {
            if (waypoints == null || waypoints.Count <= 1) return 0;

            if (mode == RouteMode.Loop)
            {
                return (currentIndex + 1) % waypoints.Count;
            }
            else if (mode == RouteMode.PingPong)
            {
                if (forward)
                {
                    if (currentIndex + 1 >= waypoints.Count)
                    {
                        forward = false;
                        return currentIndex - 1;
                    }
                    return currentIndex + 1;
                }
                else
                {
                    if (currentIndex - 1 < 0)
                    {
                        forward = true;
                        return 1;
                    }
                    return currentIndex - 1;
                }
            }
            else // OneWay
            {
                return Mathf.Min(currentIndex + 1, waypoints.Count - 1);
            }
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count < 2) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                }
            }
            if (mode == RouteMode.Loop && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Count - 1].transform.position, waypoints[0].transform.position);
            }
        }
    }
}
