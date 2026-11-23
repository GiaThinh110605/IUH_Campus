using System.Collections;
using IUHCampus.Core;
using UnityEngine;

namespace IUHCampus.Guardian.Ambient
{
    [DisallowMultipleComponent]
    public sealed class IUHAmbientStudent : MonoBehaviour
    {
        private const float PlayerStopDistance = 1.2f;
        private const float CameraPauseDistance = 45f;
        private const int MobilePopulationLimit = 4;
        private const float ArrivalDistance = 0.04f;
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int WalkState = Animator.StringToHash("Walk");

        [SerializeField] private Animator animator;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform player;
        [SerializeField] private Transform conversationPartner;
        [SerializeField] private Vector3[] walkRoute = new Vector3[0];
        [SerializeField] private Vector2 pauseRange = new Vector2(2f, 5f);
        [SerializeField, Range(1.2f, 1.5f)] private float walkSpeed = 1.35f;
        [SerializeField] private int seed = 1;
        [SerializeField, Min(0)] private int populationIndex;
        [SerializeField] private bool reducePopulationOnMobile = true;
        [SerializeField] private LayerMask groundLayers = ~0;
        [SerializeField, Min(0.1f)] private float groundProbeHeight = 2f;
        [SerializeField, Min(0.1f)] private float groundProbeDepth = 5f;
        [SerializeField, Min(1f)] private float turnSpeed = 100f;

        private System.Random random;
        private float idlePhase;
        private float pauseRemaining;
        private float nextConversationTurn;
        private int routeIndex;
        private int routeDirection = 1;
        private int animationState;
        private bool hasAnimationState;
        private bool walking;
        private Quaternion idleFacing;
        private Coroutine referenceRefresh;

        private bool HasRoute => walkRoute != null && walkRoute.Length >= 2;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            random = new System.Random(seed);
            idlePhase = (float)random.NextDouble();
            idleFacing = transform.rotation;
            nextConversationTurn = RandomRange(2f, 5f);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                var culling = animator.cullingMode;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                UpdateAnimation();
                if (animator.runtimeAnimatorController != null) animator.Update(0f);
                animator.cullingMode = culling;
            }
        }

        private void OnEnable()
        {
            if (reducePopulationOnMobile && Application.isMobilePlatform && populationIndex >= MobilePopulationLimit)
            {
                gameObject.SetActive(false);
                return;
            }
            referenceRefresh = StartCoroutine(RefreshReferences());
        }

        private void OnDisable()
        {
            if (referenceRefresh != null) StopCoroutine(referenceRefresh);
            referenceRefresh = null;
        }

        private IEnumerator RefreshReferences()
        {
            var interval = new WaitForSecondsRealtime(IUHPlayerLocator.RetryInterval);
            while (true)
            {
                if (player == null) player = IUHPlayerLocator.Player;
                if (viewCamera == null) viewCamera = Camera.main;
                yield return interval;
            }
        }

        private void Update()
        {
            bool distant = viewCamera != null &&
                (viewCamera.transform.position - transform.position).sqrMagnitude > CameraPauseDistance * CameraPauseDistance;
            if (animator != null && animator.enabled == distant)
            {
                animator.enabled = !distant;
                if (!distant) hasAnimationState = false;
            }
            if (distant) return;

            walking = false;
            bool blocked = player != null &&
                (player.position - transform.position).sqrMagnitude < PlayerStopDistance * PlayerStopDistance;
            if (!blocked)
            {
                if (HasRoute) UpdateRoute(Time.deltaTime);
                else UpdateConversation(Time.deltaTime);
            }
            UpdateAnimation();
        }

        private void UpdateRoute(float deltaTime)
        {
            if (pauseRemaining > 0f)
            {
                pauseRemaining -= deltaTime;
                return;
            }
            int lastIndex = Mathf.Min(walkRoute.Length, 4) - 1;
            routeIndex = Mathf.Clamp(routeIndex, 0, lastIndex);
            Vector3 target = walkRoute[routeIndex];
            Vector3 offset = target - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= ArrivalDistance * ArrivalDistance)
            {
                if (routeIndex == lastIndex) routeDirection = -1;
                else if (routeIndex == 0) routeDirection = 1;
                routeIndex += routeDirection;
                pauseRemaining = RandomRange(pauseRange.x, pauseRange.y);
                return;
            }
            walking = true;
            Vector3 position = transform.position + offset.normalized * Mathf.Min(offset.magnitude, walkSpeed * deltaTime);
            transform.position = GroundPosition(position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(offset), turnSpeed * deltaTime);
        }

        private Vector3 GroundPosition(Vector3 position)
        {
            if (Physics.Raycast(position + Vector3.up * groundProbeHeight, Vector3.down,
                out RaycastHit hit, groundProbeDepth, groundLayers, QueryTriggerInteraction.Ignore))
                position.y = hit.point.y;
            return position;
        }

        private void UpdateConversation(float deltaTime)
        {
            nextConversationTurn -= deltaTime;
            if (nextConversationTurn <= 0f)
            {
                nextConversationTurn = RandomRange(2f, 5f);
                if (conversationPartner != null)
                {
                    Vector3 direction = conversationPartner.position - transform.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0.001f)
                        idleFacing = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), RandomRange(5f, 15f));
                }
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, idleFacing, 12f * deltaTime);
        }

        private void UpdateAnimation()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            int state = walking ? WalkState : IdleState;
            if (hasAnimationState && animationState == state) return;
            if (!animator.HasState(0, state)) return;
            if (!hasAnimationState) animator.Play(state, 0, idlePhase);
            else animator.CrossFadeInFixedTime(state, 0.15f, 0);
            animationState = state;
            hasAnimationState = true;
        }

        private float RandomRange(float minimum, float maximum) =>
            minimum + (maximum - minimum) * (float)random.NextDouble();

        private void OnValidate()
        {
            walkSpeed = Mathf.Clamp(walkSpeed, 1.2f, 1.5f);
            pauseRange.x = Mathf.Max(0f, pauseRange.x);
            pauseRange.y = Mathf.Max(pauseRange.x, pauseRange.y);
        }
    }
}
