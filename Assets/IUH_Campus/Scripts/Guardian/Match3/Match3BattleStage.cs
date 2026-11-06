using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace IUHCampus.Guardian.Match3
{
    /// <summary>
    /// Turns the spot where the encounter happened into a side-on duel:
    /// the student stands on the left, the Data Monster on the right, and the main
    /// camera frames both in the upper part of the screen so the board fits below.
    /// Everything is restored on <see cref="Exit"/>.
    /// </summary>
    public class Match3BattleStage : MonoBehaviour
    {
        [Header("Framing")]
        [Tooltip("0 = automatic (monster size + 2 m, at least 3.5 m).")]
        [SerializeField] private float m_DuelDistance = 0f;
        [SerializeField] private float m_CameraFov = 40f;
        [SerializeField] private float m_CameraElevation = 1.2f;
        [Tooltip("Screen height (0-1 from the bottom) where the ground line sits. The board covers the area below.")]
        [SerializeField, Range(0.2f, 0.6f)] private float m_GroundScreenY = 0.47f;
        [Tooltip("Share of the screen height the taller fighter should fill.")]
        [SerializeField, Range(0.2f, 0.6f)] private float m_FighterScreenHeight = 0.36f;

        [Header("Hit Feedback")]
        [SerializeField] private float m_LungeDistance = 0.9f;
        [SerializeField] private float m_LungeDuration = 0.28f;
        [SerializeField] private float m_ShakeAmount = 0.12f;
        [SerializeField] private float m_ShakeDuration = 0.25f;

        private Transform m_Player;
        private Transform m_Monster;
        private Camera m_Camera;
        private CinemachineBrain m_Brain;
        private bool m_BrainWasEnabled;

        private Vector3 m_PlayerStartPos;
        private Quaternion m_PlayerStartRot;
        private Quaternion m_MonsterStartRot;
        private Vector3 m_MonsterStartScale;
        private Vector3 m_CamStartPos;
        private Quaternion m_CamStartRot;
        private float m_CamStartFov;
        private GameObject m_PlayerOverhead;
        private bool m_PlayerOverheadWasActive;

        private Vector3 m_PlayerHome;
        private Vector3 m_MonsterHome;

        public bool IsActive { get; private set; }

        public void Enter(Transform player, Transform monster)
        {
            if (IsActive) Exit(false);
            if (player == null || monster == null) return;

            m_Player = player;
            m_Monster = monster;
            m_PlayerStartPos = player.position;
            m_PlayerStartRot = player.rotation;
            m_MonsterStartRot = monster.rotation;
            m_MonsterStartScale = monster.localScale;

            Bounds monsterBounds = GetBounds(monster);
            Bounds playerBounds = GetBounds(player);

            Vector3 away = Flat(player.position - monster.position);
            if (away.sqrMagnitude < 0.01f) away = -Flat(monster.forward);
            if (away.sqrMagnitude < 0.01f) away = Vector3.back;
            away.Normalize();

            float monsterRadius = Mathf.Max(monsterBounds.extents.x, monsterBounds.extents.z);
            float distance = m_DuelDistance > 0f ? m_DuelDistance : Mathf.Max(3.5f, monsterRadius + 2f);

            Vector3 duelPos = monster.position + away * distance;
            duelPos.y = player.position.y;
            Teleport(player, duelPos);

            player.rotation = Quaternion.LookRotation(-away, Vector3.up);
            monster.rotation = Quaternion.LookRotation(away, Vector3.up);
            StopLocomotion(player);

            m_PlayerHome = player.position;
            m_MonsterHome = monster.position;

            HidePlayerOverhead();
            TakeCamera(monsterBounds, playerBounds);
            IsActive = true;
        }

        public void Exit(bool restorePlayerPosition)
        {
            if (!IsActive) return;
            IsActive = false;
            StopAllCoroutines();

            if (m_Monster != null)
            {
                m_Monster.position = m_MonsterHome;
                m_Monster.rotation = m_MonsterStartRot;
                m_Monster.localScale = m_MonsterStartScale;
            }

            if (m_Player != null)
            {
                Teleport(m_Player, restorePlayerPosition ? m_PlayerStartPos : m_PlayerHome);
                if (restorePlayerPosition) m_Player.rotation = m_PlayerStartRot;
            }

            ReleaseCamera();

            if (m_PlayerOverhead != null) m_PlayerOverhead.SetActive(m_PlayerOverheadWasActive);
            m_PlayerOverhead = null;
            m_Player = null;
            m_Monster = null;
        }

        public void PlayPlayerAttack() => Lunge(m_Player, m_PlayerHome, m_Monster, m_LungeDistance);
        public void PlayMonsterAttack(bool charged) => Lunge(m_Monster, m_MonsterHome, m_Player, m_LungeDistance * (charged ? 1.6f : 1f));
        public void PlayPlayerHit() => Shake(m_Player, m_PlayerHome);
        public void PlayMonsterHit() => Shake(m_Monster, m_MonsterHome);

        public IEnumerator PlayMonsterDefeat(float duration = 0.6f)
        {
            if (m_Monster == null) yield break;
            Vector3 from = m_Monster.localScale;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                m_Monster.localScale = Vector3.Lerp(from, from * 0.05f, k * k);
                m_Monster.Rotate(Vector3.up, 720f * Time.deltaTime, Space.World);
                yield return null;
            }
        }

        private void Lunge(Transform actor, Vector3 home, Transform target, float distance)
        {
            if (!IsActive || actor == null || target == null) return;
            StartCoroutine(LungeRoutine(actor, home, target, distance));
        }

        private IEnumerator LungeRoutine(Transform actor, Vector3 home, Transform target, float distance)
        {
            Vector3 dir = Flat(target.position - home).normalized;
            float half = m_LungeDuration * 0.5f;
            for (float t = 0f; t < m_LungeDuration; t += Time.deltaTime)
            {
                float k = t < half ? t / half : 1f - (t - half) / half;
                actor.position = home + dir * (distance * Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            actor.position = home;
        }

        private void Shake(Transform actor, Vector3 home)
        {
            if (!IsActive || actor == null) return;
            StartCoroutine(ShakeRoutine(actor, home));
        }

        private IEnumerator ShakeRoutine(Transform actor, Vector3 home)
        {
            // Wait for the attacker's lunge to land before reacting.
            yield return new WaitForSeconds(m_LungeDuration * 0.5f);
            for (float t = 0f; t < m_ShakeDuration; t += Time.deltaTime)
            {
                float fade = 1f - t / m_ShakeDuration;
                Vector2 r = Random.insideUnitCircle * (m_ShakeAmount * fade);
                actor.position = home + new Vector3(r.x, 0f, r.y);
                yield return null;
            }
            actor.position = home;
        }

        private void TakeCamera(Bounds monsterBounds, Bounds playerBounds)
        {
            m_Camera = Camera.main;
            if (m_Camera == null) return;

            m_Brain = m_Camera.GetComponent<CinemachineBrain>();
            m_BrainWasEnabled = m_Brain != null && m_Brain.enabled;
            if (m_Brain != null) m_Brain.enabled = false;

            m_CamStartPos = m_Camera.transform.position;
            m_CamStartRot = m_Camera.transform.rotation;
            m_CamStartFov = m_Camera.fieldOfView;

            float ground = Mathf.Min(m_PlayerHome.y, monsterBounds.min.y);
            float tallest = Mathf.Max(playerBounds.max.y, monsterBounds.max.y) - ground;
            tallest = Mathf.Max(tallest, 1.8f);

            float separation = Vector3.Distance(Flat(m_PlayerHome), Flat(m_MonsterHome))
                               + Mathf.Max(monsterBounds.extents.x, monsterBounds.extents.z) + 0.6f;

            // Visible height so the fighters fill the requested share, and wide enough to fit both.
            float aspect = Mathf.Max(0.5f, m_Camera.aspect);
            float visibleHeight = Mathf.Max(tallest / m_FighterScreenHeight, separation * 1.35f / aspect);
            float halfFov = m_CameraFov * 0.5f * Mathf.Deg2Rad;
            float camDistance = visibleHeight * 0.5f / Mathf.Tan(halfFov);

            Vector3 right = Flat(m_MonsterHome - m_PlayerHome).normalized;
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            Vector3 mid = (m_PlayerHome + m_MonsterHome) * 0.5f;
            mid.y = ground + (0.5f - m_GroundScreenY) * visibleHeight;

            Vector3 camPos = mid - forward * camDistance + Vector3.up * m_CameraElevation;
            m_Camera.transform.SetPositionAndRotation(camPos, Quaternion.LookRotation(mid - camPos, Vector3.up));
            m_Camera.fieldOfView = m_CameraFov;
        }

        private void ReleaseCamera()
        {
            if (m_Camera == null) return;
            m_Camera.transform.SetPositionAndRotation(m_CamStartPos, m_CamStartRot);
            m_Camera.fieldOfView = m_CamStartFov;

            if (m_Brain != null && m_BrainWasEnabled)
            {
                m_Brain.enabled = true;
                if (m_Brain.ActiveVirtualCamera is CinemachineVirtualCameraBase vcam)
                {
                    vcam.PreviousStateIsValid = false;
                }
            }
            m_Camera = null;
            m_Brain = null;
        }

        private void HidePlayerOverhead()
        {
            m_PlayerOverhead = IUHPlayerOverheadHUD.Instance != null ? IUHPlayerOverheadHUD.Instance.gameObject : null;
            m_PlayerOverheadWasActive = m_PlayerOverhead != null && m_PlayerOverhead.activeSelf;
            if (m_PlayerOverhead != null) m_PlayerOverhead.SetActive(false);
        }

        private static void Teleport(Transform t, Vector3 pos)
        {
            var cc = t.GetComponent<CharacterController>();
            bool ccEnabled = cc != null && cc.enabled;
            if (ccEnabled) cc.enabled = false;
            t.position = pos;
            if (ccEnabled) cc.enabled = true;
        }

        private static void StopLocomotion(Transform t)
        {
            var animator = t.GetComponentInChildren<Animator>();
            if (animator == null) return;
            foreach (var p in animator.parameters)
            {
                if (p.type != AnimatorControllerParameterType.Float) continue;
                if (p.name == "Speed" || p.name == "MotionSpeed") animator.SetFloat(p.name, 0f);
            }
        }

        private static Bounds GetBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            bool any = false;
            Bounds b = new Bounds(root.position + Vector3.up, Vector3.one * 1.8f);
            foreach (var r in renderers)
            {
                if (!r.enabled || r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
                if (r.GetComponentInParent<Canvas>() != null) continue;
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
