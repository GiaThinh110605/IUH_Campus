using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace IUHCampus.Guardian.AR
{
    public class IUHARPlaneReticle : MonoBehaviour
    {
        [Header("Reticle Visual")]
        [SerializeField] private GameObject m_ReticleObject;
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

            // Tap to place
            bool isTapped = false;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                isTapped = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                isTapped = true;
            }

            if (isTapped && m_PlaneDetected && m_EncounterSpawner != null)
            {
                m_EncounterSpawner.PlaceCorruptedObject(m_PlacementPosition, m_PlacementRotation);
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

            // Fallback for Editor simulation or Physics surfaces
            Ray ray = m_ARCamera.ScreenPointToRay(screenCenter);
            if (Physics.Raycast(ray, out RaycastHit hit, 20f))
            {
                m_PlacementPosition = hit.point;
                Vector3 cameraForward = m_ARCamera.transform.forward;
                cameraForward.y = 0f;
                m_PlacementRotation = Quaternion.LookRotation(cameraForward.normalized);
                m_PlaneDetected = true;

                UpdateVisuals(true);
                return;
            }

            // Default projected position if no hit
            m_PlacementPosition = m_ARCamera.transform.position + m_ARCamera.transform.forward * 2.0f;
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
                m_ReticleObject.transform.position = m_PlacementPosition;
                m_ReticleObject.transform.rotation = m_PlacementRotation;
            }

            if (m_StatusInstructionText != null)
            {
                m_StatusInstructionText.text = detected 
                    ? "✨ <b>ĐÃ TÌM THẤY MẶT PHẲNG</b>\n[Chạm màn hình để đặt Balo Nhiễm Độc]" 
                    : "📱 <b>HÃY QUÉT CAMERA VÀO SÀN NHÀ / MẶT ĐẤT</b>";
            }
        }
    }
}

