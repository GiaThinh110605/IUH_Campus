using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.DailyActivities
{
    public class IUHMascotNPC : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float m_InteractRadius = 3.0f;
        [SerializeField] private KeyCode m_InteractKey = KeyCode.E;

        [Header("Components")]
        [SerializeField] private Transform m_ModelTransform;
        [SerializeField] private GameObject m_PromptBillboard;
        [SerializeField] private ParticleSystem m_CelebrationParticles;
        [SerializeField] private Light m_MascotGlow;

        [Header("Quest Item Prefab/Spawns")]
        [SerializeField] private GameObject[] m_QuestItemInstances;

        private Transform m_PlayerTransform;
        private Vector3 m_BaseModelPos;
        private bool m_IsDancing = false;
        private float m_DanceTimer = 0f;

        private void Start()
        {
            if (m_ModelTransform != null) m_BaseModelPos = m_ModelTransform.localPosition;
            FindPlayer();
            if (m_PromptBillboard != null) m_PromptBillboard.SetActive(false);

            // Connect event
            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.OnActivitiesStateChanged += RefreshQuestItemVisibility;
            }

            RefreshQuestItemVisibility();
        }

        private void OnDestroy()
        {
            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.OnActivitiesStateChanged -= RefreshQuestItemVisibility;
            }
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            // Cute idle animation: gentle bob & look towards player
            if (m_ModelTransform != null)
            {
                if (m_IsDancing)
                {
                    m_DanceTimer += Time.deltaTime;
                    float jump = Mathf.Abs(Mathf.Sin(m_DanceTimer * 8f)) * 0.4f;
                    m_ModelTransform.localPosition = m_BaseModelPos + Vector3.up * jump;
                    m_ModelTransform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.World);
                    if (m_DanceTimer > 4.0f) m_IsDancing = false;
                }
                else
                {
                    float bob = Mathf.Sin(Time.time * 2.5f) * 0.08f;
                    m_ModelTransform.localPosition = m_BaseModelPos + Vector3.up * bob;
                }
            }

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
            bool inRange = dist <= m_InteractRadius;

            if (m_PromptBillboard != null && m_PromptBillboard.activeSelf != inRange)
            {
                m_PromptBillboard.SetActive(inRange);
            }

            // Look towards player when close
            if (inRange && m_ModelTransform != null && !m_IsDancing)
            {
                Vector3 lookDir = (m_PlayerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    m_ModelTransform.rotation = Quaternion.Slerp(m_ModelTransform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
                }
            }

            if (inRange && Input.GetKeyDown(m_InteractKey))
            {
                if (IUHCampusActivitiesHUD.Instance != null && IUHCampusActivitiesHUD.Instance.IsMascotDialogOpen) return;
                Interact();
            }
        }

        public void Interact()
        {
            var mgr = IUHCampusDailyActivitiesManager.Instance;
            if (mgr == null) return;

            if (mgr.MascotState == MascotQuestState.NotStarted)
            {
                IUHCampusActivitiesHUD.Instance?.OpenMascotDialog(
                    title: "🤖 LINH VẬT IUH CHÀO BẠN!",
                    message: "Xin chào bạn! Mình là Linh Vật IUH! Hôm nay mình đang chế tạo Thiết Bị Tự Động Hóa Khuôn Viên nhưng lỡ làm rơi mất 3 Vi Mạch Năng Lượng ở quanh trường!\n\nBạn có thể giúp mình nhặt lại 3 Vi Mạch không? Mình sẽ tặng bạn 150 Vàng và Huy Hiệu Bạn Thân Linh Vật!",
                    actionButtonText: "Nhận Thử Thách Ngay!",
                    onAction: () =>
                    {
                        mgr.AcceptMascotQuest();
                    }
                );
            }
            else if (mgr.MascotState == MascotQuestState.InProgress)
            {
                if (mgr.MascotItemsCollected >= mgr.MascotItemsTarget)
                {
                    IUHCampusActivitiesHUD.Instance?.OpenMascotDialog(
                        title: "🎉 HOÀN THÀNH THỬ THÁCH!",
                        message: "Woa! Bạn đã tìm đủ 3 Vi Mạch Năng Lượng rồi! Cảm ơn bạn rất nhiều, bạn thật là một sinh viên IUH tuyệt vời!\n\nĐây là phần thưởng dành cho bạn!",
                        actionButtonText: "Nhận 150 Vàng & Huy Hiệu",
                        onAction: () =>
                        {
                            mgr.CompleteMascotQuest(() =>
                            {
                                PlayCelebration();
                            });
                        }
                    );
                }
                else
                {
                    IUHCampusActivitiesHUD.Instance?.OpenMascotDialog(
                        title: "🤖 LINH VẬT ĐANG CHỜ BẠN...",
                        message: $"Bạn đã tìm được {mgr.MascotItemsCollected}/{mgr.MascotItemsTarget} Vi Mạch Năng Lượng.\n\nHãy quan sát các luồng sáng phát ra quanh sân trường để nhặt đủ nhé!",
                        actionButtonText: "Tiếp Tục Tìm Kiếm",
                        onAction: null
                    );
                }
            }
            else // Completed
            {
                IUHCampusActivitiesHUD.Instance?.OpenMascotDialog(
                    title: "✨ CẢM ƠN BẠN THÂN LINH VẬT!",
                    message: "Cảm ơn bạn đã giúp đỡ mình hôm nay! Thiết bị đã hoạt động trơn tru rồi. Hãy tiếp tục khám phá các sự kiện khác trong trường và quay lại vào ngày mai nhé!",
                    actionButtonText: "Tuyệt Vời!",
                    onAction: null
                );
            }
        }

        public void PlayCelebration()
        {
            m_IsDancing = true;
            m_DanceTimer = 0f;

            if (m_CelebrationParticles != null)
            {
                m_CelebrationParticles.Play();
            }

            if (m_MascotGlow != null)
            {
                m_MascotGlow.color = new Color(0.2f, 1f, 0.5f);
                m_MascotGlow.intensity = 3.5f;
            }
        }

        public void SetQuestItemInstances(GameObject[] items)
        {
            m_QuestItemInstances = items;
            RefreshQuestItemVisibility();
        }

        private void RefreshQuestItemVisibility()
        {
            if (m_QuestItemInstances == null) return;
            var mgr = IUHCampusDailyActivitiesManager.Instance;
            bool shouldShow = (mgr != null && mgr.MascotState == MascotQuestState.InProgress);

            for (int i = 0; i < m_QuestItemInstances.Length; i++)
            {
                if (m_QuestItemInstances[i] != null)
                {
                    // If in progress and not collected yet
                    m_QuestItemInstances[i].SetActive(shouldShow);
                }
            }
        }
    }
}

