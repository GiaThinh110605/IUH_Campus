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
        private bool hasJump, hasFreeFall, initialized;
        private float lastOffset;
        private readonly RaycastHit[] hits = new RaycastHit[16];
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int FreeFall = Animator.StringToHash("FreeFall");

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
            foreach (var parameter in animator.parameters)
            {
                if (parameter.nameHash == Jump) hasJump = true;
                if (parameter.nameHash == FreeFall) hasFreeFall = true;
            }
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
            for (int i = 0; i < visualBranches.Length; i++) baseY[i] = visualBranches[i].localPosition.y;
        }

        public void AlignNow()
        {
            if (!initialized) return;
            RestoreBranches();
            if (controller != null && (!controller.Grounded || (hasJump && animator.GetBool(Jump)) || (hasFreeFall && animator.GetBool(FreeFall))))
            {
                ApplyOffset(lastOffset);
                return;
            }
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
            lastOffset = 0f;
        }
    }
}
