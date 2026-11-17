using UnityEngine;
using StarterAssets;
using IUHCampus.Guardian.Match3;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.World
{

    [DefaultExecutionOrder(-100)]
    public class IUHConceptPreviewController : MonoBehaviour
    {
        public Camera overviewCamera;
        public Vector3 entrance = new Vector3(0, 1.2f, -38);
        Camera walkingCamera;
        ThirdPersonController movement;
        private static int escapeHandledFrame = -1;
        public static bool EscapeHandledThisFrame => escapeHandledFrame == Time.frameCount;
        bool overview, freeCursor;
        public bool IsExplorationPaused => overview || freeCursor;

        private void Awake()
        {
            EnsurePlayerCameras();
        }

        public void EnsurePlayerCameras()
        {
            // 1. Kiểm tra MainCamera
            if (walkingCamera == null)
            {
                walkingCamera = Camera.main;
            }
            if (walkingCamera == null)
            {
                var mainGo = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainGo != null) walkingCamera = mainGo.GetComponent<Camera>();
            }

            // Nếu chưa có MainCamera trong scene, nạp từ Resources hoặc tạo mới chuẩn URP
            if (walkingCamera == null)
            {
                var mainPrefab = Resources.Load<GameObject>("MainCamera");
                GameObject camObj = null;
                if (mainPrefab != null)
                {
                    camObj = Instantiate(mainPrefab);
                    camObj.name = "MainCamera";
                }
                else
                {
                    camObj = new GameObject("MainCamera");
                    camObj.tag = "MainCamera";
                    var cam = camObj.AddComponent<Camera>();
                    cam.nearClipPlane = 0.2f;
                    cam.farClipPlane = 1000f;
                    cam.clearFlags = CameraClearFlags.Skybox;
                    camObj.AddComponent<AudioListener>();
                    var uData = camObj.AddComponent<UniversalAdditionalCameraData>();
                    uData.renderPostProcessing = true;
                    uData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    camObj.AddComponent<CinemachineBrain>();
                }
                walkingCamera = camObj.GetComponent<Camera>();
            }

            // Đảm bảo CinemachineBrain luôn có trên MainCamera
            if (walkingCamera != null && walkingCamera.GetComponent<CinemachineBrain>() == null)
            {
                walkingCamera.gameObject.AddComponent<CinemachineBrain>();
            }

            // 2. Đảm bảo PlayerFollowCamera (CinemachineCamera) bám theo nhân vật
            movement = GetComponent<ThirdPersonController>();
            Transform targetTransform = movement != null && movement.CinemachineCameraTarget != null
                ? movement.CinemachineCameraTarget.transform
                : (transform.Find("PlayerCameraRoot") ?? transform);

            var vcam = FindAnyObjectByType<CinemachineCamera>();
            if (vcam == null)
            {
                var followPrefab = Resources.Load<GameObject>("PlayerFollowCamera");
                GameObject vcamObj = null;
                if (followPrefab != null)
                {
                    vcamObj = Instantiate(followPrefab);
                    vcamObj.name = "PlayerFollowCamera";
                    vcam = vcamObj.GetComponent<CinemachineCamera>();
                }
                else
                {
                    vcamObj = new GameObject("PlayerFollowCamera");
                    vcam = vcamObj.AddComponent<CinemachineCamera>();
                    var follow = vcamObj.AddComponent<CinemachineThirdPersonFollow>();
                    follow.CameraDistance = 3.5f;
                    follow.CameraSide = 0.6f;
                    follow.ShoulderOffset = new Vector3(0.5f, 0f, 0f);
                }
            }

            if (vcam != null && targetTransform != null)
            {
                vcam.Follow = targetTransform;
                vcam.Target.TrackingTarget = targetTransform;

                // Triệt tiêu hoàn toàn rung giật camera: tắt Perlin Noise và tắt giật collider Obstacle Avoidance
                var perlin = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
                if (perlin != null)
                {
                    perlin.enabled = false;
                    perlin.AmplitudeGain = 0f;
                    perlin.FrequencyGain = 0f;
                }
                var followComp = vcam.GetComponent<CinemachineThirdPersonFollow>();
                if (followComp != null)
                {
                    followComp.AvoidObstacles.Enabled = false;
                    followComp.Damping = new Vector3(0.02f, 0.02f, 0.02f);
                }
            }

            if (movement != null)
            {
                movement.SetCameraOrientation(Vector3.forward);
            }
        }

        void Start()
        {
            EnsurePlayerCameras();

            // Tránh hiện tượng physics tunneling khi mới load scene trên mobile
            Time.maximumDeltaTime = 0.05f;

            movement = GetComponent<ThirdPersonController>();

            // Đảm bảo sinh viên luôn xuất hiện an toàn trên mặt đất (Y >= 0.2f)
            var cc = GetComponent<CharacterController>();
            bool ccWasEnabled = cc != null && cc.enabled;
            if (cc != null) cc.enabled = false;
            if (transform.position.y < 0.15f)
            {
                transform.position = new Vector3(transform.position.x, 0.25f, transform.position.z);
            }
            Physics.SyncTransforms();
            if (cc != null) cc.enabled = ccWasEnabled;
            movement?.ResetMotion();

            if (Application.isMobilePlatform)
            {
                freeCursor = false;
            }

            Apply();
        }
        public static bool IsAnyUIOpen()
        {
            if (IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance != null)
            {
                if (IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance.IsMascotDialogOpen ||
                    IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance.IsAlbumOpen)
                    return true;
            }
            if (IUHCampus.Combat.IUHCombatHUD.Instance != null && IUHCampus.Combat.IUHCombatHUD.Instance.IsGuideOpen)
                return true;
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
                return true;
            return false;
        }

        void Update()
        {
            if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.BlocksExplorationInput) return;
            bool view=false,escape=false;
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null){view=Keyboard.current.vKey.wasPressedThisFrame;escape=Keyboard.current.escapeKey.wasPressedThisFrame;}
#else
            view=Input.GetKeyDown(KeyCode.V);escape=Input.GetKeyDown(KeyCode.Escape);
#endif
            if(view){overview=!overview;Apply();}
            if (escape)
            {
                escapeHandledFrame = Time.frameCount;
                if (!IsAnyUIOpen()) freeCursor = !freeCursor;
                Apply();
            }
            if (IsAnyUIOpen())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (movement != null) movement.enabled = false;
            }
            // Tự động giải cứu nếu nhân vật lọt xuống dưới mặt đất (Y < 0.05f)
            if (transform.position.y < 0.05f)
            {
                var cc = GetComponent<CharacterController>();
                bool enabled = cc != null && cc.enabled;
                if (cc != null) cc.enabled = false;
                transform.position = new Vector3(transform.position.x, 0.25f, transform.position.z);
                Physics.SyncTransforms();
                if (cc != null) cc.enabled = enabled;
                movement?.ResetMotion();
            }
        }
        public void Apply()
        {
            bool uiOpen = IsAnyUIOpen();
            bool unlock = overview || freeCursor || uiOpen;
            if(walkingCamera!=null)walkingCamera.enabled=!overview;
            if(overviewCamera!=null)overviewCamera.enabled=overview;
            if(movement!=null){movement.ResetMotion();movement.enabled=!overview&&!freeCursor&&!uiOpen;}
            Cursor.lockState=unlock?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=unlock;
        }
    }
}
