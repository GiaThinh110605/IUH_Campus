using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using StarterAssets;
using IUHCampus.Core;
using IUHCampus.Save;
using IUHCampus.UI;
using IUHCampus.World;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.Minigames.PhotoHunter
{

    public class IUHPhotoHunterManager : MonoBehaviour
    {
        public enum PhotoHunterState
        {
            Inactive,
            Briefing,
            Exploring,
            ViewfinderMode,
            Victory
        }

        public static IUHPhotoHunterManager Instance { get; private set; }

        [Header("Landmark Targets (3 Core Campus Objectives)")]
        [SerializeField] private List<IUHPhotoTarget> m_Targets = new List<IUHPhotoTarget>();

        [Header("Camera & Viewfinder Settings")]
        [SerializeField] private Camera m_ShootingCamera;
        [SerializeField] private float m_NormalFov = 60.0f;
        [SerializeField] private float m_ViewfinderFov = 42.0f;
        [SerializeField] private float m_ZoomSpeed = 8.0f;
        [Tooltip("LayerMask for raycast occluders (Building, Environment, Ground, Default). NEVER excludes Building.")]
        [SerializeField] private LayerMask m_ObstacleLayerMask;

        [Header("UI Controller")]
        [SerializeField] private IUHPhotoHunterUI m_UI;

        [Header("Audio")]
        [SerializeField] private AudioSource m_AudioSource;

        private GameObject m_PlayerObject;
        private ThirdPersonController m_ThirdPersonController;
        private StarterAssetsInputs m_StarterAssetsInputs;
        private CharacterController m_CharacterController;

        private PhotoHunterState m_CurrentState = PhotoHunterState.Inactive;
        private int m_CurrentScore = 0;
        private int m_CompletedCount = 0;
        private bool m_IsCapturing = false;
        private float m_TargetCameraFov;
        private PhotoHunterSaveContainer m_CurrentSaveContainer;

        private Unity.Cinemachine.CinemachineCamera m_CinemachineCamera;
        private float m_OriginalCinemachineFov = 60.0f;
        private bool m_HasSavedCinemachineFov = false;
        private Unity.Cinemachine.CinemachineThirdPersonFollow m_PhotoFollow;
        private float m_OriginalCameraDistance;
        private float m_OriginalArmLength;
        private Vector3 m_OriginalShoulderOffset;
        private bool m_OriginalAvoidObstacles;
        private Renderer[] m_HiddenPlayerRenderers;
        private bool[] m_PlayerRendererStates;

        private bool m_HasSavedSpeeds = false;
        private float m_OriginalMoveSpeed = 2.0f;
        private float m_OriginalSprintSpeed = 5.335f;
        private float m_OriginalJumpHeight = 1.2f;

        private bool m_HoldsControllerLock = false;

        private int m_CurrentSessionId = 0;

        public PhotoHunterState CurrentState => m_CurrentState;
        public bool IsMissionActive => m_CurrentState == PhotoHunterState.Exploring || m_CurrentState == PhotoHunterState.ViewfinderMode;
        public bool HasActiveSessionOrUI => m_CurrentState != PhotoHunterState.Inactive;
        public bool HasActiveModal => m_CurrentState == PhotoHunterState.Briefing || m_CurrentState == PhotoHunterState.Victory;
        public int CurrentScore => m_CurrentScore;
        public int CompletedCount => m_CompletedCount;
        public int TotalTargetsCount => m_Targets != null ? m_Targets.Count : 3;
        public bool IsAllCompleted => m_CompletedCount >= TotalTargetsCount && TotalTargetsCount > 0;
        public LayerMask ObstacleLayerMask => m_ObstacleLayerMask;
        public int CurrentSessionId => m_CurrentSessionId;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (m_AudioSource == null)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSource.playOnAwake = false;
            }

            ConfigureObstacleLayerMask();
            FindSceneReferences();
        }

        private void Start()
        {
            FindSceneReferences();
            LoadProgress();

            if (m_UI != null)
            {
                m_UI.HideAllDialogs();
            }

            m_TargetCameraFov = m_NormalFov;
        }

        private void OnDisable()
        {
            bool wasHoldingLock = m_HoldsControllerLock || HasActiveModal;
            CancelMission(restoreCursorAndController: false);
            if (wasHoldingLock && CanEnablePlayerController())
            {
                SafeEnablePlayerController();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void ConfigureObstacleLayerMask()
        {

            int buildingLayer = LayerMask.NameToLayer("Building");
            int envLayer = LayerMask.NameToLayer("Environment");
            int groundLayer = LayerMask.NameToLayer("Ground");
            int defaultLayer = LayerMask.NameToLayer("Default");

            int mask = 0;
            if (buildingLayer >= 0) mask |= (1 << buildingLayer);
            if (envLayer >= 0) mask |= (1 << envLayer);
            if (groundLayer >= 0) mask |= (1 << groundLayer);
            if (defaultLayer >= 0) mask |= (1 << defaultLayer);

            if (mask == 0)
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                int uiLayer = LayerMask.NameToLayer("UI");
                int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
                int excluded = (1 << 2) | (1 << 5);
                if (playerLayer >= 0) excluded |= (1 << playerLayer);
                if (uiLayer >= 0) excluded |= (1 << uiLayer);
                if (ignoreRaycast >= 0) excluded |= (1 << ignoreRaycast);
                mask = ~excluded;
            }

            m_ObstacleLayerMask = mask;
        }

        private void FindSceneReferences()
        {
            if (m_PlayerObject == null)
            {
                m_PlayerObject = GameObject.FindGameObjectWithTag("Player");
                if (m_PlayerObject == null) m_PlayerObject = GameObject.Find("PlayerArmature");
            }

            if (m_PlayerObject != null)
            {
                if (m_ThirdPersonController == null)
                    m_ThirdPersonController = m_PlayerObject.GetComponent<ThirdPersonController>();
                if (m_StarterAssetsInputs == null)
                    m_StarterAssetsInputs = m_PlayerObject.GetComponent<StarterAssetsInputs>();
                if (m_CharacterController == null)
                    m_CharacterController = m_PlayerObject.GetComponent<CharacterController>();
            }

            if (m_ShootingCamera == null)
            {
                m_ShootingCamera = Camera.main;
                if (m_ShootingCamera != null) m_NormalFov = m_ShootingCamera.fieldOfView;
            }

            var brain = m_ShootingCamera != null
                ? m_ShootingCamera.GetComponent<Unity.Cinemachine.CinemachineBrain>() : null;
            var liveCamera = brain != null ? brain.ActiveVirtualCamera as Unity.Cinemachine.CinemachineCamera : null;
            if (liveCamera != null && !m_HasSavedCinemachineFov)
                m_CinemachineCamera = liveCamera;
            if (m_CinemachineCamera == null)
            {
                m_CinemachineCamera = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsInactive.Exclude);
            }

            if (m_UI == null)
            {
                m_UI = FindAnyObjectByType<IUHPhotoHunterUI>(FindObjectsInactive.Include);
            }

            if (m_Targets == null || m_Targets.Count == 0)
            {
                var foundTargets = FindObjectsByType<IUHPhotoTarget>(FindObjectsInactive.Include);
                m_Targets = new List<IUHPhotoTarget>(foundTargets);
            }
        }

        private void SaveAndZeroMovementSpeed()
        {
            if (m_ThirdPersonController != null)
            {
                if (!m_HasSavedSpeeds)
                {
                    m_OriginalMoveSpeed = m_ThirdPersonController.MoveSpeed;
                    m_OriginalSprintSpeed = m_ThirdPersonController.SprintSpeed;
                    m_OriginalJumpHeight = m_ThirdPersonController.JumpHeight;
                    m_HasSavedSpeeds = true;
                }
                m_ThirdPersonController.MoveSpeed = 0f;
                m_ThirdPersonController.SprintSpeed = 0f;
                m_ThirdPersonController.JumpHeight = 0f;
            }
        }

        private void RestoreMovementSpeed()
        {
            if (m_ThirdPersonController != null && m_HasSavedSpeeds)
            {
                m_ThirdPersonController.MoveSpeed = m_OriginalMoveSpeed;
                m_ThirdPersonController.SprintSpeed = m_OriginalSprintSpeed;
                m_ThirdPersonController.JumpHeight = m_OriginalJumpHeight;
                m_HasSavedSpeeds = false;
            }
        }

        private void RestoreCinemachineFov()
        {
            if (m_PhotoFollow != null)
            {
                m_PhotoFollow.CameraDistance = m_OriginalCameraDistance;
                m_PhotoFollow.VerticalArmLength = m_OriginalArmLength;
                m_PhotoFollow.ShoulderOffset = m_OriginalShoulderOffset;
                m_PhotoFollow.AvoidObstacles.Enabled = m_OriginalAvoidObstacles;
                m_PhotoFollow = null;
                if (m_CinemachineCamera != null) m_CinemachineCamera.PreviousStateIsValid = false;
            }
            if (m_HiddenPlayerRenderers != null)
            {
                for (int i = 0; i < m_HiddenPlayerRenderers.Length; i++)
                    if (m_HiddenPlayerRenderers[i] != null)
                        m_HiddenPlayerRenderers[i].enabled = m_PlayerRendererStates[i];
                m_HiddenPlayerRenderers = null;
                m_PlayerRendererStates = null;
            }
            if (m_CinemachineCamera != null && m_HasSavedCinemachineFov)
            {
                m_CinemachineCamera.Lens.FieldOfView = m_OriginalCinemachineFov;
                m_HasSavedCinemachineFov = false;
            }
            if (m_ShootingCamera != null)
            {
                m_ShootingCamera.fieldOfView = m_NormalFov;
            }
        }

        private void Update()
        {

            if (IsExternalUIOpen()) return;

            if (m_CurrentState == PhotoHunterState.Exploring)
            {

                if (IUHInputHelper.IsPhotoToggleKeyDown())
                {
                    EnterViewfinderMode();
                    return;
                }
            }
            else if (m_CurrentState == PhotoHunterState.ViewfinderMode)
            {

                if (IUHInputHelper.IsPhotoExitKeyDown())
                {
                    ExitViewfinderMode();
                    return;
                }

                if (IUHInputHelper.IsPhotoCaptureKeyDown(
                    EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
                {
                    TriggerCapturePhoto();
                    return;
                }
            }
        }

        public bool CanEnablePlayerController()
        {
            if (IsExternalUIOpen()) return false;
            if (IUHDeliveryDialogueUI.Instance != null && IUHDeliveryDialogueUI.Instance.IsOpen) return false;
            if (IUHMainMenuUI.Instance != null && IUHMainMenuUI.Instance.IsOpen) return false;

            var sittingCtrl = UnityEngine.Object.FindAnyObjectByType<IUHCampus.Player.IUHPlayerSittingController>();
            if (sittingCtrl != null && sittingCtrl.IsSitting) return false;

            return true;
        }

        private void SafeEnablePlayerController()
        {
            m_HoldsControllerLock = false;
            if (m_ThirdPersonController != null && CanEnablePlayerController())
            {
                m_ThirdPersonController.enabled = true;
            }
        }

        private bool IsExternalUIOpen()
        {
            if (IUHMiniGamePortalUI.Instance != null && IUHMiniGamePortalUI.Instance.IsOpen) return true;
            if (IUHCustomizationUI.Instance != null && IUHCustomizationUI.Instance.IsOpen) return true;
            if (IUHDeliveryDialogueUI.Instance != null && IUHDeliveryDialogueUI.Instance.IsOpen) return true;
            if (IUHMainMenuUI.Instance != null && IUHMainMenuUI.Instance.IsOpen) return true;
            return false;
        }

        public void OpenBriefing()
        {
            var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);
            if (HasActiveSessionOrUI) CancelMission(restoreCursorAndController: false);

            var grabMgr = IUHGrabCampusManager.Instance;
            if (grabMgr != null)
            {
                Debug.Log("[Photo Hunter] Dừng nhiệm vụ Grab Campus trước khi mở Photo Hunter.");
                grabMgr.CancelMission();
            }

            FindSceneReferences();
            m_CurrentSessionId++;
            m_CurrentState = PhotoHunterState.Briefing;

            if (IUHMiniGamePortalUI.Instance != null && IUHMiniGamePortalUI.Instance.IsOpen)
            {
                IUHMiniGamePortalUI.Instance.SetMenuOpen(false);
            }

            if (m_ThirdPersonController != null)
            {
                m_ThirdPersonController.enabled = false;
            }
            m_HoldsControllerLock = true;

            if (m_UI != null)
            {
                m_UI.HideAllDialogs();
                m_UI.ShowBriefing(true, IsAllCompleted);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            PlaySound(IUHSoundSynthesizer.GetClickSound(), 0.8f);
        }

        public void OpenAlbumFromBriefing()
        {
            ShowVictoryDialog();
        }

        public void StartMission(bool fastTravelToGate1 = false)
        {
            IUHCampus.Core.IUHGameFlowManager.Instance?.NotifyGameStarted(3);
            FindSceneReferences();
            RestoreCinemachineFov();
            m_CurrentSessionId++;
            m_CurrentState = PhotoHunterState.Exploring;

            RestoreMovementSpeed();
            SafeEnablePlayerController();

            if (m_ThirdPersonController != null)
            {
                m_ThirdPersonController.ResetMotion();
                if (m_CinemachineCamera != null && m_ThirdPersonController.CinemachineCameraTarget != null)
                {
                    m_CinemachineCamera.Follow = m_ThirdPersonController.CinemachineCameraTarget.transform;
                    m_CinemachineCamera.PreviousStateIsValid = false;
                }
            }

            if (m_UI != null)
            {
                m_UI.HideAllDialogs();
                m_UI.ShowHUD(true, IsAllCompleted);
                UpdateUIProgress();
            }

            if (CanEnablePlayerController())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (fastTravelToGate1 && m_Targets.Count > 0)
            {
                var gate1 = m_Targets.Find(t => t.TargetId.Contains("gate_01"));
                if (gate1 != null && gate1.ScoutVantagePoint != Vector3.zero)
                {
                    TeleportPlayer(gate1.ScoutVantagePoint, gate1.ScoutLookDirection);
                }
            }

            PlaySound(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
            Debug.Log("[Photo Hunter] Bắt đầu nhiệm vụ Thợ Săn Khoảnh Khắc!");
        }

        public void EnterViewfinderMode()
        {
            if (m_CurrentState != PhotoHunterState.Exploring) return;

            FindSceneReferences();
            m_CurrentState = PhotoHunterState.ViewfinderMode;

            SaveAndZeroMovementSpeed();
            if (CanEnablePlayerController())
            {
                if (m_ThirdPersonController != null)
                {
                    m_ThirdPersonController.enabled = true;
                }
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (m_CinemachineCamera != null)
            {
                if (!m_HasSavedCinemachineFov)
                {
                    m_OriginalCinemachineFov = m_CinemachineCamera.Lens.FieldOfView;
                    m_HasSavedCinemachineFov = true;
                }
                m_CinemachineCamera.Lens.FieldOfView = m_ViewfinderFov;

                m_PhotoFollow = m_CinemachineCamera.GetComponent<Unity.Cinemachine.CinemachineThirdPersonFollow>();
                if (m_PhotoFollow != null)
                {
                    m_OriginalCameraDistance = m_PhotoFollow.CameraDistance;
                    m_OriginalArmLength = m_PhotoFollow.VerticalArmLength;
                    m_OriginalShoulderOffset = m_PhotoFollow.ShoulderOffset;
                    m_OriginalAvoidObstacles = m_PhotoFollow.AvoidObstacles.Enabled;
                    m_PhotoFollow.CameraDistance = 0.05f;
                    m_PhotoFollow.VerticalArmLength = 0f;
                    m_PhotoFollow.ShoulderOffset = Vector3.zero;
                    m_PhotoFollow.AvoidObstacles.Enabled = false;
                    m_CinemachineCamera.PreviousStateIsValid = false;
                }
            }
            if (m_PlayerObject != null)
            {
                m_HiddenPlayerRenderers = m_PlayerObject.GetComponentsInChildren<Renderer>(true);
                m_PlayerRendererStates = new bool[m_HiddenPlayerRenderers.Length];
                for (int i = 0; i < m_HiddenPlayerRenderers.Length; i++)
                {
                    m_PlayerRendererStates[i] = m_HiddenPlayerRenderers[i].enabled;
                    m_HiddenPlayerRenderers[i].enabled = false;
                }
            }
            if (m_ShootingCamera != null)
            {
                m_ShootingCamera.fieldOfView = m_ViewfinderFov;
            }

            if (m_UI != null)
            {
                m_UI.ShowViewfinder(true);
            }

            PlaySound(IUHSoundSynthesizer.GetClickSound(), 0.9f);
        }

        public void ExitViewfinderMode()
        {
            if (m_CurrentState != PhotoHunterState.ViewfinderMode) return;

            m_CurrentState = PhotoHunterState.Exploring;

            RestoreMovementSpeed();
            SafeEnablePlayerController();

            RestoreCinemachineFov();

            if (CanEnablePlayerController())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (m_UI != null)
            {
                m_UI.ShowViewfinder(false);
            }

            PlaySound(IUHSoundSynthesizer.GetClickSound(), 0.7f);
        }

        public void CancelMission(bool restoreCursorAndController = true)
        {
            m_CurrentSessionId++;
            m_IsCapturing = false;
            StopAllCoroutines();

            RestoreMovementSpeed();
            RestoreCinemachineFov();

            if (restoreCursorAndController && CanEnablePlayerController())
            {
                SafeEnablePlayerController();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                m_HoldsControllerLock = false;
            }

            m_CurrentState = PhotoHunterState.Inactive;

            if (m_UI != null)
            {
                m_UI.HideAllDialogs();
            }

            Debug.Log("[Photo Hunter] Đã kết thúc / đóng sạch mọi trạng thái Photo Hunter.");
        }

        public void CancelMission()
        {
            CancelMission(true);
        }

        public void RestartMission()
        {
            if(!IUHPhotoHunterStorage.ResetProgress())
            {
                m_UI?.ShowPersistenceError("Không lưu được lượt mới. Tiến trình trước vẫn được giữ.");
                return;
            }
            m_CurrentSessionId++;
            m_IsCapturing = false;
            StopAllCoroutines();

            m_CurrentSaveContainer = new PhotoHunterSaveContainer();
            m_CurrentScore = 0;
            m_CompletedCount = 0;

            foreach (var t in m_Targets)
            {
                if (t != null) t.SetCompleted(false);
            }

            SyncScoreToWorldState();
            StartMission(false);
        }

        public void TriggerCapturePhoto()
        {
            if (m_IsCapturing) return;
            if (m_CurrentState != PhotoHunterState.ViewfinderMode) return;
            if (IsExternalUIOpen()) return;

            FindSceneReferences();
            if (m_ShootingCamera == null)
            {
                m_UI?.ShowToast("Lỗi: Không tìm thấy camera ngắm chụp!", 2.5f, false);
                return;
            }

            Transform playerTrans = m_PlayerObject != null ? m_PlayerObject.transform : null;

            IUHPhotoTarget aimedTarget = null;
            float smallestAngle = float.MaxValue;

            foreach (var target in m_Targets)
            {
                if (target == null) continue;
                float angle = target.GetFacingAngle(m_ShootingCamera);
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    aimedTarget = target;
                }
            }

            if (aimedTarget == null)
            {
                PlaySound(IUHSoundSynthesizer.GetPhotoWarningSound(), 0.9f);
                m_UI?.ShowToast("Chưa xác định được mục tiêu trong tầm ngắm.", 2.5f, false);
                return;
            }

            if (!aimedTarget.ValidatePhoto(m_ShootingCamera, playerTrans, m_ObstacleLayerMask, out string reason))
            {

                PlaySound(IUHSoundSynthesizer.GetPhotoWarningSound(), 0.9f);
                m_UI?.ShowToast(reason, 3.0f, false);
                return;
            }

            StartCoroutine(CaptureTargetRoutine(m_CurrentSessionId, aimedTarget));
        }

        private IEnumerator CaptureTargetRoutine(int sessionId, IUHPhotoTarget target)
        {
            m_IsCapturing = true;

            PlaySound(IUHSoundSynthesizer.GetCameraShutterSound(), 1.0f);
            m_UI?.TriggerFlashShutter();

            bool captureFinished = false;
            bool captureSuccess = false;
            string photoPath = string.Empty;

            IUHPhotoCaptureService.Instance.CaptureAndSavePhoto(
                m_ShootingCamera,
                target.TargetId,
                sessionId,
                (sId) => sId == m_CurrentSessionId && IsMissionActive,
                (success, path) => {
                    captureFinished = true;
                    captureSuccess = success;
                    photoPath = path;
                });

            while (!captureFinished)
            {
                if (sessionId != m_CurrentSessionId || !IsMissionActive)
                {
                    m_IsCapturing = false;
                    yield break;
                }
                yield return null;
            }

            if (sessionId != m_CurrentSessionId || !IsMissionActive)
            {
                m_IsCapturing = false;
                yield break;
            }

            if (!captureSuccess || string.IsNullOrEmpty(photoPath))
            {
                m_IsCapturing = false;
                PlaySound(IUHSoundSynthesizer.GetPhotoWarningSound(), 0.9f);
                m_UI?.ShowToast("Lỗi ghi file ảnh ra đĩa! Không thể ghi nhận hoàn thành.", 3.0f, false);
                yield break;
            }

            bool txSuccess = ExecutePhotoTransaction(
                target.TargetId,
                target.DisplayName,
                photoPath,
                target.RewardScore,
                out int awardedPoints);

            if (!txSuccess)
            {
                m_IsCapturing = false;
                PlaySound(IUHSoundSynthesizer.GetPhotoWarningSound(), 0.9f);
                m_UI?.ShowToast("Lỗi lưu tiến trình bền vững! Đã hủy ghi nhận chụp ảnh.", 3.0f, false);
                yield break;
            }

            target.SetCompleted(true);
            if (awardedPoints > 0)
            {
                PlaySound(IUHSoundSynthesizer.GetPhotoSuccessSound(), 1.0f);
                m_UI?.ShowToast($"Chụp thành công: {target.DisplayName}! (+{awardedPoints} điểm)", 3.5f, true);
            }
            else
            {
                m_UI?.ShowToast($"Đã lưu lại bức ảnh mới cho {target.DisplayName}!", 2.5f, true);
            }

            UpdateUIProgress();

            yield return new WaitForSeconds(0.6f);
            m_IsCapturing = false;

            if (sessionId != m_CurrentSessionId || !IsMissionActive) yield break;

            if (m_CompletedCount >= m_Targets.Count && m_Targets.Count > 0)
            {
                yield return new WaitForSeconds(1.0f);
                if (sessionId == m_CurrentSessionId && IsMissionActive)
                {
                    ShowVictoryDialog();
                }
            }
        }

        public bool ExecutePhotoTransaction(
            string targetId,
            string displayName,
            string photoPath,
            int rewardScore,
            out int awardedPoints)
        {
            if (m_CurrentSaveContainer == null) m_CurrentSaveContainer = new PhotoHunterSaveContainer();

            PhotoHunterSaveContainer tentativeContainer = m_CurrentSaveContainer.Clone();
            string replacedPath=tentativeContainer.photoRecords.Find(r=>r!=null&&r.targetId==targetId)?.filePath;
            bool isNewCompletion = tentativeContainer.RecordCompletion(
                targetId,
                displayName,
                photoPath,
                rewardScore,
                out awardedPoints);

            bool saveDiskSuccess = IUHPhotoHunterStorage.SaveProgress(tentativeContainer);
            if (!saveDiskSuccess)
            {
                awardedPoints = 0;

                try { if (!string.Equals(photoPath,replacedPath,StringComparison.Ordinal)&&IUHPhotoHunterStorage.IsManagedJpeg(photoPath)&&File.Exists(photoPath)) File.Delete(photoPath); } catch (IOException) { } catch(UnauthorizedAccessException) { }
                return false;
            }

            m_CurrentSaveContainer = tentativeContainer;
            if(!string.Equals(replacedPath,photoPath,StringComparison.Ordinal)&&IUHPhotoHunterStorage.IsManagedJpeg(replacedPath))
            {try{if(File.Exists(replacedPath))File.Delete(replacedPath);}catch(IOException){}catch(UnauthorizedAccessException){}}
            if (isNewCompletion)
            {
                m_CurrentScore = m_CurrentSaveContainer.totalScore;
                m_CompletedCount = m_CurrentSaveContainer.completedTargetIds.Count;
                SyncScoreToWorldState();
            }
            return true;
        }

        public void ShowVictoryDialog()
        {
            m_CurrentState = PhotoHunterState.Victory;
            RestoreMovementSpeed();
            RestoreCinemachineFov();

            if (m_ThirdPersonController != null)
            {
                m_ThirdPersonController.enabled = false;
            }
            m_HoldsControllerLock = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (m_UI != null)
            {
                m_UI.HideAllDialogs();
                m_UI.PopulateVictoryDialog(m_CurrentScore, m_CurrentSaveContainer?.photoRecords);
                m_UI.ShowVictory(true);
            }

            PlaySound(IUHSoundSynthesizer.GetPhotoSuccessSound(), 1.0f);
            Debug.Log($"[Photo Hunter] Đã hoàn thành toàn bộ 3/3 địa điểm! Tổng điểm: {m_CurrentScore}");
        }

        public PhotoHunterSaveContainer CurrentSaveContainer => m_CurrentSaveContainer;

        public void SetCustomSaveContainerForTest(PhotoHunterSaveContainer container)
        {
            m_CurrentSaveContainer = container;
            if (m_CurrentSaveContainer != null)
            {
                m_CurrentScore = m_CurrentSaveContainer.totalScore;
                if (m_Targets != null && m_Targets.Count > 0)
                {
                    m_CompletedCount = 0;
                    foreach (var target in m_Targets)
                    {
                        if (target == null) continue;
                        bool completed = m_CurrentSaveContainer.completedTargetIds.Contains(target.TargetId);
                        target.SetCompleted(completed);
                        if (completed) m_CompletedCount++;
                    }
                }
                else
                {
                    m_CompletedCount = m_CurrentSaveContainer.completedTargetIds.Count;
                }
            }
            else
            {
                m_CurrentScore = 0;
                m_CompletedCount = 0;
            }
            SyncScoreToWorldState();
            UpdateUIProgress();
        }

        private void LoadProgress()
        {
            m_CurrentSaveContainer = IUHPhotoHunterStorage.LoadProgress();

            var validTargetIds = new HashSet<string>();
            if (m_Targets != null)
            {
                foreach (var t in m_Targets)
                {
                    if (t != null && !string.IsNullOrEmpty(t.TargetId))
                        validTargetIds.Add(t.TargetId);
                }
            }

            m_CurrentSaveContainer.SanitizeAndValidate(validTargetIds, 100);
            m_CurrentScore = m_CurrentSaveContainer.totalScore;

            if (m_Targets != null)
            {
                m_CompletedCount = 0;
                foreach (var target in m_Targets)
                {
                    if (target == null) continue;
                    bool completed = m_CurrentSaveContainer.completedTargetIds.Contains(target.TargetId);
                    target.SetCompleted(completed);
                    if (completed) m_CompletedCount++;
                }
            }

            SyncScoreToWorldState();
            UpdateUIProgress();
        }

        private void SyncScoreToWorldState()
        {
            if (WorldState.Instance != null)
            {
                WorldState.Instance.SetObjectState("MINIGAME_PHOTO_HUNTER_SCORE", m_CurrentScore.ToString());
                WorldState.Instance.SetObjectState("MINIGAME_PHOTO_HUNTER_COMPLETED", m_CompletedCount.ToString());
            }
        }

        private void UpdateUIProgress()
        {
            if (m_UI != null)
            {
                int total = m_Targets != null ? m_Targets.Count : 3;
                m_UI.UpdateHUD(m_CompletedCount, total, m_CurrentScore, m_Targets);
            }
        }

        private void TeleportPlayer(Vector3 pos, Vector3 lookDir)
        {
            if (m_PlayerObject != null)
            {
                Vector3 positionDelta = pos - m_PlayerObject.transform.position;
                if (m_CharacterController != null) m_CharacterController.enabled = false;
                m_PlayerObject.transform.position = pos;
                if (lookDir != Vector3.zero)
                {
                    m_PlayerObject.transform.rotation = Quaternion.LookRotation(new Vector3(lookDir.x, 0, lookDir.z));
                    if (m_ThirdPersonController != null)
                        m_ThirdPersonController.SetCameraOrientation(lookDir);
                }
                if (m_CharacterController != null) m_CharacterController.enabled = true;
                if (m_ThirdPersonController != null) m_ThirdPersonController.ResetMotion();
                if (m_CinemachineCamera != null && m_CinemachineCamera.Follow != null)
                {
                    m_CinemachineCamera.OnTargetObjectWarped(m_CinemachineCamera.Follow, positionDelta);
                    m_CinemachineCamera.PreviousStateIsValid = false;
                }
            }
        }

        private void PlaySound(AudioClip clip, float volume = 1.0f)
        {
            if (m_AudioSource != null && clip != null)
            {
                m_AudioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
