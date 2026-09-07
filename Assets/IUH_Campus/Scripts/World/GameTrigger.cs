using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.World
{
    [RequireComponent(typeof(Collider))]
    public class GameTrigger : MonoBehaviour
    {
        [Header("Trigger Identity")]
        public string triggerId = "TRG_EnterBuildingG";
        public string displayName = "Lối vào Nhà G";
        public bool triggerOnce = false;

        private bool m_HasTriggered = false;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && m_HasTriggered) return;
            if (other.CompareTag("Player"))
            {
                m_HasTriggered = true;
                Debug.Log($"[GameTrigger] Player entered: '{displayName}' (ID: {triggerId})");
                GameEvents.TriggerTriggerEntered(triggerId, other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[GameTrigger] Player exited: '{displayName}' (ID: {triggerId})");
                GameEvents.TriggerTriggerExited(triggerId, other.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.35f);
            Collider col = GetComponent<Collider>();
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }
}
