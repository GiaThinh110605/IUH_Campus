using UnityEngine;

namespace IUHCampus
{
    /// <summary>
    /// Procedural Humanoid Character Animator.
    /// Drives realistic procedural walk, run, and idle cycles for student humanoid model:
    /// alternating arm swings, leg strides, and rhythmic torso bobbing.
    /// </summary>
    public class IUHCharacterAnimator : MonoBehaviour
    {
        [Header("Limb References")]
        [SerializeField] private Transform m_ArmLeft;
        [SerializeField] private Transform m_ArmRight;
        [SerializeField] private Transform m_LegLeft;
        [SerializeField] private Transform m_LegRight;
        [SerializeField] private Transform m_Torso;

        [Header("Animation Parameters")]
        [SerializeField] private float m_WalkStepFreq = 8.5f;
        [SerializeField] private float m_RunStepFreq = 13.5f;
        [SerializeField] private float m_ArmSwingAngle = 36.0f;
        [SerializeField] private float m_LegStrideAngle = 34.0f;
        [SerializeField] private float m_BobAmount = 0.045f;

        private float m_Cycle = 0.0f;
        private Vector3 m_TorsoBasePos;
        private Quaternion m_ArmLeftBaseRot;
        private Quaternion m_ArmRightBaseRot;
        private Quaternion m_LegLeftBaseRot;
        private Quaternion m_LegRightBaseRot;

        private void Awake()
        {
            if (m_Torso != null) m_TorsoBasePos = m_Torso.localPosition;
            if (m_ArmLeft != null) m_ArmLeftBaseRot = m_ArmLeft.localRotation;
            if (m_ArmRight != null) m_ArmRightBaseRot = m_ArmRight.localRotation;
            if (m_LegLeft != null) m_LegLeftBaseRot = m_LegLeft.localRotation;
            if (m_LegRight != null) m_LegRightBaseRot = m_LegRight.localRotation;
        }

        public void BindLimbs(Transform torso, Transform armL, Transform armR, Transform legL, Transform legR)
        {
            m_Torso = torso;
            m_ArmLeft = armL;
            m_ArmRight = armR;
            m_LegLeft = legL;
            m_LegRight = legR;

            if (m_Torso != null) m_TorsoBasePos = m_Torso.localPosition;
            if (m_ArmLeft != null) m_ArmLeftBaseRot = m_ArmLeft.localRotation;
            if (m_ArmRight != null) m_ArmRightBaseRot = m_ArmRight.localRotation;
            if (m_LegLeft != null) m_LegLeftBaseRot = m_LegLeft.localRotation;
            if (m_LegRight != null) m_LegRightBaseRot = m_LegRight.localRotation;
        }

        public void UpdateAnimation(float currentSpeed, float walkSpeed, float runSpeed, bool isGrounded)
        {
            float speedRatio = Mathf.Clamp01(currentSpeed / runSpeed);
            bool isMoving = currentSpeed > 0.08f && isGrounded;

            if (isMoving)
            {
                float freq = Mathf.Lerp(m_WalkStepFreq, m_RunStepFreq, speedRatio);
                m_Cycle += Time.deltaTime * freq;

                float sin = Mathf.Sin(m_Cycle);
                float cos = Mathf.Cos(m_Cycle);
                float bob = Mathf.Abs(sin) * m_BobAmount * speedRatio;

                // Torso vertical bounce
                if (m_Torso != null)
                {
                    m_Torso.localPosition = m_TorsoBasePos + Vector3.up * bob;
                }

                // Arm swings (Arm Left swings opposite to Leg Left)
                float armAngle = sin * m_ArmSwingAngle * (0.6f + 0.4f * speedRatio);
                if (m_ArmLeft != null)
                {
                    m_ArmLeft.localRotation = m_ArmLeftBaseRot * Quaternion.Euler(armAngle, 0, 0);
                }
                if (m_ArmRight != null)
                {
                    m_ArmRight.localRotation = m_ArmRightBaseRot * Quaternion.Euler(-armAngle, 0, 0);
                }

                // Leg strides
                float legAngle = -sin * m_LegStrideAngle * (0.6f + 0.4f * speedRatio);
                if (m_LegLeft != null)
                {
                    m_LegLeft.localRotation = m_LegLeftBaseRot * Quaternion.Euler(legAngle, 0, 0);
                }
                if (m_LegRight != null)
                {
                    m_LegRight.localRotation = m_LegRightBaseRot * Quaternion.Euler(-legAngle, 0, 0);
                }
            }
            else
            {
                // Idle breathing posture
                float breath = Mathf.Sin(Time.time * 2.2f) * 0.008f;
                if (m_Torso != null) m_Torso.localPosition = Vector3.Lerp(m_Torso.localPosition, m_TorsoBasePos + Vector3.up * breath, Time.deltaTime * 6f);
                if (m_ArmLeft != null) m_ArmLeft.localRotation = Quaternion.Slerp(m_ArmLeft.localRotation, m_ArmLeftBaseRot, Time.deltaTime * 8f);
                if (m_ArmRight != null) m_ArmRight.localRotation = Quaternion.Slerp(m_ArmRight.localRotation, m_ArmRightBaseRot, Time.deltaTime * 8f);
                if (m_LegLeft != null) m_LegLeft.localRotation = Quaternion.Slerp(m_LegLeft.localRotation, m_LegLeftBaseRot, Time.deltaTime * 8f);
                if (m_LegRight != null) m_LegRight.localRotation = Quaternion.Slerp(m_LegRight.localRotation, m_LegRightBaseRot, Time.deltaTime * 8f);
            }
        }
    }
}
