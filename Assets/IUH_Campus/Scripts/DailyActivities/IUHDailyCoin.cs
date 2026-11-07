using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.DailyActivities
{
    public class IUHDailyCoin : MonoBehaviour
    {
        [Header("Coin Identification")]
        [SerializeField] private int m_CoinIndex = 0;
        [SerializeField] private float m_CollectRadius = 1.6f;
        [SerializeField] private float m_SpinSpeed = 160f;
        [SerializeField] private float m_BobAmplitude = 0.15f;
        [SerializeField] private float m_BobFrequency = 3f;

        [Header("Visual Components")]
        [SerializeField] private Transform m_VisualTransform;
        [SerializeField] private Light m_CoinLight;
        [SerializeField] private ParticleSystem m_SparkleParticles;

        private Vector3 m_BasePosition;
        private bool m_IsCollected = false;
        private Transform m_PlayerTransform;
        private Transform m_DroneTransform;

        public int CoinIndex { get => m_CoinIndex; set => m_CoinIndex = value; }

        private void Start()
        {
            m_BasePosition = transform.position;
            FindTargets();

            // Check if already collected today
            if (IUHCampusDailyActivitiesManager.Instance != null && IUHCampusDailyActivitiesManager.Instance.IsCoinCollected(m_CoinIndex))
            {
                m_IsCollected = true;
                gameObject.SetActive(false);
            }
        }

        private void FindTargets()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;

            var drone = GameObject.Find("IUH_GUARDIAN_DRONE");
            if (drone != null) m_DroneTransform = drone.transform;
        }

        private void Update()
        {
            if (m_IsCollected) return;

            // Idle animation: Spin & Bob
            float newY = m_BasePosition.y + Mathf.Sin(Time.time * m_BobFrequency + m_CoinIndex) * m_BobAmplitude;
            transform.position = new Vector3(m_BasePosition.x, newY, m_BasePosition.z);

            if (m_VisualTransform != null)
            {
                m_VisualTransform.Rotate(Vector3.up, m_SpinSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                transform.Rotate(Vector3.up, m_SpinSpeed * Time.deltaTime, Space.World);
            }

            // Check distance with Student or Drone
            if (m_PlayerTransform == null && m_DroneTransform == null)
            {
                FindTargets();
                return;
            }

            bool inRange = false;
            if (m_PlayerTransform != null && Vector3.Distance(transform.position, m_PlayerTransform.position) <= m_CollectRadius)
            {
                inRange = true;
            }
            else if (m_DroneTransform != null && Vector3.Distance(transform.position, m_DroneTransform.position) <= m_CollectRadius + 0.8f)
            {
                inRange = true;
            }

            if (inRange)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (m_IsCollected) return;
            m_IsCollected = true;

            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.CollectCoin(m_CoinIndex, transform.position);
            }

            if (m_SparkleParticles != null)
            {
                m_SparkleParticles.transform.parent = null;
                m_SparkleParticles.Play();
                Destroy(m_SparkleParticles.gameObject, 1.5f);
            }

            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_IsCollected) return;
            if (other.CompareTag("Player") || other.name.Contains("Student") || other.name.Contains("DRONE"))
            {
                Collect();
            }
        }
    }
}

