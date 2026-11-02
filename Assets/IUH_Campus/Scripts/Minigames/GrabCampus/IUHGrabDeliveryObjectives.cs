using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{

    public class IUHGrabDeliveryObjectives : MonoBehaviour
    {
        private static IUHGrabDeliveryObjectives s_Instance;
        public static IUHGrabDeliveryObjectives Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = UnityEngine.Object.FindAnyObjectByType<IUHGrabDeliveryObjectives>(FindObjectsInactive.Include);
                    if (s_Instance == null)
                    {
                        var go = new GameObject("IUH_Grab_Delivery_Objectives");
                        s_Instance = go.AddComponent<IUHGrabDeliveryObjectives>();
                    }
                }
                return s_Instance;
            }
        }

        private GameObject m_StallRoot;
        private Transform m_StallFoodItemsRoot;
        private GameObject m_StallPromptCanvas;
        private Text m_TxtStallPrompt;
        private Light m_StallLantern;
        private Transform m_ShopkeeperRoot;

        private GameObject m_CustomerRoot;
        private Transform m_CustomerBody;
        private Transform m_CustomerRightArm;
        private GameObject m_CustomerPromptCanvas;
        private Text m_TxtCustomerPrompt;
        private Light m_CustomerSpotlight;

        private IUHGrabCampusManager.DeliveryOrder m_CurrentOrder;
        private bool m_IsCargoCollected = false;

        public bool IsCargoCollected => m_IsCargoCollected;

        private void Awake()
        {
            if (s_Instance == null) s_Instance = this;
            else if (s_Instance != this) { Destroy(gameObject); return; }
        }

        private void Update()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr == null || !mgr.IsMissionActive) return;

            var cam = Camera.main;

            if (m_StallPromptCanvas != null && m_StallPromptCanvas.activeSelf && cam != null)
            {
                m_StallPromptCanvas.transform.rotation = cam.transform.rotation;
            }

            if (m_CustomerPromptCanvas != null && m_CustomerPromptCanvas.activeSelf && cam != null)
            {
                m_CustomerPromptCanvas.transform.rotation = cam.transform.rotation;
            }

            if (m_CustomerBody != null)
            {
                float breathe = Mathf.Sin(Time.time * 2.2f) * 0.03f;
                m_CustomerBody.localPosition = new Vector3(0f, 0.95f + breathe, 0f);
            }

            if (m_CustomerRightArm != null)
            {

                float wave = Mathf.Sin(Time.time * 6.5f) * 32f;
                m_CustomerRightArm.localRotation = Quaternion.Euler(-135f + wave, 0f, 25f);
            }

            CheckProximityInteractions();
        }

        private void CheckProximityInteractions()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr == null || !mgr.IsMissionActive) return;

            var target = mgr.ActiveTargetTransform;
            if (target == null) return;

            bool interactPressed = IUHInputHelper.IsInteractKeyDown() || Input.GetKeyDown(KeyCode.E);
            float bikeSpeed = (mgr.Motorbike != null) ? mgr.Motorbike.CurrentSpeedKmH : 0f;

            if (mgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.PickingUp && !m_IsCargoCollected && m_StallRoot != null)
            {
                float distToStall = Vector3.Distance(target.position, m_StallRoot.transform.position);
                if (distToStall < 6.0f)
                {
                    if (m_TxtStallPrompt != null)
                    {
                        m_TxtStallPrompt.text = $"<size=22><color=#FEF08A><b>🧋 {m_CurrentOrder?.pickupName}</b></color></size>\n<size=19><color=#38BDF8><b>[ DỪNG XE & BẤM [E] ĐỂ NHẬN HÀNG ]</b></color></size>";
                    }

                    if (interactPressed || (distToStall < 3.2f && bikeSpeed < 1.5f))
                    {
                        CollectCargoAtStall();
                    }
                }
            }

            else if (mgr.CurrentPhase == IUHGrabCampusManager.MissionPhase.Delivering && m_CustomerRoot != null)
            {
                float distToCustomer = Vector3.Distance(target.position, m_CustomerRoot.transform.position);
                if (distToCustomer < 6.0f)
                {
                    if (m_TxtCustomerPrompt != null)
                    {
                        m_TxtCustomerPrompt.text = $"<size=22><color=#4ADE80><b>👋 {m_CurrentOrder?.dropoffName}</b></color></size>\n<size=19><color=#FEF08A><b>[ DỪNG XE & BẤM [E] ĐỂ GIAO HÀNG ]</b></color></size>";
                    }

                    if (interactPressed || (distToCustomer < 3.2f && bikeSpeed < 1.5f))
                    {
                        DeliverCargoToCustomer();
                    }
                }
            }
        }

        public void SetupForOrder(IUHGrabCampusManager.DeliveryOrder order)
        {
            m_CurrentOrder = order;
            m_IsCargoCollected = false;

            if (order == null) return;

            BuildOrPositionStall(order.pickupPosition, order.pickupName);
            BuildOrPositionCustomer(order.dropoffPosition, order.dropoffName);

            var bikeCargo = IUHGrabCargoVisual.Instance;
            if (bikeCargo != null) bikeCargo.SetEmpty();
        }

        private void BuildOrPositionStall(Vector3 pos, string stallName)
        {
            if (m_StallRoot == null)
            {
                m_StallRoot = new GameObject("Physical_Food_Stall_Pickup");
                m_StallRoot.transform.SetParent(transform, false);

                var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.name = "Counter_Table";
                table.transform.SetParent(m_StallRoot.transform, false);
                table.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                table.transform.localScale = new Vector3(2.6f, 1.1f, 1.2f);
                if (table.GetComponent<Collider>() != null) Destroy(table.GetComponent<Collider>());
                var tableMat = new Material(Shader.Find("Standard"));
                tableMat.color = new Color(0.38f, 0.24f, 0.12f);
                table.GetComponent<Renderer>().material = tableMat;

                var canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                canopy.name = "Stall_Canopy_Awning";
                canopy.transform.SetParent(m_StallRoot.transform, false);
                canopy.transform.localPosition = new Vector3(0f, 2.3f, 0.1f);
                canopy.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
                canopy.transform.localScale = new Vector3(2.8f, 0.14f, 1.6f);
                if (canopy.GetComponent<Collider>() != null) Destroy(canopy.GetComponent<Collider>());
                var canopyMat = new Material(Shader.Find("Standard"));
                canopyMat.color = new Color(0.92f, 0.22f, 0.18f);
                canopy.GetComponent<Renderer>().material = canopyMat;

                var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sign.name = "Stall_Sign_ToCoToCo";
                sign.transform.SetParent(m_StallRoot.transform, false);
                sign.transform.localPosition = new Vector3(0f, 2.65f, 0.2f);
                sign.transform.localScale = new Vector3(2.4f, 0.45f, 0.08f);
                if (sign.GetComponent<Collider>() != null) Destroy(sign.GetComponent<Collider>());
                var signMat = new Material(Shader.Find("Standard"));
                signMat.color = new Color(0.98f, 0.85f, 0.25f);
                signMat.EnableKeyword("_EMISSION");
                signMat.SetColor("_EmissionColor", new Color(0.98f, 0.85f, 0.25f) * 1.5f);
                sign.GetComponent<Renderer>().material = signMat;

                CreateStallPole(m_StallRoot.transform, new Vector3(-1.25f, 1.4f, -0.55f));
                CreateStallPole(m_StallRoot.transform, new Vector3(1.25f, 1.4f, -0.55f));
                CreateStallPole(m_StallRoot.transform, new Vector3(-1.25f, 1.6f, 0.55f));
                CreateStallPole(m_StallRoot.transform, new Vector3(1.25f, 1.6f, 0.55f));

                BuildShopkeeperNPC(m_StallRoot.transform, new Vector3(0f, 0f, 0.65f));

                var lightGo = new GameObject("LanternLight");
                lightGo.transform.SetParent(m_StallRoot.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 2.1f, 0f);
                m_StallLantern = lightGo.AddComponent<Light>();
                m_StallLantern.type = LightType.Point;
                m_StallLantern.color = new Color(1.0f, 0.85f, 0.45f);
                m_StallLantern.range = 8.5f;
                m_StallLantern.intensity = 3.5f;

                var foodGo = new GameObject("Stall_Food_Items");
                foodGo.transform.SetParent(m_StallRoot.transform, false);
                foodGo.transform.localPosition = new Vector3(0f, 1.15f, -0.1f);
                m_StallFoodItemsRoot = foodGo.transform;

                BuildBubbleTeaStallItem(m_StallFoodItemsRoot);

                BuildStallPromptCanvas();
            }

            Vector3 groundPos = IUHGrabCampusManager.GetGroundPosition(pos);
            m_StallRoot.transform.position = groundPos;
            m_StallRoot.SetActive(true);

            if (m_StallFoodItemsRoot != null) m_StallFoodItemsRoot.gameObject.SetActive(true);

            if (m_TxtStallPrompt != null)
            {
                m_TxtStallPrompt.text = $"<size=22><color=#FEF08A><b>🧋 {stallName}</b></color></size>\n<size=19><color=#38BDF8><b>[ DỪNG XE & BẤM [E] ĐỂ NHẬN HÀNG ]</b></color></size>";
            }
            if (m_StallPromptCanvas != null) m_StallPromptCanvas.SetActive(true);
        }

        private void BuildShopkeeperNPC(Transform parent, Vector3 localPos)
        {
            var sk = new GameObject("Shopkeeper_NPC");
            sk.transform.SetParent(parent, false);
            sk.transform.localPosition = localPos;
            sk.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(sk.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            body.transform.localScale = new Vector3(0.42f, 0.55f, 0.28f);
            if (body.GetComponent<Collider>() != null) Destroy(body.GetComponent<Collider>());
            var bMat = new Material(Shader.Find("Standard"));
            bMat.color = new Color(0.95f, 0.95f, 0.95f);
            body.GetComponent<Renderer>().material = bMat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(sk.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            head.transform.localScale = new Vector3(0.26f, 0.28f, 0.26f);
            if (head.GetComponent<Collider>() != null) Destroy(head.GetComponent<Collider>());
            var hMat = new Material(Shader.Find("Standard"));
            hMat.color = new Color(0.96f, 0.82f, 0.72f);
            head.GetComponent<Renderer>().material = hMat;

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Cap";
            cap.transform.SetParent(sk.transform, false);
            cap.transform.localPosition = new Vector3(0f, 1.70f, 0f);
            cap.transform.localScale = new Vector3(0.28f, 0.08f, 0.28f);
            if (cap.GetComponent<Collider>() != null) Destroy(cap.GetComponent<Collider>());
            var cMat = new Material(Shader.Find("Standard"));
            cMat.color = new Color(0.92f, 0.22f, 0.18f);
            cap.GetComponent<Renderer>().material = cMat;

            m_ShopkeeperRoot = sk.transform;
        }

        private void CreateStallPole(Transform parent, Vector3 localPos)
        {
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "StallPole";
            pole.transform.SetParent(parent, false);
            pole.transform.localPosition = localPos;
            pole.transform.localScale = new Vector3(0.06f, 1.1f, 0.06f);
            if (pole.GetComponent<Collider>() != null) Destroy(pole.GetComponent<Collider>());
            var poleMat = new Material(Shader.Find("Standard"));
            poleMat.color = new Color(0.2f, 0.15f, 0.1f);
            pole.GetComponent<Renderer>().material = poleMat;
        }

        private void BuildBubbleTeaStallItem(Transform parent)
        {

            var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "Pickup_Tray";
            tray.transform.SetParent(parent, false);
            tray.transform.localPosition = Vector3.zero;
            tray.transform.localScale = new Vector3(0.6f, 0.04f, 0.35f);
            if (tray.GetComponent<Collider>() != null) Destroy(tray.GetComponent<Collider>());
            var trayMat = new Material(Shader.Find("Standard"));
            trayMat.color = new Color(0.2f, 0.16f, 0.12f);
            tray.GetComponent<Renderer>().material = trayMat;

            CreateDetailedCup(parent, new Vector3(-0.16f, 0.18f, 0f), new Color(0.92f, 0.72f, 0.45f), "ToCoToCo Brown Sugar");

            CreateDetailedCup(parent, new Vector3(0.16f, 0.18f, 0f), new Color(0.45f, 0.75f, 0.45f), "ToCoToCo Matcha Cream");
        }

        private void CreateDetailedCup(Transform parent, Vector3 localPos, Color teaColor, string name)
        {
            var cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cup.name = name;
            cup.transform.SetParent(parent, false);
            cup.transform.localPosition = localPos;
            cup.transform.localScale = new Vector3(0.14f, 0.16f, 0.14f);
            if (cup.GetComponent<Collider>() != null) Destroy(cup.GetComponent<Collider>());
            var cupMat = new Material(Shader.Find("Standard"));
            cupMat.color = teaColor;
            cup.GetComponent<Renderer>().material = cupMat;

            var lid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lid.name = "Lid";
            lid.transform.SetParent(cup.transform, false);
            lid.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            lid.transform.localScale = new Vector3(1.1f, 0.35f, 1.1f);
            if (lid.GetComponent<Collider>() != null) Destroy(lid.GetComponent<Collider>());
            var lidMat = new Material(Shader.Find("Standard"));
            lidMat.color = new Color(0.95f, 0.95f, 0.95f);
            lid.GetComponent<Renderer>().material = lidMat;

            var straw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            straw.name = "Straw";
            straw.transform.SetParent(cup.transform, false);
            straw.transform.localPosition = new Vector3(0.15f, 1.25f, 0f);
            straw.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            straw.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
            if (straw.GetComponent<Collider>() != null) Destroy(straw.GetComponent<Collider>());
            var strawMat = new Material(Shader.Find("Standard"));
            strawMat.color = new Color(0.95f, 0.35f, 0.15f);
            straw.GetComponent<Renderer>().material = strawMat;
        }

        private void BuildStallPromptCanvas()
        {
            m_StallPromptCanvas = new GameObject("StallPromptCanvas");
            m_StallPromptCanvas.transform.SetParent(m_StallRoot.transform, false);
            m_StallPromptCanvas.transform.localPosition = new Vector3(0f, 2.75f, 0f);

            var canvas = m_StallPromptCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320f, 90f);
            m_StallPromptCanvas.transform.localScale = Vector3.one * 0.010f;

            var bgGo = new GameObject("PromptBg");
            bgGo.transform.SetParent(m_StallPromptCanvas.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.12f, 0.08f, 0.92f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            var outline = bgGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.98f, 0.85f, 0.25f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            var txtGo = new GameObject("TxtPrompt");
            txtGo.transform.SetParent(bgGo.transform, false);
            m_TxtStallPrompt = txtGo.AddComponent<Text>();
            m_TxtStallPrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtStallPrompt.fontSize = 20;
            m_TxtStallPrompt.fontStyle = FontStyle.Bold;
            m_TxtStallPrompt.alignment = TextAnchor.MiddleCenter;
            m_TxtStallPrompt.color = Color.white;
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
        }

        private void BuildOrPositionCustomer(Vector3 pos, string dropoffName)
        {
            if (m_CustomerRoot == null)
            {
                m_CustomerRoot = new GameObject("Physical_Customer_Dropoff");
                m_CustomerRoot.transform.SetParent(transform, false);

                var bodyGo = new GameObject("StudentBody");
                bodyGo.transform.SetParent(m_CustomerRoot.transform, false);
                bodyGo.transform.localPosition = new Vector3(0f, 0.95f, 0f);
                m_CustomerBody = bodyGo.transform;

                var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                torso.name = "Student_Torso";
                torso.transform.SetParent(m_CustomerBody, false);
                torso.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                torso.transform.localScale = new Vector3(0.42f, 0.48f, 0.28f);
                if (torso.GetComponent<Collider>() != null) Destroy(torso.GetComponent<Collider>());
                var torsoMat = new Material(Shader.Find("Standard"));
                torsoMat.color = new Color(0.12f, 0.38f, 0.72f);
                torso.GetComponent<Renderer>().material = torsoMat;

                var bag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bag.name = "Student_Backpack";
                bag.transform.SetParent(m_CustomerBody, false);
                bag.transform.localPosition = new Vector3(0f, 0.35f, -0.20f);
                bag.transform.localScale = new Vector3(0.32f, 0.40f, 0.18f);
                if (bag.GetComponent<Collider>() != null) Destroy(bag.GetComponent<Collider>());
                var bagMat = new Material(Shader.Find("Standard"));
                bagMat.color = new Color(0.85f, 0.35f, 0.15f);
                bag.GetComponent<Renderer>().material = bagMat;

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Student_Head";
                head.transform.SetParent(m_CustomerBody, false);
                head.transform.localPosition = new Vector3(0f, 0.88f, 0f);
                head.transform.localScale = new Vector3(0.26f, 0.28f, 0.26f);
                if (head.GetComponent<Collider>() != null) Destroy(head.GetComponent<Collider>());
                var headMat = new Material(Shader.Find("Standard"));
                headMat.color = new Color(0.96f, 0.82f, 0.72f);
                head.GetComponent<Renderer>().material = headMat;

                var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hair.name = "Student_Hair";
                hair.transform.SetParent(m_CustomerBody, false);
                hair.transform.localPosition = new Vector3(0f, 0.98f, -0.02f);
                hair.transform.localScale = new Vector3(0.28f, 0.16f, 0.28f);
                if (hair.GetComponent<Collider>() != null) Destroy(hair.GetComponent<Collider>());
                var hairMat = new Material(Shader.Find("Standard"));
                hairMat.color = new Color(0.1f, 0.08f, 0.06f);
                hair.GetComponent<Renderer>().material = hairMat;

                var rArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                rArm.name = "Student_RightArm";
                rArm.transform.SetParent(m_CustomerBody, false);
                rArm.transform.localPosition = new Vector3(0.26f, 0.55f, 0f);
                rArm.transform.localRotation = Quaternion.Euler(-135f, 0f, 25f);
                rArm.transform.localScale = new Vector3(0.11f, 0.36f, 0.11f);
                if (rArm.GetComponent<Collider>() != null) Destroy(rArm.GetComponent<Collider>());
                var armMat = new Material(Shader.Find("Standard"));
                armMat.color = new Color(0.12f, 0.38f, 0.72f);
                rArm.GetComponent<Renderer>().material = armMat;
                m_CustomerRightArm = rArm.transform;

                var lArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                lArm.name = "Student_LeftArm";
                lArm.transform.SetParent(m_CustomerBody, false);
                lArm.transform.localPosition = new Vector3(-0.26f, 0.30f, 0f);
                lArm.transform.localScale = new Vector3(0.11f, 0.36f, 0.11f);
                if (lArm.GetComponent<Collider>() != null) Destroy(lArm.GetComponent<Collider>());
                lArm.GetComponent<Renderer>().material = armMat;

                CreateCustomerLeg(m_CustomerBody, new Vector3(-0.11f, -0.42f, 0f));
                CreateCustomerLeg(m_CustomerBody, new Vector3(0.11f, -0.42f, 0f));

                var lightGo = new GameObject("CustomerLight");
                lightGo.transform.SetParent(m_CustomerRoot.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 2.8f, 0f);
                m_CustomerSpotlight = lightGo.AddComponent<Light>();
                m_CustomerSpotlight.type = LightType.Spot;
                m_CustomerSpotlight.spotAngle = 65f;
                m_CustomerSpotlight.range = 9.0f;
                m_CustomerSpotlight.intensity = 4.0f;
                m_CustomerSpotlight.color = new Color(0.4f, 0.95f, 1.0f);

                BuildCustomerPromptCanvas();
            }

            Vector3 groundPos = IUHGrabCampusManager.GetGroundPosition(pos);
            m_CustomerRoot.transform.position = groundPos;
            m_CustomerRoot.SetActive(true);

            if (m_TxtCustomerPrompt != null)
            {
                m_TxtCustomerPrompt.text = $"<size=22><color=#4ADE80><b>👋 {dropoffName}</b></color></size>\n<size=19><color=#FEF08A><b>[ DỪNG XE & BẤM [E] ĐỂ GIAO HÀNG ]</b></color></size>";
            }
            if (m_CustomerPromptCanvas != null) m_CustomerPromptCanvas.SetActive(true);
        }

        private void CreateCustomerLeg(Transform parent, Vector3 localPos)
        {
            var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Leg";
            leg.transform.SetParent(parent, false);
            leg.transform.localPosition = localPos;
            leg.transform.localScale = new Vector3(0.12f, 0.42f, 0.12f);
            if (leg.GetComponent<Collider>() != null) Destroy(leg.GetComponent<Collider>());
            var legMat = new Material(Shader.Find("Standard"));
            legMat.color = new Color(0.15f, 0.20f, 0.28f);
            leg.GetComponent<Renderer>().material = legMat;
        }

        private void BuildCustomerPromptCanvas()
        {
            m_CustomerPromptCanvas = new GameObject("CustomerPromptCanvas");
            m_CustomerPromptCanvas.transform.SetParent(m_CustomerRoot.transform, false);
            m_CustomerPromptCanvas.transform.localPosition = new Vector3(0f, 2.45f, 0f);

            var canvas = m_CustomerPromptCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320f, 90f);
            m_CustomerPromptCanvas.transform.localScale = Vector3.one * 0.010f;

            var bgGo = new GameObject("PromptBg");
            bgGo.transform.SetParent(m_CustomerPromptCanvas.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            var outline = bgGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 1.0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            var txtGo = new GameObject("TxtPrompt");
            txtGo.transform.SetParent(bgGo.transform, false);
            m_TxtCustomerPrompt = txtGo.AddComponent<Text>();
            m_TxtCustomerPrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtCustomerPrompt.fontSize = 20;
            m_TxtCustomerPrompt.fontStyle = FontStyle.Bold;
            m_TxtCustomerPrompt.alignment = TextAnchor.MiddleCenter;
            m_TxtCustomerPrompt.color = Color.white;
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
        }

        public void CollectCargoAtStall()
        {
            if (m_IsCargoCollected) return;
            m_IsCargoCollected = true;

            if (m_StallFoodItemsRoot != null) m_StallFoodItemsRoot.gameObject.SetActive(false);

            if (m_TxtStallPrompt != null)
            {
                m_TxtStallPrompt.text = "<size=20><color=#4ADE80><b>✅ ĐÃ LẤY HÀNG THÀNH CÔNG!</b></color></size>";
            }

            string cargoName = m_CurrentOrder != null ? m_CurrentOrder.pickupName : "Trà Sữa ToCoToCo";
            var bikeCargo = IUHGrabCargoVisual.Instance;
            if (bikeCargo != null)
            {
                int integrity = IUHGrabCampusManager.Instance != null ? IUHGrabCampusManager.Instance.CargoIntegrity : 100;
                bikeCargo.SetCargoLoaded(cargoName, integrity);
            }

            IUHGrabCampusManager.Instance?.OnCargoCollectedFromStall();
        }

        public void DeliverCargoToCustomer()
        {
            if (m_TxtCustomerPrompt != null)
            {
                m_TxtCustomerPrompt.text = "<size=20><color=#F43F5E><b>💖 CẢM ƠN TÀI XẾ NHIỀU NHA!</b></color></size>";
            }

            var bikeCargo = IUHGrabCargoVisual.Instance;
            if (bikeCargo != null)
            {
                bikeCargo.SetCargoDelivered();
            }

            IUHGrabCampusManager.Instance?.OnCargoDeliveredToCustomer();
        }

        public void HideObjectives()
        {
            if (m_StallRoot != null) m_StallRoot.SetActive(false);
            if (m_CustomerRoot != null) m_CustomerRoot.SetActive(false);
            var bikeCargo = IUHGrabCargoVisual.Instance;
            if (bikeCargo != null) bikeCargo.SetEmpty();
        }
    }
}
