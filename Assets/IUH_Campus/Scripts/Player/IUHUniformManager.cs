using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Player
{
    /// <summary>
    /// Manages authentic IUH student uniform selection and human character appearance.
    /// Supports 5 IUH uniforms, hotkeys 1-5, and an in-game Wardrobe UI (Key 'U').
    /// </summary>
    public class IUHUniformManager : MonoBehaviour
    {
        [System.Serializable]
        public class UniformOutfit
        {
            public string id;
            public string name;
            public string description;
            public Color previewColor;
            public Material bodyMaterial;
            public Material armsMaterial;
            public Material legsMaterial;
        }

        [Header("Outfits")]
        [SerializeField] private UniformOutfit[] m_Outfits;
        [SerializeField] private int m_CurrentOutfitIndex = 0;

        [Header("Human Hair")]
        [SerializeField] private GameObject m_HairObject;
        [SerializeField] private Material m_HairMaterial;

        [Header("UI Settings")]
        public bool showWardrobeUI = false;

        private SkinnedMeshRenderer m_SkinnedMeshRenderer;

        public int CurrentOutfitIndex => m_CurrentOutfitIndex;
        public int OutfitCount => m_Outfits != null ? m_Outfits.Length : 0;

        private void Awake()
        {
            m_SkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            InitializeOutfits();
            EnsureHumanHair();
            ApplyOutfit(m_CurrentOutfitIndex);
        }

        private void InitializeOutfits()
        {
            if (m_Outfits != null && m_Outfits.Length >= 5) return;

            string[] ids = new string[] {
                "1_Polo_White_BlueYellow",
                "2_PE_Sports_BlueYellow",
                "3_Female_RedTrim",
                "4_Polo_White_BlueTrim",
                "5_Military_GDQP"
            };

            string[] names = new string[] {
                "Polo Trắng Viền Xanh Vàng (Truyền Thống)",
                "Đồng Phục Thể Dục Xanh Bích (PE)",
                "Nữ Sơ Mi Cổ Sen Viền Đỏ",
                "Polo Trắng Viền Xanh Biển",
                "Quân Sự GDQP Xanh Rêu"
            };

            string[] descs = new string[] {
                "Áo polo trắng viền cổ xanh-vàng, logo IUH ngực trái, quần tây đen.",
                "Áo thể dục xanh bích cổ tim vàng, tay trắng sọc vàng-xanh, quần thể thao.",
                "Áo sơ mi trắng cổ cánh sen viền đỏ nổi bật, logo hoa sen đỏ IUH.",
                "Áo polo trắng thanh lịch viền chỉ xanh dương mảnh, quần jeans năng động.",
                "Đồng phục Giáo dục Quốc phòng xanh rêu chiến thuật đặc trưng sinh viên IUH."
            };

            Color[] colors = new Color[] {
                new Color(0.1f, 0.45f, 0.85f),
                new Color(0.15f, 0.55f, 0.95f),
                new Color(0.85f, 0.2f, 0.2f),
                new Color(0.0f, 0.6f, 0.9f),
                new Color(0.25f, 0.42f, 0.22f)
            };

            m_Outfits = new UniformOutfit[5];
            for (int i = 0; i < 5; i++)
            {
                m_Outfits[i] = new UniformOutfit
                {
                    id = ids[i],
                    name = names[i],
                    description = descs[i],
                    previewColor = colors[i],
#if UNITY_EDITOR
                    bodyMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/IUH_Campus/Materials/Uniforms/M_IUH_Uniform_{ids[i]}_Body.mat"),
                    armsMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/IUH_Campus/Materials/Uniforms/M_IUH_Uniform_{ids[i]}_Arms.mat"),
                    legsMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/IUH_Campus/Materials/Uniforms/M_IUH_Uniform_{ids[i]}_Legs.mat")
#endif
                };
            }
        }

        public void EnsureHumanHair()
        {
            var anim = GetComponent<Animator>();
            if (anim == null || !anim.isHuman) return;

            var head = anim.GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;

            Transform existing = head.Find("Student_Hair");
            if (existing != null)
            {
                m_HairObject = existing.gameObject;
            }
            else
            {
                // Find hair template from campus
                var campus = GameObject.Find("campus_P0_progress");
                if (campus != null)
                {
                    var srcHair = campus.transform.Find("Student_1_Hair");
                    if (srcHair != null)
                    {
                        m_HairObject = Instantiate(srcHair.gameObject, head);
                        m_HairObject.name = "Student_Hair";
                        m_HairObject.transform.localPosition = new Vector3(0f, 0.08f, -0.02f);
                        m_HairObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                        m_HairObject.transform.localScale = Vector3.one * 1.05f;
                    }
                }
            }

            if (m_HairObject != null)
            {
#if UNITY_EDITOR
                if (m_HairMaterial == null)
                {
                    m_HairMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Student_Hair.mat");
                }
#endif
                if (m_HairMaterial != null)
                {
                    var mr = m_HairObject.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = m_HairMaterial;
                }
            }
        }

        private void Update()
        {
            // Toggle Wardrobe UI with Key U
            if (IsKeyTriggered(KeyCode.U))
            {
                showWardrobeUI = !showWardrobeUI;
            }

            // Quick outfit hotkeys: 1, 2, 3, 4, 5
            if (IsKeyTriggered(KeyCode.Alpha1) || IsKeyTriggered(KeyCode.Keypad1)) ApplyOutfit(0);
            if (IsKeyTriggered(KeyCode.Alpha2) || IsKeyTriggered(KeyCode.Keypad2)) ApplyOutfit(1);
            if (IsKeyTriggered(KeyCode.Alpha3) || IsKeyTriggered(KeyCode.Keypad3)) ApplyOutfit(2);
            if (IsKeyTriggered(KeyCode.Alpha4) || IsKeyTriggered(KeyCode.Keypad4)) ApplyOutfit(3);
            if (IsKeyTriggered(KeyCode.Alpha5) || IsKeyTriggered(KeyCode.Keypad5)) ApplyOutfit(4);
        }

        private bool IsKeyTriggered(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (code)
                {
                    case KeyCode.U: return Keyboard.current.uKey.wasPressedThisFrame;
                    case KeyCode.Alpha1:
                    case KeyCode.Keypad1: return Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2:
                    case KeyCode.Keypad2: return Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3:
                    case KeyCode.Keypad3: return Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame;
                    case KeyCode.Alpha4:
                    case KeyCode.Keypad4: return Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame;
                    case KeyCode.Alpha5:
                    case KeyCode.Keypad5: return Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame;
                }
                return false;
            }
#endif
            try
            {
                return Input.GetKeyDown(code);
            }
            catch
            {
                return false;
            }
        }

        public void ApplyOutfit(int index)
        {
            if (m_Outfits == null || m_Outfits.Length == 0) InitializeOutfits();
            if (index < 0 || index >= m_Outfits.Length) return;

            m_CurrentOutfitIndex = index;
            var outfit = m_Outfits[index];

            if (m_SkinnedMeshRenderer == null)
            {
                m_SkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (m_SkinnedMeshRenderer != null && outfit.bodyMaterial != null && outfit.armsMaterial != null && outfit.legsMaterial != null)
            {
                Material[] mats = new Material[3];
                mats[0] = outfit.bodyMaterial; // Body
                mats[1] = outfit.armsMaterial; // Arms & Face
                mats[2] = outfit.legsMaterial; // Legs & Shoes
                m_SkinnedMeshRenderer.sharedMaterials = mats;
            }

            Debug.Log($"[IUHUniformManager] Đã mặc đồng phục: {outfit.name}");
        }

        private void OnGUI()
        {
            // 1. Floating HUD badge at top-right
            DrawCurrentOutfitBadge();

            // 2. Full wardrobe UI modal when toggled with 'U'
            if (showWardrobeUI)
            {
                DrawWardrobeModal();
            }
        }

        private void DrawCurrentOutfitBadge()
        {
            if (m_Outfits == null || m_CurrentOutfitIndex >= m_Outfits.Length) return;

            int bw = 380;
            int bh = 42;
            int bx = Screen.width - bw - 20;
            int by = 20;

            Color prev = GUI.color;

            // Background
            GUI.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);
            GUI.Box(new Rect(bx, by, bw, bh), GUIContent.none);

            // Left colored stripe
            GUI.color = m_Outfits[m_CurrentOutfitIndex].previewColor;
            GUI.Box(new Rect(bx, by, 6, bh), GUIContent.none);

            // Content
            GUI.color = Color.white;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = Color.white;

            string txt = $"🎓 IUH: {m_Outfits[m_CurrentOutfitIndex].name}\n<color=#85C1E9>[Phím 1-5] • [U] Mở tủ đồ</color>";
            GUI.Label(new Rect(bx + 14, by + 4, bw - 20, bh - 8), txt, labelStyle);

            GUI.color = prev;
        }

        private void DrawWardrobeModal()
        {
            int mw = 680;
            int mh = 480;
            int mx = (Screen.width - mw) / 2;
            int my = (Screen.height - mh) / 2;

            Color prev = GUI.color;

            // Background dark overlay
            GUI.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);
            GUI.Box(new Rect(mx, my, mw, mh), GUIContent.none);

            // Header banner
            GUI.color = new Color(0.12f, 0.45f, 0.85f, 1f);
            GUI.Box(new Rect(mx, my, mw, 52), GUIContent.none);

            // Header title
            GUI.color = Color.white;
            GUIStyle headStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(mx, my + 12, mw, 30), "👔 TỦ ĐỒ ĐỒNG PHỤC SINH VIÊN IUH", headStyle);

            // Close button [X]
            if (GUI.Button(new Rect(mx + mw - 44, my + 10, 32, 32), "✕"))
            {
                showWardrobeUI = false;
            }

            // Uniform cards
            int startY = my + 68;
            int itemH = 68;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;

            GUIStyle descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12
            };
            descStyle.normal.textColor = new Color(0.8f, 0.85f, 0.9f);

            for (int i = 0; i < m_Outfits.Length; i++)
            {
                var outfit = m_Outfits[i];
                int iy = startY + i * (itemH + 10);
                bool isCurrent = (i == m_CurrentOutfitIndex);

                // Row background
                GUI.color = isCurrent ? new Color(0.16f, 0.28f, 0.44f, 0.85f) : new Color(0.10f, 0.14f, 0.20f, 0.80f);
                GUI.Box(new Rect(mx + 20, iy, mw - 40, itemH), GUIContent.none);

                // Color accent bar
                GUI.color = outfit.previewColor;
                GUI.Box(new Rect(mx + 20, iy, 8, itemH), GUIContent.none);

                // Hotkey badge [1], [2], ...
                GUI.color = new Color(0.2f, 0.24f, 0.32f);
                GUI.Box(new Rect(mx + 36, iy + 14, 38, 38), GUIContent.none);
                GUI.color = Color.white;
                GUIStyle keyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                keyStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(mx + 36, iy + 14, 38, 38), $"[{i + 1}]", keyStyle);

                // Title & Description
                GUI.Label(new Rect(mx + 84, iy + 10, mw - 260, 24), outfit.name, titleStyle);
                GUI.Label(new Rect(mx + 84, iy + 34, mw - 260, 28), outfit.description, descStyle);

                // Equip button
                if (isCurrent)
                {
                    GUI.color = new Color(0.3f, 0.8f, 0.4f);
                    GUIStyle eqStyle = new GUIStyle(GUI.skin.box)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    eqStyle.normal.textColor = Color.white;
                    GUI.Box(new Rect(mx + mw - 150, iy + 18, 110, 32), "✓ Đang Mặc", eqStyle);
                }
                else
                {
                    GUI.color = Color.white;
                    if (GUI.Button(new Rect(mx + mw - 150, iy + 18, 110, 32), "Mặc Ngay"))
                    {
                        ApplyOutfit(i);
                    }
                }
            }

            // Footer hint
            GUI.color = new Color(0.7f, 0.8f, 0.9f);
            GUIStyle footStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(mx, my + mh - 30, mw, 24), "💡 Phím tắt nhanh: Nhấn [1], [2], [3], [4], [5] bất cứ lúc nào để đổi trang phục tức thì.", footStyle);

            GUI.color = prev;
        }
    }
}
