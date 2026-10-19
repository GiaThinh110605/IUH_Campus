using System.Collections;
using UnityEngine;
using IUHCampus.Interaction;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Player
{
    /// <summary>
    /// Controls realistic humanoid sitting on benches/chairs and standing up.
    /// Handles camera-free look while seated, procedural bone posing in LateUpdate,
    /// and smooth positional transitions.
    /// </summary>
    public class IUHPlayerSittingController : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("Maximum distance to detect a bench for sitting")]
        public float benchDetectionRadius = 2.4f;

        [Header("Sitting Posture Tuning")]
        [Tooltip("Seat surface height above ground (standard bench is ~0.42m)")]
        [Range(0.35f, 0.55f)] public float targetSeatHeightAboveGround = 0.42f;
        [Range(60f, 95f)] public float thighAngle = 82f;
        [Range(0f, 20f)] public float legSpread = 6f;
        [Range(10f, 40f)] public float armRestAngle = 22f;
        [Range(40f, 80f)] public float elbowBendAngle = 60f;

        [Header("Transition")]
        [Tooltip("Time to sit down or stand up")]
        public float transitionDuration = 0.45f;

        [Header("State")]
        [SerializeField] private bool m_IsSitting = false;
        [SerializeField] private IUHBenchSeat m_TargetBench;

        public bool IsSitting => m_IsSitting;
        public IUHBenchSeat CurrentBench => m_TargetBench;

        // References
        private CharacterController m_CharacterController;
        private StarterAssets.ThirdPersonController m_ThirdPersonController;
        private StarterAssets.StarterAssetsInputs m_StarterInputs;
        private Animator m_Animator;

        // Humanoid Bones
        private Transform m_Hips;
        private Transform m_LeftUpperLeg;
        private Transform m_RightUpperLeg;
        private Transform m_LeftLowerLeg;
        private Transform m_RightLowerLeg;
        private Transform m_LeftUpperArm;
        private Transform m_RightUpperArm;
        private Transform m_LeftLowerArm;
        private Transform m_RightLowerArm;
        private Vector3 m_StandingHipLocalPos = new Vector3(0f, 0.98f, 0f);

        // Interpolation
        private float m_SitBlend = 0f;
        private Coroutine m_TransitionRoutine;
        private Vector3 m_StandPosition;
        private Quaternion m_StandRotation;
        private Vector3 m_SeatPosition;
        private Quaternion m_SeatRotation;
        private IUHBenchSeat m_NearbyBench;

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_ThirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
            m_StarterInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
            m_Animator = GetComponent<Animator>();

            CacheBones();
        }

        private void CacheBones()
        {
            if (m_Animator == null || !m_Animator.isHuman) return;

            m_Hips = m_Animator.GetBoneTransform(HumanBodyBones.Hips);
            if (m_Hips != null)
            {
                m_StandingHipLocalPos = m_Hips.localPosition;
            }

            m_LeftUpperLeg = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            m_RightUpperLeg = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            m_LeftLowerLeg = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            m_RightLowerLeg = m_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            m_LeftUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            m_RightUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            m_LeftLowerArm = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            m_RightLowerArm = m_Animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        }

        private void Update()
        {
            if (!m_IsSitting)
            {
                DetectNearbyBench();

                if (m_NearbyBench != null && IsInteractPressed())
                {
                    SitOnBench(m_NearbyBench);
                }
            }
            else
            {
                // Sitting: check for stand up trigger
                if (m_TransitionRoutine == null && (IsStandUpPressed() || HasMovementInput()))
                {
                    StandUp();
                }
            }
        }

        private bool IsInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.eKey.wasPressedThisFrame;
            }
#endif
            try { return Input.GetKeyDown(KeyCode.E); } catch { return false; }
        }

        private bool IsStandUpPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.eKey.wasPressedThisFrame ||
                       Keyboard.current.spaceKey.wasPressedThisFrame;
            }
