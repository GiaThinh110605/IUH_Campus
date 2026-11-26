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
        [SerializeField]
        Material m_NightSkyboxMaterial;
        StoryProgress progress;
        IUHStoryWorldLock world;
        IUHStoryDialogueUI ui;
        IUHStoryBeacon beacon;
        IUHStoryNightShift night;
        IUHStoryPeaceCrowd crowd;
        Transform player;
        IUHUpgradeShopUI shop;
        AftershockSchedule aftershockSchedule;
        int lastAftershockZone = -1;
        readonly Dictionary<string, Transform> targets = new Dictionary<string, Transform>();
        void Start()
        {
            if (m_StoryScript == null)
            {
                Debug.LogError("[Story] Missing story script");
                return;
            }

            progress = StoryProgress.Deserialize(PlayerPrefs.GetString(ProgressKey), m_StoryScript.checkIns.Length, m_StoryScript.ToChapterDefs());
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
            night.SetNightSkyboxMaterial(m_NightSkyboxMaterial);
            crowd = gameObject.AddComponent<IUHStoryPeaceCrowd>();
            crowd.SetDisaster(StoryDisaster.IsDisasterAtStart(progress.Phase));
            shop = IUHUpgradeShopUI.Instance ?? FindAnyObjectByType<IUHUpgradeShopUI>(FindObjectsInactive.Include);
            if (progress.Phase == StoryPhase.Completed)
            {
                world.RestoreToys();
                StartCoroutine(Aftershocks());
            }
            else
            {
                StartCoroutine(Run());
            }
        }

        IEnumerator Run()
        {
            if (progress.Phase == StoryPhase.Prologue)
                yield return Prologue();
            if (progress.Phase == StoryPhase.Incident)
            {
                beacon.MoveTo(null);
                ui.SetObjective(null);
                crowd.SetDisaster(true);
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
            crowd.SetDisaster(false);
            IUHCampusFireworksManager.Instance?.TriggerVictoryCelebration();
            ui.ShowCaption(m_StoryScript.endingCaption, 5);
            yield return Say(m_StoryScript.endingLines);
            progress.FinishEnding();
            Save();
            world.RestoreToys();
            yield return Aftershocks();
        }

        bool CanCountAftershock()
        {
            return Time.timeScale > 0 && !ui.IsBusy && (shop == null || !shop.IsOpen) &&
                (Match3CombatManager.Instance == null || !Match3CombatManager.Instance.BlocksExplorationInput);
        }

        IEnumerator Aftershocks()
        {
            aftershockSchedule = new AftershockSchedule(() => Random.value);
            var delay = new WaitForSecondsRealtime(.5f);
            while (true)
            {
                yield return delay;
                if (!aftershockSchedule.Tick(.5f, CanCountAftershock())) continue;
                var zones = m_StoryScript.aftershockZones;
                if (zones == null || zones.Length == 0)
                {
                    aftershockSchedule.Complete();
                    continue;
                }
                lastAftershockZone = AftershockSchedule.PickZone(() => Random.value, zones.Length, lastAftershockZone);
                yield return RunAftershock(zones[lastAftershockZone]);
                aftershockSchedule.Complete();
            }
        }

        IEnumerator RunAftershock(IUHStoryScript.AftershockZone zone)
        {
            var names = new List<string>();
            foreach (var name in zone.encounterObjectNames)
            {
                if (world.FindEncounter(name) != null) names.Add(name);
            }
            var picked = AftershockSchedule.PickEncounters(() => Random.value, names);
            if (picked.Count == 0) yield break;
            crowd.SetDisaster(true);
            var active = new List<GameObject>();
            var positions = new List<Vector3>();
            foreach (var name in picked)
            {
                var encounter = world.FindEncounter(name);
                encounter.SetActive(true);
                encounter.GetComponent<IUHMonsterEncounter>()?.ResetForAftershock();
                encounter.GetComponent<AR.IUHARCorruptedObjectEncounter>()?.ResetForAftershock();
                active.Add(encounter);
                positions.Add(encounter.transform.position);
            }
            var corruption = IUHStoryCorruptionZone.Create(positions, m_StoryScript.fogMaterial);
            beacon.MoveTo(corruption.transform.position);
            ui.SetObjective(string.Format(m_StoryScript.aftershockObjectiveFormat, zone.title, active.Count));
            yield return Say(new[] { string.Format(m_StoryScript.aftershockWarningFormat, zone.title) });
            var delay = new WaitForSecondsRealtime(.5f);
            while (active.Count > 0)
            {
                yield return delay;
                if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.BlocksExplorationInput) continue;
                active.RemoveAll(encounter => encounter == null || !encounter.activeSelf);
                ui.SetObjective(string.Format(m_StoryScript.aftershockObjectiveFormat, zone.title, active.Count));
            }
            corruption.Purify();
            IUHGuardianInventory.Instance?.AddGold(m_StoryScript.aftershockGold);
            yield return Say(new[] { m_StoryScript.aftershockClearedLine });
            crowd.SetDisaster(false);
            beacon.MoveTo(null);
            ui.SetObjective(null);
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
