using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

namespace IUHCampus.Guardian.Visual
{
    [DefaultExecutionOrder(20000)]
    [DisallowMultipleComponent]
    public sealed class IUHVisualFeetGrounder : MonoBehaviour
    {
        private const int MaxSamples = 128;
        private const float SoleBand = .035f;
        private const float ProbeHeight = 3f;
        private const float MaximumVisibleGroundSeparation = .4f;
        private const float MaximumCalibrationDrift = .15f;
        private const float OffsetSpeed = 1f;
        private const float AirborneRiseSpeed = .15f;
        private const float JumpHoldSeconds = .3f;
        private const float SupportProbeLift = .2f;
        private const float SupportProbeDistance = .85f;
        private const float MinimumSupportNormalY = .65f;
        private const float MaximumVisibleGroundUndershoot = .03f;
        private const float PositionEpsilon = .0001f;
        [SerializeField] private IUHVisualGroundSurface surface;
        [SerializeField, Min(0f)] private float maximumOffset = 2f;
        [SerializeField] private float soleClearance = .002f;
        private Transform[] visualBranches;
        private float[] baseY;
        private Transform[] bones;
        private Matrix4x4[] bindPoses, matrices;
        private int[] usedBones;
        private Vector3[] vertices;
        private BoneWeight[] weights;
        private int[] samples;
        private Animator animator;
        private ThirdPersonController controller;
        private bool initialized;
        private float lastOffset;
        private bool calibrated, playerOffsetApplied;
        private float calibrationOffset, calibratedSoleLocalY, visualGroundGap;
        private float previousRootY, jumpUntil;
        private float[] appliedLocalY;
        private StarterAssetsInputs inputs;
        private CharacterController capsule;
        private readonly RaycastHit[] hits = new RaycastHit[16];

        public void Configure(IUHVisualGroundSurface groundSurface)
        {
            surface = groundSurface;
            if (!initialized) Initialize();
        }

        private void Awake() => Initialize();
        // Restore before animation and UniVRM's 11000 LateUpdate copies world poses.
        private void Update() { if (initialized) RestoreBranches(); }
        private void LateUpdate() => AlignNow();

        private void Initialize()
        {
            if (initialized) return;
            animator = GetComponent<Animator>();
            controller = GetComponent<ThirdPersonController>();
            inputs = GetComponent<StarterAssetsInputs>();
            capsule = GetComponent<CharacterController>();
            if (animator == null || !animator.isHuman) return;
            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            SkinnedMeshRenderer body = null;
            foreach (var renderer in renderers)
                if (renderer.name == "Body") { body = renderer; break; }
            if (body == null || body.sharedMesh == null || !body.sharedMesh.isReadable) return;
            Mesh mesh = body.sharedMesh;
            vertices = mesh.vertices;
            weights = mesh.boneWeights;
            bindPoses = mesh.bindposes;
            bones = body.bones;
            matrices = new Matrix4x4[bones.Length];
            var footBones = new HashSet<int>();
            CacheFoot(footBones, HumanBodyBones.LeftFoot);
            CacheFoot(footBones, HumanBodyBones.RightFoot);
            CacheFoot(footBones, HumanBodyBones.LeftToes);
            CacheFoot(footBones, HumanBodyBones.RightToes);
            samples = SelectSamples(footBones);
            CacheUsedBones();
            CacheBranches(renderers, footBones);
            initialized = samples.Length > 0 && visualBranches.Length > 0;
        }

        private void CacheFoot(HashSet<int> indices, HumanBodyBones humanBone)
        {
            Transform foot = animator.GetBoneTransform(humanBone);
            if (foot == null) return;
            for (int i = 0; i < bones.Length; i++)
                if (bones[i] == foot) indices.Add(i);
        }

