using UnityEngine;

namespace IUHCampus
{
    /// <summary>
    /// Automatic Sliding Glass Door Controller.
    /// Detects player proximity via Trigger Collider, smoothly slides door panels open,
    /// and auto-closes after player passes through.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class IUHAutomaticDoor : MonoBehaviour
    {
        [Header("Door Leaves")]
        [SerializeField] private Transform m_DoorLeft;
        [SerializeField] private Transform m_DoorRight;

        [Header("Movement Settings")]
        [SerializeField] private float m_OpenDistance = 1.1f;
        [SerializeField] private float m_SlideSpeed = 3.5f;
        [SerializeField] private float m_CloseDelay = 1.5f;

        private Vector3 m_LeftClosedPos;
        private Vector3 m_RightClosedPos;
        private Vector3 m_LeftOpenPos;
        private Vector3 m_RightOpenPos;
        private bool m_IsOpen = false;
        private float m_CloseTimer = 0.0f;
        private int m_Occupants = 0;

        private void Awake()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            col.isTrigger = true;

            if (m_DoorLeft != null)
            {
                m_LeftClosedPos = m_DoorLeft.localPosition;
                m_LeftOpenPos = m_LeftClosedPos + new Vector3(-m_OpenDistance, 0, 0);
            }
            if (m_DoorRight != null)
            {
                m_RightClosedPos = m_DoorRight.localPosition;
                m_RightOpenPos = m_RightClosedPos + new Vector3(m_OpenDistance, 0, 0);
            }
        }

        public void BindDoors(Transform leftDoor, Transform rightDoor, float openDist = 1.1f)
        {
            m_DoorLeft = leftDoor;
            m_DoorRight = rightDoor;
            m_OpenDistance = openDist;

            if (m_DoorLeft != null)
            {
                m_LeftClosedPos = m_DoorLeft.localPosition;
                m_LeftOpenPos = m_LeftClosedPos + new Vector3(-m_OpenDistance, 0, 0);
            }
            if (m_DoorRight != null)
            {
                m_RightClosedPos = m_DoorRight.localPosition;
                m_RightOpenPos = m_RightClosedPos + new Vector3(m_OpenDistance, 0, 0);
            }
        }

        private void Update()
        {
            if (m_Occupants > 0)
            {
                m_IsOpen = true;
                m_CloseTimer = m_CloseDelay;
            }
            else if (m_IsOpen)
            {
                m_CloseTimer -= Time.deltaTime;
                if (m_CloseTimer <= 0.0f)
                {
                    m_IsOpen = false;
                }
            }

            // Smooth sliding movement
            Vector3 targetLeft = m_IsOpen ? m_LeftOpenPos : m_LeftClosedPos;
            Vector3 targetRight = m_IsOpen ? m_RightOpenPos : m_RightClosedPos;

            if (m_DoorLeft != null)
            {
                m_DoorLeft.localPosition = Vector3.Lerp(m_DoorLeft.localPosition, targetLeft, Time.deltaTime * m_SlideSpeed);
            }
            if (m_DoorRight != null)
            {
                m_DoorRight.localPosition = Vector3.Lerp(m_DoorRight.localPosition, targetRight, Time.deltaTime * m_SlideSpeed);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
            {
                m_Occupants++;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
            {
                m_Occupants = Mathf.Max(0, m_Occupants - 1);
            }
        }
    }
}
