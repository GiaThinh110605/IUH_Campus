using System.Collections;
using UnityEngine;
using IUHCampus.Core;
using IUHCampus.World;

namespace IUHCampus.Interaction
{
    public enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing,
        Locked
    }

    [RequireComponent(typeof(WorldObjectID))]
    public class DoorController : MonoBehaviour, IInteractable
    {
        [Header("Door Configuration")]
        public string doorName = "Cửa";
        public DoorState currentState = DoorState.Closed;
        public bool isLocked = false;
        public string requiredKeyId = "";
        public bool autoOpen = false;
        public bool isSlidingDoor = true;

        [Header("Movement Settings")]
        public Vector3 openOffset = new Vector3(0, 0, 1.8f);
        public Vector3 openRotationOffset = Vector3.zero;
        public float transitionDuration = 1.2f;

        [Header("References")]
        public Transform doorLeafLeft;
        public Transform doorLeafRight;

        private Vector3 m_ClosedPosLeft;
        private Vector3 m_ClosedPosRight;
        private Coroutine m_MoveCoroutine;
        private WorldObjectID m_WorldID;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (m_WorldID == null) m_WorldID = GetComponent<WorldObjectID>();
            if (m_ClosedPosLeft == Vector3.zero && doorLeafLeft != null) m_ClosedPosLeft = doorLeafLeft.localPosition;
            if (m_ClosedPosRight == Vector3.zero && doorLeafRight != null) m_ClosedPosRight = doorLeafRight.localPosition;
        }

        public bool CanInteract()
        {
            return currentState == DoorState.Closed || currentState == DoorState.Open;
        }

        public string GetInteractionText()
        {
            if (isLocked) return "[E] " + doorName + " (Đã khóa)";
            return (currentState == DoorState.Open) ? "[E] Đóng " + doorName : "[E] Mở " + doorName;
        }

        public void Interact(GameObject source)
        {
            EnsureInitialized();
            if (isLocked)
            {
                Debug.LogWarning($"[DoorController] Cửa '{doorName}' (ID: {m_WorldID?.objectId}) đang khóa. Cần chìa khóa: {requiredKeyId}");
                GameEvents.TriggerDoorLocked(m_WorldID?.objectId);
                return;
            }

            if (currentState == DoorState.Closed)
            {
                OpenDoor();
            }
            else if (currentState == DoorState.Open)
            {
                CloseDoor();
            }
        }

        public void OpenDoor()
        {
            EnsureInitialized();
            if (currentState == DoorState.Open || currentState == DoorState.Opening) return;
            if (m_MoveCoroutine != null) StopCoroutine(m_MoveCoroutine);
            m_MoveCoroutine = StartCoroutine(AnimateDoor(true));
        }

        public void CloseDoor()
        {
            EnsureInitialized();
            if (currentState == DoorState.Closed || currentState == DoorState.Closing) return;
            if (m_MoveCoroutine != null) StopCoroutine(m_MoveCoroutine);
            m_MoveCoroutine = StartCoroutine(AnimateDoor(false));
        }

        public void SetStateDirectly(bool open)
        {
            EnsureInitialized();
            if (m_MoveCoroutine != null) StopCoroutine(m_MoveCoroutine);
            currentState = open ? DoorState.Open : DoorState.Closed;
            if (isSlidingDoor)
            {
                if (doorLeafLeft != null) doorLeafLeft.localPosition = open ? m_ClosedPosLeft - openOffset * 0.5f : m_ClosedPosLeft;
                if (doorLeafRight != null) doorLeafRight.localPosition = open ? m_ClosedPosRight + openOffset * 0.5f : m_ClosedPosRight;
            }
            else
            {
                transform.localPosition = open ? transform.localPosition + openOffset : transform.localPosition;
            }

            if (m_WorldID != null && !string.IsNullOrEmpty(m_WorldID.objectId))
            {
                WorldState.Instance?.SetObjectState(m_WorldID.objectId, currentState.ToString());
            }
            GameEvents.TriggerDoorStateChanged(m_WorldID?.objectId, currentState);
        }

        private IEnumerator AnimateDoor(bool opening)
        {
            currentState = opening ? DoorState.Opening : DoorState.Closing;
            GameEvents.TriggerDoorStateChanged(m_WorldID?.objectId, currentState);

            float elapsed = 0f;
            Vector3 startPosLeft = (doorLeafLeft != null) ? doorLeafLeft.localPosition : Vector3.zero;
            Vector3 targetPosLeft = opening ? (m_ClosedPosLeft - openOffset * 0.5f) : m_ClosedPosLeft;

            Vector3 startPosRight = (doorLeafRight != null) ? doorLeafRight.localPosition : Vector3.zero;
            Vector3 targetPosRight = opening ? (m_ClosedPosRight + openOffset * 0.5f) : m_ClosedPosRight;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

                if (doorLeafLeft != null) doorLeafLeft.localPosition = Vector3.Lerp(startPosLeft, targetPosLeft, t);
                if (doorLeafRight != null) doorLeafRight.localPosition = Vector3.Lerp(startPosRight, targetPosRight, t);

                yield return null;
            }

            if (doorLeafLeft != null) doorLeafLeft.localPosition = targetPosLeft;
            if (doorLeafRight != null) doorLeafRight.localPosition = targetPosRight;

            currentState = opening ? DoorState.Open : DoorState.Closed;
            GameEvents.TriggerDoorStateChanged(m_WorldID?.objectId, currentState);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (autoOpen && other.CompareTag("Player") && currentState == DoorState.Closed && !isLocked)
            {
                OpenDoor();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (autoOpen && other.CompareTag("Player") && currentState == DoorState.Open)
            {
                CloseDoor();
            }
        }
    }
}
