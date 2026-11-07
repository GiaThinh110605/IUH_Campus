using UnityEngine;

namespace IUHCampus.Guardian
{
    public class IUHCameraShake : MonoBehaviour
    {
        public static IUHCameraShake Instance { get; private set; }

        private Transform m_TargetTransform;
        private Vector3 m_OriginalLocalPos;
        private float m_ShakeTimeRemaining;
        private float m_CurrentShakeMagnitude;
        private float m_TotalShakeDuration;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_TargetTransform = transform;
            m_OriginalLocalPos = transform.localPosition;
        }

        private void OnEnable()
        {
            m_OriginalLocalPos = transform.localPosition;
        }

        public static void Shake(float duration = 0.2f, float magnitude = 0.3f)
        {
            if (Instance != null)
            {
                Instance.TriggerShake(duration, magnitude);
            }
            else
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    var comp = cam.GetComponent<IUHCameraShake>() ?? cam.gameObject.AddComponent<IUHCameraShake>();
                    comp.TriggerShake(duration, magnitude);
                }
            }
        }

        public void TriggerShake(float duration, float magnitude)
        {
            m_TotalShakeDuration = duration;
            m_ShakeTimeRemaining = duration;
            m_CurrentShakeMagnitude = Mathf.Max(m_CurrentShakeMagnitude, magnitude);
        }

        private void LateUpdate()
        {
            if (m_ShakeTimeRemaining > 0)
            {
                m_ShakeTimeRemaining -= Time.deltaTime;
                float damper = 1.0f - Mathf.Clamp01(1.0f - (m_ShakeTimeRemaining / Mathf.Max(0.01f, m_TotalShakeDuration)));

                float offsetX = (Random.value * 2.0f - 1.0f) * m_CurrentShakeMagnitude * damper;
                float offsetY = (Random.value * 2.0f - 1.0f) * m_CurrentShakeMagnitude * damper;

                m_TargetTransform.localPosition = m_OriginalLocalPos + new Vector3(offsetX, offsetY, 0);

                if (m_ShakeTimeRemaining <= 0)
                {
                    m_TargetTransform.localPosition = m_OriginalLocalPos;
                    m_CurrentShakeMagnitude = 0;
                }
            }
            else
            {
                m_OriginalLocalPos = m_TargetTransform.localPosition;
            }
        }
    }
}

