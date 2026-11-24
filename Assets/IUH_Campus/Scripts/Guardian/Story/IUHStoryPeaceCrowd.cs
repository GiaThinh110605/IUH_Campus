using System;
using System.Collections.Generic;
using IUHCampus.DailyActivities;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    // Hides campus students and the IUH mascot while a disaster is running; shows them again in peace time.
    // Only toggles GameObjects: no change to IUHAmbientStudent / IUHMascotNPC logic or quest state.
    public sealed class IUHStoryPeaceCrowd : MonoBehaviour
    {
        public const string StudentsRootName = "NPC_STUDENTS";
        public static event Action<bool> DisasterChanged;
        readonly List<GameObject> crowd = new List<GameObject>();
        bool initialized;
        public bool IsDisaster { get; private set; }
        public IReadOnlyList<GameObject> Crowd => crowd;

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.gameObject.scene.IsValid() && t.name == StudentsRootName) crowd.Add(t.gameObject);
            }
            foreach (var mascot in FindObjectsByType<IUHMascotNPC>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mascot.gameObject.scene.IsValid() && !crowd.Contains(mascot.gameObject)) crowd.Add(mascot.gameObject);
            }
        }

        public void SetDisaster(bool disaster)
        {
            Initialize();
            bool changed = IsDisaster != disaster;
            IsDisaster = disaster;
            if (disaster && IUHCampusActivitiesHUD.Instance != null && IUHCampusActivitiesHUD.Instance.IsMascotDialogOpen)
                IUHCampusActivitiesHUD.Instance.CloseMascotDialog();
            foreach (var target in crowd)
            {
                if (target != null && target.activeSelf == disaster) target.SetActive(!disaster);
            }
            if (changed) DisasterChanged?.Invoke(disaster);
        }
    }
}
