using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHLootCoinPickup : MonoBehaviour
    {
        [SerializeField] private int m_GoldAmount = 15;
        [SerializeField] private float m_MagnetRadius = 3.5f;
        [SerializeField] private float m_CollectRadius = 0.8f;
        [SerializeField] private float m_FlySpeed = 8f;

        private Transform m_PlayerTransform;
        private bool m_IsCollected = false;
        private float m_SpawnTime;

        public void Initialize(int gold)
        {
            m_GoldAmount = gold;
            m_SpawnTime = Time.time;
            FindPlayer();
        }

        private void Start()
        {
            m_SpawnTime = Time.time;
            FindPlayer();
            Destroy(gameObject, 20f); // auto cleanup if not picked up
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (m_IsCollected) return;
            if (Time.time < m_SpawnTime + 0.35f) return; // let it bounce first

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            Vector3 targetPos = m_PlayerTransform.position + Vector3.up * 0.8f;
            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist <= m_MagnetRadius)
            {
                // Turn off physics and fly to player
                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                transform.position = Vector3.MoveTowards(transform.position, targetPos, m_FlySpeed * Time.deltaTime);

                if (dist <= m_CollectRadius)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            if (m_IsCollected) return;
            m_IsCollected = true;

            IUHCombatAudio.PlayCoin();

            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_GoldAmount);
            }

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 0.5f, $"+{m_GoldAmount} Vàng", new Color(1f, 0.9f, 0.2f), 1f);

            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_IsCollected) return;
            if (other.name.Contains("Student") || other.CompareTag("Player") || other.name.Contains("DRONE"))
            {
                Collect();
            }
        }
    }
}

