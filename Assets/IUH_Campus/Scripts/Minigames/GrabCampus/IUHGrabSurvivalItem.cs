using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// 3D World Pickup for Grab Campus Survival Items (Vật phẩm sinh tồn).
    /// Items: Đèn pin, Pin dự phòng, Bùa hộ mệnh, Pháo sáng, Gậy tự vệ, Bộ sơ cứu, Chìa khóa.
    /// Features floating sinusoidal hovering, rotating aura, proximity prompt, and auto-collection.
    /// </summary>
    public class IUHGrabSurvivalItem : MonoBehaviour
    {
        public enum ItemType
        {
            Flashlight,   // Đèn pin
            Battery,      // Pin dự phòng
            Talisman,     // Bùa hộ mệnh
            Flare,        // Pháo sáng
            DefenseBat,   // Gậy tự vệ
            FirstAidKit,  // Bộ sơ cứu
            Key           // Chìa khóa
        }

        [Header("Item Config")]
        public ItemType type = ItemType.Battery;
        public int amount = 1;
        public string displayName = "Pin Dự Phòng";
        public string description = "Nạp đầy 100% năng lượng cho đèn pin.";

        [Header("Visual Effects")]
        [SerializeField] private float m_HoverSpeed = 2.8f;
        [SerializeField] private float m_HoverHeight = 0.22f;
        [SerializeField] private float m_RotateSpeed = 65f;
        [SerializeField] private Light m_ItemLight;

        private Vector3 m_BasePos;
        private AudioSource m_AudioSource;
        private Transform m_TargetTransform;
        private bool m_IsCollected = false;
        private GameObject m_PromptGo;
        private Text m_TxtPrompt;

        private void Awake()
        {
            m_BasePos = transform.position;
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 0.5f;
            m_AudioSource.playOnAwake = false;

            SetupVisualAura();
            SetupPromptUI();
        }

        private void SetupVisualAura()
        {
            if (m_ItemLight == null)
            {
                var lightGo = new GameObject("ItemGlowLight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
                m_ItemLight = lightGo.AddComponent<Light>();
                m_ItemLight.type = LightType.Point;
                m_ItemLight.range = 4.5f;
                m_ItemLight.intensity = 2.2f;
                m_ItemLight.color = GetItemColor();
            }
        }

        private void SetupPromptUI()
        {
            m_PromptGo = new GameObject("PromptCanvas");
            m_PromptGo.transform.SetParent(transform, false);
            m_PromptGo.transform.localPosition = new Vector3(0f, 1.1f, 0f);

            var canvas = m_PromptGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 60f);
            m_PromptGo.transform.localScale = Vector3.one * 0.015f;

            var txtGo = new GameObject("TxtPrompt");
            txtGo.transform.SetParent(m_PromptGo.transform, false);
            m_TxtPrompt = txtGo.AddComponent<Text>();
            m_TxtPrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtPrompt.fontSize = 20;
            m_TxtPrompt.alignment = TextAnchor.MiddleCenter;
            m_TxtPrompt.fontStyle = FontStyle.Bold;
            m_TxtPrompt.color = Color.white;
            m_TxtPrompt.text = $"[E] Nhặt {displayName}";

            var outline = txtGo.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            m_PromptGo.SetActive(false);
        }

        public Color GetItemColor()
        {
            switch (type)
            {
                case ItemType.Flashlight: return new Color(0.2f, 0.8f, 1.0f); // Bright Cyan
                case ItemType.Battery: return new Color(0.2f, 1.0f, 0.5f);    // Emerald
                case ItemType.Talisman: return new Color(1.0f, 0.85f, 0.1f);  // Gold
                case ItemType.Flare: return new Color(1.0f, 0.25f, 0.1f);     // Red-Orange
                case ItemType.DefenseBat: return new Color(0.7f, 0.4f, 1.0f); // Purple
                case ItemType.FirstAidKit: return new Color(1.0f, 0.2f, 0.3f);// Red-Pink
                case ItemType.Key: return new Color(1.0f, 0.95f, 0.4f);       // Pale Gold
                default: return Color.yellow;
            }
        }

        private void Update()
        {
            if (m_IsCollected) return;

            // Hover and rotate
            float yOffset = Mathf.Sin(Time.time * m_HoverSpeed) * m_HoverHeight;
            transform.position = m_BasePos + new Vector3(0f, yOffset, 0f);
            transform.Rotate(Vector3.up, m_RotateSpeed * Time.deltaTime, Space.World);

            // Billboarding prompt canvas to face camera
            if (m_PromptGo != null && m_PromptGo.activeSelf && Camera.main != null)
            {
                m_PromptGo.transform.rotation = Camera.main.transform.rotation;
            }

            // Proximity check with player / motorbike
            FindTarget();
            if (m_TargetTransform == null) return;

            float dist = Vector3.Distance(transform.position, m_TargetTransform.position);
            if (dist < 4.2f)
            {
                if (m_PromptGo != null) m_PromptGo.SetActive(true);

                // Auto-collect when very close (< 2.2m) or press E
                bool interactPressed = IUHInputHelper.IsInteractKeyDown() || Input.GetKeyDown(KeyCode.E);
                if (dist < 2.4f || interactPressed)
                {
                    Collect();
                }
            }
            else
            {
                if (m_PromptGo != null && m_PromptGo.activeSelf) m_PromptGo.SetActive(false);
            }
        }

        private void FindTarget()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null && mgr.ActiveTargetTransform != null)
            {
                m_TargetTransform = mgr.ActiveTargetTransform;
                return;
            }

            if (m_TargetTransform == null)
            {
                var bike = IUHGrabCampusManager.Instance != null ? IUHGrabCampusManager.Instance.Motorbike : null;
                if (bike != null) m_TargetTransform = bike.transform;
            }
        }

        public void Collect()
        {
            if (m_IsCollected) return;
            m_IsCollected = true;

            var inv = IUHGrabInventoryManager.Instance;
            if (inv != null)
            {
                inv.AddItem(type, amount, displayName);
            }

            // Play pickup sound
            var audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = m_AudioSource;
            if (audioSource != null)
            {
                AudioSource.PlayClipAtPoint(IUHSoundSynthesizer.GetItemPickupSound(), transform.position, 1.0f);
            }

            // Show HUD notification
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null)
            {
                mgr.ShowNotification($"🎒 Đã nhặt: +{amount} {displayName}!");
            }

            gameObject.SetActive(false);
        }

        public void ResetItem()
        {
            m_IsCollected = false;
            gameObject.SetActive(true);
            if (m_PromptGo != null) m_PromptGo.SetActive(false);
        }
    }
}
