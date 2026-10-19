using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// Horror & Ghost Pursuit Alert HUD (Ma đuổi & Hiệu ứng kinh dị).
    /// Mirrors Panel 4 of the Grab Campus design specification:
    /// - Danger alert banner: "⚠️ MA ĐANG ĐUỔI THEO! 👻"
    /// - Pursuit bar / Fear meter based on proximity to nearest ghost
    /// - Spooky red vignette pulse and horror whisper audio cues.
    /// </summary>
    public class IUHHorrorPursuitUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_AlertContainer;
        [SerializeField] private Text m_TxtWarning;
        [SerializeField] private Image m_ImgPursuitFill;
        [SerializeField] private Text m_TxtDistanceThreat;
        [SerializeField] private Image m_HorrorVignette;

        [Header("Pursuit Thresholds")]
        [SerializeField] private float m_ThreatStartDistance = 25f; // Starts detecting
        [SerializeField] private float m_CriticalDistance = 6f;     // Extreme danger

        public void Setup(GameObject alertContainer, Text txtWarning, Image pursuitFill, Image vignette, Text txtDistanceThreat = null)
        {
            m_AlertContainer = alertContainer;
            m_TxtWarning = txtWarning;
            m_ImgPursuitFill = pursuitFill;
            m_HorrorVignette = vignette;
            m_TxtDistanceThreat = txtDistanceThreat;
        }

        private Transform m_PlayerOrBikeTransform;
        private AudioSource m_AudioSource;
        private float m_HeartbeatTimer = 0f;

        private void Awake()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
            AutoWireReferences();
        }

        private void AutoWireReferences()
        {
            if (m_AlertContainer == null) m_AlertContainer = transform.Find("DangerAlertBanner")?.gameObject;
            if (m_TxtWarning == null) m_TxtWarning = transform.Find("DangerAlertBanner/TxtWarning")?.GetComponent<Text>();
            if (m_TxtDistanceThreat == null) m_TxtDistanceThreat = transform.Find("DangerAlertBanner/TxtDistanceThreat")?.GetComponent<Text>();
            if (m_ImgPursuitFill == null) m_ImgPursuitFill = transform.Find("DangerAlertBanner/PursuitBarBg/PursuitBarFill")?.GetComponent<Image>();
            if (m_HorrorVignette == null) m_HorrorVignette = transform.Find("Vignette")?.GetComponent<Image>();
        }

        private void Start()
        {
            if (m_AlertContainer != null) m_AlertContainer.SetActive(false);
            if (m_HorrorVignette != null) m_HorrorVignette.color = new Color(0.8f, 0f, 0f, 0f);
        }

        private void Update()
        {
            FindTarget();
            if (m_PlayerOrBikeTransform == null)
            {
                HideAlert();
                return;
            }

            // Find closest active ghost
            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            float minDistance = float.MaxValue;
            IUHGhostHazard nearestGhost = null;

            foreach (var g in ghosts)
            {
                if (g == null || !g.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(m_PlayerOrBikeTransform.position, g.transform.position);
                if (d < minDistance)
                {
                    minDistance = d;
                    nearestGhost = g;
                }
            }

            if (minDistance <= m_ThreatStartDistance && nearestGhost != null)
            {
                ShowAlert(minDistance);
            }
            else
            {
                HideAlert();
            }
        }

        private void FindTarget()
        {
            var bike = FindFirstObjectByType<IUHGrabMotorbike>();
            if (bike != null && bike.IsMounted)
            {
                m_PlayerOrBikeTransform = bike.transform;
                return;
            }

            var player = GameObject.Find("PlayerArmature");
            if (player != null) m_PlayerOrBikeTransform = player.transform;
            else
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) m_PlayerOrBikeTransform = p.transform;
            }
        }

        private void ShowAlert(float distance)
        {
            if (m_AlertContainer != null && !m_AlertContainer.activeSelf)
            {
                m_AlertContainer.SetActive(true);
            }

            // Danger ratio: 0 at 25m, 1 at <= 6m
            float dangerRatio = Mathf.Clamp01(1.0f - ((distance - m_CriticalDistance) / (m_ThreatStartDistance - m_CriticalDistance)));

            // Update Threat Bar
            if (m_ImgPursuitFill != null)
            {
                m_ImgPursuitFill.fillAmount = dangerRatio;
                m_ImgPursuitFill.color = Color.Lerp(new Color(1f, 0.7f, 0.1f), new Color(1f, 0.1f, 0.1f), dangerRatio);
            }

            // Text
            if (m_TxtDistanceThreat != null)
            {
                m_TxtDistanceThreat.text = $"CỰ LY NGUY HIỂM: {distance:F0}m — NÉ TRÁNH NGAY!";
            }

            // Flash warning banner
            if (m_TxtWarning != null)
            {
                float pulse = 0.7f + Mathf.PingPong(Time.time * 6f, 0.3f);
                m_TxtWarning.color = new Color(1f, pulse, pulse);
            }

            // Horror Vignette Pulse
            if (m_HorrorVignette != null)
            {
                float vignetteAlpha = Mathf.Lerp(0.1f, 0.45f, dangerRatio) * (0.8f + Mathf.Sin(Time.time * 8f) * 0.2f);
                m_HorrorVignette.color = new Color(0.9f, 0.05f, 0.05f, vignetteAlpha);
            }

            // Heartbeat audio pulse
            m_HeartbeatTimer += Time.deltaTime;
            float heartbeatInterval = Mathf.Lerp(1.2f, 0.4f, dangerRatio);
            if (m_HeartbeatTimer >= heartbeatInterval)
            {
                m_HeartbeatTimer = 0f;
                if (m_AudioSource != null)
                {
                    m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetHeartbeatSound(), 0.65f + dangerRatio * 0.35f);
                }
            }
        }

        private void HideAlert()
        {
            if (m_AlertContainer != null && m_AlertContainer.activeSelf)
            {
                m_AlertContainer.SetActive(false);
            }

            if (m_HorrorVignette != null && m_HorrorVignette.color.a > 0f)
            {
                m_HorrorVignette.color = Color.Lerp(m_HorrorVignette.color, new Color(0.8f, 0f, 0f, 0f), Time.deltaTime * 3f);
            }
        }
    }
}
