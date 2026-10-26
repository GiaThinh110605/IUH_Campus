using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Interactive 3D Cargo & Delivery Box Visualizer on the Grab Motorbike.
    /// Provides clear physical feedback for:
    /// - Thermal Delivery Bag mounted on Rear Cargo Rack
    /// - Loaded state: Physical parcels / bubble tea cups on rack + glowing 3D badge
    /// - Dynamic damage feedback (sparks/smoke when ghosts attack)
    /// - Delivery completion celebration
    /// </summary>
    public class IUHGrabCargoVisual : MonoBehaviour
    {
        private static IUHGrabCargoVisual s_Instance;
        public static IUHGrabCargoVisual Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = UnityEngine.Object.FindAnyObjectByType<IUHGrabCargoVisual>(FindObjectsInactive.Include);
                    if (s_Instance == null)
                    {
                        var bike = UnityEngine.Object.FindAnyObjectByType<IUHGrabMotorbike>(FindObjectsInactive.Include);
                        if (bike != null)
                        {
                            s_Instance = bike.GetComponent<IUHGrabCargoVisual>();
                            if (s_Instance == null) s_Instance = bike.gameObject.AddComponent<IUHGrabCargoVisual>();
                            s_Instance.BuildDeliveryBoxVisuals();
                        }
                    }
                }
                return s_Instance;
            }
        }

        [Header("Cargo Box State")]
        [SerializeField] private bool m_IsLoaded = false;
        [SerializeField] private string m_CargoName = "Trà Sữa ToCoToCo";
        [SerializeField] private int m_Integrity = 100;

        // Visual transforms
        private Transform m_CargoBoxRoot;
        private Transform m_CargoItemsRoot;
        private GameObject m_FloatingBadgeGo;
        private Text m_TxtBadgeTitle;
        private Text m_TxtBadgeIntegrity;
        private Light m_CargoGlowLight;
        private ParticleSystem m_DamageParticles;
        private ParticleSystem m_SparkleParticles;

        public bool IsLoaded => m_IsLoaded;

        private void Awake()
        {
            if (s_Instance == null) s_Instance = this;
            BuildDeliveryBoxVisuals();
        }

        private void Start()
        {
            SetEmpty();
        }

        private void Update()
        {
            // Billboard floating badge towards camera
            if (m_FloatingBadgeGo != null && m_FloatingBadgeGo.activeSelf)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    m_FloatingBadgeGo.transform.rotation = cam.transform.rotation;
                }
            }
        }

        public void BuildDeliveryBoxVisuals()
        {
            if (m_CargoBoxRoot != null) return;

            // Find Rear_Cargo_Rack on bike
            Transform rack = transform.Find("Visual_Root/Rear_Cargo_Rack");
            if (rack == null) rack = transform.Find("Rear_Cargo_Rack");
            if (rack == null) rack = transform;

            // 1. Cargo Box Root
            var boxGo = new GameObject("Delivery_Thermal_Box");
            boxGo.transform.SetParent(rack, false);
            boxGo.transform.localPosition = new Vector3(0f, 0.28f, -0.05f);
            boxGo.transform.localRotation = Quaternion.identity;
            m_CargoBoxRoot = boxGo.transform;

            // Determine brand color (Grab green, Xanh SM teal, be yellow, IUH blue)
            Color boxColor = new Color(0f, 0.69f, 0.31f); // Grab Green
            if (IUHCustomizationManager.Instance != null)
            {
                boxColor = IUHCustomizationManager.Instance.ActiveConfig.primaryColor;
            }

            // Main insulated box body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Thermal_Bag_Body";
            body.transform.SetParent(m_CargoBoxRoot, false);
            body.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            body.transform.localScale = new Vector3(0.52f, 0.44f, 0.48f);
            StripCollider(body);

            var bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = boxColor;
            bodyMat.SetFloat("_Glossiness", 0.35f);
            body.GetComponent<Renderer>().material = bodyMat;

            // Reflective horizontal safety stripe
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Reflective_Stripe";
            stripe.transform.SetParent(m_CargoBoxRoot, false);
            stripe.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            stripe.transform.localScale = new Vector3(0.53f, 0.08f, 0.49f);
            StripCollider(stripe);

            var stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = new Color(0.95f, 0.98f, 1.0f);
            stripeMat.EnableKeyword("_EMISSION");
            stripeMat.SetColor("_EmissionColor", new Color(0.8f, 0.9f, 1.0f) * 0.8f);
            stripe.GetComponent<Renderer>().material = stripeMat;

            // Top lid
            var lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "Thermal_Bag_Lid";
            lid.transform.SetParent(m_CargoBoxRoot, false);
            lid.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            lid.transform.localScale = new Vector3(0.54f, 0.06f, 0.50f);
            StripCollider(lid);
            var lidMat = new Material(Shader.Find("Standard"));
            lidMat.color = boxColor * 0.85f;
            lid.GetComponent<Renderer>().material = lidMat;

            // 2. Cargo Items Root (Visible parcels/drinks when loaded)
            var itemsGo = new GameObject("Loaded_Cargo_Contents");
            itemsGo.transform.SetParent(m_CargoBoxRoot, false);
            itemsGo.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            m_CargoItemsRoot = itemsGo.transform;

            // Physical item: Cup carrier with 2 bubble tea cups on top of bag
            var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "Drink_Tray";
            tray.transform.SetParent(m_CargoItemsRoot, false);
            tray.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            tray.transform.localScale = new Vector3(0.38f, 0.04f, 0.22f);
            StripCollider(tray);
            var trayMat = new Material(Shader.Find("Standard"));
            trayMat.color = new Color(0.28f, 0.20f, 0.12f);
            tray.GetComponent<Renderer>().material = trayMat;

            // Cup 1 (left)
            CreateBubbleTeaCup(m_CargoItemsRoot, new Vector3(-0.10f, 0.14f, 0f), new Color(0.92f, 0.72f, 0.45f));
            // Cup 2 (right)
            CreateBubbleTeaCup(m_CargoItemsRoot, new Vector3(0.10f, 0.14f, 0f), new Color(0.45f, 0.75f, 0.45f));

            // Strip any remaining colliders recursively so CharacterController never gets blocked
            StripAllColliders(transform);

            // 3. Cargo Glow Light
            var lightGo = new GameObject("CargoGlowLight");
            lightGo.transform.SetParent(m_CargoBoxRoot, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            m_CargoGlowLight = lightGo.AddComponent<Light>();
            m_CargoGlowLight.type = LightType.Point;
            m_CargoGlowLight.range = 3.2f;
            m_CargoGlowLight.intensity = 2.0f;
            m_CargoGlowLight.color = new Color(0.2f, 1.0f, 0.5f);
            m_CargoGlowLight.enabled = false;

            // 4. Floating 3D Billboard Badge
            BuildFloatingBadge();
        }

        private static void StripCollider(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }
        }

        public static void StripAllColliders(Transform root)
        {
            if (root == null) return;
            var colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in colliders)
            {
                if (c is CharacterController) continue;
                c.enabled = false;
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);
            }
        }

        private void CreateBubbleTeaCup(Transform parent, Vector3 localPos, Color teaColor)
        {
            var cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cup.name = "BubbleTeaCup";
            cup.transform.SetParent(parent, false);
            cup.transform.localPosition = localPos;
            cup.transform.localScale = new Vector3(0.09f, 0.10f, 0.09f);
            StripCollider(cup);

            var cupMat = new Material(Shader.Find("Standard"));
            cupMat.color = teaColor;
            cupMat.SetFloat("_Glossiness", 0.8f);
            cup.GetComponent<Renderer>().material = cupMat;

            // Dome Lid
            var lid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lid.name = "CupLid";
            lid.transform.SetParent(cup.transform, false);
            lid.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            lid.transform.localScale = new Vector3(1.05f, 0.5f, 1.05f);
            StripCollider(lid);
            var lidMat = new Material(Shader.Find("Standard"));
            lidMat.color = new Color(0.95f, 0.95f, 0.98f, 0.8f);
            lid.GetComponent<Renderer>().material = lidMat;

            // Straw
            var straw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            straw.name = "Straw";
            straw.transform.SetParent(cup.transform, false);
            straw.transform.localPosition = new Vector3(0.1f, 1.45f, 0.05f);
            straw.transform.localRotation = Quaternion.Euler(15f, 0f, 12f);
            straw.transform.localScale = new Vector3(0.12f, 0.6f, 0.12f);
            StripCollider(straw);
            var strawMat = new Material(Shader.Find("Standard"));
            strawMat.color = new Color(1.0f, 0.3f, 0.2f);
            straw.GetComponent<Renderer>().material = strawMat;
        }

        private void BuildFloatingBadge()
        {
            m_FloatingBadgeGo = new GameObject("FloatingCargoBadge");
            m_FloatingBadgeGo.transform.SetParent(m_CargoBoxRoot, false);
            m_FloatingBadgeGo.transform.localPosition = new Vector3(0f, 0.95f, 0f);

            var canvas = m_FloatingBadgeGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 80f);
            m_FloatingBadgeGo.transform.localScale = Vector3.one * 0.007f;

            // Background panel
            var bgGo = new GameObject("BadgeBg");
            bgGo.transform.SetParent(m_FloatingBadgeGo.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Outline
            var outline = bgGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 1.0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title text
            var titleGo = new GameObject("TxtTitle");
            titleGo.transform.SetParent(bgGo.transform, false);
            m_TxtBadgeTitle = titleGo.AddComponent<Text>();
            m_TxtBadgeTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtBadgeTitle.fontSize = 20;
            m_TxtBadgeTitle.fontStyle = FontStyle.Bold;
            m_TxtBadgeTitle.alignment = TextAnchor.MiddleCenter;
            m_TxtBadgeTitle.color = new Color(1.0f, 0.94f, 0.5f);
            m_TxtBadgeTitle.text = "📦 ĐÃ NHẬN HÀNG";
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.45f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.sizeDelta = Vector2.zero;

            // Integrity text
            var intGo = new GameObject("TxtIntegrity");
            intGo.transform.SetParent(bgGo.transform, false);
            m_TxtBadgeIntegrity = intGo.AddComponent<Text>();
            m_TxtBadgeIntegrity.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtBadgeIntegrity.fontSize = 17;
            m_TxtBadgeIntegrity.fontStyle = FontStyle.Bold;
            m_TxtBadgeIntegrity.alignment = TextAnchor.MiddleCenter;
            m_TxtBadgeIntegrity.color = new Color(0.3f, 1.0f, 0.5f);
            m_TxtBadgeIntegrity.text = "Độ bền: 100%";
            var intRt = intGo.GetComponent<RectTransform>();
            intRt.anchorMin = new Vector2(0f, 0f);
            intRt.anchorMax = new Vector2(1f, 0.48f);
            intRt.sizeDelta = Vector2.zero;

            m_FloatingBadgeGo.SetActive(false);
        }

        /// <summary>
        /// Called when food/cargo is picked up from stall.
        /// </summary>
        public void SetCargoLoaded(string cargoName, int integrity = 100)
        {
            if (m_CargoBoxRoot == null) BuildDeliveryBoxVisuals();

            m_IsLoaded = true;
            m_CargoName = cargoName;
            m_Integrity = integrity;

            if (m_CargoItemsRoot != null) m_CargoItemsRoot.gameObject.SetActive(true);
            if (m_CargoGlowLight != null)
            {
                m_CargoGlowLight.enabled = true;
                m_CargoGlowLight.color = new Color(0.2f, 1.0f, 0.5f);
            }

            if (m_FloatingBadgeGo != null)
            {
                m_FloatingBadgeGo.SetActive(true);
                if (m_TxtBadgeTitle != null) m_TxtBadgeTitle.text = $"📦 {m_CargoName.ToUpper()}";
                UpdateIntegrityVisuals(integrity);
            }

            Debug.Log($"[IUH Grab Cargo] Đã chất hàng lên thùng xe máy: {m_CargoName} (Độ bền: {integrity}%)");
        }

        /// <summary>
        /// Updates integrity and shows visual damage feedback when hit by ghosts.
        /// </summary>
        public void UpdateCargoIntegrity(int integrity)
        {
            m_Integrity = integrity;
            UpdateIntegrityVisuals(integrity);
        }

        private void UpdateIntegrityVisuals(int integrity)
        {
            if (m_TxtBadgeIntegrity != null)
            {
                if (integrity > 70)
                {
                    m_TxtBadgeIntegrity.text = $"<color=#4ADE80>Độ bền: {integrity}% (Hoàn hảo)</color>";
                    if (m_CargoGlowLight != null) m_CargoGlowLight.color = new Color(0.2f, 1.0f, 0.5f);
                }
                else if (integrity > 40)
                {
                    m_TxtBadgeIntegrity.text = $"<color=#FACC15>Độ bền: {integrity}% (Hư hại nhẹ)</color>";
                    if (m_CargoGlowLight != null) m_CargoGlowLight.color = new Color(1.0f, 0.85f, 0.2f);
                }
                else
                {
                    m_TxtBadgeIntegrity.text = $"<color=#F87171>Độ bền: {integrity}% (Sắp nát!)</color>";
                    if (m_CargoGlowLight != null) m_CargoGlowLight.color = new Color(1.0f, 0.2f, 0.2f);
                }
            }
        }

        /// <summary>
        /// Called when cargo is successfully delivered to customer NPC.
        /// </summary>
        public void SetCargoDelivered()
        {
            m_IsLoaded = false;
            if (m_CargoItemsRoot != null) m_CargoItemsRoot.gameObject.SetActive(false);

            if (m_FloatingBadgeGo != null)
            {
                m_FloatingBadgeGo.SetActive(true);
                if (m_TxtBadgeTitle != null) m_TxtBadgeTitle.text = "✅ ĐÃ GIAO THÀNH CÔNG!";
                if (m_TxtBadgeIntegrity != null) m_TxtBadgeIntegrity.text = "<color=#38BDF8>+Tiền thưởng & EXP</color>";
            }

            if (m_CargoGlowLight != null)
            {
                m_CargoGlowLight.color = new Color(0.3f, 0.8f, 1.0f);
            }
        }

        /// <summary>
        /// Resets to empty state.
        /// </summary>
        public void SetEmpty()
        {
            m_IsLoaded = false;
            if (m_CargoItemsRoot != null) m_CargoItemsRoot.gameObject.SetActive(false);
            if (m_FloatingBadgeGo != null) m_FloatingBadgeGo.SetActive(false);
            if (m_CargoGlowLight != null) m_CargoGlowLight.enabled = false;
        }
    }
}
