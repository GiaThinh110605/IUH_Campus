using System.Collections;
using UnityEngine;

namespace IUHCampus.Guardian.AR
{
    /// <summary>
    /// Điều khiển toàn bộ cử động sống động của Quái vật 3D trong không gian AR:
    /// - Nhấp nhô lơ lửng bồng bềnh (Hovering / Levitation)
    /// - Lắc lư nghiêng mình ma quái (Floating Sway & Demonic Roll)
    /// - Lồng ngực phập phồng hít thở (Breathing Pulse)
    /// - Luôn ngước nhìn và theo dõi ống kính camera người chơi
    /// - Động tác gầm rú, lao tới tung đòn (Attack Roar & Lunge)
    /// - Động tác giật lùi, co rúm khi bị người chơi vuốt chém trúng (Hit Flinch Reaction)
    /// </summary>
    public class IUHARMonsterLivingController : MonoBehaviour
    {
        [Header("Framing & Base Transforms")]
        public Vector3 BaseScale = new Vector3(0.40f, 0.40f, 0.40f);
        public Vector3 BasePosition = Vector3.zero;
        public Camera ARCamera;

        [Header("Animation States")]
        private float m_AttackTimer = 0f;
        private float m_HitTimer = 0f;
        private float m_BreatheSpeed = 2.8f;
        private float m_HoverSpeed = 2.2f;

        private Renderer[] m_Renderers;
        private MaterialPropertyBlock m_PropBlock;

        public void Initialize(Camera cam, Vector3 baseScale)
        {
            ARCamera = cam;
            BaseScale = baseScale;
            BasePosition = transform.localPosition;
            m_Renderers = GetComponentsInChildren<Renderer>(true);
            m_PropBlock = new MaterialPropertyBlock();
        }

        public void TriggerAttackRoar()
        {
            m_AttackTimer = 0.75f;
        }

        public void TriggerHitReaction()
        {
            m_HitTimer = 0.38f;
            StartCoroutine(FlashHitRoutine());
        }

        private IEnumerator FlashHitRoutine()
        {
            if (m_Renderers == null || m_Renderers.Length == 0) yield break;

            // Chớp sáng đỏ / trắng cảnh báo trúng đòn
            for (int f = 0; f < 2; f++)
            {
                SetMonsterColorTint(new Color(1f, 0.2f, 0.2f, 1f));
                yield return new WaitForSeconds(0.06f);
                SetMonsterColorTint(Color.white);
                yield return new WaitForSeconds(0.06f);
            }
        }

        private void SetMonsterColorTint(Color color)
        {
            if (m_Renderers == null) return;
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                if (m_Renderers[i] != null)
                {
                    m_Renderers[i].GetPropertyBlock(m_PropBlock);
                    m_PropBlock.SetColor("_BaseColor", color);
                    m_PropBlock.SetColor("_Color", color);
                    m_Renderers[i].SetPropertyBlock(m_PropBlock);
                }
            }
        }

        private void Update()
        {
            FaceCamera();

            float t = Time.time;
            float hoverY = Mathf.Sin(t * m_HoverSpeed) * 0.075f;
            float swayPitch = Mathf.Cos(t * 1.9f) * 3.8f;
            float swayRoll = Mathf.Sin(t * 1.5f) * 4.5f;
            float breathe = 1.0f + Mathf.Sin(t * m_BreatheSpeed) * 0.045f;

            Vector3 currentPos = BasePosition + new Vector3(0f, hoverY, 0f);
            Vector3 currentScale = new Vector3(BaseScale.x * breathe, BaseScale.y * (2.0f - breathe), BaseScale.z * breathe);
            Quaternion currentRot = Quaternion.Euler(swayPitch, 0f, swayRoll);

            ApplyCombatOffsets(ref currentPos, ref currentScale, ref currentRot);

            transform.localPosition = currentPos;
            transform.localRotation = currentRot;
            transform.localScale = currentScale;
        }

        private void FaceCamera()
        {
            if (ARCamera == null || transform.parent == null) return;
            Vector3 toCam = ARCamera.transform.position - transform.parent.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.001f)
            {
                transform.parent.rotation = Quaternion.LookRotation(toCam);
            }
        }

        private void ApplyCombatOffsets(ref Vector3 currentPos, ref Vector3 currentScale, ref Quaternion currentRot)
        {
            if (m_AttackTimer > 0f)
            {
                m_AttackTimer -= Time.deltaTime;
                float progress = 1f - Mathf.Clamp01(m_AttackTimer / 0.75f);
                float lungeZ = Mathf.Sin(progress * Mathf.PI) * 0.28f;
                float shake = Mathf.Sin(progress * 38f) * 0.025f;
                currentPos += new Vector3(shake, 0.06f, lungeZ);
                currentScale *= 1.12f;
                currentRot *= Quaternion.Euler(Mathf.Sin(progress * Mathf.PI) * 12f, 0f, 0f);
            }

            if (m_HitTimer > 0f)
            {
                m_HitTimer -= Time.deltaTime;
                float progress = 1f - Mathf.Clamp01(m_HitTimer / 0.38f);
                float flinchZ = Mathf.Sin(progress * Mathf.PI) * 0.16f;
                float recoilX = (Random.value - 0.5f) * 0.04f;
                currentPos += new Vector3(recoilX, 0f, -flinchZ);
                currentScale *= 0.94f;
            }
        }
    }
}
