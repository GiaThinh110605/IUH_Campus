using System;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian
{
    public enum PlayerSkinType
    {
        Default = 0,
        IUH_Student = 1,
        Graduate = 2,
        Cyber = 3
    }

    public enum PlayerTrailType
    {
        None = 0,
        CyberSparks = 1,
        CherryPetals = 2,
        GoldenStars = 3
    }

    public class IUHPlayerCustomizer : MonoBehaviour
    {
        public static IUHPlayerCustomizer Instance { get; private set; }

        private const string PREF_SKIN = "IUH_EQUIPPED_SKIN";
        private const string PREF_TRAIL = "IUH_EQUIPPED_TRAIL";
        private const string PREF_OWNED_PREFIX = "IUH_OWNED_";

        [Header("Renderers")]
        [SerializeField] private SkinnedMeshRenderer m_BodyRenderer;
        [SerializeField] private Renderer m_BackpackRenderer;

        [Header("Graduation Hat Accessory")]
        [SerializeField] private GameObject m_GraduationHatObj;

        [Header("Cyber Aura Accessory")]
        [SerializeField] private GameObject m_CyberAuraObj;

        [Header("Footstep Trails")]
        [SerializeField] private TrailRenderer m_LeftFootTrail;
        [SerializeField] private TrailRenderer m_RightFootTrail;

        private PlayerSkinType m_CurrentSkin = PlayerSkinType.Default;
        private PlayerTrailType m_CurrentTrail = PlayerTrailType.None;

        public PlayerSkinType CurrentSkin => m_CurrentSkin;
        public PlayerTrailType CurrentTrail => m_CurrentTrail;

        public event Action OnCustomizationChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            FindComponents();
            LoadCustomization();
        }

        private void Start()
        {
            ApplySkin(m_CurrentSkin, false);
            ApplyTrail(m_CurrentTrail, false);
        }

        public void FindComponents()
        {
            if (m_BodyRenderer == null)
            {
                m_BodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (m_BackpackRenderer == null)
            {
                var bp = transform.Find("Student backpack") ?? transform.Find("Armature/Student backpack");
                if (bp != null) m_BackpackRenderer = bp.GetComponent<Renderer>();
            }

            EnsureAccessories();
            EnsureTrails();
        }

        private void EnsureAccessories()
        {
            // 1. Graduation Mortarboard Hat
            if (m_GraduationHatObj == null)
            {
                var headBone = FindBoneRecursive(transform, "head");
                if (headBone != null)
                {
                    var existingHat = headBone.Find("GRADUATION_MORTARBOARD_HAT");
                    if (existingHat != null)
                    {
                        m_GraduationHatObj = existingHat.gameObject;
                    }
                    else
                    {
                        m_GraduationHatObj = CreateMortarboardHat(headBone);
                    }
                }
            }

            if (m_GraduationHatObj != null)
            {
                m_GraduationHatObj.SetActive(false);
            }

            // 2. Cyber Aura Core Light
            if (m_CyberAuraObj == null)
            {
                var existingAura = transform.Find("CYBER_AURA_LIGHT");
                if (existingAura != null)
                {
                    m_CyberAuraObj = existingAura.gameObject;
                }
                else
                {
                    GameObject auraGO = new GameObject("CYBER_AURA_LIGHT");
                    auraGO.transform.SetParent(transform, false);
                    auraGO.transform.localPosition = new Vector3(0f, 1.1f, -0.2f);
                    var light = auraGO.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(0.1f, 0.9f, 1f);
                    light.range = 3.5f;
                    light.intensity = 2.5f;
                    m_CyberAuraObj = auraGO;
                }
            }

            if (m_CyberAuraObj != null)
            {
                m_CyberAuraObj.SetActive(false);
            }
        }

        private GameObject CreateMortarboardHat(Transform headParent)
        {
            GameObject hatRoot = new GameObject("GRADUATION_MORTARBOARD_HAT");
            hatRoot.transform.SetParent(headParent, false);
            hatRoot.transform.localPosition = new Vector3(0f, 0.22f, -0.02f);
            hatRoot.transform.localRotation = Quaternion.Euler(-5f, 0f, 0f);

            // Skullcap
            GameObject skullCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            skullCap.name = "CapBase";
            skullCap.transform.SetParent(hatRoot.transform, false);
            skullCap.transform.localPosition = Vector3.zero;
            skullCap.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);

            // Square Board
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "SquareBoard";
            board.transform.SetParent(hatRoot.transform, false);
            board.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            board.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            board.transform.localScale = new Vector3(0.34f, 0.015f, 0.34f);

            // Tassel Button
            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            button.name = "TasselButton";
            button.transform.SetParent(hatRoot.transform, false);
            button.transform.localPosition = new Vector3(0f, 0.07f, 0f);
            button.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);

            // Set materials
            Material capMat = Resources.Load<Material>("IUH_Campus/Materials/M_IUH_Student_DarkPants") 
                ?? Resources.Load<Material>("IUH_Campus/Materials/M_IUH_DarkMetal");
            Material goldMat = Resources.Load<Material>("IUH_Campus/Materials/M_IUH_Cloth_Yellow");

            if (capMat != null)
            {
                skullCap.GetComponent<Renderer>().sharedMaterial = capMat;
                board.GetComponent<Renderer>().sharedMaterial = capMat;
            }
            if (goldMat != null)
            {
                button.GetComponent<Renderer>().sharedMaterial = goldMat;
            }

            // Remove colliders from hat
            foreach (var col in hatRoot.GetComponentsInChildren<Collider>())
            {
                DestroyImmediate(col);
            }

            return hatRoot;
        }

        private void EnsureTrails()
        {
            var leftFoot = FindBoneRecursive(transform, "left_foot") ?? FindBoneRecursive(transform, "leftfoot");
            var rightFoot = FindBoneRecursive(transform, "right_foot") ?? FindBoneRecursive(transform, "rightfoot");

            if (leftFoot != null && m_LeftFootTrail == null)
            {
                m_LeftFootTrail = leftFoot.GetComponent<TrailRenderer>();
                if (m_LeftFootTrail == null) m_LeftFootTrail = SetupFootTrail(leftFoot.gameObject);
            }

            if (rightFoot != null && m_RightFootTrail == null)
            {
                m_RightFootTrail = rightFoot.GetComponent<TrailRenderer>();
                if (m_RightFootTrail == null) m_RightFootTrail = SetupFootTrail(rightFoot.gameObject);
            }
        }

        private TrailRenderer SetupFootTrail(GameObject footGO)
        {
            var tr = footGO.AddComponent<TrailRenderer>();
            tr.time = 0.45f;
            tr.startWidth = 0.12f;
            tr.endWidth = 0.01f;
            tr.minVertexDistance = 0.1f;
            tr.autodestruct = false;
            tr.emitting = false;

            // Simple additive or unlit material
            Shader s = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            Material mat = new Material(s);
            tr.material = mat;

            return tr;
        }

        private Transform FindBoneRecursive(Transform current, string boneNameLower)
        {
            if (current.name.ToLower().Contains(boneNameLower)) return current;
            for (int i = 0; i < current.childCount; i++)
            {
                var found = FindBoneRecursive(current.GetChild(i), boneNameLower);
                if (found != null) return found;
            }
            return null;
        }

        public bool IsSkinOwned(PlayerSkinType skin)
        {
            if (skin == PlayerSkinType.Default) return true;
            return PlayerPrefs.GetInt(PREF_OWNED_PREFIX + skin.ToString(), 0) == 1;
        }

        public void UnlockSkin(PlayerSkinType skin)
        {
            PlayerPrefs.SetInt(PREF_OWNED_PREFIX + skin.ToString(), 1);
            PlayerPrefs.Save();
            OnCustomizationChanged?.Invoke();
        }

        public bool IsTrailOwned(PlayerTrailType trail)
        {
            if (trail == PlayerTrailType.None) return true;
            return PlayerPrefs.GetInt(PREF_OWNED_PREFIX + trail.ToString(), 0) == 1;
        }

        public void UnlockTrail(PlayerTrailType trail)
        {
            PlayerPrefs.SetInt(PREF_OWNED_PREFIX + trail.ToString(), 1);
            PlayerPrefs.Save();
            OnCustomizationChanged?.Invoke();
        }

        public void ApplySkin(PlayerSkinType skin, bool save = true)
        {
            m_CurrentSkin = skin;
            if (save)
            {
                PlayerPrefs.SetInt(PREF_SKIN, (int)skin);
                PlayerPrefs.Save();
            }

            if (m_BodyRenderer == null) FindComponents();
            if (m_BodyRenderer == null) return;

            var mats = m_BodyRenderer.sharedMaterials;
            if (mats == null || mats.Length < 3) return;

            switch (skin)
            {
                case PlayerSkinType.Default:
                    // Green Guardian jacket
                    Material defJacket = LoadMaterial("IUH_Campus/SharedCampusConcept/Materials/Student jacket");
                    if (defJacket != null)
                    {
                        mats[0] = defJacket;
                        mats[1] = defJacket;
                        mats[2] = defJacket;
                    }
                    if (m_GraduationHatObj != null) m_GraduationHatObj.SetActive(false);
                    if (m_CyberAuraObj != null) m_CyberAuraObj.SetActive(false);
                    break;

                case PlayerSkinType.IUH_Student:
                    // White/Navy Polo shirt + Jeans
                    Material polo = LoadMaterial("IUH_Campus/Materials/M_IUH_Student_BluePolo");
                    Material jeans = LoadMaterial("IUH_Campus/Materials/M_IUH_Student_Jeans");
                    Material white = LoadMaterial("IUH_Campus/Materials/M_IUH_Student_WhiteShirt");
                    if (polo != null) mats[0] = polo;
                    if (jeans != null) mats[1] = jeans;
                    if (white != null) mats[2] = white;

                    if (m_GraduationHatObj != null) m_GraduationHatObj.SetActive(false);
                    if (m_CyberAuraObj != null) m_CyberAuraObj.SetActive(false);
                    break;

                case PlayerSkinType.Graduate:
                    // Burgundy/Red graduation robe with gold accents + Hat
                    Material redRobe = LoadMaterial("IUH_Campus/Materials/M_IUH_Cloth_Red");
                    Material goldAccent = LoadMaterial("IUH_Campus/Materials/M_IUH_Cloth_Yellow");
                    Material darkPants = LoadMaterial("IUH_Campus/Materials/M_IUH_Student_DarkPants");
                    if (redRobe != null) mats[0] = redRobe;
                    if (goldAccent != null) mats[1] = goldAccent;
                    if (darkPants != null) mats[2] = darkPants;

                    if (m_GraduationHatObj != null) m_GraduationHatObj.SetActive(true);
                    if (m_CyberAuraObj != null) m_CyberAuraObj.SetActive(false);
                    break;

                case PlayerSkinType.Cyber:
                    // Futuristic glowing cyan metallic armor
                    Material cyanNeon = LoadMaterial("IUH_Campus/Materials/M_IUH_Waypoint_Holo") 
                        ?? LoadMaterial("IUH_Campus/Materials/M_IUH_Station_GreenGlow");
                    Material darkMetal = LoadMaterial("IUH_Campus/Materials/M_IUH_DarkMetal");
                    if (cyanNeon != null) mats[0] = cyanNeon;
                    if (darkMetal != null) mats[1] = darkMetal;
                    if (cyanNeon != null) mats[2] = cyanNeon;

                    if (m_GraduationHatObj != null) m_GraduationHatObj.SetActive(false);
                    if (m_CyberAuraObj != null) m_CyberAuraObj.SetActive(true);
                    break;
            }

            m_BodyRenderer.sharedMaterials = mats;
            OnCustomizationChanged?.Invoke();
        }

        public void ApplyTrail(PlayerTrailType trail, bool save = true)
        {
            m_CurrentTrail = trail;
            if (save)
            {
                PlayerPrefs.SetInt(PREF_TRAIL, (int)trail);
                PlayerPrefs.Save();
            }

            if (m_LeftFootTrail == null || m_RightFootTrail == null) EnsureTrails();

            bool hasTrail = trail != PlayerTrailType.None;
            if (m_LeftFootTrail != null) m_LeftFootTrail.emitting = hasTrail;
            if (m_RightFootTrail != null) m_RightFootTrail.emitting = hasTrail;

            if (!hasTrail)
            {
                OnCustomizationChanged?.Invoke();
                return;
            }

            Color startCol = Color.white;
            Color endCol = Color.clear;

            switch (trail)
            {
                case PlayerTrailType.CyberSparks:
                    startCol = new Color(0.15f, 0.95f, 1f, 0.9f);
                    endCol = new Color(0f, 0.4f, 1f, 0f);
                    break;

                case PlayerTrailType.CherryPetals:
                    startCol = new Color(1f, 0.45f, 0.75f, 0.9f);
                    endCol = new Color(1f, 0.8f, 0.9f, 0f);
                    break;

                case PlayerTrailType.GoldenStars:
                    startCol = new Color(1f, 0.90f, 0.2f, 0.95f);
                    endCol = new Color(1f, 0.5f, 0f, 0f);
                    break;
            }

            ApplyTrailGradient(m_LeftFootTrail, startCol, endCol);
            ApplyTrailGradient(m_RightFootTrail, startCol, endCol);
            OnCustomizationChanged?.Invoke();
        }

        private void ApplyTrailGradient(TrailRenderer tr, Color start, Color end)
        {
            if (tr == null) return;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(start, 0.0f), new GradientColorKey(end, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(start.a, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            tr.colorGradient = grad;
        }

        private Material LoadMaterial(string relativePath)
        {
#if UNITY_EDITOR
            string fullPath = "Assets/" + relativePath + ".mat";
            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (mat != null) return mat;
#endif
            return Resources.Load<Material>(relativePath);
        }

        private void LoadCustomization()
        {
            m_CurrentSkin = (PlayerSkinType)PlayerPrefs.GetInt(PREF_SKIN, (int)PlayerSkinType.Default);
            m_CurrentTrail = (PlayerTrailType)PlayerPrefs.GetInt(PREF_TRAIL, (int)PlayerTrailType.None);
        }
    }
}

