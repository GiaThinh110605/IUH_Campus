using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.DailyActivities
{
    public class IUHMascotQuestItem : MonoBehaviour
    {
        [SerializeField] private int m_ItemIndex = 0;
        [SerializeField] private float m_CollectRadius = 1.6f;
        [SerializeField] private float m_SpinSpeed = 120f;
        [SerializeField] private float m_BobAmplitude = 0.2f;

        private Vector3 m_BasePos;
        private bool m_Collected = false;
        private Transform m_PlayerTransform;

        public int ItemIndex { get => m_ItemIndex; set => m_ItemIndex = value; }

        private void Start()
        {
            m_BasePos = transform.position;
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (m_Collected) return;

            // Spin & Bob
            float y = m_BasePos.y + Mathf.Sin(Time.time * 2.5f + m_ItemIndex) * m_BobAmplitude;
            transform.position = new Vector3(m_BasePos.x, y, m_BasePos.z);
            transform.Rotate(Vector3.up, m_SpinSpeed * Time.deltaTime, Space.World);

            if (m_PlayerTransform == null)
            {
                var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
                if (student != null) m_PlayerTransform = student.transform;
                return;
            }

            if (Vector3.Distance(transform.position, m_PlayerTransform.position) <= m_CollectRadius)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (m_Collected) return;
            m_Collected = true;

            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.CollectMascotQuestItem(m_ItemIndex, transform.position);
            }

            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_Collected) return;
            if (other.CompareTag("Player") || other.name.Contains("Student") || other.name.Contains("DRONE"))
            {
                Collect();
            }
        }
    }
}

