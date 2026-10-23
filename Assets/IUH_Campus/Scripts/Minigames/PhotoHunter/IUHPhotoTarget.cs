using System;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Minigames.PhotoHunter
{
    /// <summary>
    /// Represents a photographic landmark objective in the campus.
    /// Evaluates realistic geometric criteria: distance, view frustum, viewport framing,
    /// architectural landmark coverage, and physical occlusion (e.g. Building layer walls).
    /// </summary>
    [SelectionBase]
    public class IUHPhotoTarget : MonoBehaviour
    {
        [Header("Target Identification")]
        [Tooltip("Stable unique ID, never change once deployed (used for save files & quests).")]
        [SerializeField] private string m_TargetId = "photo_target_01";
        [SerializeField] private string m_DisplayName = "Địa Điểm Khuôn Viên";
        [TextArea(2, 4)]
        [SerializeField] private string m_Description = "Mô tả góc chụp và đặc điểm nhận dạng công trình.";

        [Header("Structure & Bounds References")]
        [Tooltip("Direct reference to the root transform of the physical building/monument in the scene.")]
        [SerializeField] private Transform m_StructureRoot;
        [Tooltip("Optional direct reference to main structure collider.")]
        [SerializeField] private Collider m_StructureCollider;
        [Tooltip("Center of interest for aiming. If null, this transform is used.")]
        [SerializeField] private Transform m_TargetCenter;
        [SerializeField] private Vector3 m_CenterOffset = Vector3.zero;

        [Header("Architectural Key Feature Points (Coverage Check)")]
        [Tooltip("Local offsets representing distinct landmark features (e.g., left wing, right wing, top arch) to ensure framing captures the structure.")]
        [SerializeField] private List<Vector3> m_KeyFeatureOffsets = new List<Vector3>();

        [Header("Validation Geometry Constraints")]
        [Tooltip("Minimum shooting distance (meters) to avoid being unrealistically close to walls.")]
        [SerializeField] private float m_MinDistance = 5.0f;
        [Tooltip("Maximum shooting distance (meters) to ensure the landmark is clearly captured.")]
        [SerializeField] private float m_MaxDistance = 45.0f;
        [Tooltip("Maximum allowed angle (degrees) between camera forward axis and target direction.")]
        [SerializeField] private float m_MaxAngleDegrees = 50.0f;
        [Tooltip("Safe margin inside viewport [0..1]. E.g. 0.08 means point must fall within [0.08 .. 0.92].")]
        [Range(0.04f, 0.30f)]
        [SerializeField] private float m_ViewportMargin = 0.08f;
        [Tooltip("Minimum ratio of key landmark points that must be visible in the viewport.")]
        [Range(0.2f, 1.0f)]
        [SerializeField] private float m_RequiredFeatureVisibilityRatio = 0.5f;

        [Header("Fast Travel & Rewards")]
        [Tooltip("Optimal vantage point for the player when teleporting to scout this location.")]
        [SerializeField] private Vector3 m_ScoutVantagePoint = Vector3.zero;
        [Tooltip("Initial direction player faces at vantage point.")]
        [SerializeField] private Vector3 m_ScoutLookDirection = Vector3.forward;
        [SerializeField] private int m_RewardScore = 100;

        // Runtime State
        private bool m_IsCompleted = false;

        public string TargetId => m_TargetId;
        public string DisplayName => m_DisplayName;
        public string Description => m_Description;
        public float MinDistance => m_MinDistance;
        public float MaxDistance => m_MaxDistance;
        public Vector3 ScoutVantagePoint => m_ScoutVantagePoint;
        public Vector3 ScoutLookDirection => m_ScoutLookDirection;
        public int RewardScore => m_RewardScore;
        public bool IsCompleted => m_IsCompleted;

        public void SetCompleted(bool completed)
        {
            m_IsCompleted = completed;
        }

        public void Configure(
            string id,
            string displayName,
            string desc,
            Vector3 vantagePoint,
            Vector3 lookDir,
            float minDist = 5f,
            float maxDist = 45f,
            Transform structureRoot = null,
            Collider structureCol = null)
        {
            m_TargetId = id;
            m_DisplayName = displayName;
            m_Description = desc;
            m_ScoutVantagePoint = vantagePoint;
            m_ScoutLookDirection = lookDir;
            m_MinDistance = minDist;
            m_MaxDistance = maxDist;
            if (structureRoot != null) m_StructureRoot = structureRoot;
            if (structureCol != null) m_StructureCollider = structureCol;
        }

        public void Configure(
            string id,
            string displayName,
            string desc,
            Vector3 vantagePoint,
            float minDist = 5f,
            float maxDist = 45f)
        {
            Configure(id, displayName, desc, vantagePoint, Vector3.forward, minDist, maxDist, null, null);
        }

        public void SetStructureReferences(Transform structureRoot, Collider structureCollider)
        {
            m_StructureRoot = structureRoot;
            m_StructureCollider = structureCollider;
        }

        public Transform StructureRoot => m_StructureRoot;
        public Collider StructureCollider => m_StructureCollider;
        public float RequiredFeatureVisibilityRatio
        {
            get => m_RequiredFeatureVisibilityRatio;
            set => m_RequiredFeatureVisibilityRatio = value;
        }

        public void SetKeyFeatureOffsets(List<Vector3> offsets)
        {
            m_KeyFeatureOffsets = offsets != null ? new List<Vector3>(offsets) : new List<Vector3>();
        }

        /// <summary>
        /// Computes world space target center.
        /// </summary>
        public Vector3 GetWorldCenter()
        {
            if (m_TargetCenter != null)
                return m_TargetCenter.position + m_CenterOffset;
            return transform.TransformPoint(m_CenterOffset);
        }

        /// <summary>
        /// Gets all world space key feature points including center.
        /// </summary>
        public List<Vector3> GetWorldFeaturePoints()
        {
            var list = new List<Vector3>();
            list.Add(GetWorldCenter());

            if (m_KeyFeatureOffsets != null)
            {
                foreach (var offset in m_KeyFeatureOffsets)
                {
                    list.Add(transform.TransformPoint(offset));
                }
            }

            return list;
        }

        /// <summary>
        /// Calculates the angle between the camera's forward axis and the direction to this target.
        /// Used to detect which target the player is aiming at.
        /// </summary>
        public float GetFacingAngle(Camera camera)
        {
            if (camera == null) return 180f;
            Vector3 camPos = camera.transform.position;
            Vector3 dirToTarget = (GetWorldCenter() - camPos).normalized;
            return Vector3.Angle(camera.transform.forward, dirToTarget);
        }

        /// <summary>
        /// Validates whether the active game camera captures this landmark legitimately.
        /// Performs strictly physical and geometric checks without artificial confidence mockups.
        /// </summary>
        /// <param name="shootingCamera">The actual camera used to render the game world.</param>
        /// <param name="playerTransform">Player transform to ignore player's own colliders during raycast.</param>
        /// <param name="obstacleMask">LayerMask for occluders (Building, Environment, Default, etc.).</param>
        /// <param name="failureReason">Vietnamese descriptive explanation if validation fails.</param>
        /// <returns>True if the photo qualifies as a valid capture.</returns>
        public bool ValidatePhoto(Camera shootingCamera, Transform playerTransform, LayerMask obstacleMask, out string failureReason)
        {
            if (m_IsCompleted)
            {
                failureReason = $"Địa điểm \"{m_DisplayName}\" đã được chụp hoàn thành trước đó!";
                return false;
            }

            if (shootingCamera == null)
            {
                failureReason = "Không tìm thấy camera ngắm chụp!";
                return false;
            }

            Vector3 camPos = shootingCamera.transform.position;
            Vector3 camForward = shootingCamera.transform.forward;
            Vector3 targetCenter = GetWorldCenter();

            // 1. Distance Range Check
            float distance = Vector3.Distance(camPos, targetCenter);
            if (distance < m_MinDistance)
            {
                failureReason = $"Quá gần mục tiêu ({distance:F1}m < {m_MinDistance:F0}m)! Hãy lùi lại để lấy toàn cảnh \"{m_DisplayName}\".";
                return false;
            }

            if (distance > m_MaxDistance)
            {
                failureReason = $"Khoảng cách quá xa ({distance:F1}m > {m_MaxDistance:F0}m)! Hãy tiến lại gần \"{m_DisplayName}\" hơn.";
                return false;
            }

            // 2. Camera Facing Angle Check (Target in front of camera)
            Vector3 dirToTarget = (targetCenter - camPos).normalized;
            float angle = Vector3.Angle(camForward, dirToTarget);
            if (angle > m_MaxAngleDegrees)
            {
                failureReason = $"Mục tiêu \"{m_DisplayName}\" chưa ở phía trước ống kính máy ảnh (Lệch {angle:F0}°)!";
                return false;
            }

            // 3. Viewport Boundary & Framing Coverage Check
            Vector3 vpCenter = shootingCamera.WorldToViewportPoint(targetCenter);
            if (vpCenter.z <= 0f)
            {
                failureReason = $"Mục tiêu \"{m_DisplayName}\" đang nằm phía sau máy ảnh!";
                return false;
            }

            float minV = m_ViewportMargin;
            float maxV = 1.0f - m_ViewportMargin;
            if (vpCenter.x < minV || vpCenter.x > maxV || vpCenter.y < minV || vpCenter.y > maxV)
            {
                failureReason = $"Mục tiêu \"{m_DisplayName}\" chưa nằm trọn vẹn trong khung ngắm!";
                return false;
            }

            // Check multi-point landmark coverage (to ensure building is substantially in frame)
            var featurePoints = GetWorldFeaturePoints();
            int visibleFeatureCount = 0;
            for (int i = 0; i < featurePoints.Count; i++)
            {
                Vector3 vp = shootingCamera.WorldToViewportPoint(featurePoints[i]);
                if (vp.z > 0f && vp.x >= 0.02f && vp.x <= 0.98f && vp.y >= 0.02f && vp.y <= 0.98f)
                {
                    visibleFeatureCount++;
                }
            }

            float coverageRatio = (float)visibleFeatureCount / featurePoints.Count;
            if (coverageRatio < m_RequiredFeatureVisibilityRatio)
            {
                failureReason = $"Góc máy chỉ thấy một phần nhỏ của \"{m_DisplayName}\"! Hãy lùi lại hoặc căn giữa công trình.";
                return false;
            }

            // 4. Physical Occlusion Check (Raycast through Building, Environment, etc.)
            // Determines if foreign solid obstacles (such as walls of other buildings) block the view.
            int playerLayer = LayerMask.NameToLayer("Player");
            int uiLayer = LayerMask.NameToLayer("UI");
            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            int triggerLayer = LayerMask.NameToLayer("Trigger");

            bool hasClearLineOfSight = false;
            string occluderName = "";

            // Check center and key features for clear lines of sight
            for (int pIdx = 0; pIdx < featurePoints.Count; pIdx++)
            {
                Vector3 testPoint = featurePoints[pIdx];
                Vector3 rayDir = (testPoint - camPos);
                float rayDist = rayDir.magnitude;
                if (rayDist < 0.1f) continue;
                rayDir /= rayDist;

                RaycastHit[] hits = Physics.RaycastAll(camPos, rayDir, rayDist, obstacleMask, QueryTriggerInteraction.Ignore);
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                bool pointOccluded = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    var hitCol = hits[i].collider;
                    if (hitCol == null) continue;

                    // Skip triggers
                    if (hitCol.isTrigger) continue;

                    // Skip player character
                    int colLayer = hitCol.gameObject.layer;
                    if (colLayer == playerLayer || colLayer == uiLayer || colLayer == ignoreRaycast || colLayer == triggerLayer)
                        continue;
                    if (playerTransform != null && (hitCol.transform.root == playerTransform.root || hitCol.CompareTag("Player")))
                        continue;

                    // If ray reached this target's own physical structure or children, line of sight reached!
                    if (IsPartOfTarget(hitCol))
                    {
                        break;
                    }

                    // Solid obstacle in front of the target
                    if (hits[i].distance < rayDist - 1.2f)
                    {
                        pointOccluded = true;
                        occluderName = hitCol.gameObject.name;
                        break;
                    }
                }

                if (!pointOccluded)
                {
                    hasClearLineOfSight = true;
                    break;
                }
            }

            if (!hasClearLineOfSight)
            {
                string blockerDetail = string.IsNullOrEmpty(occluderName) ? "công trình/tường chắn" : occluderName;
                failureReason = $"Mục tiêu \"{m_DisplayName}\" bị che khuất bởi {blockerDetail}!";
                return false;
            }

            // All physical, geometric and coverage criteria satisfied!
            failureReason = string.Empty;
            return true;
        }

        /// <summary>
        /// Explicitly verifies whether the hit collider belongs to this landmark structure without string guessing.
        /// </summary>
        public bool IsPartOfTarget(Collider hitCollider)
        {
            if (hitCollider == null) return false;

            // Direct match
            if (hitCollider == m_StructureCollider) return true;
            if (hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform)) return true;

            // Physical structure root reference
            if (m_StructureRoot != null && (hitCollider.transform == m_StructureRoot || hitCollider.transform.IsChildOf(m_StructureRoot)))
                return true;

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = GetWorldCenter();
            Gizmos.color = m_IsCompleted ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(center, 1.5f);

            // Draw minimum and maximum range rings
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawWireSphere(center, m_MinDistance);
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(center, m_MaxDistance);

            // Draw feature points
            if (m_KeyFeatureOffsets != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var offset in m_KeyFeatureOffsets)
                {
                    Gizmos.DrawSphere(transform.TransformPoint(offset), 0.4f);
                }
            }

            // Draw vantage scout position
            if (m_ScoutVantagePoint != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(m_ScoutVantagePoint, 0.5f);
                Gizmos.DrawLine(m_ScoutVantagePoint, center);
            }
        }
    }
}
