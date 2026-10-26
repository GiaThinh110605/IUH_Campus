using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Holographic 3D Waypoint beacon for Grab Campus deliveries (Pickup & Dropoff).
    /// Features:
    /// - 40-meter towering glowing sky beam visible across the whole campus.
    /// - Pulsing multi-ring ground landing zone.
    /// - Rotating diamond holographic marker.
    /// - Auto-facing camera billboard with prominent instructions:
    ///   * "[ DỪNG XE & BẤM [E] ĐỂ LẤY ĐỒ ]" at Pickup
    ///   * "[ DỪNG XE & BẤM [E] ĐỂ GIAO HÀNG CHO SINH VIÊN ]" at Dropoff
    /// </summary>
    public class IUHDeliveryWaypoint : MonoBehaviour
    {
        public enum WaypointType { Pickup, Dropoff }

        [Header("Waypoint Config")]
        [SerializeField] private WaypointType m_Type = WaypointType.Pickup;
        [SerializeField] private Color m_BeaconColor = new Color(0f, 1f, 0.45f, 0.85f);
        [SerializeField] private float m_PulseSpeed = 3.2f;
        [SerializeField] private float m_Radius = 4.0f;

        [Header("Visual Elements")]
        [SerializeField] private Transform m_CylinderBeam;
        [SerializeField] private Transform m_GroundRing;
        [SerializeField] private Transform m_FloatingIcon;
        [SerializeField] private Light m_PointLight;
        [SerializeField] private Text m_LabelText;
        [SerializeField] private Transform m_BillboardCanvas;

        public WaypointType Type => m_Type;
        public float Radius => m_Radius;

        private void Awake()
        {
            AutoWireVisuals();
        }

        private void AutoWireVisuals()
        {
            if (m_CylinderBeam == null) m_CylinderBeam = transform.Find("SkyLightBeam");
            if (m_GroundRing == null) m_GroundRing = transform.Find("RingOuter");
            if (m_FloatingIcon == null) m_FloatingIcon = transform.Find("DiamondMarker");
            if (m_PointLight == null) m_PointLight = GetComponentInChildren<Light>(true);

            var canvasTrans = transform.Find("BillboardCanvas");
            if (canvasTrans != null)
            {
                m_BillboardCanvas = canvasTrans;
                if (m_LabelText == null) m_LabelText = canvasTrans.GetComponentInChildren<Text>(true);
            }

            // Ensure collider is a trigger
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        public void SetLabel(string text)
        {
            if (m_LabelText == null) AutoWireVisuals();
            if (m_LabelText != null)
            {
                m_LabelText.text = text;
            }
        }

        public void Setup(WaypointType type, Color color, string label = null)
        {
            m_Type = type;
            m_BeaconColor = color;
            AutoWireVisuals();

            if (m_PointLight != null)
            {
                m_PointLight.color = color;
                m_PointLight.range = 16.0f;
                m_PointLight.intensity = 4.0f;
            }

            // Apply color to materials
            if (m_CylinderBeam != null)
            {
                var rend = m_CylinderBeam.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = rend.material;
                    mat.color = new Color(color.r, color.g, color.b, 0.65f);
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 2.5f);
                }
            }

            if (m_FloatingIcon != null)
            {
                var rend = m_FloatingIcon.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = color;
                    rend.material.EnableKeyword("_EMISSION");
                    rend.material.SetColor("_EmissionColor", color * 2.0f);
                }
            }

            if (!string.IsNullOrEmpty(label))
            {
                SetLabel(label);
            }
        }

        private void Update()
        {
            // 1. Rotate billboard to always face main camera
            var cam = Camera.main;
            if (m_BillboardCanvas != null && cam != null)
            {
                m_BillboardCanvas.rotation = cam.transform.rotation;
            }

            // 2. Pulsing ground ring scale
            if (m_GroundRing != null)
            {
                float ringScale = 1.0f + Mathf.Sin(Time.time * m_PulseSpeed) * 0.22f;
                m_GroundRing.localScale = new Vector3(ringScale, 0.05f, ringScale);
            }

            // 3. Rotating & bobbing floating diamond
            if (m_FloatingIcon != null)
            {
                m_FloatingIcon.Rotate(0f, 50f * Time.deltaTime, 0f, Space.Self);
                float bobY = 2.4f + Mathf.Sin(Time.time * 2.8f) * 0.28f;
                m_FloatingIcon.localPosition = new Vector3(0f, bobY, 0f);
            }

            // 4. Slow rotation on towering sky beam
            if (m_CylinderBeam != null)
            {
                m_CylinderBeam.Rotate(0f, -25f * Time.deltaTime, 0f, Space.Self);
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
