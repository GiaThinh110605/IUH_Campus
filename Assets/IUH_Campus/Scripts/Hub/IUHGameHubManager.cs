using UnityEngine;
using UnityEngine.SceneManagement;

namespace IUHCampus.Hub
{
    /// <summary>
    /// Trình điều phối chuyển cảnh (Hub Scene Manager) cho IUH Campus.
    /// Quản lý 6 Minigames độc lập cho 3 thành viên: Gia Thịnh (2), Huy (2), Nhật (2) + 1 Hub Sân Trường.
    /// </summary>
    public class IUHGameHubManager : MonoBehaviour
    {
        [Header("Main Hub")]
        public string hubSceneName = "IUH_Campus_Main";

        [Header("Gia Thinh Scenes")]
        public string thinhSprintSceneName = "Scene_Thinh_SprintToClass";       // Game 1: Chạy điểm danh
        public string thinhRideHailingSceneName = "Scene_Thinh_RideHailingRush"; // Game 4: Xe ôm công nghệ Grab/Be/Xanh

        [Header("Huy Scenes")]
        public string huyDroneSceneName = "Scene_Huy_DronePatrol";              // Game 2: FPV Drone tháp kính 13 tầng
        public string huyRoboBattleSceneName = "Scene_Huy_RoboBattleLab";       // Game 5: Đua Robot & Sumo xưởng cơ khí

        [Header("Nhat Scenes")]
        public string nhatParkingSceneName = "Scene_Nhat_ParkingMaster";        // Game 3: Bãi đỗ xe giờ cao điểm
        public string nhatCanteenSceneName = "Scene_Nhat_CanteenRush";          // Game 6: Canteen & Trà đào Nguyễn Văn Bảo

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip transitionSfx;

        public static IUHGameHubManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        // ================= GIA THỊNH =================
        public void LoadThinhSprintGame()
        {
            LoadSceneWithFeedback(thinhSprintSceneName, "Minigame 1: Chay Bo Diem Danh (Gia Thinh)");
        }

        public void LoadThinhRideHailingGame()
        {
            LoadSceneWithFeedback(thinhRideHailingSceneName, "Minigame 4: Xe Om Cong Nghe Grab/Be/Xanh (Gia Thinh)");
        }

        // ================= HUY =================
        public void LoadHuyDroneGame()
        {
            LoadSceneWithFeedback(huyDroneSceneName, "Minigame 2: FPV Drone Thap Kinh 13 Tang (Huy)");
        }

        public void LoadHuyRoboBattleGame()
        {
            LoadSceneWithFeedback(huyRoboBattleSceneName, "Minigame 5: Dua Robot Xuong Co Khi IUH (Huy)");
        }

        // ================= NHẬT =================
        public void LoadNhatParkingGame()
        {
            LoadSceneWithFeedback(nhatParkingSceneName, "Minigame 3: Xep Xe & Giai Cuu Xe Ket (Nhat)");
        }

        public void LoadNhatCanteenGame()
        {
            LoadSceneWithFeedback(nhatCanteenSceneName, "Minigame 6: Canteen & Tra Dao Nguyen Van Bao (Nhat)");
        }

        // ================= RETURN HUB =================
        public void ReturnToMainCampusHub()
        {
            LoadSceneWithFeedback(hubSceneName, "Tro Ve San Truong Trung Tam IUH");
        }

        private void LoadSceneWithFeedback(string sceneName, string logMessage)
        {
            if (audioSource != null && transitionSfx != null)
            {
                audioSource.PlayOneShot(transitionSfx);
            }
            Debug.Log("[IUH Hub Transition] Loading: " + logMessage + " -> Scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}
