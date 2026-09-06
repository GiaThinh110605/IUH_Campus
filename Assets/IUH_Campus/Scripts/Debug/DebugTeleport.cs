using UnityEngine;
using IUHCampus.Player;
using IUHCampus.Save;

namespace IUHCampus.Debugging
{
    public class DebugTeleport : MonoBehaviour
    {
        [Header("Shortcuts")]
        [Tooltip("F1: Courtyard Plaza | F2: Lobby | F3: Computer Lab | F5: Quick Save | F9: Quick Load")]
        public bool shortcutsEnabled = true;

        private void Update()
        {
            if (!shortcutsEnabled) return;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                PlayerSpawnManager.Instance?.SpawnPlayer("SPAWN_MainCampus");
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                PlayerSpawnManager.Instance?.SpawnPlayer("SPAWN_BuildingG_Lobby");
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                PlayerSpawnManager.Instance?.SpawnPlayer("SPAWN_BuildingG_Floor02");
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                PlayerSpawnManager.Instance?.SpawnPlayer("SPAWN_BuildingG_Floor03");
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveSystem.SaveGame();
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                PlayerSpawnManager.Instance?.SpawnPlayer("SPAWN_BuildingG_Floor04");
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                SaveSystem.LoadGame();
            }
        }
    }
}
