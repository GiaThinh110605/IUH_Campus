using UnityEngine;
using IUHCampus.Interaction;
using IUHCampus.NPC;
using IUHCampus.Player;
using IUHCampus.World;

namespace IUHCampus.Debugging
{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance { get; private set; }

        public bool showGizmos = true;
        public bool showWorldObjectIDs = true;
        public bool showSpawnPoints = true;
        public bool showWaypoints = true;
        public bool showTriggers = true;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            if (showSpawnPoints)
            {
                Gizmos.color = Color.magenta;
                if (PlayerSpawnManager.Instance != null)
                {
                    foreach (var sp in PlayerSpawnManager.Instance.spawnPoints)
                    {
                        if (sp.spawnTransform != null)
                        {
                            Gizmos.DrawWireCube(sp.spawnTransform.position + Vector3.up * 1f, new Vector3(0.8f, 2f, 0.8f));
                        }
                    }
                }
            }
        }
    }
}
