using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public enum BalloonFlightState
    {
        Grounded,
        Ascending,
        AirborneHover,
        Descending
    }

    /// <summary>
    /// Điều khiển Khinh Khí Cầu Chiến Đấu IUH (IUH Cyber Hot Air Balloon Platform):
    /// Cung cấp trạm cất cánh lên độ cao chiến đấu 35m - 45m, ụ pháo phòng không hỗ trợ,
    /// cơ chế khóa an toàn chống rơi (Anti-Fall Lock) và trạm nạp năng lượng hồi phục cho nhân vật.
    /// </summary>
    public class IUHHotAirBalloonController : MonoBehaviour
    {
        public static IUHHotAirBalloonController Instance { get; private set; }

        [Header("Altitude & Flight")]
        [SerializeField] private float m_GroundY = 0.2f;
        [SerializeField] private float m_TargetAltitudeY = 36.0f;
        [SerializeField] private float m_VerticalSpeed = 6.0f;
        [SerializeField] private float m_BobAmplitude = 0.35f;
        [SerializeField] private float m_BobFrequency = 1.1f;

        [Header("Basket Platform & Anti-Fall Safety")]
        [SerializeField] private Transform m_BasketTransform;
        [SerializeField] private Transform m_PlayerAnchorPoint;
        [SerializeField] private float m_InteractionRadius = 6.5f;
        [SerializeField] private float m_MaxWalkRadius = 1.85f;
        [SerializeField] private float m_FloorLocalY = 0.22f;
        [SerializeField] private Text m_BillboardPromptText;
        [SerializeField] private GameObject m_EnergyShieldVisual;

        [Header("Automated Plasma Turrets")]
        [SerializeField] private Transform[] m_TurretPivots;
        [SerializeField] private Transform[] m_TurretMuzzles;
        [SerializeField] private LineRenderer[] m_TurretLasers;
        [SerializeField] private float m_TurretRange = 85f;
        [SerializeField] private float m_TurretFireInterval = 0.6f;
        [SerializeField] private int m_TurretDamage = 35;

        [Header("Audio & Effects")]
        [SerializeField] private ParticleSystem m_BurnerFlameVFX;
        [SerializeField] private AudioSource m_BurnerAudioSource;

        private BalloonFlightState m_State = BalloonFlightState.Grounded;
        private Transform m_PlayerTransform;
        private CharacterController m_PlayerCC;
        private bool m_IsPlayerOnBoard = false;
        private float m_NextTurretFireTime = 0f;
        private Vector3 m_BasePosition;
        private float m_CurrentY;

        public BalloonFlightState State => m_State;
        public bool IsPlayerOnBoard => m_IsPlayerOnBoard;
        public bool IsPlayerInBasket => m_IsPlayerOnBoard;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_BasePosition = transform.position;
            m_CurrentY = m_BasePosition.y;

            if (m_BasketTransform == null) m_BasketTransform = transform.Find("Combat_Basket") ?? transform;
            if (m_PlayerAnchorPoint == null) m_PlayerAnchorPoint = transform.Find("Combat_Basket/PlayerAnchorPoint") ?? m_BasketTransform;
        }

        private void Start()
        {
            FindPlayer();
            SetShieldActive(false);
            SetBurnerActive(false);
            UpdatePrompt();
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null)
            {
                m_PlayerTransform = student.transform;
                m_PlayerCC = student.GetComponent<CharacterController>();
            }
        }

        private void Update()
        {
            if (m_PlayerTransform == null) FindPlayer();

            CheckPlayerBoardingState();
            HandleInteractionInput();
            UpdateFlightMotion();
            UpdateAutomatedTurrets();
        }

        private void LateUpdate()
        {
            // Anti-Fall Lock System: Khóa an toàn giữ người chơi luôn ở trên sàn giỏ khinh khí cầu
            EnforceAntiFallBasketConstraint();
        }

        private void CheckPlayerBoardingState()
        {
            if (m_PlayerTransform == null || m_BasketTransform == null) return;

            // Nếu người chơi đang kích hoạt cánh bay tự do thì tự động rời khinh khí cầu
            if (IUHAerialFlightController.Instance != null && IUHAerialFlightController.Instance.IsFlying)
            {
                if (m_IsPlayerOnBoard)
                {
                    DisembarkPlayer();
                }
                UpdatePrompt();
                return;
            }

            // Khi ở mặt đất: Tự động phát hiện người chơi bước vào giỏ
            if (m_State == BalloonFlightState.Grounded)
            {
                Vector3 basketCenter = m_BasketTransform.position;
                Vector2 horizDist = new Vector2(m_PlayerTransform.position.x - basketCenter.x, m_PlayerTransform.position.z - basketCenter.z);

                if (horizDist.magnitude <= m_MaxWalkRadius && Mathf.Abs(m_PlayerTransform.position.y - basketCenter.y) < 2.0f)
                {
                    m_IsPlayerOnBoard = true;
                }
                else if (horizDist.magnitude > m_InteractionRadius)
                {
                    m_IsPlayerOnBoard = false;
                }
            }
            else
            {
                // Khi đang bay trên không (Ascending, AirborneHover, Descending):
                // Nếu người chơi đã lên khinh khí cầu, luôn duy trì trạng thái OnBoard
                // liên tục nạp đầy năng lượng bay (Stamina)
                if (m_IsPlayerOnBoard && IUHAerialFlightController.Instance != null)
                {
                    IUHAerialFlightController.Instance.InstantRecharge();
                }
            }

            UpdatePrompt();
        }

        public bool IsPlayerNear
        {
            get
            {
                if (m_PlayerTransform == null || m_BasketTransform == null) return false;
                return Vector3.Distance(m_PlayerTransform.position, m_BasketTransform.position) <= m_InteractionRadius || m_IsPlayerOnBoard;
            }
        }

        private void HandleInteractionInput()
        {
            if (m_PlayerTransform == null) return;
            float dist = Vector3.Distance(m_PlayerTransform.position, m_BasketTransform.position);
            bool inInteractRange = dist <= m_InteractionRadius || m_IsPlayerOnBoard;
            if (!inInteractRange) return;

            bool interactPressed = Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E) || IUHCampus.Core.IUHInputHelper.VirtualInteractPressed;
            if (interactPressed)
            {
                IUHCampus.Core.IUHInputHelper.VirtualInteractPressed = false;
                ToggleBoardOrLaunch();
            }
        }

        public void ToggleBoardOrLaunch()
        {
            if (m_State == BalloonFlightState.Grounded)
            {
                LaunchBalloon();
            }
            else if (m_State == BalloonFlightState.AirborneHover)
            {
                DescendBalloon();
            }
        }

        /// <summary>
        /// Hệ thống chống rơi: Khóa vị trí nhân vật trong bán kính sàn giỏ khinh khí cầu,
        /// hoàn toàn triệt tiêu nguy cơ rơi tự do do gravity hoặc giật lag vật lý khi nâng hạ độ cao.
        /// </summary>
        private void EnforceAntiFallBasketConstraint()
        {
            if (!m_IsPlayerOnBoard || m_PlayerTransform == null || m_BasketTransform == null) return;

            // Nếu người chơi đang bật chế độ bay tự do bằng cánh, không khóa vị trí
            if (IUHAerialFlightController.Instance != null && IUHAerialFlightController.Instance.IsFlying)
            {
                m_IsPlayerOnBoard = false;
                return;
            }

            // Chuyển tọa độ thế giới của nhân vật sang hệ tọa độ cục bộ của giỏ
            Vector3 localPos = m_BasketTransform.InverseTransformPoint(m_PlayerTransform.position);

            // Giới hạn bán kính ngang không vượt quá thành giỏ (m_MaxWalkRadius = 1.55m)
            Vector2 horizontal = new Vector2(localPos.x, localPos.z);
            if (horizontal.magnitude > m_MaxWalkRadius)
            {
                horizontal = horizontal.normalized * m_MaxWalkRadius;
                localPos.x = horizontal.x;
                localPos.z = horizontal.y;
            }

            // Cố định độ cao Y đứng trên mặt sàn giỏ khinh khí cầu
            localPos.y = m_FloorLocalY;

            // Áp dụng vị trí thế giới mới trực tiếp vào Transform
            Vector3 lockedWorldPos = m_BasketTransform.TransformPoint(localPos);

            if (m_PlayerCC != null)
            {
                m_PlayerCC.enabled = false;
                m_PlayerTransform.position = lockedWorldPos;
                m_PlayerCC.enabled = true;
            }
            else
            {
                m_PlayerTransform.position = lockedWorldPos;
            }
        }

        public void LaunchBalloon()
        {
            if (m_State == BalloonFlightState.Ascending || m_State == BalloonFlightState.AirborneHover) return;

            // Đưa người chơi an toàn vào tâm giỏ và kích hoạt cất cánh
            m_IsPlayerOnBoard = true;
            SnapPlayerToBasket();

            m_State = BalloonFlightState.Ascending;
            SetBurnerActive(true);
            SetShieldActive(true);

            IUHCombatAudio.PlayLaserShoot();
        }

        public void DescendBalloon()
        {
            if (m_State == BalloonFlightState.Descending || m_State == BalloonFlightState.Grounded) return;

            m_State = BalloonFlightState.Descending;
            SetBurnerActive(false);
        }

        public void DisembarkPlayer()
        {
            m_IsPlayerOnBoard = false;
        }

        private void SnapPlayerToBasket()
        {
            if (m_PlayerTransform == null || m_BasketTransform == null) return;

            Vector3 spawnTarget = m_PlayerAnchorPoint != null ? m_PlayerAnchorPoint.position : (m_BasketTransform.position + Vector3.up * m_FloorLocalY);

            if (m_PlayerCC != null) m_PlayerCC.enabled = false;
            m_PlayerTransform.position = spawnTarget;
            if (m_PlayerCC != null) m_PlayerCC.enabled = true;
        }

        private void UpdateFlightMotion()
        {
            switch (m_State)
            {
                case BalloonFlightState.Grounded:
                    m_CurrentY = m_GroundY;
                    break;

                case BalloonFlightState.Ascending:
                    m_CurrentY = Mathf.MoveTowards(m_CurrentY, m_TargetAltitudeY, m_VerticalSpeed * Time.deltaTime);
                    if (Mathf.Abs(m_CurrentY - m_TargetAltitudeY) < 0.05f)
                    {
                        m_CurrentY = m_TargetAltitudeY;
                        m_State = BalloonFlightState.AirborneHover;
                    }
                    break;

                case BalloonFlightState.AirborneHover:
                    float bob = Mathf.Sin(Time.time * m_BobFrequency) * m_BobAmplitude;
                    m_CurrentY = m_TargetAltitudeY + bob;
                    break;

                case BalloonFlightState.Descending:
                    m_CurrentY = Mathf.MoveTowards(m_CurrentY, m_GroundY, m_VerticalSpeed * 0.9f * Time.deltaTime);
                    if (Mathf.Abs(m_CurrentY - m_GroundY) < 0.05f)
                    {
                        m_CurrentY = m_GroundY;
                        m_State = BalloonFlightState.Grounded;
                        SetShieldActive(false);
                    }
                    break;
            }

            Vector3 newPos = new Vector3(m_BasePosition.x, m_CurrentY, m_BasePosition.z);
            transform.position = newPos;
        }

        private void UpdateAutomatedTurrets()
        {
            // Tự động tìm diệt quái vật bay khi khinh khí cầu đang ở trên không
            if (m_State != BalloonFlightState.AirborneHover && m_State != BalloonFlightState.Ascending)
            {
                return;
            }

            IUHRealtimeMonsterTarget target = FindNearestAerialMonster();
            if (target == null) return;

            // Xoay ụ pháo hướng về mục tiêu
            if (m_TurretPivots != null)
            {
                foreach (var pivot in m_TurretPivots)
                {
                    if (pivot == null) continue;
                    Vector3 dir = (target.transform.position - pivot.position).normalized;
                    Quaternion lookRot = Quaternion.LookRotation(dir);
                    pivot.rotation = Quaternion.Slerp(pivot.rotation, lookRot, Time.deltaTime * 12f);
                }
            }

            if (Time.time >= m_NextTurretFireTime)
            {
                m_NextTurretFireTime = Time.time + m_TurretFireInterval;
                FireTurrets(target);
            }
        }

        private IUHRealtimeMonsterTarget FindNearestAerialMonster()
        {
            var targets = FindObjectsByType<IUHRealtimeMonsterTarget>(FindObjectsSortMode.None);
            IUHRealtimeMonsterTarget best = null;
            float closestDist = m_TurretRange;

            foreach (var t in targets)
            {
                if (t == null || !t.gameObject.activeInHierarchy || t.IsDead) continue;
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    best = t;
                }
            }
            return best;
        }

        private void FireTurrets(IUHRealtimeMonsterTarget target)
        {
            if (target == null) return;

            target.TakeDamage(m_TurretDamage);

            if (m_TurretLasers != null && m_TurretMuzzles != null)
            {
                for (int i = 0; i < m_TurretLasers.Length; i++)
                {
                    if (m_TurretLasers[i] == null || i >= m_TurretMuzzles.Length || m_TurretMuzzles[i] == null) continue;
                    StartCoroutine(LaserTracerRoutine(m_TurretLasers[i], m_TurretMuzzles[i].position, target.transform.position));
                }
            }

            IUHCombatAudio.PlayLaserShoot();
        }

        private IEnumerator LaserTracerRoutine(LineRenderer laser, Vector3 start, Vector3 end)
        {
            laser.enabled = true;
            laser.SetPosition(0, start);
            laser.SetPosition(1, end);
            yield return new WaitForSeconds(0.08f);
            if (laser != null) laser.enabled = false;
        }

        private void SetBurnerActive(bool active)
        {
            if (m_BurnerFlameVFX != null)
            {
                if (active) m_BurnerFlameVFX.Play(); else m_BurnerFlameVFX.Stop();
            }
            if (m_BurnerAudioSource != null)
            {
                if (active && !m_BurnerAudioSource.isPlaying) m_BurnerAudioSource.Play();
                else if (!active && m_BurnerAudioSource.isPlaying) m_BurnerAudioSource.Stop();
            }
        }

        private void SetShieldActive(bool active)
        {
            if (m_EnergyShieldVisual != null)
            {
                m_EnergyShieldVisual.SetActive(active);
            }
        }

        private void UpdatePrompt()
        {
            if (m_BillboardPromptText == null) return;

            if (m_PlayerTransform == null)
            {
                m_BillboardPromptText.gameObject.SetActive(false);
                return;
            }

            float dist = Vector3.Distance(m_PlayerTransform.position, m_BasketTransform.position);
            bool showPrompt = dist <= m_InteractionRadius * 1.5f || m_IsPlayerOnBoard;

            if (!showPrompt)
            {
                m_BillboardPromptText.gameObject.SetActive(false);
                return;
            }

            m_BillboardPromptText.gameObject.SetActive(true);

            if (m_State == BalloonFlightState.Grounded)
            {
                m_BillboardPromptText.text = "<color=#00E5FF>[F]</color> LÊN KHINH KHÍ CẦU KHÔNG CHIẾN\n<size=11><color=#FFD54F>Bay lên 36m • Giỏ khóa an toàn • Nạp năng lượng</color></size>";
            }
            else if (m_State == BalloonFlightState.AirborneHover)
            {
                m_BillboardPromptText.text = "<color=#FF5252>[F]</color> HẠ CÁNH  •  <color=#FFD54F>[K]</color> BẮN PHÁO HOA LỄ HỘI\n<size=11><color=#00E5FF>[Space x 2] hoặc [J] BUNG CÁNH BAY TỰ DO</color></size>";
            }
            else if (m_State == BalloonFlightState.Ascending)
            {
                m_BillboardPromptText.text = "<color=#FFD54F>ĐANG CẤT CÁNH LÊN 36M (KHÓA AN TOÀN KÍCH HOẠT)...</color>";
            }
            else if (m_State == BalloonFlightState.Descending)
            {
                m_BillboardPromptText.text = "<color=#FFB74D>ĐANG HẠ CÁNH VỀ SÂN TRƯỜNG...</color>";
            }

            Camera cam = Camera.main;
            if (cam != null && m_BillboardPromptText != null)
            {
                m_BillboardPromptText.transform.rotation = Quaternion.LookRotation(m_BillboardPromptText.transform.position - cam.transform.position);
            }
        }
    }
}