        private int[] SelectSamples(HashSet<int> feet)
        {
            var candidates = new List<int>();
            float minimum = float.PositiveInfinity;
            for (int i = 0; i < vertices.Length && i < weights.Length; i++)
            {
                BoneWeight w = weights[i];
                float influence = (feet.Contains(w.boneIndex0) ? w.weight0 : 0f) + (feet.Contains(w.boneIndex1) ? w.weight1 : 0f)
                    + (feet.Contains(w.boneIndex2) ? w.weight2 : 0f) + (feet.Contains(w.boneIndex3) ? w.weight3 : 0f);
                if (influence < .25f) continue;
                candidates.Add(i);
                minimum = Mathf.Min(minimum, vertices[i].y);
            }
            candidates.RemoveAll(i => vertices[i].y > minimum + SoleBand);
            if (candidates.Count <= MaxSamples) return candidates.ToArray();
            var result = new int[MaxSamples];
            var distances = new float[candidates.Count];
            for (int i = 0; i < distances.Length; i++) distances[i] = float.PositiveInfinity;
            int next = 0;
            for (int count = 0; count < MaxSamples; count++)
            {
                result[count] = candidates[next];
                Vector3 chosen = vertices[candidates[next]];
                float farthest = -1f;
                for (int j = 0; j < candidates.Count; j++)
                {
                    distances[j] = Mathf.Min(distances[j], (vertices[candidates[j]] - chosen).sqrMagnitude);
                    if (distances[j] > farthest) { farthest = distances[j]; next = j; }
                }
            }
            return result;
        }

        private void CacheUsedBones()
        {
            var used = new HashSet<int>();
            foreach (int index in samples)
            {
                BoneWeight w = weights[index];
                if (w.weight0 > 0f) used.Add(w.boneIndex0);
                if (w.weight1 > 0f) used.Add(w.boneIndex1);
                if (w.weight2 > 0f) used.Add(w.boneIndex2);
                if (w.weight3 > 0f) used.Add(w.boneIndex3);
            }
            usedBones = new int[used.Count];
            used.CopyTo(usedBones);
        }

        private void CacheBranches(SkinnedMeshRenderer[] renderers, HashSet<int> feet)
        {
            var branches = new List<Transform>();
            foreach (Transform child in transform)
            {
                bool visual = false;
                foreach (var renderer in renderers)
                    if (renderer.transform == child || renderer.transform.IsChildOf(child)) visual = true;
                foreach (int foot in feet)
                    if (bones[foot] == child || bones[foot].IsChildOf(child)) visual = true;
                if (visual) branches.Add(child);
            }
            visualBranches = branches.ToArray();
            baseY = new float[visualBranches.Length];
            appliedLocalY = new float[visualBranches.Length];
            for (int i = 0; i < visualBranches.Length; i++) baseY[i] = visualBranches[i].localPosition.y;
        }

        public void AlignNow()
        {
            if (!initialized) return;
            if (controller != null) { AlignPlayer(); return; }
            RestoreBranches();
            foreach (int index in usedBones) matrices[index] = bones[index].localToWorldMatrix * bindPoses[index];
            Vector3 contact = new Vector3(0f, float.PositiveInfinity, 0f);
            foreach (int index in samples)
            {
                BoneWeight w = weights[index];
                Vector3 v = vertices[index];
                Vector3 point = matrices[w.boneIndex0].MultiplyPoint3x4(v) * w.weight0
                    + matrices[w.boneIndex1].MultiplyPoint3x4(v) * w.weight1
                    + matrices[w.boneIndex2].MultiplyPoint3x4(v) * w.weight2
                    + matrices[w.boneIndex3].MultiplyPoint3x4(v) * w.weight3;
                if (point.y < contact.y) contact = point;
            }
            Vector3 probe = new Vector3(contact.x, transform.position.y + ProbeHeight, contact.z);
            if (TryGround(probe, out float ground))
                lastOffset = Mathf.Clamp(ground + soleClearance - contact.y, -maximumOffset, maximumOffset);
            ApplyOffset(lastOffset);
        }

        private void AlignPlayer()
        {
            RemovePlayerOffset();
            float rootY = transform.position.y;
            float riseSpeed = (rootY - previousRootY) / Mathf.Max(Time.deltaTime, PositionEpsilon);
            previousRootY = rootY;
            if ((inputs != null && inputs.jump) || (calibrated && riseSpeed > AirborneRiseSpeed))
                jumpUntil = Time.time + JumpHoldSeconds;
            bool supported = TryPlayerSupport(out float physicalGround);
            bool airborne = !supported || Time.time < jumpUntil;
            if (!calibrated && !airborne) CalibratePlayer(rootY, physicalGround);
            if (calibrated && !airborne)
            {
                float visible = SelectSupportedVisibleGround(physicalGround, visualGroundGap);
                float target = Mathf.Clamp(visible + soleClearance - rootY - calibratedSoleLocalY,
                    calibrationOffset - MaximumCalibrationDrift, calibrationOffset + MaximumCalibrationDrift);
                lastOffset = Mathf.MoveTowards(lastOffset, target, OffsetSpeed * Time.deltaTime);
            }
            ApplyOffset(lastOffset);
            for (int i = 0; i < visualBranches.Length; i++) appliedLocalY[i] = visualBranches[i].localPosition.y;
            playerOffsetApplied = true;
        }

