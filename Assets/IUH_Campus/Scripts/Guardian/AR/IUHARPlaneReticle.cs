using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Guardian.AR
{
    /// <summary>
    /// Điều khiển vòng nhắm định vị Hologram trên mặt phẳng AR (AR Plane Surface Tracking Reticle):
    /// - Quét nhận diện bề mặt sàn nhà / mặt đất bằng ARRaycastManager (hoặc Physics Raycast fallback).
    /// - Hiệu ứng vòng xoay Hologram đồng tâm (Concentric Rings) nhấp nháy phát quang neon.
    /// - Cho phép Chạm màn hình / Click chuột / Phím [E] để xác nhận vị trí đặt Dị Tượng Nhiễm Độc.
    /// </summary>
    public class IUHARPlaneReticle : MonoBehaviour
    {
        [Header("Reticle Visual")]
        [SerializeField] private GameObject m_ReticleObject;
        [SerializeField] private Transform m_OuterHoloRing;
        [SerializeField] private Transform m_InnerHoloRing;
        [SerializeField] private Text m_StatusInstructionText;

        [Header("Placement Target")]
        [SerializeField] private IUHARCorruptedObjectEncounter m_EncounterSpawner;

        private Camera m_ARCamera;
        private bool m_PlaneDetected = false;
        private Vector3 m_PlacementPosition;
        private Quaternion m_PlacementRotation = Quaternion.identity;

#if UNITY_XR_ARFOUNDATION
        private ARRaycastManager m_RaycastManager;
        private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
#endif

        private void Awake()
        {
            m_ARCamera = Camera.main;
#if UNITY_XR_ARFOUNDATION
            m_RaycastManager = GetComponent<ARRaycastManager>() ?? FindAnyObjectByType<ARRaycastManager>();
#endif
            if (m_ReticleObject != null) m_ReticleObject.SetActive(false);
        }

        private void Update()
        {
            if (m_EncounterSpawner != null && m_EncounterSpawner.IsPlaced)
            {
                if (m_ReticleObject != null && m_ReticleObject.activeSelf) m_ReticleObject.SetActive(false);
                if (m_StatusInstructionText != null && m_StatusInstructionText.gameObject.activeSelf) m_StatusInstructionText.gameObject.SetActive(false);
                return;
            }

            UpdateReticlePosition();
            AnimateHologramRings();

            // Nhận diện thao tác đặt: Cảm ứng chạm (Touch), Click chuột hoặc phím [E]/[Space]
            bool isTapped = false;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            {
                isTapped = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                isTapped = true;
            }
#if ENABLE_INPUT_SYSTEM
            else if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                isTapped = true;
            }
#endif
            try
            {
                if (!isTapped && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
                {
                    isTapped = true;
                }
            }
            catch {}

            if (isTapped && m_PlaneDetected && m_EncounterSpawner != null)
            {
                m_EncounterSpawner.PlaceCorruptedObject(m_PlacementPosition, m_PlacementRotation);
            }
        }

        private void AnimateHologramRings()
        {
            if (m_ReticleObject == null || !m_ReticleObject.activeSelf) return;

            // Xoay 2 vành tròn đồng tâm ngược chiều nhau
            if (m_OuterHoloRing != null)
            {
                m_OuterHoloRing.Rotate(Vector3.up * 45f * Time.deltaTime, Space.Self);
            }
            if (m_InnerHoloRing != null)
            {
                m_InnerHoloRing.Rotate(Vector3.up * -70f * Time.deltaTime, Space.Self);
            }
        }

        private void UpdateReticlePosition()
        {
            if (m_ARCamera == null) m_ARCamera = Camera.main;
            if (m_ARCamera == null) return;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

#if UNITY_XR_ARFOUNDATION
            if (m_RaycastManager != null && m_RaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;
                m_PlacementPosition = hitPose.position;
                m_PlacementRotation = hitPose.rotation;
                m_PlaneDetected = true;

                UpdateVisuals(true);
                return;
            }
#endif

            // Fallback cho chế độ Editor simulation hoặc bề mặt gạch sàn trường
            Ray ray = m_ARCamera.ScreenPointToRay(screenCenter);
            if (Physics.Raycast(ray, out RaycastHit hit, 25f))
            {
                m_PlacementPosition = hit.point;
                Vector3 cameraForward = m_ARCamera.transform.forward;
                cameraForward.y = 0f;
                m_PlacementRotation = cameraForward.sqrMagnitude > 0.001f 
                    ? Quaternion.LookRotation(cameraForward.normalized) 
                    : Quaternion.identity;
                m_PlaneDetected = true;

                UpdateVisuals(true);
                return;
            }

            // Điểm chiếu mặc định nếu chưa chạm mặt phẳng
            m_PlacementPosition = m_ARCamera.transform.position + m_ARCamera.transform.forward * 2.2f;
            m_PlacementPosition.y = 0f;
            m_PlacementRotation = Quaternion.identity;
            m_PlaneDetected = true;

            UpdateVisuals(true);
        }

        private void UpdateVisuals(bool detected)
        {
            if (m_ReticleObject != null)
            {
                if (!m_ReticleObject.activeSelf) m_ReticleObject.SetActive(true);

                // Di chuyển mượt mà tới điểm tiếp xúc mặt phẳng
                m_ReticleObject.transform.position = Vector3.Lerp(m_ReticleObject.transform.position, m_PlacementPosition, Time.deltaTime * 18f);
                m_ReticleObject.transform.rotation = Quaternion.Slerp(m_ReticleObject.transform.rotation, m_PlacementRotation, Time.deltaTime * 12f);
            }

            if (m_StatusInstructionText != null)
            {
                m_StatusInstructionText.text = detected 
                    ? "✨ <b>ĐÃ TÌM THẤY MẶT PHẲNG AR</b>\n<size=12><color=#00E5FF>[Chạm màn hình hoặc nhấn [E] để đặt Dị Tượng]</color></size>" 
                    : "📱 <b>HÃY QUÉT CAMERA VÀO SÀN NHÀ / MẶT ĐẤT</b>\n<size=12><color=#FDE047>[Đang tìm kiếm bề mặt AR...]</color></size>";
            }
        }
    }
}