#endif
            try { return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space); } catch { return false; }
        }

        private bool HasMovementInput()
        {
            if (m_StarterInputs != null && m_StarterInputs.move.sqrMagnitude > 0.1f) return true;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.wKey.wasPressedThisFrame ||
                       Keyboard.current.sKey.wasPressedThisFrame ||
                       Keyboard.current.aKey.wasPressedThisFrame ||
                       Keyboard.current.dKey.wasPressedThisFrame;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
                       Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D);
            }
            catch { return false; }
        }

        private void DetectNearbyBench()
        {
            var benches = FindObjectsByType<IUHBenchSeat>(FindObjectsSortMode.None);
            IUHBenchSeat closest = null;
            float minDist = benchDetectionRadius;

            Vector3 pos = transform.position;
            foreach (var b in benches)
            {
                if (b == null || b.IsOccupied) continue;
                float dist = Vector3.Distance(pos, b.transform.position);
                // Also check distance to seat surface
                Vector3 seatPos;
                Quaternion seatRot;
                b.GetSeatTransform(pos, out seatPos, out seatRot);
                float seatDist = Vector3.Distance(pos, seatPos);
                float effectiveDist = Mathf.Min(dist, seatDist);

                if (effectiveDist < minDist)
                {
                    minDist = effectiveDist;
                    closest = b;
                }
            }

            m_NearbyBench = closest;
        }

        public void SitOnBench(IUHBenchSeat bench)
        {
            if (bench == null || m_IsSitting || m_TransitionRoutine != null) return;

            m_TargetBench = bench;
            m_TargetBench.SetOccupant(gameObject);

            // Compute target seat transform
            m_StandPosition = transform.position;
            m_StandRotation = transform.rotation;
            bench.GetSeatTransform(m_StandPosition, out m_SeatPosition, out m_SeatRotation);

            if (m_TransitionRoutine != null) StopCoroutine(m_TransitionRoutine);
            m_TransitionRoutine = StartCoroutine(SitTransition(true));
        }

        public void StandUp()
        {
            if (!m_IsSitting || m_TransitionRoutine != null) return;

            // Stand position: slightly in front of the bench in the seat's facing direction
            Vector3 forward = m_SeatRotation * Vector3.forward;
            forward.y = 0;
            forward.Normalize();
            m_StandPosition = m_SeatPosition + forward * 0.75f;
            m_StandPosition.y = transform.position.y; // Keep current ground plane Y
            m_StandRotation = m_SeatRotation;

            if (m_TransitionRoutine != null) StopCoroutine(m_TransitionRoutine);
            m_TransitionRoutine = StartCoroutine(SitTransition(false));
        }

        private IEnumerator SitTransition(bool sittingDown)
        {
            if (sittingDown)
            {
                m_IsSitting = true;
                if (m_CharacterController != null) m_CharacterController.enabled = false;
                if (m_ThirdPersonController != null) m_ThirdPersonController.enabled = false;
                if (m_StarterInputs != null) m_StarterInputs.move = Vector2.zero;

                Vector3 startPos = transform.position;
                Quaternion startRot = transform.rotation;
                float elapsed = 0f;

                while (elapsed < transitionDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
                    transform.position = Vector3.Lerp(startPos, m_SeatPosition, t);
                    transform.rotation = Quaternion.Slerp(startRot, m_SeatRotation, t);
                    m_SitBlend = t;
                    yield return null;
                }

                transform.position = m_SeatPosition;
                transform.rotation = m_SeatRotation;
                m_SitBlend = 1f;
            }
            else
            {
                Vector3 startPos = transform.position;
                Quaternion startRot = transform.rotation;
                float elapsed = 0f;

                while (elapsed < transitionDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
                    transform.position = Vector3.Lerp(startPos, m_StandPosition, t);
                    transform.rotation = Quaternion.Slerp(startRot, m_StandRotation, t);
                    m_SitBlend = 1f - t;
                    yield return null;
                }

                transform.position = m_StandPosition;
                transform.rotation = m_StandRotation;
                m_SitBlend = 0f;

                if (m_TargetBench != null)
                {
                    m_TargetBench.SetOccupant(null);
                    m_TargetBench = null;
                }

                if (m_CharacterController != null) m_CharacterController.enabled = true;
                if (m_ThirdPersonController != null) m_ThirdPersonController.enabled = true;
                m_IsSitting = false;
            }

            m_TransitionRoutine = null;
        }

        private void LateUpdate()
        {
            if (m_SitBlend <= 0.001f || m_Animator == null) return;

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            // Hip sink down to bench seat cushion level
            if (m_Hips != null)
            {
                float standingHipY = m_StandingHipLocalPos.y;
                float actualSink = (standingHipY - targetSeatHeightAboveGround) * m_SitBlend;
                m_Hips.localPosition = new Vector3(m_StandingHipLocalPos.x, standingHipY - actualSink, m_StandingHipLocalPos.z);
            }

            // Left leg
            if (m_LeftUpperLeg != null && m_LeftLowerLeg != null)
            {
                Quaternion origKnee = m_LeftLowerLeg.rotation;
                Quaternion thighPitch = Quaternion.AngleAxis(-thighAngle * m_SitBlend, right);
                Quaternion thighSpread = Quaternion.AngleAxis(-legSpread * m_SitBlend, forward);

                m_LeftUpperLeg.rotation = (thighPitch * thighSpread) * m_LeftUpperLeg.rotation;
                m_LeftLowerLeg.rotation = origKnee;
            }

            // Right leg
            if (m_RightUpperLeg != null && m_RightLowerLeg != null)
            {
                Quaternion origKnee = m_RightLowerLeg.rotation;
                Quaternion thighPitch = Quaternion.AngleAxis(-thighAngle * m_SitBlend, right);
                Quaternion thighSpread = Quaternion.AngleAxis(legSpread * m_SitBlend, forward);

                m_RightUpperLeg.rotation = (thighPitch * thighSpread) * m_RightUpperLeg.rotation;
                m_RightLowerLeg.rotation = origKnee;
            }

            // Arms rested naturally
            if (m_LeftUpperArm != null && m_LeftLowerArm != null)
            {
                m_LeftUpperArm.rotation = Quaternion.AngleAxis(-armRestAngle * m_SitBlend, right) * m_LeftUpperArm.rotation;
                m_LeftLowerArm.rotation = Quaternion.AngleAxis(elbowBendAngle * m_SitBlend, right) * m_LeftLowerArm.rotation;
            }

            if (m_RightUpperArm != null && m_RightLowerArm != null)
            {
                m_RightUpperArm.rotation = Quaternion.AngleAxis(-armRestAngle * m_SitBlend, right) * m_RightUpperArm.rotation;
                m_RightLowerArm.rotation = Quaternion.AngleAxis(elbowBendAngle * m_SitBlend, right) * m_RightLowerArm.rotation;
            }
        }

        private void OnGUI()
        {
            if (m_IsSitting)
            {
                DrawPromptBadge("🛋️  Đang ngồi ghế • Nhấn [E], [Space] hoặc [WASD] để đứng dậy");
            }
            else if (m_NearbyBench != null)
            {
                DrawPromptBadge("🪑  Nhấn [E] để ngồi xuống ghế");
            }
        }

        private void DrawPromptBadge(string text)
        {
            int width = 520;
            int height = 48;
            int x = (Screen.width - width) / 2;
            int y = Screen.height - height - 40;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            boxStyle.normal.textColor = Color.white;

            // Background glassmorphism badge
            Color prevColor = GUI.color;
            GUI.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);
            GUI.Box(new Rect(x, y, width, height), GUIContent.none);

            // Border outline
            GUI.color = new Color(0.2f, 0.65f, 1.0f, 0.9f);
            GUI.Box(new Rect(x, y, width, 2), GUIContent.none);

            // Text
            GUI.color = Color.white;
            GUI.Label(new Rect(x, y, width, height), text, boxStyle);

            GUI.color = prevColor;
        }
    }
}
