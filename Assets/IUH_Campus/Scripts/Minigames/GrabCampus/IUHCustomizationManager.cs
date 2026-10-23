using System;
using UnityEngine;
using IUHCampus.Player;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Manages Character Uniform & Motorbike Themes (Xanh SM, Grab, Be, Default).
    /// Dynamically recolors bike fairings, rider helmet, backpack, and player outfits.
    /// Exactly mirrors the "Campus Adventure" wardrobe/garage UI from design specifications.
    /// </summary>
    public class IUHCustomizationManager : MonoBehaviour
    {
        public enum BrandTheme
        {
            XanhSM = 0,
            Grab = 1,
            Be = 2,
            DefaultIUH = 3
        }

        [System.Serializable]
        public class ThemeConfig
        {
            public BrandTheme theme;
            public string displayName;
            public string tagline;
            public string description;
            public string bonusStats;
            public Color primaryColor;
            public Color secondaryColor;
            public float speedMultiplier;
        }

        public static IUHCustomizationManager Instance { get; private set; }

        [Header("Active Theme")]
        [SerializeField] private BrandTheme m_CurrentTheme = BrandTheme.Grab;

        [Header("Configs")]
        [SerializeField] private ThemeConfig[] m_Configs;

        [Header("References")]
        [SerializeField] private IUHGrabMotorbike m_Motorbike;
        [SerializeField] private IUHUniformManager m_UniformManager;

        public BrandTheme CurrentTheme => m_CurrentTheme;
        public ThemeConfig ActiveConfig => GetConfig(m_CurrentTheme);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            InitializeConfigs();
            m_CurrentTheme = IUHCampus.Core.PlayerLoadout.Current.theme;
        }

        private void Start()
        {
            ApplyTheme(m_CurrentTheme);
        }

        private void InitializeConfigs()
        {
            m_Configs = new ThemeConfig[4]
            {
                new ThemeConfig
                {
                    theme = BrandTheme.XanhSM,
                    displayName = "XANH SM",
                    tagline = "Lịch sự - Thân thiện - Xanh",
                    description = "Trang phục chính thức của hãng xe thuần điện Xanh SM. Tinh thần xanh – sạch – đẹp.",
                    bonusStats = "⚡ +5% Tốc độ  •  🛡️ +5% Sức bền  •  🌟 +10% Nhận diện",
                    primaryColor = new Color(0.0f, 0.72f, 0.85f), // Cyan Electric
                    secondaryColor = new Color(0.05f, 0.15f, 0.25f),
                    speedMultiplier = 1.05f
                },
                new ThemeConfig
                {
                    theme = BrandTheme.Grab,
                    displayName = "Grab",
                    tagline = "Nhanh chóng - An toàn - Tiện lợi",
                    description = "Màu xanh lá quen thuộc của đối tác tài xế Grab. Siêu tốc và linh hoạt né ma.",
                    bonusStats = "⚡ +10% Tốc độ  •  👻 +5% Né ma  •  📦 Bền kiện hàng",
                    primaryColor = new Color(0.0f, 0.70f, 0.32f), // Grab Green
                    secondaryColor = new Color(0.02f, 0.40f, 0.18f),
                    speedMultiplier = 1.10f
                },
                new ThemeConfig
                {
                    theme = BrandTheme.Be,
                    displayName = "be",
                    tagline = "Đồng hành - Linh hoạt - Dễ dàng",
                    description = "Sắc vàng rực rỡ và thân thiện của tài xế be. Gia tăng tiền thưởng cho mỗi đơn.",
                    bonusStats = "💰 +15% Tiền thưởng  •  🛡️ +10% Sức bền",
                    primaryColor = new Color(1.0f, 0.76f, 0.03f), // Be Yellow
                    secondaryColor = new Color(0.08f, 0.12f, 0.28f), // Be Navy
                    speedMultiplier = 1.05f
                },
                new ThemeConfig
                {
                    theme = BrandTheme.DefaultIUH,
                    displayName = "Sinh Viên IUH",
                    tagline = "Năng động - Sáng tạo - Trí tuệ",
                    description = "Đồng phục sinh viên Đại học Công nghiệp TP.HCM (IUH) truyền thống với xe thể thao.",
                    bonusStats = "🎓 Chuẩn sinh viên IUH  •  ⚡ Cân bằng toàn diện",
                    primaryColor = new Color(0.12f, 0.35f, 0.80f), // IUH Blue
                    secondaryColor = new Color(0.15f, 0.18f, 0.22f),
                    speedMultiplier = 1.0f
                }
            };
        }

        public ThemeConfig GetConfig(BrandTheme theme)
        {
            if (m_Configs == null || m_Configs.Length == 0) InitializeConfigs();
            foreach (var c in m_Configs)
            {
                if (c.theme == theme) return c;
            }
            return m_Configs[0];
        }

        public void ApplyTheme(BrandTheme theme)
        {
            m_CurrentTheme = theme;
            IUHCampus.Core.PlayerLoadout.Current.theme = theme;
            IUHCampus.Core.PlayerLoadout.Current.Save();

            var config = GetConfig(theme);

            // 1. Recolor Motorbike Fairing & Decals
            if (m_Motorbike == null) m_Motorbike = FindFirstObjectByType<IUHGrabMotorbike>();
            if (m_Motorbike != null)
            {
                RecolorTransformChildren(m_Motorbike.transform, config.primaryColor, config.secondaryColor);
            }

            // 2. Recolor Rider Figure (Helmet & Backpack)
            var rider = GameObject.Find("Rider_Figure");
            if (rider != null)
            {
                RecolorRiderAccessories(rider.transform, config.primaryColor, config.secondaryColor);
            }

            // 3. Switch Uniform if UniformManager is present
            if (m_UniformManager == null) m_UniformManager = FindFirstObjectByType<IUHUniformManager>();
            if (m_UniformManager != null)
            {
                if (theme == BrandTheme.DefaultIUH) m_UniformManager.ApplyOutfit(0);
                else if (theme == BrandTheme.Grab) m_UniformManager.ApplyOutfit(1);
                else if (theme == BrandTheme.Be) m_UniformManager.ApplyOutfit(0);
                else if (theme == BrandTheme.XanhSM) m_UniformManager.ApplyOutfit(3);
            }

            Debug.Log($"[IUH Customization] Đã chuyển đổi trang phục & xe máy sang: {config.displayName} ({config.tagline})");
        }

        private void RecolorTransformChildren(Transform bikeRoot, Color primary, Color secondary)
        {
            var fairing = bikeRoot.Find("Visual_Root/Body_Fairing");
            if (fairing != null) SetMeshColor(fairing, primary);

            var mask = bikeRoot.Find("Visual_Root/Steer_Fork/Front_Mask");
            if (mask != null) SetMeshColor(mask, primary);

            var fender = bikeRoot.Find("Visual_Root/Steer_Fork/Front_Fender");
            if (fender != null) SetMeshColor(fender, primary);

            var box = bikeRoot.Find("Visual_Root/Chassis_Frame/Delivery_Box");
            if (box != null) SetMeshColor(box, primary);
        }

        private void RecolorRiderAccessories(Transform riderRoot, Color primary, Color secondary)
        {
            var rends = riderRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in rends)
            {
                if (r == null) continue;
                string n = r.gameObject.name;
                if (n.Contains("Jacket") || n.Contains("Torso") || n.Contains("Shirt") || n.Contains("Arm"))
                {
                    SetRendererColor(r, primary);
                }
                else if (n.Contains("Helmet") || n.Contains("Nón"))
                {
                    SetRendererColor(r, primary);
                }
                else if (n.Contains("Backpack") || n.Contains("Balo"))
                {
                    SetRendererColor(r, primary);
                }
                else if (n.Contains("Stripe") || n.Contains("Trim"))
                {
                    SetRendererColor(r, secondary);
                }
            }
        }

        private void SetMeshColor(Transform target, Color col)
        {
            var mr = target.GetComponent<MeshRenderer>();
            if (mr != null) SetRendererColor(mr, col);
        }

        private void SetRendererColor(MeshRenderer mr, Color col)
        {
            if (mr.material != null)
            {
                mr.material.color = col;
            }
        }
    }
}
