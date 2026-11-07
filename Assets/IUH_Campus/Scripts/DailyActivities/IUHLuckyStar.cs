using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.DailyActivities
{
    public class IUHLuckyStar : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float m_CollectRadius = 1.8f;
        [SerializeField] private float m_SpinSpeed = 90f;
        [SerializeField] private float m_BobAmplitude = 0.25f;

        [Header("Components")]
        [SerializeField] private Transform m_StarTransform;
        [SerializeField] private Light m_StarLight;
        [SerializeField] private GameObject m_BeaconBeam;
        [SerializeField] private ParticleSystem m_BurstParticles;

        private Vector3 m_BasePos;
        private bool m_IsCollected = false;
        private Transform m_PlayerTransform;
        private Transform m_DroneTransform;

        private void Start()
        {
            m_BasePos = transform.position;
            FindTargets();

            if (IUHCampusDailyActivitiesManager.Instance != null && IUHCampusDailyActivitiesManager.Instance.LuckyStarCollected)
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

            // Float & Spin
            float y = m_BasePos.y + Mathf.Sin(Time.time * 2f) * m_BobAmplitude;
            transform.position = new Vector3(m_BasePos.x, y, m_BasePos.z);

            if (m_StarTransform != null)
            {
                m_StarTransform.Rotate(Vector3.up, m_SpinSpeed * Time.deltaTime, Space.World);
                m_StarTransform.Rotate(Vector3.forward, m_SpinSpeed * 0.4f * Time.deltaTime, Space.Self);
            }
            else
            {
                transform.Rotate(Vector3.up, m_SpinSpeed * Time.deltaTime, Space.World);
            }

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
            else if (m_DroneTransform != null && Vector3.Distance(transform.position, m_DroneTransform.position) <= m_CollectRadius + 1.0f)
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

            if (m_BurstParticles != null)
            {
                m_BurstParticles.transform.parent = null;
                m_BurstParticles.Play();
                Destroy(m_BurstParticles.gameObject, 2f);
            }

            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.CollectLuckyStar(transform.position, (msg) =>
                {
                    // Callback
                });
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

