using UnityEngine;

namespace IUHCampus.Interaction
{
    /// <summary>
    /// Component placed on campus benches/chairs allowing player to sit realistically.
    /// Prevents walking through the bench via solid collider and provides seat alignment.
    /// </summary>
    [SelectionBase]
    public class IUHBenchSeat : MonoBehaviour, IInteractable
    {
        [Header("Seat Configuration")]
        [Tooltip("Height offset above bench surface for character hips")]
        public float seatHeightOffset = 0.12f;

        [Tooltip("Forward offset for seat position relative to bench center")]
        public float seatForwardOffset = 0.0f;

        [Tooltip("Maximum distance to interact with bench")]
        public float interactionRadius = 2.4f;

        [Tooltip("Seat direction override. If zero, calculates automatically based on bench orientation or player approach")]
        public Vector3 facingDirection = Vector3.zero;

        [Header("State")]
        [SerializeField] private GameObject m_CurrentOccupant;

        public bool IsOccupied => m_CurrentOccupant != null;
        public GameObject Occupant => m_CurrentOccupant;

        private Collider m_Collider;
        private MeshRenderer m_Renderer;

        private void Awake()
        {
            EnsureCollider();
        }

        public void EnsureCollider()
        {
            m_Collider = GetComponent<Collider>();
            m_Renderer = GetComponent<MeshRenderer>();

            if (m_Collider == null)
            {
                var mf = GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    var mc = gameObject.AddComponent<MeshCollider>();
                    mc.convex = false;
                    m_Collider = mc;
                }
                else
                {
                    var bc = gameObject.AddComponent<BoxCollider>();
                    if (m_Renderer != null)
                    {
                        bc.center = transform.InverseTransformPoint(m_Renderer.bounds.center);
                        bc.size = m_Renderer.bounds.size;
                    }
                    m_Collider = bc;
                }
            }
        }

        /// <summary>
        /// Calculates the best seat position and forward facing direction for a player approaching from playerPos.
        /// Keeps the character's root at ground level so feet rest on the floor!
        /// </summary>
        public void GetSeatTransform(Vector3 playerPos, out Vector3 seatPosition, out Quaternion seatRotation)
        {
            Bounds b = m_Renderer != null ? m_Renderer.bounds : (m_Collider != null ? m_Collider.bounds : new Bounds(transform.position, Vector3.one));

            // Ground level where player stands (keep character root on the floor!)
            float groundY = playerPos.y;
            if (groundY < b.min.y - 0.5f || groundY > b.max.y + 0.5f)
            {
                groundY = b.min.y;
            }

            // Determine bench primary axes
            Vector3 center = b.center;

            // Calculate facing direction
            Vector3 forwardDir;
            if (facingDirection != Vector3.zero)
            {
                forwardDir = facingDirection.normalized;
            }
            else
            {
                // Align perpendicular to longest bench horizontal dimension
                Vector3 size = b.size;
                Vector3 candidate1 = (size.x >= size.z) ? Vector3.forward : Vector3.right;
                Vector3 candidate2 = -candidate1;

                // Face the direction player is approaching from (sitting facing outwards towards player approach)
                Vector3 approachDir = (playerPos - center).normalized;
                approachDir.y = 0;
                forwardDir = (Vector3.Dot(candidate1, approachDir) >= Vector3.Dot(candidate2, approachDir)) ? candidate1 : candidate2;
            }

            // Place character root on the GROUND at the front edge of the bench seat
            // Hips will settle down onto the cushion, and feet stay on the floor in front of the bench!
            Vector3 rootPos = new Vector3(center.x, groundY, center.z);
            rootPos += forwardDir * (seatForwardOffset + 0.12f);

            seatPosition = rootPos;
            seatRotation = Quaternion.LookRotation(forwardDir, Vector3.up);
        }

        public bool CanInteract()
        {
            return !IsOccupied;
        }

        public void Interact(GameObject source)
        {
            if (source == null || IsOccupied) return;

            var sitController = source.GetComponent<IUHCampus.Player.IUHPlayerSittingController>();
            if (sitController != null)
            {
                sitController.SitOnBench(this);
            }
        }

        public string GetInteractionText()
        {
            return IsOccupied ? "Ghế đã có người ngồi" : "Nhấn [E] để ngồi xuống ghế";
        }

        public void SetOccupant(GameObject occupant)
        {
            m_CurrentOccupant = occupant;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 pos;
            Quaternion rot;
            GetSeatTransform(transform.position + transform.forward * 2f, out pos, out rot);
            Gizmos.DrawWireSphere(pos, 0.25f);
            Gizmos.DrawRay(pos, rot * Vector3.forward * 0.8f);
        }
    }
}
