using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// High-Visibility Circular Radar Mini-Map for Grab Campus ("Chuyến xe tâm linh").
    /// Displays:
    /// 1. Rotating player vehicle heading arrow.
    /// 2. High-contrast Shop Pickup Badge (Cyan/Gold with bold label "QUÁN" + distance).
    /// 3. High-contrast Customer Dropoff Badge (Neon Green/Emerald with bold label "KTX" + distance).
    /// 4. Edge-clamping with directional pointer arrows when targets are outside the radar radius.
    /// 5. Ghost Threat Blips (Crimson pulsing hazard diamonds).
    /// 6. Realtime route guide text at the bottom.
    /// </summary>
    public class IUHMinimapUI : MonoBehaviour
    {
        [Header("Radar Settings")]
        [SerializeField] private float m_RadarWorldRadius = 140f;
        [SerializeField] private RectTransform m_RadarDisk;
        [SerializeField] private RectTransform m_PlayerIcon;
        [SerializeField] private RectTransform m_ObjectiveBlip; // Dropoff Destination (Green KTX)
        [SerializeField] private RectTransform m_PickupBlip;    // Shop Pickup (Cyan QUÁN)
        [SerializeField] private RectTransform m_BlipContainer;
        [SerializeField] private Text m_TxtRouteInfo;

        [Header("Ghost Tracking")]
        [SerializeField] private GameObject m_GhostBlipPrefab;
        private List<RectTransform> m_SpawnedGhostBlips = new List<RectTransform>();

        private Transform m_TargetTransform;

        // Cached Blip sub-elements
        private Text m_TxtPickupLabel;
        private Text m_TxtPickupDist;
        private Image m_ImgPickupBg;
        private RectTransform m_PickupArrow;

        private Text m_TxtDropoffLabel;
        private Text m_TxtDropoffDist;
        private Image m_ImgDropoffBg;
        private RectTransform m_DropoffArrow;

        public void Setup(RectTransform radarDisk, RectTransform playerIcon, RectTransform objectiveBlip, RectTransform blipContainer)
        {
            m_RadarDisk = radarDisk;
            m_PlayerIcon = playerIcon;
            m_ObjectiveBlip = objectiveBlip;
            m_BlipContainer = blipContainer;
            AutoWireReferences();
            CreateGhostBlipPool(8);
        }

        private void Awake()
        {
            AutoWireReferences();
        }

        private void Start()
        {
            AutoWireReferences();
            FindPlayerOrVehicle();
            CreateGhostBlipPool(8);
        }

        private void AutoWireReferences()
        {
            if (m_RadarDisk == null) m_RadarDisk = GetComponent<RectTransform>();
            if (m_PlayerIcon == null) m_PlayerIcon = transform.Find("PlayerArrow")?.GetComponent<RectTransform>();
            if (m_BlipContainer == null) m_BlipContainer = m_RadarDisk;

            // Route info label
            if (m_TxtRouteInfo == null) m_TxtRouteInfo = transform.Find("RouteInfoBadge/TxtRouteInfo")?.GetComponent<Text>();
            if (m_TxtRouteInfo == null) m_TxtRouteInfo = transform.Find("TxtRouteInfo")?.GetComponent<Text>();
            if (m_TxtRouteInfo == null && m_RadarDisk != null)
            {
                var infoGO = new GameObject("RouteInfoBadge");
                infoGO.transform.SetParent(transform.parent != null ? transform.parent : transform, false);
                var infoRt = infoGO.AddComponent<RectTransform>();
                infoRt.anchorMin = new Vector2(0f, 0f);
                infoRt.anchorMax = new Vector2(0f, 0f);
                infoRt.pivot = new Vector2(0f, 0f);
                infoRt.anchoredPosition = new Vector2(15f, 15f);
                infoRt.sizeDelta = new Vector2(260f, 44f);

                var bgImg = infoGO.AddComponent<Image>();
                bgImg.color = new Color(0.04f, 0.08f, 0.14f, 0.88f);
                var outline = infoGO.AddComponent<Outline>();
                outline.effectColor = new Color(0.06f, 0.72f, 0.82f, 0.8f);

                var txtGO = new GameObject("TxtRouteInfo");
                txtGO.transform.SetParent(infoGO.transform, false);
                m_TxtRouteInfo = txtGO.AddComponent<Text>();
                m_TxtRouteInfo.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                m_TxtRouteInfo.fontSize = 11;
                m_TxtRouteInfo.fontStyle = FontStyle.Bold;
                m_TxtRouteInfo.alignment = TextAnchor.MiddleLeft;
                m_TxtRouteInfo.color = Color.white;
                var txtRt = txtGO.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = new Vector2(8f, 2f);
                txtRt.offsetMax = new Vector2(-8f, -2f);
            }

            // High-Visibility Pickup Blip (Cyan / Gold)
            BuildOrStylePickupBlip();

            // High-Visibility Dropoff Blip (Neon Green / Emerald)
            BuildOrStyleDropoffBlip();
        }

        private void BuildOrStylePickupBlip()
        {
            if (m_BlipContainer == null) return;
            var existing = m_BlipContainer.Find("PickupBlip");
            GameObject blipGO = existing != null ? existing.gameObject : new GameObject("PickupBlip");
            blipGO.transform.SetParent(m_BlipContainer, false);

            m_PickupBlip = blipGO.GetComponent<RectTransform>();
            if (m_PickupBlip == null) m_PickupBlip = blipGO.AddComponent<RectTransform>();
            m_PickupBlip.sizeDelta = new Vector2(26f, 26f);

            var oldText = blipGO.GetComponent<Text>();
            if (oldText != null) DestroyImmediate(oldText);

            m_ImgPickupBg = blipGO.GetComponent<Image>();
            if (m_ImgPickupBg == null) m_ImgPickupBg = blipGO.AddComponent<Image>();
            m_ImgPickupBg.color = new Color(0.06f, 0.75f, 0.95f, 0.95f); // Vibrant cyan

            var outline = blipGO.GetComponent<Outline>();
            if (outline == null) outline = blipGO.AddComponent<Outline>();
            outline.effectColor = new Color(1.0f, 0.85f, 0.1f, 1.0f); // Gold border
            outline.effectDistance = new Vector2(2f, -2f);

            // Centered Text Label ("QUÁN")
            var lblTrans = blipGO.transform.Find("LblText");
            GameObject lblGO = lblTrans != null ? lblTrans.gameObject : new GameObject("LblText");
            lblGO.transform.SetParent(blipGO.transform, false);
            m_TxtPickupLabel = lblGO.GetComponent<Text>();
            if (m_TxtPickupLabel == null) m_TxtPickupLabel = lblGO.AddComponent<Text>();
            m_TxtPickupLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtPickupLabel.text = "QUÁN";
            m_TxtPickupLabel.fontSize = 9;
            m_TxtPickupLabel.fontStyle = FontStyle.Bold;
            m_TxtPickupLabel.alignment = TextAnchor.MiddleCenter;
            m_TxtPickupLabel.color = Color.white;
            var lRt = lblGO.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.sizeDelta = Vector2.zero;

            // Distance Tag underneath
            var distTrans = blipGO.transform.Find("DistTag");
            GameObject distGO = distTrans != null ? distTrans.gameObject : new GameObject("DistTag");
            distGO.transform.SetParent(blipGO.transform, false);
            m_TxtPickupDist = distGO.GetComponent<Text>();
            if (m_TxtPickupDist == null) m_TxtPickupDist = distGO.AddComponent<Text>();
            m_TxtPickupDist.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtPickupDist.fontSize = 9;
            m_TxtPickupDist.fontStyle = FontStyle.Bold;
            m_TxtPickupDist.alignment = TextAnchor.MiddleCenter;
            m_TxtPickupDist.color = new Color(1f, 0.9f, 0.3f);
            var dRt = distGO.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0f);
            dRt.anchorMax = new Vector2(0.5f, 0f);
            dRt.pivot = new Vector2(0.5f, 1f);
            dRt.anchoredPosition = new Vector2(0f, -2f);
            dRt.sizeDelta = new Vector2(40f, 14f);

            blipGO.SetActive(false);
        }

        private void BuildOrStyleDropoffBlip()
        {
            if (m_BlipContainer == null) return;
            var existing = m_BlipContainer.Find("ObjectiveBlip");
            GameObject blipGO = existing != null ? existing.gameObject : new GameObject("ObjectiveBlip");
            blipGO.transform.SetParent(m_BlipContainer, false);

            m_ObjectiveBlip = blipGO.GetComponent<RectTransform>();
            if (m_ObjectiveBlip == null) m_ObjectiveBlip = blipGO.AddComponent<RectTransform>();
            m_ObjectiveBlip.sizeDelta = new Vector2(26f, 26f);

            var oldText = blipGO.GetComponent<Text>();
            if (oldText != null) DestroyImmediate(oldText);

            m_ImgDropoffBg = blipGO.GetComponent<Image>();
            if (m_ImgDropoffBg == null) m_ImgDropoffBg = blipGO.AddComponent<Image>();
            m_ImgDropoffBg.color = new Color(0.12f, 0.85f, 0.35f, 0.95f); // Vibrant neon green

            var outline = blipGO.GetComponent<Outline>();
            if (outline == null) outline = blipGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.02f, 0.40f, 0.15f, 1.0f); // Dark emerald border
            outline.effectDistance = new Vector2(2f, -2f);

            // Centered Text Label ("KTX")
            var lblTrans = blipGO.transform.Find("LblText");
            GameObject lblGO = lblTrans != null ? lblTrans.gameObject : new GameObject("LblText");
            lblGO.transform.SetParent(blipGO.transform, false);
            m_TxtDropoffLabel = lblGO.GetComponent<Text>();
            if (m_TxtDropoffLabel == null) m_TxtDropoffLabel = lblGO.AddComponent<Text>();
            m_TxtDropoffLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtDropoffLabel.text = "KTX";
            m_TxtDropoffLabel.fontSize = 9;
            m_TxtDropoffLabel.fontStyle = FontStyle.Bold;
            m_TxtDropoffLabel.alignment = TextAnchor.MiddleCenter;
            m_TxtDropoffLabel.color = Color.white;
            var lRt = lblGO.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.sizeDelta = Vector2.zero;

            // Distance Tag underneath
            var distTrans = blipGO.transform.Find("DistTag");
            GameObject distGO = distTrans != null ? distTrans.gameObject : new GameObject("DistTag");
            distGO.transform.SetParent(blipGO.transform, false);
            m_TxtDropoffDist = distGO.GetComponent<Text>();
            if (m_TxtDropoffDist == null) m_TxtDropoffDist = distGO.AddComponent<Text>();
            m_TxtDropoffDist.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtDropoffDist.fontSize = 9;
            m_TxtDropoffDist.fontStyle = FontStyle.Bold;
            m_TxtDropoffDist.alignment = TextAnchor.MiddleCenter;
            m_TxtDropoffDist.color = new Color(0.4f, 1f, 0.6f);
            var dRt = distGO.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0f);
            dRt.anchorMax = new Vector2(0.5f, 0f);
            dRt.pivot = new Vector2(0.5f, 1f);
            dRt.anchoredPosition = new Vector2(0f, -2f);
            dRt.sizeDelta = new Vector2(40f, 14f);

            blipGO.SetActive(false);
        }

        private void FindPlayerOrVehicle()
        {
            if (IUHGrabCampusManager.Instance != null && IUHGrabCampusManager.Instance.ActiveTargetTransform != null)
            {
                m_TargetTransform = IUHGrabCampusManager.Instance.ActiveTargetTransform;
                return;
            }

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

            while (m_SpawnedGhostBlips.Count < count)
            {
                int i = m_SpawnedGhostBlips.Count;
                var blipGO = new GameObject($"GhostBlip_{i}");
                blipGO.transform.SetParent(m_BlipContainer, false);
                var rect = blipGO.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(14f, 14f);
                rect.localRotation = Quaternion.Euler(0f, 0f, 45f); // Diamond shape

                var img = blipGO.AddComponent<Image>();
                img.color = new Color(0.95f, 0.15f, 0.25f, 0.95f);

                var outl = blipGO.AddComponent<Outline>();
                outl.effectColor = new Color(0.4f, 0.0f, 0.05f, 0.9f);
                outl.effectDistance = new Vector2(1.5f, -1.5f);

                blipGO.SetActive(false);
                m_SpawnedGhostBlips.Add(rect);
            }
        }

        private void Update()
        {
            FindPlayerOrVehicle();
            if (m_TargetTransform == null || m_RadarDisk == null) return;

            float radarUiRadius = m_RadarDisk.rect.width * 0.44f;

            // 1. Rotate Player Arrow
            if (m_PlayerIcon != null)
            {
                float heading = m_TargetTransform.eulerAngles.y;
                m_PlayerIcon.localRotation = Quaternion.Euler(0f, 0f, -heading);
            }

            // 2. Position Objective Blips (Pickup & Dropoff)
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
            Vector3 playerPos = m_TargetTransform.position;

            float distPickup = Vector3.Distance(new Vector3(playerPos.x, 0f, playerPos.z), new Vector3(order.pickupPosition.x, 0f, order.pickupPosition.z));
            float distDropoff = Vector3.Distance(new Vector3(playerPos.x, 0f, playerPos.z), new Vector3(order.dropoffPosition.x, 0f, order.dropoffPosition.z));

            // Update Distance text
            if (m_TxtPickupDist != null) m_TxtPickupDist.text = $"{distPickup:F0}m";
            if (m_TxtDropoffDist != null) m_TxtDropoffDist.text = $"{distDropoff:F0}m";

            // 1. Position Pickup Blip (Tiệm Trà Sữa)
            if (m_PickupBlip != null)
            {
                if (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.PickingUp)
                {
                    PositionBlip(m_PickupBlip, order.pickupPosition, radarUiRadius);
                    m_PickupBlip.gameObject.SetActive(true);
                    float pulse = 1.0f + Mathf.Sin(Time.time * 6f) * 0.20f;
                    m_PickupBlip.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    m_PickupBlip.gameObject.SetActive(false);
                }
            }

            // 2. Position Dropoff Blip (Bạn Sinh Viên KTX)
            if (m_ObjectiveBlip != null)
            {
                PositionBlip(m_ObjectiveBlip, order.dropoffPosition, radarUiRadius);
                m_ObjectiveBlip.gameObject.SetActive(true);

                if (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.Delivering)
                {
                    float pulse = 1.10f + Mathf.Sin(Time.time * 7f) * 0.25f;
                    m_ObjectiveBlip.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    // While picking up, keep dropoff slightly smaller so pickup is primary
                    m_ObjectiveBlip.localScale = Vector3.one * 0.85f;
                }
            }

            // 3. Dynamic Route Info Strip
            if (m_TxtRouteInfo != null)
            {
                m_TxtRouteInfo.gameObject.SetActive(true);
                if (grabMgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.PickingUp)
                {
                    m_TxtRouteInfo.text = $"<color=#38BDF8><b>[1/2] LẤY HÀNG:</b> {order.pickupName}</color> ({distPickup:F0}m)\n➔ 🏁 <color=#4ADE80>Giao đến: {order.dropoffName}</color> ({distDropoff:F0}m)";
                }
                else
                {
                    m_TxtRouteInfo.text = $"<color=#4ADE80><b>[2/2] GIAO HÀNG:</b> {order.dropoffName}</color> ({distDropoff:F0}m)\n📦 <color=#FDE047>Kiện hàng: Còn nguyên vẹn ({grabMgr.CargoIntegrity}%)</color>";
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

            // Clamp smoothly to radar radius
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

                    // Pulse threat color
                    var img = blip.GetComponent<Image>();
                    if (img != null)
                    {
                        float a = 0.6f + Mathf.PingPong(Time.time * 5f, 0.4f);
                        img.color = new Color(1f, 0.15f, 0.25f, a);
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
