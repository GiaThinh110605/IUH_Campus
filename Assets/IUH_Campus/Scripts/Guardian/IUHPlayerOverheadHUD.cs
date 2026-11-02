using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Guardian
{
    public class IUHPlayerOverheadHUD : MonoBehaviour
    {
        public static IUHPlayerOverheadHUD Instance { get; private set; }

        [Header("UI Bars")]
        [SerializeField] private Image m_HpFill;
        [SerializeField] private Text m_HpText;
        [SerializeField] private Image m_ShieldFill;
        [SerializeField] private Text m_ShieldText;
        [SerializeField] private Image m_EnergyFill;
        [SerializeField] private Text m_EnergyText;

        public Image HpFill => m_HpFill;
        public Text HpText => m_HpText;
        public Image ShieldFill => m_ShieldFill;
        public Text ShieldText => m_ShieldText;
        public Image EnergyFill => m_EnergyFill;
        public Text EnergyText => m_EnergyText;

        private void Awake()
        {
            Instance = this;
        }

        public void SetHp(int current, int max)
        {
            if (m_HpFill != null) m_HpFill.fillAmount = max > 0 ? (float)current / max : 0f;
            if (m_HpText != null) m_HpText.text = $"{current}/{max}";
        }

        public void SetShield(int current)
        {
            if (m_ShieldFill != null) m_ShieldFill.fillAmount = Mathf.Clamp01((float)current / 50f);
            if (m_ShieldText != null) m_ShieldText.text = current > 0 ? $"Giáp: {current}" : "";
        }

        public void SetEnergy(int current, int max)
        {
            if (m_EnergyFill != null) m_EnergyFill.fillAmount = max > 0 ? (float)current / max : 0f;
            if (m_EnergyText != null) m_EnergyText.text = $"{current}/{max}";
        }
    }
}

