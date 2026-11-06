using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IUHCampus.Guardian.Match3
{
    /// <summary>
    /// Moves the fight into the dedicated IUH_CombatArena scene: loads it on top of the campus,
    /// spawns look-alike copies of the student and the monster on the arena platform, films them
    /// with the arena camera and fades in. The real player and monster never move, so campus
    /// props can't get in the way. <see cref="Exit"/> puts everything back.
    /// </summary>
    public class Match3BattleStage : MonoBehaviour
    {
        [Header("Arena Scene")]
        [SerializeField] private string m_ArenaSceneName = "IUH_CombatArena";
        [SerializeField] private Image m_FadeOverlay;
        [SerializeField] private float m_FadeDuration = 0.35f;

        [Header("Framing")]
        [SerializeField] private float m_CameraFov = 40f;
        [SerializeField] private float m_CameraElevation = 1.0f;
        [Tooltip("Horizontal screen position (0-1) of the student. The monster mirrors it on the right.")]
        [SerializeField, Range(0.05f, 0.4f)] private float m_FighterScreenX = 0.17f;
        [Tooltip("Screen height (0-1 from the bottom) where the fighters' feet sit.")]
        [SerializeField, Range(0f, 0.5f)] private float m_GroundScreenY = 0.1f;
        [Tooltip("Share of the screen height the taller fighter should fill.")]
        [SerializeField, Range(0.2f, 0.8f)] private float m_FighterScreenHeight = 0.6f;
        [Tooltip("0 = fighters face each other in profile, 1 = they face the camera.")]
        [SerializeField, Range(0f, 1f)] private float m_FaceCamera = 0.35f;

        [Header("Music")]
        [SerializeField] private AudioClip m_CombatMusic;
        [SerializeField, Range(0f, 1f)] private float m_MusicVolume = 0.55f;
        [SerializeField] private float m_MusicFadeIn = 0.8f;

        [Header("Hit Feedback")]
        [SerializeField] private float m_LungeDistance = 0.9f;
        [SerializeField] private float m_LungeDuration = 0.28f;
        [SerializeField] private float m_ShakeAmount = 0.12f;
        [SerializeField] private float m_ShakeDuration = 0.25f;

        private static readonly HashSet<Type> KeepTypes = new HashSet<Type>
        {
            typeof(Transform), typeof(RectTransform), typeof(MeshFilter), typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer), typeof(Animator), typeof(LODGroup), typeof(Light),
            typeof(ParticleSystem), typeof(ParticleSystemRenderer)
        };

        private Match3ArenaAnchors m_Anchors;
        private Camera m_ArenaCamera;
        private Camera m_MainCamera;
        private bool m_MainCameraWasEnabled;
        private CinemachineBrain m_Brain;
        private bool m_BrainWasEnabled;
        private bool m_FogWasOn;
        private GameObject m_PlayerOverhead;
        private bool m_PlayerOverheadWasActive;

        private AudioSource m_MusicSource;
        private readonly List<AudioSource> m_PausedLoops = new List<AudioSource>();

        private GameObject m_PlayerProxy;
        private GameObject m_MonsterProxy;
        private Vector3 m_PlayerHome;
        private Vector3 m_MonsterHome;

        public bool IsActive { get; private set; }
        public GameObject FadeOverlayObject => m_FadeOverlay != null ? m_FadeOverlay.gameObject : null;

        /// <summary>
        /// Fade to black, load the arena if needed, place both fighters, fade back in.
        /// <paramref name="onReady"/> runs while the screen is still black, so the HUD can appear with the arena.
        /// </summary>
        public IEnumerator Enter(Transform player, Transform monster, Action onReady = null)
        {
            if (monster == null) { onReady?.Invoke(); yield break; }

            bool firstEntry = !IsActive;
            if (firstEntry)
            {
                yield return Fade(0f, 1f);
                yield return LoadArena();
                if (m_Anchors == null)
                {
                    Debug.LogWarning($"[IUH Guardian] Arena scene '{m_ArenaSceneName}' is missing or has no Match3ArenaAnchors. Run IUH Campus > Guardian > Build Match-3 Combat Arena.");
                    onReady?.Invoke();
                    yield return Fade(1f, 0f);
                    yield break;
                }
                TakeCamera();
                HidePlayerOverhead();
                StartMusic();
                IsActive = true;
            }

            DestroyProxies();
            m_PlayerProxy = player != null ? CreateProxy(player.gameObject, "Arena_Student") : null;
            m_MonsterProxy = CreateProxy(monster.gameObject, "Arena_Monster");
            PlaceFighters();
            onReady?.Invoke();

            if (firstEntry) yield return Fade(1f, 0f);
        }

        public void Exit()
        {
            StopAllCoroutines();
            SetFade(0f);
            if (!IsActive) return;
            IsActive = false;

            DestroyProxies();
            ReleaseCamera();
            StopMusic();

            if (m_PlayerOverhead != null) m_PlayerOverhead.SetActive(m_PlayerOverheadWasActive);
            m_PlayerOverhead = null;

            Scene arena = SceneManager.GetSceneByName(m_ArenaSceneName);
            if (arena.isLoaded) SceneManager.UnloadSceneAsync(arena);
            m_Anchors = null;
        }

        public void PlayPlayerAttack() => Lunge(m_PlayerProxy, m_PlayerHome, m_MonsterHome, m_LungeDistance);
        public void PlayMonsterAttack(bool charged) => Lunge(m_MonsterProxy, m_MonsterHome, m_PlayerHome, m_LungeDistance * (charged ? 1.6f : 1f));
        public void PlayPlayerHit() => Shake(m_PlayerProxy, m_PlayerHome);
        public void PlayMonsterHit() => Shake(m_MonsterProxy, m_MonsterHome);

        public IEnumerator PlayMonsterDefeat(float duration = 0.6f)
        {
            if (m_MonsterProxy == null) yield break;
            Transform t = m_MonsterProxy.transform;
            Vector3 from = t.localScale;
            for (float time = 0f; time < duration && t != null; time += Time.deltaTime)
            {
                float k = time / duration;
                t.localScale = Vector3.Lerp(from, from * 0.05f, k * k);
                t.Rotate(Vector3.up, 720f * Time.deltaTime, Space.World);
                yield return null;
            }
        }

        // ── Arena scene ──

        private IEnumerator LoadArena()
        {
            Scene arena = SceneManager.GetSceneByName(m_ArenaSceneName);
            if (!arena.isLoaded)
            {
                if (!Application.CanStreamedLevelBeLoaded(m_ArenaSceneName)) yield break;
                AsyncOperation op = SceneManager.LoadSceneAsync(m_ArenaSceneName, LoadSceneMode.Additive);
                while (op != null && !op.isDone) yield return null;
                arena = SceneManager.GetSceneByName(m_ArenaSceneName);
            }

            m_Anchors = null;
            if (!arena.isLoaded) yield break;
            foreach (GameObject root in arena.GetRootGameObjects())
            {
                m_Anchors = root.GetComponentInChildren<Match3ArenaAnchors>(true);
                if (m_Anchors != null) break;
            }
        }

        private void TakeCamera()
        {
            m_ArenaCamera = m_Anchors.ArenaCamera;

            m_MainCamera = Camera.main;
            if (m_MainCamera != null && m_MainCamera != m_ArenaCamera)
            {
                m_Brain = m_MainCamera.GetComponent<CinemachineBrain>();
                m_BrainWasEnabled = m_Brain != null && m_Brain.enabled;
                if (m_Brain != null) m_Brain.enabled = false;
                m_MainCameraWasEnabled = m_MainCamera.enabled;
                m_MainCamera.enabled = false;
            }

            // Campus fog would wash out the dark arena.
            m_FogWasOn = RenderSettings.fog;
            RenderSettings.fog = false;

            if (m_ArenaCamera != null)
            {
                m_ArenaCamera.fieldOfView = m_CameraFov;
                m_ArenaCamera.enabled = true;
            }
        }

        private void ReleaseCamera()
        {
            if (m_ArenaCamera != null) m_ArenaCamera.enabled = false;
            m_ArenaCamera = null;
            RenderSettings.fog = m_FogWasOn;

            if (m_MainCamera != null)
            {
                m_MainCamera.enabled = m_MainCameraWasEnabled;
                if (m_Brain != null && m_BrainWasEnabled)
                {
                    m_Brain.enabled = true;
                    if (m_Brain.ActiveVirtualCamera is CinemachineVirtualCameraBase vcam) vcam.PreviousStateIsValid = false;
                }
            }
            m_MainCamera = null;
            m_Brain = null;
        }

        // ── Music ──

        private void StartMusic()
        {
            // Pause campus loops (background music, engine hum) so the battle theme plays alone.
            m_PausedLoops.Clear();
            foreach (var src in FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude))
            {
                if (src != null && src != m_MusicSource && src.loop && src.isPlaying)
                {
                    src.Pause();
                    m_PausedLoops.Add(src);
                }
            }

            if (m_CombatMusic == null) return;
            if (m_MusicSource == null)
            {
                m_MusicSource = gameObject.AddComponent<AudioSource>();
                m_MusicSource.playOnAwake = false;
                m_MusicSource.loop = true;
                m_MusicSource.spatialBlend = 0f;
            }
            m_MusicSource.clip = m_CombatMusic;
            m_MusicSource.volume = 0f;
            m_MusicSource.Play();
            StartCoroutine(FadeMusicIn());
        }

        private IEnumerator FadeMusicIn()
        {
            for (float t = 0f; t < m_MusicFadeIn && m_MusicSource != null; t += Time.unscaledDeltaTime)
            {
                m_MusicSource.volume = Mathf.Lerp(0f, m_MusicVolume, t / m_MusicFadeIn);
                yield return null;
            }
            if (m_MusicSource != null) m_MusicSource.volume = m_MusicVolume;
        }

        private void StopMusic()
        {
            if (m_MusicSource != null) m_MusicSource.Stop();
            foreach (var src in m_PausedLoops)
            {
                if (src != null) src.UnPause();
            }
            m_PausedLoops.Clear();
        }

        // ── Fighters ──

        private void PlaceFighters()
        {
            Transform center = m_Anchors.Center;
            Vector3 right = Flat(center.right).normalized;
            if (right.sqrMagnitude < 0.01f) right = Vector3.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            float ground = center.position.y;

            Bounds monsterBounds = GetBounds(m_MonsterProxy.transform);
            Bounds playerBounds = m_PlayerProxy != null ? GetBounds(m_PlayerProxy.transform) : new Bounds(Vector3.zero, new Vector3(0.6f, 1.8f, 0.6f));

            float aspect = m_ArenaCamera != null ? Mathf.Max(0.5f, m_ArenaCamera.aspect) : 16f / 9f;
            float tallest = Mathf.Max(1.8f, Mathf.Max(playerBounds.size.y, monsterBounds.size.y));
            float monsterRadius = Mathf.Max(monsterBounds.extents.x, monsterBounds.extents.z);
            float spanShare = 1f - 2f * m_FighterScreenX;

            float visibleHeight = tallest / m_FighterScreenHeight;
            float distance = Mathf.Max(spanShare * visibleHeight * aspect, monsterRadius + 1.5f);
            visibleHeight = Mathf.Max(visibleHeight, distance / (spanShare * aspect));

            Vector3 mid = center.position;
            Quaternion faceCam = Quaternion.LookRotation(-forward, Vector3.up);

            if (m_PlayerProxy != null)
            {
                m_PlayerHome = StandOn(m_PlayerProxy.transform, mid - right * (distance * 0.5f), ground);
                m_PlayerProxy.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(right, Vector3.up), faceCam, m_FaceCamera);
            }
            else
            {
                m_PlayerHome = mid - right * (distance * 0.5f);
            }

            m_MonsterHome = StandOn(m_MonsterProxy.transform, mid + right * (distance * 0.5f), ground);
            m_MonsterProxy.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(-right, Vector3.up), faceCam, m_FaceCamera);

            if (m_ArenaCamera != null)
            {
                float halfFov = m_CameraFov * 0.5f * Mathf.Deg2Rad;
                float camDistance = visibleHeight * 0.5f / Mathf.Tan(halfFov);
                Vector3 target = mid;
                target.y = ground + (0.5f - m_GroundScreenY) * visibleHeight;
                Vector3 camPos = target - forward * camDistance + Vector3.up * m_CameraElevation;
                m_ArenaCamera.transform.SetPositionAndRotation(camPos, Quaternion.LookRotation(target - camPos, Vector3.up));
            }
        }

        /// <summary>Put a proxy at <paramref name="spot"/> with its lowest point resting on <paramref name="ground"/>.</summary>
        private static Vector3 StandOn(Transform proxy, Vector3 spot, float ground)
        {
            proxy.position = spot;
            Bounds b = GetBounds(proxy);
            proxy.position += Vector3.up * (ground - b.min.y);
            return proxy.position;
        }

        private GameObject CreateProxy(GameObject source, string name)
        {
            // Cloning under an inactive holder keeps Awake/OnEnable of gameplay scripts from running.
            var holder = new GameObject("ProxyHolder");
            holder.SetActive(false);
            GameObject clone = Instantiate(source, holder.transform);
            clone.name = name;
            clone.SetActive(true);
            StripToVisuals(clone);

            Vector3 worldScale = source.transform.lossyScale;
            clone.transform.SetParent(m_Anchors.transform, false);
            clone.transform.localScale = worldScale;
            Destroy(holder);

            foreach (var animator in clone.GetComponentsInChildren<Animator>(true))
            {
                animator.applyRootMotion = false;
                foreach (var p in animator.parameters)
                {
                    if (p.type == AnimatorControllerParameterType.Float && (p.name == "Speed" || p.name == "MotionSpeed")) animator.SetFloat(p.name, 0f);
                    if (p.type == AnimatorControllerParameterType.Bool && p.name == "Grounded") animator.SetBool(p.name, true);
                    if (p.type == AnimatorControllerParameterType.Bool && (p.name == "Jump" || p.name == "FreeFall")) animator.SetBool(p.name, false);
                }
            }
            return clone;
        }

        private static void StripToVisuals(GameObject clone)
        {
            // UI attached to the character (overhead bars, prompts) is not part of its look.
            foreach (var canvas in clone.GetComponentsInChildren<Canvas>(true))
            {
                if (canvas != null && canvas.gameObject != clone) DestroyImmediate(canvas.gameObject);
            }

            // Remove everything that isn't visual. Components that others require go in a later pass.
            for (int pass = 0; pass < 4; pass++)
            {
                var remaining = new List<Component>();
                foreach (var c in clone.GetComponentsInChildren<Component>(true))
                {
                    if (c != null && !KeepTypes.Contains(c.GetType())) remaining.Add(c);
                }
                if (remaining.Count == 0) break;

                var required = new HashSet<Type>();
                foreach (var c in remaining)
                {
                    foreach (RequireComponent req in Attribute.GetCustomAttributes(c.GetType(), typeof(RequireComponent), true))
                    {
                        if (req.m_Type0 != null) required.Add(req.m_Type0);
                        if (req.m_Type1 != null) required.Add(req.m_Type1);
                        if (req.m_Type2 != null) required.Add(req.m_Type2);
                    }
                }

                foreach (var c in remaining)
                {
                    bool isRequired = false;
                    foreach (var t in required)
                    {
                        if (t.IsAssignableFrom(c.GetType())) { isRequired = true; break; }
                    }
                    if (!isRequired || pass == 3) DestroyImmediate(c);
                }
            }

            foreach (var r in clone.GetComponentsInChildren<Renderer>(true)) r.enabled = true;
        }

        private void DestroyProxies()
        {
            if (m_PlayerProxy != null) Destroy(m_PlayerProxy);
            if (m_MonsterProxy != null) Destroy(m_MonsterProxy);
            m_PlayerProxy = null;
            m_MonsterProxy = null;
        }

        private void HidePlayerOverhead()
        {
            m_PlayerOverhead = IUHPlayerOverheadHUD.Instance != null ? IUHPlayerOverheadHUD.Instance.gameObject : null;
            m_PlayerOverheadWasActive = m_PlayerOverhead != null && m_PlayerOverhead.activeSelf;
            if (m_PlayerOverhead != null) m_PlayerOverhead.SetActive(false);
        }

        // ── Feedback ──

        private void Lunge(GameObject actor, Vector3 home, Vector3 targetPos, float distance)
        {
            if (!IsActive || actor == null) return;
            StartCoroutine(LungeRoutine(actor.transform, home, targetPos, distance));
        }

        private IEnumerator LungeRoutine(Transform actor, Vector3 home, Vector3 targetPos, float distance)
        {
            Vector3 dir = Flat(targetPos - home).normalized;
            float half = m_LungeDuration * 0.5f;
            for (float t = 0f; t < m_LungeDuration && actor != null; t += Time.deltaTime)
            {
                float k = t < half ? t / half : 1f - (t - half) / half;
                actor.position = home + dir * (distance * Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            if (actor != null) actor.position = home;
        }

        private void Shake(GameObject actor, Vector3 home)
        {
            if (!IsActive || actor == null) return;
            StartCoroutine(ShakeRoutine(actor.transform, home));
        }

        private IEnumerator ShakeRoutine(Transform actor, Vector3 home)
        {
            yield return new WaitForSeconds(m_LungeDuration * 0.5f);
            for (float t = 0f; t < m_ShakeDuration && actor != null; t += Time.deltaTime)
            {
                float fade = 1f - t / m_ShakeDuration;
                Vector2 r = UnityEngine.Random.insideUnitCircle * (m_ShakeAmount * fade);
                actor.position = home + new Vector3(r.x, 0f, r.y);
                yield return null;
            }
            if (actor != null) actor.position = home;
        }

        // ── Fade ──

        private IEnumerator Fade(float from, float to)
        {
            if (m_FadeOverlay == null) yield break;
            for (float t = 0f; t < m_FadeDuration; t += Time.unscaledDeltaTime)
            {
                SetFade(Mathf.Lerp(from, to, t / m_FadeDuration));
                yield return null;
            }
            SetFade(to);
        }

        private void SetFade(float alpha)
        {
            if (m_FadeOverlay == null) return;
            Color c = m_FadeOverlay.color;
            c.a = alpha;
            m_FadeOverlay.color = c;
            m_FadeOverlay.raycastTarget = alpha > 0.01f;
            m_FadeOverlay.gameObject.SetActive(alpha > 0.001f);
            m_FadeOverlay.transform.SetAsLastSibling();
        }

        // ── Helpers ──

        private static Bounds GetBounds(Transform root)
        {
            bool any = false;
            Bounds b = new Bounds(root.position + Vector3.up * 0.9f, new Vector3(0.6f, 1.8f, 0.6f));
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
