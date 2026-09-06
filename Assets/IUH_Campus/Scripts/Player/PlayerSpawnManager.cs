using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.Player
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        public static PlayerSpawnManager Instance { get; private set; }

        [System.Serializable]
        public struct SpawnPointEntry
        {
            public string spawnId;
            public Transform spawnTransform;
        }

        public List<SpawnPointEntry> spawnPoints = new List<SpawnPointEntry>();
        public PlayerMovement playerMovement;
        public string defaultSpawnId = "SPAWN_MainCampus";

        private Dictionary<string, Transform> m_SpawnDict = new Dictionary<string, Transform>();

        private void Awake()
        {
            Instance = this;
            RebuildDictionary();
        }

        public void RebuildDictionary()
        {
            m_SpawnDict.Clear();
            foreach (var entry in spawnPoints)
            {
                if (!string.IsNullOrEmpty(entry.spawnId) && entry.spawnTransform != null)
                {
                    m_SpawnDict[entry.spawnId] = entry.spawnTransform;
                }
            }
        }

        public void RegisterSpawnPoint(string spawnId, Transform t)
        {
            if (string.IsNullOrEmpty(spawnId) || t == null) return;
            m_SpawnDict[spawnId] = t;
        }

        public bool SpawnPlayer(string spawnId)
        {
            if (m_SpawnDict.Count == 0) RebuildDictionary();

            if (!m_SpawnDict.TryGetValue(spawnId, out Transform targetTransform))
            {
                Debug.LogWarning($"[PlayerSpawnManager] Không tìm thấy SpawnPoint ID: '{spawnId}'. Thử dùng mặc định '{defaultSpawnId}'");
                if (!m_SpawnDict.TryGetValue(defaultSpawnId, out targetTransform))
                {
                    Debug.LogError("[PlayerSpawnManager] Không tìm thấy bất kỳ SpawnPoint hợp lệ nào!");
                    return false;
                }
            }

            if (playerMovement == null)
            {
                playerMovement = FindFirstObjectByType<PlayerMovement>();
            }

            if (playerMovement != null && targetTransform != null)
            {
                playerMovement.Teleport(targetTransform.position, targetTransform.rotation);
                PlayerState state = playerMovement.GetComponent<PlayerState>();
                if (state != null)
                {
                    state.currentSpawnPointId = spawnId;
                }

                GameEvents.TriggerPlayerSpawned(spawnId, targetTransform.position);
                Debug.Log($"[PlayerSpawnManager] Đã spawn Player tại: '{spawnId}' ({targetTransform.position})");
                return true;
            }

            return false;
        }
    }
}
