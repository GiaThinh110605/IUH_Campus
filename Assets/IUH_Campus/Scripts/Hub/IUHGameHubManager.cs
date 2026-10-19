using UnityEngine;
using UnityEngine.SceneManagement;

namespace IUHCampus.Hub
{
    /// <summary>
    /// Trình điều phối chuyển cảnh (Hub Scene Manager) cho IUH Campus.
    /// Nhóm 2 thành viên: Gia Thịnh (3D + AR + Mobile) & Nhật (2D + Cloud + Docker).
    /// </summary>
    public class IUHGameHubManager : MonoBehaviour
    {
        [Header("Main Hub")]
        public string hubSceneName = "IUH_Campus_Main";

        [Header("Gia Thinh Scenes")]
        public string thinhSprintSceneName = "Scene_Thinh_SprintToClass";       // Game 1: 3D Chạy điểm danh
        public string thinhRideHailingSceneName = "Scene_Thinh_RideHailingRush"; // Game 2: 3D Xe ôm Grab/Be/Xanh
        public string thinhARSceneName = "Scene_AR_CampusHologram";             // Chế độ AR Hologram

        [Header("Nhat Scenes")]
        public string nhat2DCanteenSceneName = "Scene_2D_Nhat_CanteenRush";     // Game 3: 2D Canteen Tilemap
        public string nhatParkingSceneName = "Scene_Nhat_ParkingMaster";        // Game 4: 3D Bãi đỗ xe & Unblock

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
            LoadSceneWithFeedback(thinhSprintSceneName, "Minigame 1: 3D Chay Bo Diem Danh (Gia Thinh)");
        }

        public void LoadThinhRideHailingGame()
        {
            LoadSceneWithFeedback(thinhRideHailingSceneName, "Minigame 2: 3D Xe Om Cong Nghe Grab/Be/Xanh (Gia Thinh)");
        }

        public void LoadThinhARMode()
        {
            LoadSceneWithFeedback(thinhARSceneName, "Che Do AR Hologram Quan The IUH (Gia Thinh)");
        }

        // ================= NHẬT =================
        public void LoadNhat2DCanteenGame()
        {
            LoadSceneWithFeedback(nhat2DCanteenSceneName, "Minigame 3: 2D Tilemap Canteen Overcooked (Nhat)");
        }

        public void LoadNhatParkingGame()
        {
            LoadSceneWithFeedback(nhatParkingSceneName, "Minigame 4: 3D Xep Xe & Giai Cuu Xe Ket (Nhat)");
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
