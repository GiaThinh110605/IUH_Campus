using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Core;
using IUHCampus.Interaction;

namespace IUHCampus.World
{
    public class WorldState : MonoBehaviour
    {
        private static WorldState s_Instance;
        public static WorldState Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = FindFirstObjectByType<WorldState>();
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        [System.Serializable]
        public struct StateEntry
        {
            public string objectId;
            public string stateValue;
        }

        private Dictionary<string, string> m_StateDictionary = new Dictionary<string, string>();

        private void Awake()
        {
            s_Instance = this;
            GameEvents.OnDoorStateChanged += HandleDoorStateChanged;
            GameEvents.OnObjectInteracted += HandleObjectInteracted;
        }

        private void OnDestroy()
        {
            GameEvents.OnDoorStateChanged -= HandleDoorStateChanged;
            GameEvents.OnObjectInteracted -= HandleObjectInteracted;
        }

        private void HandleDoorStateChanged(string doorId, DoorState state)
        {
            if (string.IsNullOrEmpty(doorId)) return;
            SetObjectState(doorId, state.ToString());
        }

        private void HandleObjectInteracted(string objectId, GameObject source)
        {
            if (string.IsNullOrEmpty(objectId)) return;
            SetObjectState(objectId, "Interacted");
        }

        public void SetObjectState(string objectId, string value)
        {
            m_StateDictionary[objectId] = value;
            Debug.Log($"[WorldState] '{objectId}' => '{value}'");
        }

        public string GetObjectState(string objectId)
        {
            if (m_StateDictionary.TryGetValue(objectId, out string val))
            {
                return val;
            }
            return "";
        }

        public Dictionary<string, string> GetAllStates()
        {
            return new Dictionary<string, string>(m_StateDictionary);
        }

        public void RestoreStates(Dictionary<string, string> states)
        {
            if (states == null) return;
            m_StateDictionary = new Dictionary<string, string>(states);

            // Apply to active scene doors and objects
            DoorController[] doors = FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var door in doors)
            {
                door.EnsureInitialized();
                WorldObjectID wid = door.GetComponent<WorldObjectID>();
                if (wid != null && m_StateDictionary.TryGetValue(wid.objectId, out string stateStr))
                {
                    if (stateStr == "Open") door.SetStateDirectly(true);
                    else if (stateStr == "Closed") door.SetStateDirectly(false);
                }
            }
        }
    }
}
