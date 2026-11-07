using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Guardian
{
    /// <summary>
    /// Sci-Fi OSD Heads-Up-Display for the IUH Guardian Drone.
    /// Handles altitude, speed, battery, compass, target recognition, shutter flash, report popup,
    /// and cinematic status overlay.
    /// Flight UI is ONLY visible when piloting the drone.
    /// Report popup is ONLY visible when taking a photo.
    /// </summary>
    public class IUHDroneHUD : MonoBehaviour
    {
        public enum DroneVisionMode
        {
            Normal = 0,
            CyberScan = 1,
            NightVision = 2
        }

        [Header("Flight Overlay Root (Visible only when piloting drone)")]
        [SerializeField] private GameObject m_DroneFlightOverlayRoot;

        [Header("UI Telemetry References")]
        [SerializeField] private Text m_AltitudeText;
        [SerializeField] private Text m_SpeedText;
        [SerializeField] private Text m_BatteryText;
        [SerializeField] private Text m_CompassText;
        [SerializeField] private Text m_TargetInfoText;
        [SerializeField] private Text m_VisionModeText;
        [SerializeField] private Text m_CinematicStatusText;
        [SerializeField] private Image m_VisionTintImage;
        [SerializeField] private Image m_FlashImage;
        [SerializeField] private GameObject m_CrosshairReticle;

        [Header("Recon Report Popup (Visible ONLY on photo capture)")]
        [SerializeField] private GameObject m_ReportPopupPanel;
        [SerializeField] private Text m_ReportPopupTitle;
        [SerializeField] private Text m_ReportPopupDesc;

        [Header("Student HUD Launch Button (Visible only when controlling student)")]
        [SerializeField] private GameObject m_StudentLaunchDroneButton;

        private IUHGuardianDroneController m_DroneController;
        private Coroutine m_FlashRoutine;
        private Coroutine m_ReportRoutine;

        private void Awake()
        {
            // ALWAYS guarantee report popup is hidden on game launch
            if (m_ReportPopupPanel != null) m_ReportPopupPanel.SetActive(false);
            if (m_FlashImage != null) m_FlashImage.color = new Color(1, 1, 1, 0);
            if (m_CinematicStatusText != null) m_CinematicStatusText.gameObject.SetActive(false);
            if (m_DroneFlightOverlayRoot != null) m_DroneFlightOverlayRoot.SetActive(false);
            if (m_StudentLaunchDroneButton != null) m_StudentLaunchDroneButton.SetActive(true);
        }

        public void Initialize(IUHGuardianDroneController controller)
        {
            m_DroneController = controller;
            SetDroneHUDActive(false);
        }

        public void SetDroneHUDActive(bool active)
        {
            if (m_DroneFlightOverlayRoot != null)
            {
                m_DroneFlightOverlayRoot.SetActive(active);
            }
            if (m_StudentLaunchDroneButton != null)
            {
                m_StudentLaunchDroneButton.SetActive(!active);
            }
            // Ensure report popup remains hidden unless actively triggered
            if (m_ReportPopupPanel != null)
            {
                m_ReportPopupPanel.SetActive(false);
            }
        }

        public void UpdateTelemetry(float altitude, float speedKmh, float batteryPercent, float headingAngle, string targetName)
        {
            if (m_AltitudeText != null)
            {
                m_AltitudeText.text = $"ALT: {altitude:F1} m";
            }
            if (m_SpeedText != null)
            {
                m_SpeedText.text = $"SPD: {speedKmh:F1} km/h";
            }
            if (m_BatteryText != null)
            {
                m_BatteryText.text = $"BAT: {batteryPercent:F0}%";
                m_BatteryText.color = batteryPercent > 20f ? Color.cyan : Color.red;
            }
            if (m_CompassText != null)
            {
                string dir = "N";
                if (headingAngle >= 337.5f || headingAngle < 22.5f) dir = "N";
                else if (headingAngle >= 22.5f && headingAngle < 67.5f) dir = "NE";
                else if (headingAngle >= 67.5f && headingAngle < 112.5f) dir = "E";
                else if (headingAngle >= 112.5f && headingAngle < 157.5f) dir = "SE";
                else if (headingAngle >= 157.5f && headingAngle < 202.5f) dir = "S";
                else if (headingAngle >= 202.5f && headingAngle < 247.5f) dir = "SW";
                else if (headingAngle >= 247.5f && headingAngle < 292.5f) dir = "W";
                else if (headingAngle >= 292.5f && headingAngle < 337.5f) dir = "NW";

                m_CompassText.text = $"{dir} {headingAngle:F0}°";
            }
            if (m_TargetInfoText != null)
            {
                if (string.IsNullOrEmpty(targetName))
                {
                    m_TargetInfoText.text = "QUÉT TRINH SÁT: [KHÔNG CÓ MỤC TIÊU]";
                    m_TargetInfoText.color = new Color(0.7f, 0.9f, 1f, 0.7f);
                }
                else
                {
                    m_TargetInfoText.text = $"MỤC TIÊU PHÁT HIỆN: [ {targetName.ToUpper()} ]";
                    m_TargetInfoText.color = Color.yellow;
                }
            }
        }

        public void SetCinematicProgress(string text, bool visible)
        {
            if (m_CinematicStatusText != null)
            {
                m_CinematicStatusText.gameObject.SetActive(visible);
                m_CinematicStatusText.text = text;
            }
        }

        public void SetVisionModeUI(DroneVisionMode mode)
        {
            if (m_VisionModeText != null)
            {
                switch (mode)
                {
                    case DroneVisionMode.Normal:
                        m_VisionModeText.text = "MẮT THẦN: [BÌNH THƯỜNG]";
                        m_VisionModeText.color = Color.white;
                        break;
                    case DroneVisionMode.CyberScan:
                        m_VisionModeText.text = "MẮT THẦN: [CYBER QUÉT NHIỆT / DỮ LIỆU]";
                        m_VisionModeText.color = new Color(1f, 0.3f, 0.8f);
                        break;
                    case DroneVisionMode.NightVision:
                        m_VisionModeText.text = "MẮT THẦN: [HỒNG NGOẠI ĐÊM]";
                        m_VisionModeText.color = Color.green;
                        break;
                }
            }

            if (m_VisionTintImage != null)
            {
                switch (mode)
                {
                    case DroneVisionMode.Normal:
                        m_VisionTintImage.color = new Color(0, 0, 0, 0);
                        break;
                    case DroneVisionMode.CyberScan:
                        m_VisionTintImage.color = new Color(0.6f, 0f, 0.8f, 0.18f);
                        break;
                    case DroneVisionMode.NightVision:
                        m_VisionTintImage.color = new Color(0f, 0.8f, 0.2f, 0.16f);
                        break;
                }
            }
        }

        public void TriggerShutterFlash()
        {
            if (m_FlashRoutine != null) StopCoroutine(m_FlashRoutine);
            m_FlashRoutine = StartCoroutine(ShutterFlashRoutine());
        }

        private IEnumerator ShutterFlashRoutine()
        {
            if (m_FlashImage == null) yield break;
            m_FlashImage.color = new Color(1f, 1f, 1f, 0.85f);
            float elapsed = 0f;
            float duration = 0.35f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(0.85f, 0f, elapsed / duration);
                m_FlashImage.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            m_FlashImage.color = new Color(1f, 1f, 1f, 0f);
        }

        /// <summary>
        /// Displays the recon report popup for 3.5 seconds ONLY when a photo is taken.
        /// </summary>
        public void ShowReconReport(string targetName, int goldReward, int dataCoreReward)
        {
            if (m_ReportRoutine != null) StopCoroutine(m_ReportRoutine);
            m_ReportRoutine = StartCoroutine(ReconReportRoutine(targetName, goldReward, dataCoreReward));
        }

        private IEnumerator ReconReportRoutine(string targetName, int goldReward, int dataCoreReward)
        {
            if (m_ReportPopupPanel == null) yield break;
            m_ReportPopupPanel.SetActive(true);

            if (m_ReportPopupTitle != null)
            {
                m_ReportPopupTitle.text = "📸 BÁO CÁO TRINH SÁT DRONE";
            }
            if (m_ReportPopupDesc != null)
            {
                m_ReportPopupDesc.text = $"Đã ghi nhận dữ liệu:\n<b>{targetName}</b>\n\nPhần thưởng: <b>+{goldReward} Vàng</b> | <b>+{dataCoreReward} Lõi Dữ Liệu</b>!";
            }

            yield return new WaitForSecondsRealtime(3.5f);
            m_ReportPopupPanel.SetActive(false);
        }

        public void ShowActionNotification(string title, string message)
        {
            if (m_ReportRoutine != null) StopCoroutine(m_ReportRoutine);
            m_ReportRoutine = StartCoroutine(ActionNotificationRoutine(title, message));
        }

        private IEnumerator ActionNotificationRoutine(string title, string message)
        {
            if (m_ReportPopupPanel == null) yield break;
            m_ReportPopupPanel.SetActive(true);

            if (m_ReportPopupTitle != null) m_ReportPopupTitle.text = title;
            if (m_ReportPopupDesc != null) m_ReportPopupDesc.text = message;

            yield return new WaitForSecondsRealtime(3.0f);
            m_ReportPopupPanel.SetActive(false);
        }

        // On-screen button hooks
        public void OnClickCapturePhoto()
        {
            if (m_DroneController != null) m_DroneController.CapturePhotoRecon();
        }

        public void OnClickDropSupply()
        {
            if (m_DroneController != null) m_DroneController.DropSupplyAirdrop();
        }

        public void OnClickDropEmp()
        {
            if (m_DroneController != null) m_DroneController.DropEmpBomb();
        }

        public void OnClickToggleCinematic()
        {
            if (m_DroneController != null) m_DroneController.ToggleCinematicMode();
        }

        public void OnClickToggleCameraView()
        {
            if (m_DroneController != null) m_DroneController.ToggleCameraView();
        }

        public void OnClickToggleVision()
        {
            if (m_DroneController != null) m_DroneController.ToggleVisionMode();
        }

        public void OnClickReturnToPlayer()
        {
            if (m_DroneController != null) m_DroneController.SetControlled(false);
        }

        public void OnClickLaunchDroneFromStudent()
        {
            if (m_DroneController != null) m_DroneController.SetControlled(true);
        }
    }
}
