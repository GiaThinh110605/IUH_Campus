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
    /// và điểm nạp năng lượng hồi phục cho nhân vật.
    /// </summary>
    public class IUHHotAirBalloonController : MonoBehaviour
    {
        public static IUHHotAirBalloonController Instance { get; private set; }

        [Header("Altitude & Flight")]
        [SerializeField] private float m_GroundY = 0.3f;
        [SerializeField] private float m_TargetAltitudeY = 36.0f;
        [SerializeField] private float m_VerticalSpeed = 5.5f;
        [SerializeField] private float m_BobAmplitude = 0.45f;
        [SerializeField] private float m_BobFrequency = 1.2f;

        [Header("Basket Platform & Interaction")]
        [SerializeField] private Transform m_BasketTransform;
        [SerializeField] private Transform m_PlayerAnchorPoint;
        [SerializeField] private float m_InteractionRadius = 4.2f;
        [SerializeField] private Text m_BillboardPromptText;
        [SerializeField] private GameObject m_EnergyShieldVisual;

        [Header("Automated Plasma Turrets")]
        [SerializeField] private Transform[] m_TurretPivots;
        [SerializeField] private Transform[] m_TurretMuzzles;
        [SerializeField] private LineRenderer[] m_TurretLasers;
        [SerializeField] private float m_TurretRange = 75f;
        [SerializeField] private float m_TurretFireInterval = 0.65f;
        [SerializeField] private int m_TurretDamage = 35;

        [Header("Audio & Effects")]
        [SerializeField] private ParticleSystem m_BurnerFlameVFX;
        [SerializeField] private AudioSource m_BurnerAudioSource;

        private BalloonFlightState m_State = BalloonFlightState.Grounded;
        private Transform m_PlayerTransform;
        private bool m_IsPlayerInBasket = false;
        private float m_NextTurretFireTime = 0f;
        private Vector3 m_BasePosition;
        private float m_CurrentY;

        public BalloonFlightState State => m_State;
        public bool IsPlayerInBasket => m_IsPlayerInBasket;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_BasePosition = transform.position;
            m_CurrentY = m_BasePosition.y;

            if (m_BasketTransform == null) m_BasketTransform = transform;
            if (m_PlayerAnchorPoint == null) m_PlayerAnchorPoint = m_BasketTransform;
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
            if (student != null) m_PlayerTransform = student.transform;
        }

        private void Update()
        {
            if (m_PlayerTransform == null) FindPlayer();

            CheckPlayerBasketProximity();
            HandleInteractionInput();
            UpdateFlightMotion();
            UpdateAutomatedTurrets();
        }

        private void CheckPlayerBasketProximity()
        {
            if (m_PlayerTransform == null || m_BasketTransform == null) return;

            float dist = Vector3.Distance(m_PlayerTransform.position, m_BasketTransform.position);
            bool inRange = dist <= m_InteractionRadius;

            // Kiểm tra xem người chơi có đang đứng trong giỏ
            if (m_State == BalloonFlightState.Ascending || m_State == BalloonFlightState.AirborneHover)
            {
                m_IsPlayerInBasket = inRange;
                if (m_IsPlayerInBasket)
                {
                    // Nạp đầy stamina bay và hồi máu nhẹ khi đứng trong giỏ khinh khí cầu
                    if (IUHAerialFlightController.Instance != null)
                    {
                        IUHAerialFlightController.Instance.InstantRecharge();
                    }
                }
            }
            else
            {
                m_IsPlayerInBasket = inRange;
            }

            UpdatePrompt();
        }

        private void HandleInteractionInput()
        {
            if (m_PlayerTransform == null) return;
            float dist = Vector3.Distance(m_PlayerTransform.position, m_BasketTransform.position);
            if (dist > m_InteractionRadius) return;

            if (Input.GetKeyDown(KeyCode.F))
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
        }

        public void LaunchBalloon()
        {
            if (m_State == BalloonFlightState.Ascending || m_State == BalloonFlightState.AirborneHover) return;

            m_State = BalloonFlightState.Ascending;
            SetBurnerActive(true);
            SetShieldActive(true);

            // Đưa người chơi vào vị trí giỏ nếu đang ở sát
            if (m_PlayerTransform != null && m_PlayerAnchorPoint != null)
            {
                var cc = m_PlayerTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                m_PlayerTransform.position = m_PlayerAnchorPoint.position + Vector3.up * 0.2f;
                if (cc != null) cc.enabled = true;
            }

            IUHCombatAudio.PlayLaserShoot();
        }

        public void DescendBalloon()
        {
            if (m_State == BalloonFlightState.Descending || m_State == BalloonFlightState.Grounded) return;

            m_State = BalloonFlightState.Descending;
            SetBurnerActive(false);
        }

        private void UpdateFlightMotion()
        {
            Vector3 prevBasketPos = m_BasketTransform.position;

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
                    m_CurrentY = Mathf.MoveTowards(m_CurrentY, m_GroundY, m_VerticalSpeed * 0.85f * Time.deltaTime);
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

            // Nếu người chơi đang ở trong giỏ khi đang nâng/hạ, dịch chuyển người chơi theo giỏ
            if (m_IsPlayerInBasket && m_PlayerTransform != null)
            {
                Vector3 delta = m_BasketTransform.position - prevBasketPos;
                var cc = m_PlayerTransform.GetComponent<CharacterController>();
                if (cc != null && cc.enabled)
                {
                    cc.Move(delta);
                }
                else
                {
                    m_PlayerTransform.position += delta;
                }
            }
        }

        private void UpdateAutomatedTurrets()
        {
            // Chỉ kích hoạt ụ pháo tự động bắn khi khinh khí cầu đang ở trên không
            if (m_State != BalloonFlightState.AirborneHover && m_State != BalloonFlightState.Ascending)
            {
                return;
            }

            // Tìm quái vật bay gần nhất
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
                    pivot.rotation = Quaternion.Slerp(pivot.rotation, lookRot, Time.deltaTime * 10f);
                }
            }

            // Bắn đạn plasma tự động
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
                // Ưu tiên quái vật trên không (y > 2.0f)
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

            // Gây sát thương lên mục tiêu
            target.TakeDamage(m_TurretDamage);

            // Bật tia laser trong thời gian ngắn
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
            if (dist > m_InteractionRadius * 1.5f)
            {
                m_BillboardPromptText.gameObject.SetActive(false);
                return;
            }

            m_BillboardPromptText.gameObject.SetActive(true);

            if (m_State == BalloonFlightState.Grounded)
            {
                m_BillboardPromptText.text = "<color=#00E5FF>[F]</color> LÊN KHINH KHÍ CẦU KHÔNG CHIẾN\n<size=11><color=#FFD54F>Bay lên 36m • Nạp năng lượng • Hỗ trợ hỏa lực</color></size>";
            }
            else if (m_State == BalloonFlightState.AirborneHover)
            {
                m_BillboardPromptText.text = "<color=#FF5252>[F]</color> HẠ CÁNH VỀ SÂN TRƯỜNG\n<size=11><color=#00E5FF>[Space x 2] hoặc [J] BUNG CÁNH BAY TỰ DO</color></size>";
            }
            else if (m_State == BalloonFlightState.Ascending)
            {
                m_BillboardPromptText.text = "<color=#FFD54F>ĐANG CẤT CÁNH LÊN TẦNG KHÔNG...</color>";
            }
            else if (m_State == BalloonFlightState.Descending)
            {
                m_BillboardPromptText.text = "<color=#FFB74D>ĐANG HẠ CÁNH VỀ SÂN TRƯỜNG...</color>";
            }

            // Xoay billboard hướng về camera
            Camera cam = Camera.main;
            if (cam != null && m_BillboardPromptText != null)
            {
                m_BillboardPromptText.transform.rotation = Quaternion.LookRotation(m_BillboardPromptText.transform.position - cam.transform.position);
            }
        }
    }
}
