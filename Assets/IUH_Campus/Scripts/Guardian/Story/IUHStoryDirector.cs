using System.Collections;
using System.Collections.Generic;
using IUHCampus.Combat;
using IUHCampus.Guardian.Match3;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace IUHCampus.Guardian.Story
{
    [DefaultExecutionOrder(-500)]
    public sealed class IUHStoryDirector : MonoBehaviour
    {
        const string ProgressKey = "IUH_Story_Progress";
        [SerializeField]
        IUHStoryScript m_StoryScript;
        StoryProgress progress;
        IUHStoryWorldLock world;
        IUHStoryDialogueUI ui;
        IUHStoryBeacon beacon;
        IUHStoryNightShift night;
        Transform player;
        readonly Dictionary<string, Transform> targets = new Dictionary<string, Transform>();
        void Start()
        {
            if (m_StoryScript == null)
            {
                Debug.LogError("[Story] Missing story script");
                return;
            }

            progress = StoryProgress.Deserialize(PlayerPrefs.GetString(ProgressKey), m_StoryScript.checkIns.Length, m_StoryScript.ToChapterDefs());
            if (progress.Phase == StoryPhase.Completed)
                return;
            world = new IUHStoryWorldLock();
            world.CaptureAndLock();
            foreach (var target in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (target.gameObject.scene.IsValid())
                    targets[target.name] = target;
            }

            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            player = student != null ? student.transform : null;
            ui = IUHStoryDialogueUI.Create();
            beacon = IUHStoryBeacon.Create(m_StoryScript.beaconMaterial);
            night = gameObject.AddComponent<IUHStoryNightShift>();
            night.Initialize(m_StoryScript.festivalLedMaterialName);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            if (progress.Phase == StoryPhase.Prologue)
                yield return Prologue();
            if (progress.Phase == StoryPhase.Incident)
            {
                beacon.MoveTo(null);
                ui.SetObjective(null);
                night.ToNight(3);
                yield return Say(m_StoryScript.incidentLines);
                progress.FinishIncident();
                Save();
            }

            if (progress.Phase == StoryPhase.Chapter)
                night.SetNightImmediate();
            while (progress.Phase == StoryPhase.Chapter)
                yield return Chapter();
            if (progress.Phase == StoryPhase.Ending)
                yield return Ending();
        }

        IEnumerator Prologue()
        {
            if (progress.CheckInsDone == 0)
            {
                bool done = false;
                IUHStoryIntroCamera.Play(m_StoryScript.introShots, m_StoryScript.introCaption, ui, () => done = true);
                yield return new WaitUntil(() => done);
                yield return Say(m_StoryScript.prologueLines);
            }

            var delay = new WaitForSeconds(.25f);
            while (progress.Phase == StoryPhase.Prologue)
            {
                var step = m_StoryScript.checkIns[progress.CheckInsDone];
                if (!targets.TryGetValue(step.checkInObjectName, out var target) || player == null)
                {
                    Debug.LogError("[Story] Missing check-in target or player: " + step.checkInObjectName);
                    yield break;
                }

                beacon.MoveTo(target.position);
                ui.SetObjective(step.objective);
                while (HorizontalDistance(player.position, target.position) > m_StoryScript.checkInRadius)
                    yield return delay;
                progress.CompleteCheckIn(progress.CheckInsDone);
                Save();
                yield return Say(step.lines);
            }
        }

        static float HorizontalDistance(Vector3 from, Vector3 to)
        {
            from.y = to.y;
            return Vector3.Distance(from, to);
        }

        IEnumerator Chapter()
        {
            int index = progress.ChapterIndex;
            var chapter = m_StoryScript.chapters[index];
            var active = new Dictionary<string, GameObject>();
            var positions = new List<Vector3>();
            foreach (var name in chapter.encounterObjectNames)
            {
                var encounter = world.FindEncounter(name);
                if (encounter != null)
                    positions.Add(encounter.transform.position);
                if (progress.IsDefeated(name))
                    continue;
                if (encounter == null)
                {
                    Debug.LogError("[Story] Missing encounter: " + name);
                    progress.MarkEncounterDefeated(name);
                    Save();
                    continue;
                }

                world.Release(encounter);
                if (encounter.activeSelf)
                    active.Add(name, encounter);
            }

            var zone = IUHStoryCorruptionZone.Create(positions, m_StoryScript.fogMaterial);
            beacon.MoveTo(zone.transform.position);
            UpdateObjective(chapter);
            yield return Say(chapter.introLines);
            var delay = new WaitForSeconds(.5f);
            while (progress.Phase == StoryPhase.Chapter && progress.ChapterIndex == index)
            {
                yield return delay;
                if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.IsCombatActive)
                    continue;
                foreach (var entry in active)
                {
                    if (progress.IsDefeated(entry.Key) || (entry.Value != null && entry.Value.activeSelf))
                        continue;
                    bool completed = progress.MarkEncounterDefeated(entry.Key);
                    Save();
                    UpdateObjective(chapter);
                    if (completed)
                        break;
                }
            }

            zone.Purify();
            yield return Say(chapter.purifiedLines);
        }

        void UpdateObjective(IUHStoryScript.Chapter chapter)
        {
            int remaining = 0;
            foreach (var name in chapter.encounterObjectNames)
            {
                if (!progress.IsDefeated(name))
                    remaining++;
            }

            ui.SetObjective($"{chapter.objective} (còn {remaining} quái)");
        }

        IEnumerator Ending()
        {
            beacon.MoveTo(null);
            ui.SetObjective(null);
            night.SetNightImmediate();
            night.ToDay(3);
            yield return new WaitForSecondsRealtime(3);
            IUHCampusFireworksManager.Instance?.TriggerVictoryCelebration();
            ui.ShowCaption(m_StoryScript.endingCaption, 5);
            yield return Say(m_StoryScript.endingLines);
            progress.FinishEnding();
            Save();
            world.RestoreAll(progress.IsDefeated);
        }

        IEnumerator Say(IReadOnlyList<string> lines)
        {
            bool done = false;
            ui.ShowLines("Core-Bot", lines, () => done = true);
            yield return new WaitUntil(() => done);
        }

        void Save()
        {
            PlayerPrefs.SetString(ProgressKey, progress.Serialize());
            PlayerPrefs.Save();
        }

        public void ResetStory()
        {
            PlayerPrefs.DeleteKey(ProgressKey);
            PlayerPrefs.Save();
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void Update()
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
                return;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame) ResetStory();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F9)) ResetStory();
#endif
        }
    }
}
