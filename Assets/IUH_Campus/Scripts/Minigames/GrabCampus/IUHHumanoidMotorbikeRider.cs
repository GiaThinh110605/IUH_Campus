using UnityEngine;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Procedural Humanoid Motorbike Rider Controller.
    /// Uses the authentic IUH student Humanoid character model (PlayerArmature)
    /// and poses the 68-bone skeleton procedurally in LateUpdate into a realistic
    /// motorbike riding posture with helmet, backpack, and dynamic leaning into turns.
    /// </summary>
    public class IUHHumanoidMotorbikeRider : MonoBehaviour
    {
        [Header("Sitting & Posture Tuning")]
        [Tooltip("Seat height above bike root")]
        public float seatHeight = 0.58f;
        [Tooltip("Seat offset along bike forward axis")]
        public float seatForwardOffset = -0.05f;
        [Range(40f, 85f)] public float thighAngle = 65f;
        [Range(5f, 25f)] public float legSpread = 14f;
        [Range(20f, 60f)] public float kneeBend = 45f;
        [Range(10f, 35f)] public float spineLean = 18f;
        [Range(20f, 65f)] public float armReach = 42f;
        [Range(10f, 30f)] public float armInward = 16f;
        [Range(20f, 55f)] public float elbowBend = 35f;

        private Animator m_Animator;
        private Transform m_Hips;
        private Transform m_LeftUpperLeg;
        private Transform m_RightUpperLeg;
        private Transform m_LeftLowerLeg;
        private Transform m_RightLowerLeg;
        private Transform m_LeftFoot;
        private Transform m_RightFoot;
        private Transform m_Spine;
        private Transform m_Chest;
        private Transform m_Head;
        private Transform m_LeftUpperArm;
        private Transform m_RightUpperArm;
        private Transform m_LeftLowerArm;
        private Transform m_RightLowerArm;

        private Vector3 m_DefaultHipLocalPos = new Vector3(0f, 0.98f, -0.02f);
        private Transform m_BikeTransform;
        private IUHGrabMotorbike m_Motorbike;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            if (m_Animator != null)
            {
                m_Animator.enabled = false; // Disable default walk/run animations
            }

            CacheBones();
            m_BikeTransform = transform.parent;
            if (m_BikeTransform != null)
            {
                m_Motorbike = m_BikeTransform.GetComponent<IUHGrabMotorbike>();
            }
        }

        private void CacheBones()
        {
            if (m_Animator == null || !m_Animator.isHuman) return;

            m_Hips = m_Animator.GetBoneTransform(HumanBodyBones.Hips);
            if (m_Hips != null) m_DefaultHipLocalPos = m_Hips.localPosition;

            m_LeftUpperLeg = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            m_RightUpperLeg = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            m_LeftLowerLeg = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            m_RightLowerLeg = m_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            m_LeftFoot = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            m_RightFoot = m_Animator.GetBoneTransform(HumanBodyBones.RightFoot);

            m_Spine = m_Animator.GetBoneTransform(HumanBodyBones.Spine);
            m_Chest = m_Animator.GetBoneTransform(HumanBodyBones.Chest);
            m_Head = m_Animator.GetBoneTransform(HumanBodyBones.Head);

            m_LeftUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            m_RightUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            m_LeftLowerArm = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            m_RightLowerArm = m_Animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        }

        private void LateUpdate()
        {
            if (m_Hips == null) return;

            // 1. Position character root and hips firmly on the motorbike seat
            transform.localPosition = new Vector3(0f, seatHeight - m_DefaultHipLocalPos.y, seatForwardOffset);
            transform.localRotation = Quaternion.identity;

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector3 up = transform.up;

            // Lean into turns if motorbike is leaning
            float steerLean = 0f;
            if (m_Motorbike != null)
            {
                steerLean = -m_Motorbike.CurrentBankAngle * 0.4f;
            }

            // 2. Upper body leaning forward in riding posture
            if (m_Spine != null)
            {
                Quaternion spinePitch = Quaternion.AngleAxis(-spineLean, right);
                Quaternion spineRoll = Quaternion.AngleAxis(steerLean, forward);
                m_Spine.rotation = (spinePitch * spineRoll) * m_Spine.rotation;
            }

            if (m_Chest != null)
            {
                Quaternion chestPitch = Quaternion.AngleAxis(-6f, right);
                m_Chest.rotation = chestPitch * m_Chest.rotation;
            }

            // 3. Head tilted slightly upward to look forward at the road ahead
            if (m_Head != null)
            {
                Quaternion headPitch = Quaternion.AngleAxis(spineLean + 6f, right);
                m_Head.rotation = headPitch * m_Head.rotation;
            }

            // 4. Legs straddling the bike
            if (m_LeftUpperLeg != null && m_LeftLowerLeg != null)
            {
                Quaternion origKnee = m_LeftLowerLeg.rotation;
                Quaternion thighPitch = Quaternion.AngleAxis(-thighAngle, right);
                Quaternion thighSpread = Quaternion.AngleAxis(-legSpread, forward);
                m_LeftUpperLeg.rotation = (thighPitch * thighSpread) * m_LeftUpperLeg.rotation;
                m_LeftLowerLeg.rotation = origKnee * Quaternion.AngleAxis(kneeBend, Vector3.right);
            }

            if (m_RightUpperLeg != null && m_RightLowerLeg != null)
            {
                Quaternion origKnee = m_RightLowerLeg.rotation;
                Quaternion thighPitch = Quaternion.AngleAxis(-thighAngle, right);
                Quaternion thighSpread = Quaternion.AngleAxis(legSpread, forward);
                m_RightUpperLeg.rotation = (thighPitch * thighSpread) * m_RightUpperLeg.rotation;
                m_RightLowerLeg.rotation = origKnee * Quaternion.AngleAxis(kneeBend, Vector3.right);
            }

            // 5. Arms reaching forward to grasp the left & right handlebars
            if (m_LeftUpperArm != null && m_LeftLowerArm != null)
            {
                Quaternion armPitch = Quaternion.AngleAxis(-armReach, right);
                Quaternion armIn = Quaternion.AngleAxis(-armInward, up);
                m_LeftUpperArm.rotation = (armPitch * armIn) * m_LeftUpperArm.rotation;
                m_LeftLowerArm.rotation = Quaternion.AngleAxis(elbowBend, right) * m_LeftLowerArm.rotation;
            }

            if (m_RightUpperArm != null && m_RightLowerArm != null)
            {
                Quaternion armPitch = Quaternion.AngleAxis(-armReach, right);
                Quaternion armIn = Quaternion.AngleAxis(armInward, up);
                m_RightUpperArm.rotation = (armPitch * armIn) * m_RightUpperArm.rotation;
                m_RightLowerArm.rotation = Quaternion.AngleAxis(elbowBend, right) * m_RightLowerArm.rotation;
            }
        }
    }
}
