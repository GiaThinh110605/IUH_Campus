using UnityEngine;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Holographic 3D Waypoint beacon for Grab Campus deliveries (Pickup & Dropoff).
    /// Features glowing cylinder beam, pulsing ground rings, and billboard overhead marker.
    /// </summary>
    public class IUHDeliveryWaypoint : MonoBehaviour
    {
        public enum WaypointType { Pickup, Dropoff }

        [Header("Waypoint Config")]
        [SerializeField] private WaypointType m_Type = WaypointType.Pickup;
        [SerializeField] private Color m_BeaconColor = new Color(0f, 1f, 0.45f, 0.85f);
        [SerializeField] private float m_PulseSpeed = 3.0f;
        [SerializeField] private float m_Radius = 3.5f;

        [Header("Visual Elements")]
        [SerializeField] private Transform m_CylinderBeam;
        [SerializeField] private Transform m_GroundRing;
        [SerializeField] private Transform m_FloatingIcon;
        [SerializeField] private Light m_PointLight;

        public WaypointType Type => m_Type;
        public float Radius => m_Radius;

        public void Setup(WaypointType type, Color color)
        {
            m_Type = type;
            m_BeaconColor = color;

            if (m_PointLight != null)
            {
                m_PointLight.color = color;
            }
        }

        private void Update()
        {
            // Pulsing ground ring scale
            if (m_GroundRing != null)
            {
                float ringScale = 1.0f + Mathf.Sin(Time.time * m_PulseSpeed) * 0.2f;
                m_GroundRing.localScale = new Vector3(ringScale, 0.05f, ringScale);
            }

            // Rotating & bobbing floating icon
            if (m_FloatingIcon != null)
            {
                m_FloatingIcon.Rotate(0f, 45f * Time.deltaTime, 0f);
                float bobY = 2.2f + Mathf.Sin(Time.time * 2.5f) * 0.25f;
                m_FloatingIcon.localPosition = new Vector3(0f, bobY, 0f);
            }

            // Beam opacity wave
            if (m_CylinderBeam != null)
            {
                m_CylinderBeam.Rotate(0f, -20f * Time.deltaTime, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var bike = other.GetComponentInParent<IUHGrabMotorbike>();
            var player = other.CompareTag("Player") || other.name.Contains("Player");

            if (bike != null || player)
            {
                var mgr = IUHGrabCampusManager.Instance;
                if (mgr != null && mgr.IsMissionActive)
                {
                    mgr.OnWaypointReached(this);
                }
            }
        }
    }
}
