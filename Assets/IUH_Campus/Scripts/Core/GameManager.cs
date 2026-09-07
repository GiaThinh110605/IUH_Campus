using UnityEngine;
using IUHCampus.Player;
using IUHCampus.Save;
using IUHCampus.World;

namespace IUHCampus.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Startup Configuration")]
        public string initialSpawnPointId = "SPAWN_MainCampus";
        public bool loadSaveOnStart = false;

        public bool IsGameInitialized { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            BootstrapGame();
        }

        public void BootstrapGame()
        {
            Debug.Log("[GameManager] --- Bắt đầu khởi tạo hệ thống game IUH Campus ---");

            // 1. Initialize Managers
            if (WorldState.Instance == null)
            {
                gameObject.AddComponent<WorldState>();
            }

            if (SaveSystem.Instance == null)
            {
                gameObject.AddComponent<SaveSystem>();
            }

            // 2. Spawn Player
            if (PlayerSpawnManager.Instance != null)
            {
                PlayerSpawnManager.Instance.SpawnPlayer(initialSpawnPointId);
            }

            // 3. Load Save Data if requested
            if (loadSaveOnStart && SaveSystem.HasSaveFile())
            {
                SaveSystem.LoadGame();
            }

            IsGameInitialized = true;
            GameEvents.TriggerGameStateChanged("Ready");
            Debug.Log("[GameManager] --- Khởi tạo hoàn tất. Sẵn sàng trải nghiệm! ---");
        }
    }
}
