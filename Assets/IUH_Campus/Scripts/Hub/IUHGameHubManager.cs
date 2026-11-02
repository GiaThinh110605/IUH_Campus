using UnityEngine;
using UnityEngine.SceneManagement;

namespace IUHCampus.Hub
{

    public class IUHGameHubManager : MonoBehaviour
    {
        [Header("Main Hub")]
        public string hubSceneName = "IUH_Campus_Main";

        [Header("Gia Thinh Scenes")]
        public string thinhSprintSceneName = "Scene_Thinh_SprintToClass";
        public string thinhRideHailingSceneName = "Scene_Thinh_RideHailingRush";
        public string thinhARSceneName = "Scene_AR_CampusHologram";

        [Header("Nhat Scenes")]
        public string nhat2DCanteenSceneName = "Scene_2D_Nhat_CanteenRush";
        public string nhatParkingSceneName = "Scene_Nhat_ParkingMaster";

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

        public void LoadNhat2DCanteenGame()
        {
            LoadSceneWithFeedback(nhat2DCanteenSceneName, "Minigame 3: 2D Tilemap Canteen Overcooked (Nhat)");
        }

        public void LoadNhatParkingGame()
        {
            LoadSceneWithFeedback(nhatParkingSceneName, "Minigame 4: 3D Xep Xe & Giai Cuu Xe Ket (Nhat)");
        }

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
