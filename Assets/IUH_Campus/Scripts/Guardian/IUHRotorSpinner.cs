using UnityEngine;

namespace IUHCampus.Guardian
{
    /// <summary>
    /// Controls the rotational spin of a drone rotor blade.
    /// Modulates speed based on drone throttle and acceleration.
    /// </summary>
    public class IUHRotorSpinner : MonoBehaviour
    {
        [Header("Spin Configuration")]
        [SerializeField] private Vector3 m_RotationAxis = Vector3.up;
        [SerializeField] private float m_BaseSpeed = 900f; // degrees per second
        [SerializeField] private float m_MaxSpeed = 2400f;
        [SerializeField] private bool m_Clockwise = true;

        private float m_CurrentSpeed = 900f;
        private float m_TargetSpeed = 900f;

        public void SetThrottle(float throttle01)
        {
            throttle01 = Mathf.Clamp01(throttle01);
            m_TargetSpeed = Mathf.Lerp(m_BaseSpeed, m_MaxSpeed, throttle01);
        }

        public void SetSpinActive(bool active)
        {
            m_TargetSpeed = active ? m_BaseSpeed : 0f;
        }

        private void Update()
        {
            m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, m_TargetSpeed, Time.deltaTime * 8f);
            if (m_CurrentSpeed > 1f)
            {
                float dir = m_Clockwise ? 1f : -1f;
                transform.Rotate(m_RotationAxis, dir * m_CurrentSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
