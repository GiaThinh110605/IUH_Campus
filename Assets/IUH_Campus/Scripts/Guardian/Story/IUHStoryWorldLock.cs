using System;
using System.Collections.Generic;
using IUHCampus.Combat;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryWorldLock
    {
        readonly Dictionary<GameObject, bool> objects = new Dictionary<GameObject, bool>();
        readonly Dictionary<Behaviour, bool> behaviours = new Dictionary<Behaviour, bool>();
        readonly Dictionary<string, GameObject> encounters = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        public void CaptureAndLock()
        {
            if (objects.Count != 0 || behaviours.Count != 0)
                return;
            foreach (var encounter in Find<IUHMonsterEncounter>())
            {
                encounters[encounter.name] = encounter.gameObject;
                Capture(encounter.gameObject);
            }

            foreach (var monster in Find<IUHMonsterAI>())
                Capture(monster.gameObject);
            foreach (var monster in Find<IUHFlyingMonsterAI>())
                Capture(monster.gameObject);
            foreach (var transform in Find<Transform>())
            {
                if (transform.name == "IUH_HOT_AIR_BALLOON")
                    Capture(transform.gameObject);
            }

            foreach (var spawner in Find<IUHBuildingMonsterSpawner>())
            {
                Capture(spawner.gameObject);
            }
            CaptureBehaviours<IUHCampusZoneDirector>();
            CaptureBehaviours<IUHEmergencyDroneSupplyManager>();
            CaptureBehaviours<IUHCampusCombatManager>();
            CaptureBehaviours<IUHHotAirBalloonController>();
            CaptureBehaviours<IUHAerialFlightController>();
            foreach (var entry in behaviours)
                entry.Key.enabled = false;
            foreach (var entry in objects)
                entry.Key.SetActive(false);
        }

        static IEnumerable<T> Find<T>()
            where T : Component
        {
            foreach (var component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component.gameObject.scene.IsValid())
                    yield return component;
            }
        }

        void Capture(GameObject target)
        {
            if (!objects.ContainsKey(target))
                objects.Add(target, target.activeSelf);
        }

        void CaptureBehaviours<T>()
            where T : Behaviour
        {
            foreach (var behaviour in Find<T>())
                behaviours.Add(behaviour, behaviour.enabled);
        }

        public GameObject FindEncounter(string name) => encounters.TryGetValue(name, out var encounter) ? encounter : null;
        public void Release(GameObject encounter)
        {
            if (encounter != null)
                encounter.SetActive(true);
        }

        public void RestoreToys()
        {
            foreach (var entry in objects)
            {
                if (entry.Key != null && entry.Key.name == "IUH_HOT_AIR_BALLOON")
                    entry.Key.SetActive(entry.Value);
            }
            foreach (var entry in behaviours)
            {
                if (entry.Key is IUHCampusCombatManager || entry.Key is IUHHotAirBalloonController ||
                    entry.Key is IUHAerialFlightController || entry.Key is IUHEmergencyDroneSupplyManager)
                    entry.Key.enabled = entry.Value;
            }
        }

        public void RestoreAll(Func<string, bool> isDefeated)
        {
            foreach (var entry in objects)
            {
                if (entry.Key != null)
                    entry.Key.SetActive(entry.Value && !(encounters.ContainsKey(entry.Key.name) && isDefeated(entry.Key.name)));
            }

            foreach (var entry in behaviours)
            {
                if (entry.Key != null)
                    entry.Key.enabled = entry.Value;
            }
        }
    }
}