        private void CalibratePlayer(float rootY, float physicalGround)
        {
            float sole = FindSoleHeight();
            float visible = SelectSupportedVisibleGround(physicalGround, 0f);
            calibratedSoleLocalY = sole - rootY;
            visualGroundGap = visible - physicalGround;
            calibrationOffset = Mathf.Clamp(visible + soleClearance - sole, -maximumOffset, maximumOffset);
            lastOffset = calibrationOffset;
            calibrated = true;
        }

        private float SelectSupportedVisibleGround(float physicalGround, float fallbackGap)
        {
            Vector3 probe = new Vector3(transform.position.x,
                physicalGround + MaximumVisibleGroundSeparation, transform.position.z);
            if (surface != null && surface.TryGetHeight(probe, out float candidate)
                && candidate >= physicalGround - MaximumVisibleGroundUndershoot
                && candidate <= physicalGround + MaximumVisibleGroundSeparation) return candidate;
            return physicalGround + fallbackGap;
        }

        private float FindSoleHeight()
        {
            foreach (int index in usedBones) matrices[index] = bones[index].localToWorldMatrix * bindPoses[index];
            float sole = float.PositiveInfinity;
            foreach (int index in samples)
            {
                BoneWeight w = weights[index];
                Vector3 v = vertices[index];
                Vector3 point = matrices[w.boneIndex0].MultiplyPoint3x4(v) * w.weight0
                    + matrices[w.boneIndex1].MultiplyPoint3x4(v) * w.weight1
                    + matrices[w.boneIndex2].MultiplyPoint3x4(v) * w.weight2
                    + matrices[w.boneIndex3].MultiplyPoint3x4(v) * w.weight3;
                sole = Mathf.Min(sole, point.y);
            }
            return sole;
        }
        private bool TryPlayerSupport(out float height)
        {
            height = float.NegativeInfinity;
            float footY = capsule != null ? transform.TransformPoint(capsule.center).y - capsule.height * Mathf.Abs(transform.lossyScale.y) * .5f : transform.position.y;
            Vector3 probe = new Vector3(transform.position.x, footY + SupportProbeLift, transform.position.z);
            int count = Physics.RaycastNonAlloc(probe, Vector3.down, hits, SupportProbeDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.transform == transform || hit.transform.IsChildOf(transform) || hit.normal.y < MinimumSupportNormalY) continue;
                height = Mathf.Max(height, hit.point.y);
            }
            return !float.IsNegativeInfinity(height);
        }

        private void RemovePlayerOffset()
        {
            if (!playerOffsetApplied) return;
            for (int i = 0; i < visualBranches.Length; i++)
            {
                // UniVRM may already have replaced actual hips from its normalized control rig.
                if (Mathf.Abs(visualBranches[i].localPosition.y - appliedLocalY[i]) < PositionEpsilon)
                    visualBranches[i].position -= Vector3.up * lastOffset;
            }
            playerOffsetApplied = false;
        }

        private bool TryGround(Vector3 probe, out float height)
        {
            if (surface != null) return surface.TryGetHeight(probe, out height);
            height = float.NegativeInfinity;
            int count = Physics.RaycastNonAlloc(probe, Vector3.down, hits, ProbeHeight * 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].transform == transform || hits[i].transform.IsChildOf(transform)) continue;
                height = Mathf.Max(height, hits[i].point.y);
            }
            return !float.IsNegativeInfinity(height);
        }

        private void RestoreBranches()
        {
            if (controller != null) { RemovePlayerOffset(); return; }
            for (int i = 0; i < visualBranches.Length; i++)
            {
                Vector3 position = visualBranches[i].localPosition;
                position.y = baseY[i];
                visualBranches[i].localPosition = position;
            }
        }

        private void ApplyOffset(float offset)
        {
            foreach (Transform branch in visualBranches) branch.position += Vector3.up * offset;
        }

        private void OnDisable()
        {
            if (initialized) RestoreBranches();
            if (controller == null) lastOffset = 0f;
        }
    }
}






