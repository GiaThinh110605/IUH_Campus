using System;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    [CreateAssetMenu(menuName = "IUH/Story Script")]
    public sealed class IUHStoryScript : ScriptableObject
    {
        [Serializable]
        public struct CameraShot
        {
            public Vector3 position;
            public Vector3 lookAt;
            public float seconds;
        }

        [Serializable]
        public sealed class CheckInStep
        {
            public string checkInObjectName;
            public string objective;
            public string[] lines;
        }

        [Serializable]
        public sealed class Chapter
        {
            public string id;
            public string title;
            public string objective;
            public string[] introLines;
            public string[] encounterObjectNames;
            public string[] purifiedLines;
        }

        [Serializable]
        public sealed class AftershockZone
        {
            public string id;
            public string title;
            public string[] encounterObjectNames;
        }

        public AftershockZone[] aftershockZones;
        public string aftershockWarningFormat;
        public string aftershockObjectiveFormat;
        public string aftershockClearedLine;
        public int aftershockGold = 30;

        public string introCaption;
        public CameraShot[] introShots;
        public string[] prologueLines;
        public CheckInStep[] checkIns;
        public float checkInRadius = 4f;
        public string[] incidentLines;
        public Chapter[] chapters;
        public string[] endingLines;
        public string endingCaption;
        public Material fogMaterial;
        public Material beaconMaterial;
        public string festivalLedMaterialName = "Festival LED";

        public IReadOnlyList<StoryChapterDef> ToChapterDefs() =>
            Array.ConvertAll(chapters, c => new StoryChapterDef(c.id, c.encounterObjectNames));
    }
}
