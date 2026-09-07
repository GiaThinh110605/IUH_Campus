using UnityEngine;
using IUHCampus.Interaction;
using IUHCampus.UI;

namespace IUHCampus.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerState))]
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection Settings")]
        public float interactionDistance = 2.8f;
        public float sphereRadius = 0.25f;
        public LayerMask interactableLayer = ~0; // Will be set to LayerMask.GetMask("Interactable") if available

        [Header("References")]
        public Transform raycastOrigin;

        private PlayerInput m_Input;
        private PlayerState m_State;
        private IInteractable m_CurrentTarget;
        private RaycastHit[] m_Hits = new RaycastHit[8];

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_State = GetComponent<PlayerState>();

            int layer = LayerMask.NameToLayer("Interactable");
            if (layer != -1)
            {
                interactableLayer = 1 << layer;
            }

            if (raycastOrigin == null && Camera.main != null)
            {
                raycastOrigin = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (m_State != null && !m_State.interactionEnabled)
            {
                ClearTarget();
                return;
            }

            DetectInteractable();

            if (m_CurrentTarget != null && m_Input != null && m_Input.InteractTriggered)
            {
                if (m_CurrentTarget.CanInteract())
                {
                    m_CurrentTarget.Interact(gameObject);
                    // Refresh prompt
                    if (m_CurrentTarget != null && m_CurrentTarget.CanInteract())
                    {
                        InteractionUI.SetPrompt(m_CurrentTarget.GetInteractionText(), true);
                    }
                    else
                    {
                        InteractionUI.SetPrompt("", false);
                    }
                }
            }
        }

        private void DetectInteractable()
        {
            Transform origin = raycastOrigin != null ? raycastOrigin : transform;
            Ray ray = new Ray(origin.position, origin.forward);

            int hitCount = Physics.SphereCastNonAlloc(ray, sphereRadius, m_Hits, interactionDistance, interactableLayer);
            IInteractable bestTarget = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = m_Hits[i];
                if (hit.collider == null) continue;

                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        bestTarget = interactable;
                    }
                }
            }

            if (bestTarget != m_CurrentTarget)
            {
                m_CurrentTarget = bestTarget;
                if (m_CurrentTarget != null)
                {
                    InteractionUI.SetPrompt(m_CurrentTarget.GetInteractionText(), true);
                }
                else
                {
                    InteractionUI.SetPrompt("", false);
                }
            }
        }

        private void ClearTarget()
        {
            m_CurrentTarget = null;
            InteractionUI.SetPrompt("", false);
        }

        private void OnDisable()
        {
            ClearTarget();
        }
    }
}
