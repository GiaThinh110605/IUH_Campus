using UnityEngine;

namespace IUHCampus.World
{
    /// <summary>
    /// Tự động bật/tắt đèn đường sân trường và ánh sáng phát xạ trên chao kính
    /// theo giá trị global shader _IUH_NightAmount từ IUHStoryNightShift.
    /// </summary>
    public class IUHStreetLampNightLight : MonoBehaviour
    {
        [SerializeField] private Light[] m_LampLights;
        [SerializeField] private Renderer[] m_GlassRenderers;
        [SerializeField] private float m_NightIntensity = 26.0f;
        [SerializeField] private Color m_GlassEmissionColor = new Color(1f, 0.85f, 0.45f, 1f) * 4.5f;

        private static readonly int NightAmountId = Shader.PropertyToID("_IUH_NightAmount");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock m_GlassBlock;
        private float m_LastNightAmount = -1f;

        private void Awake()
        {
            m_GlassBlock = new MaterialPropertyBlock();
            ApplyNightAmount(Mathf.Clamp01(Shader.GetGlobalFloat(NightAmountId)));
        }

        private void LateUpdate()
        {
            float nightAmount = Mathf.Clamp01(Shader.GetGlobalFloat(NightAmountId));
            if (Mathf.Abs(nightAmount - m_LastNightAmount) > 0.005f)
            {
                ApplyNightAmount(nightAmount);
            }
        }

        public void ApplyNightAmount(float nightAmount)
        {
            m_LastNightAmount = nightAmount;
            bool isLit = nightAmount > 0.05f;

            if (m_LampLights != null)
            {
                float intensity = m_NightIntensity * nightAmount;
                for (int i = 0; i < m_LampLights.Length; i++)
                {
                    var light = m_LampLights[i];
                    if (light == null) continue;
                    light.enabled = isLit;
                    light.intensity = intensity;
                }
            }

            if (m_GlassRenderers != null && m_GlassRenderers.Length > 0)
            {
                if (m_GlassBlock == null) m_GlassBlock = new MaterialPropertyBlock();
                m_GlassBlock.SetColor(EmissionColorId, isLit ? m_GlassEmissionColor * nightAmount : Color.black);
                for (int i = 0; i < m_GlassRenderers.Length; i++)
                {
                    var r = m_GlassRenderers[i];
                    if (r == null) continue;
                    if (r.name == "Lamp_GlowBulb" || r.name == "Lamp_GroundGlow" || r.name == "Lamp_Halo")
                    {
                        r.enabled = isLit;
                    }
                    else
                    {
                        r.SetPropertyBlock(isLit ? m_GlassBlock : null);
                    }
                }
            }
        }
    }
}
