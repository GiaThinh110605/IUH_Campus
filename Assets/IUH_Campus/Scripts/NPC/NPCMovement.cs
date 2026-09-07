using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace IUHCampus.NPC
{
    public enum NPCState
    {
        Idle,
        Walking,
        Waiting
    }

    [RequireComponent(typeof(NPCIdentity))]
    public class NPCMovement : MonoBehaviour
    {
        [Header("Route & Navigation")]
        public NPCRoute assignedRoute;
        public float moveSpeed = 1.8f;
        public float arrivalThreshold = 0.5f;

        [Header("Current State")]
        public NPCState currentState = NPCState.Idle;
        public int currentWaypointIndex = 0;

        private NavMeshAgent m_Agent;
        private bool m_Forward = true;
        private Coroutine m_WaitCoroutine;

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            if (m_Agent != null)
            {
                m_Agent.speed = moveSpeed;
            }
        }

        private void Start()
        {
            if (assignedRoute != null && assignedRoute.waypoints.Count > 0)
            {
                MoveToWaypoint(currentWaypointIndex);
            }
        }

        private void Update()
        {
            if (assignedRoute == null || assignedRoute.waypoints.Count == 0) return;
            if (currentState != NPCState.Walking) return;

            Waypoint targetWp = assignedRoute.GetWaypoint(currentWaypointIndex);
            if (targetWp == null) return;

            float dist = Vector3.Distance(transform.position, targetWp.transform.position);

            if (m_Agent != null && m_Agent.isActiveAndEnabled && m_Agent.isOnNavMesh)
            {
                if (!m_Agent.pathPending && m_Agent.remainingDistance <= arrivalThreshold)
                {
                    OnArrivedAtWaypoint(targetWp);
                }
            }
            else
            {
                // Fallback direct movement
                Vector3 dir = (targetWp.transform.position - transform.position);
                dir.y = 0;
                if (dir.magnitude > arrivalThreshold)
                {
                    transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
                    }
                }
                else
                {
                    OnArrivedAtWaypoint(targetWp);
                }
            }
        }

        private void MoveToWaypoint(int index)
        {
            Waypoint wp = assignedRoute.GetWaypoint(index);
            if (wp == null) return;

            currentState = NPCState.Walking;
            if (m_Agent != null && m_Agent.isActiveAndEnabled && m_Agent.isOnNavMesh)
            {
                m_Agent.isStopped = false;
                m_Agent.SetDestination(wp.transform.position);
            }
        }

        private void OnArrivedAtWaypoint(Waypoint wp)
        {
            if (m_WaitCoroutine != null) StopCoroutine(m_WaitCoroutine);
            m_WaitCoroutine = StartCoroutine(WaitAndProceed(wp.waitTime));
        }

        private IEnumerator WaitAndProceed(float seconds)
        {
            currentState = NPCState.Waiting;
            if (m_Agent != null && m_Agent.isActiveAndEnabled && m_Agent.isOnNavMesh)
            {
                m_Agent.isStopped = true;
            }

            yield return new WaitForSeconds(seconds);

            currentWaypointIndex = assignedRoute.GetNextIndex(currentWaypointIndex, ref m_Forward);
            MoveToWaypoint(currentWaypointIndex);
        }
    }
}
