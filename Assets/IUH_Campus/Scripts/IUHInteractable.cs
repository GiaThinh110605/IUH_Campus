using UnityEngine;

namespace IUHCampus
{
    public enum InteractionType
    {
        Door,
        Computer,
        InfoBoard,
        Elevator,
        Chair,
        InspectionPoint,
        FireEquipment,
        NPC
    }

    /// <summary>
    /// Component marking an object as interactable for gameplay, quest triggers, and UI prompts.
    /// </summary>
    public class IUHInteractable : MonoBehaviour
    {
        [Header("Interaction Configuration")]
        [Tooltip("Type of interactive object")]
        public InteractionType interactionType = InteractionType.InspectionPoint;

        [Tooltip("Unique ID for quest tracking and script triggers")]
        public string interactionId = "Interactable_01";

        [Tooltip("Prompt displayed on screen when player approaches (e.g. 'Press E to examine')")]
        public string promptText = "Nhấn E để tương tác";

        [Tooltip("Maximum distance in meters to interact")]
        public float interactionRange = 2.2f;

        [Tooltip("Is interaction currently enabled?")]
        public bool isInteractable = true;

        [Header("Optional Target Anchors")]
        [Tooltip("Optional transform where the player should stand/align during interaction")]
        public Transform interactionAnchor;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
