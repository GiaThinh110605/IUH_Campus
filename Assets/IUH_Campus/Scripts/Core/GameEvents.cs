using System;
using UnityEngine;
using IUHCampus.Interaction;

namespace IUHCampus.Core
{
    public static class GameEvents
    {
        public static event Action<string, GameObject> OnTriggerEntered;
        public static event Action<string, GameObject> OnTriggerExited;
        public static event Action<string, DoorState> OnDoorStateChanged;
        public static event Action<string> OnDoorLocked;
        public static event Action<string, GameObject> OnObjectInteracted;
        public static event Action<string, Vector3> OnPlayerSpawned;
        public static event Action<string> OnGameStateChanged;
        public static event Action<string, int> OnFloorChanged;
        public static event Action<string, string> OnRoomEntered;
        public static event Action<string> OnLocationChanged;

        public static void TriggerFloorChanged(string buildingId, int floor)
        {
            OnFloorChanged?.Invoke(buildingId, floor);
        }

        public static void TriggerRoomEntered(string buildingId, string roomId)
        {
            OnRoomEntered?.Invoke(buildingId, roomId);
        }

        public static void TriggerLocationChanged(string locationDescription)
        {
            OnLocationChanged?.Invoke(locationDescription);
        }

        public static void TriggerTriggerEntered(string triggerId, GameObject source)
        {
            OnTriggerEntered?.Invoke(triggerId, source);
        }

        public static void TriggerTriggerExited(string triggerId, GameObject source)
        {
            OnTriggerExited?.Invoke(triggerId, source);
        }

        public static void TriggerDoorStateChanged(string doorId, DoorState state)
        {
            OnDoorStateChanged?.Invoke(doorId, state);
        }

        public static void TriggerDoorLocked(string doorId)
        {
            OnDoorLocked?.Invoke(doorId);
        }

        public static void TriggerObjectInteracted(string objectId, GameObject source)
        {
            OnObjectInteracted?.Invoke(objectId, source);
        }

        public static void TriggerPlayerSpawned(string spawnPointId, Vector3 position)
        {
            OnPlayerSpawned?.Invoke(spawnPointId, position);
        }

        public static void TriggerGameStateChanged(string newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }
    }
}
