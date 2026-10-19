using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// Circular Radar Mini-Map (Bản đồ nhỏ) for Grab Campus Gameplay.
    /// Displays campus grid radar, rotating player heading arrow,
    /// dynamic delivery destination blip, and spooky ghost threat blips.
    /// Exactly mirrors the radar shown in Image 1 (panel 2).
    /// </summary>
    public class IUHMinimapUI : MonoBehaviour
    {
        [Header("Radar Settings")]
        [SerializeField] private float m_RadarWorldRadius = 140f; // Max world meters shown in radar
        [SerializeField] private RectTransform m_RadarDisk;
        [SerializeField] private RectTransform m_PlayerIcon;
        [SerializeField] private RectTransform m_ObjectiveBlip; // Dropoff Destination Blip (Green 🏁)
        [SerializeField] private RectTransform m_PickupBlip;    // Shop Pickup Blip (Cyan/Gold 🧋)
        [SerializeField] private RectTransform m_BlipContainer;
        [SerializeField] private Text m_TxtRouteInfo;

        [Header("Ghost Tracking")]
        [SerializeField] private GameObject m_GhostBlipPrefab;
        private List<RectTransform> m_SpawnedGhostBlips = new List<RectTransform>();

        private Transform m_TargetTransform;

        public void Setup(RectTransform radarDisk, RectTransform playerIcon, RectTransform objectiveBlip, RectTransform blipContainer)
        {
            m_RadarDisk = radarDisk;
            m_PlayerIcon = playerIcon;
            m_ObjectiveBlip = objectiveBlip;
            m_BlipContainer = blipContainer;
            AutoWireReferences();
            CreateGhostBlipPool(6);
        }

        private void Awake()
        {
            AutoWireReferences();
        }

        private void AutoWireReferences()
        {
            if (m_RadarDisk == null) m_RadarDisk = GetComponent<RectTransform>();
            if (m_PlayerIcon == null) m_PlayerIcon = transform.Find("PlayerArrow")?.GetComponent<RectTransform>();
            if (m_ObjectiveBlip == null) m_ObjectiveBlip = transform.Find("ObjectiveBlip")?.GetComponent<RectTransform>();
            if (m_PickupBlip == null) m_PickupBlip = transform.Find("PickupBlip")?.GetComponent<RectTransform>();
            if (m_TxtRouteInfo == null) m_TxtRouteInfo = transform.Find("RouteInfoBadge/TxtRouteInfo")?.GetComponent<Text>();
            if (m_TxtRouteInfo == null) m_TxtRouteInfo = transform.Find("TxtRouteInfo")?.GetComponent<Text>();
            if (m_BlipContainer == null) m_BlipContainer = m_RadarDisk;

            // Auto-create PickupBlip if missing
            if (m_PickupBlip == null && m_BlipContainer != null)
            {
                var pBlipGO = new GameObject("PickupBlip");
                pBlipGO.transform.SetParent(m_BlipContainer, false);
                m_PickupBlip = pBlipGO.AddComponent<RectTransform>();
                m_PickupBlip.sizeDelta = new Vector2(18f, 18f);
                var pTxt = pBlipGO.AddComponent<Text>();
                pTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                pTxt.text = "<color=#38BDF8>🧋</color>";
                pTxt.fontSize = 18;
                pTxt.alignment = TextAnchor.MiddleCenter;
                pBlipGO.SetActive(false);
            }

            // Style ObjectiveBlip as destination flag
            if (m_ObjectiveBlip != null)
            {
                var oTxt = m_ObjectiveBlip.GetComponent<Text>();
                if (oTxt != null) oTxt.text = "<color=#22C55E>🏁</color>";
            }
        }

        private void Start()
        {
            AutoWireReferences();
            FindPlayerOrVehicle();
            CreateGhostBlipPool(6);
        }

        private void FindPlayerOrVehicle()
        {
            var bike = FindFirstObjectByType<IUHGrabMotorbike>();
            if (bike != null)
            {
                m_TargetTransform = bike.transform;
                return;
            }

            var player = GameObject.Find("PlayerArmature");
            if (player != null) m_TargetTransform = player.transform;
            else
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) m_TargetTransform = p.transform;
            }
        }

        private void CreateGhostBlipPool(int count)
        {
            if (m_BlipContainer == null) m_BlipContainer = m_RadarDisk;
            if (m_BlipContainer == null) return;

            for (int i = 0; i < count; i++)
            {
                var blipGO = new GameObject($"GhostBlip_{i}");
                blipGO.transform.SetParent(m_BlipContainer, false);
                var rect = blipGO.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(10f, 10f);

                var img = blipGO.AddComponent<Image>();
                img.color = new Color(1.0f, 0.2f, 0.25f, 0.95f);

                blipGO.SetActive(false);
                m_SpawnedGhostBlips.Add(rect);
            }
        }

        private void Update()
        {
            FindPlayerOrVehicle();
            if (m_TargetTransform == null || m_RadarDisk == null) return;

            float radarUiRadius = m_RadarDisk.rect.width * 0.45f;

            // 1. Rotate Player Arrow
            if (m_PlayerIcon != null)
            {
                float heading = m_TargetTransform.eulerAngles.y;
                m_PlayerIcon.localRotation = Quaternion.Euler(0f, 0f, -heading);
            }

            // 2. Position Objective Blip
            UpdateObjectiveBlip(radarUiRadius);

            // 3. Position Ghost Threat Blips
            UpdateGhostBlips(radarUiRadius);
        }

        private void UpdateObjectiveBlip(float radarUiRadius)
        {
            var grabMgr = IUHGrabCampusManager.Instance;
            if (grabMgr == null || !grabMgr.IsMissionActive || grabMgr.CurrentOrder == null)
            {
                if (m_ObjectiveBlip != null) m_ObjectiveBlip.gameObject.SetActive(false);
                if (m_PickupBlip != null) m_PickupBlip.gameObject.SetActive(false);
                if (m_TxtRouteInfo != null) m_TxtRouteInfo.gameObject.SetActive(false);
                return;
            }

            var order = grabMgr.CurrentOrder;

            // 1. Position Pickup Blip (Shop: Boba / Fishballs / Canteen)
            if (m_PickupBlip != null)
            {
                if (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.PickingUp)
                {
                    PositionBlip(m_PickupBlip, order.pickupPosition, radarUiRadius);
                    m_PickupBlip.gameObject.SetActive(true);
                    float pulse = 1.0f + Mathf.Sin(Time.time * 6f) * 0.25f;
                    m_PickupBlip.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    m_PickupBlip.gameObject.SetActive(false);
                }
            }

            // 2. Position Dropoff Blip (Destination: KTX / Lecture Hall - ALWAYS visible during mission!)
            if (m_ObjectiveBlip != null)
            {
                PositionBlip(m_ObjectiveBlip, order.dropoffPosition, radarUiRadius);
                m_ObjectiveBlip.gameObject.SetActive(true);

                if (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.Delivering)
                {
                    float pulse = 1.15f + Mathf.Sin(Time.time * 7f) * 0.30f;
                    m_ObjectiveBlip.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    m_ObjectiveBlip.localScale = Vector3.one;
                }
            }

            // 3. Dynamic Route Info Strip
            if (m_TxtRouteInfo != null)
            {
                m_TxtRouteInfo.gameObject.SetActive(true);
                Vector3 currentPos = m_TargetTransform.position;
                Vector3 destPos = (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.PickingUp) ? order.pickupPosition : order.dropoffPosition;
                float d = Vector3.Distance(new Vector3(currentPos.x, 0f, currentPos.z), new Vector3(destPos.x, 0f, destPos.z));

                if (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.PickingUp)
                {
                    m_TxtRouteInfo.text = $"🧋 <color=#38BDF8><b>LẤY ĐỒ:</b> {order.pickupName}</color> ({d:F0}m)\n➔ 🏁 <color=#4ADE80>Giao đến: {order.dropoffName}</color>";
                }
                else
                {
                    m_TxtRouteInfo.text = $"🏁 <color=#4ADE80><b>GIAO ĐẾN:</b> {order.dropoffName}</color> ({d:F0}m)\n📦 <color=#FDE047>Kiện hàng: Còn nguyên vẹn</color>";
                }
            }
        }

        private void PositionBlip(RectTransform blip, Vector3 worldPos, float radarUiRadius)
        {
            Vector3 diff = worldPos - m_TargetTransform.position;
            Vector2 mapOffset = new Vector2(diff.x, diff.z);
            float worldDist = mapOffset.magnitude;
            float ratio = worldDist / m_RadarWorldRadius;
            Vector2 clampedDir = mapOffset.sqrMagnitude > 0.001f ? mapOffset.normalized : Vector2.zero;
            float displayDist = Mathf.Clamp(ratio, 0f, 1f) * radarUiRadius;
            blip.anchoredPosition = clampedDir * displayDist;
        }

        private void UpdateGhostBlips(float radarUiRadius)
        {
            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            int blipIndex = 0;

            for (int i = 0; i < ghosts.Length && blipIndex < m_SpawnedGhostBlips.Count; i++)
            {
                var g = ghosts[i];
                if (g == null || !g.gameObject.activeInHierarchy) continue;

                Vector3 diff = g.transform.position - m_TargetTransform.position;
                Vector2 mapOffset = new Vector2(diff.x, diff.z);
                float dist = mapOffset.magnitude;

                if (dist <= m_RadarWorldRadius)
                {
                    float ratio = dist / m_RadarWorldRadius;
                    var blip = m_SpawnedGhostBlips[blipIndex];
                    blip.anchoredPosition = mapOffset.normalized * (ratio * radarUiRadius);
                    blip.gameObject.SetActive(true);

                    // Blink alpha
                    var img = blip.GetComponent<Image>();
                    if (img != null)
                    {
                        float a = 0.5f + Mathf.PingPong(Time.time * 4f, 0.5f);
                        img.color = new Color(1f, 0.2f, 0.2f, a);
                    }

                    blipIndex++;
                }
            }

            // Hide unused blips
            for (int j = blipIndex; j < m_SpawnedGhostBlips.Count; j++)
            {
                m_SpawnedGhostBlips[j].gameObject.SetActive(false);
            }
        }
    }
}
