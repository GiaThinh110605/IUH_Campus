using UnityEngine;

namespace IUHCampus.World
{
    // Shared world anchors for the illustrated exterior layout. Legacy scenes keep their own coordinates.
    public static class IUHReferenceLayout
    {
        public const string RootName = "IUH_REFERENCE_CAMPUS";
        public static bool IsActive => GameObject.Find(RootName) != null;
        public static Vector3 Entrance => new Vector3(395f, 0.55f, -28f);
        public static Vector3 GreenSpawn => new Vector3(375.8f, 0.65f, -78.5f);
        public static bool HasPhotoForecourt => GameObject.Find(RootName + "/PHOTO_REFERENCE_R2") != null;
        public static Vector3 DormEntrance => new Vector3(290f, 0.45f, -50f);
        public static Vector3 LibraryEntrance => new Vector3(425f, 0.65f, -11f);
        public static Vector3 FRoof => new Vector3(326f, 13.7f, -79f);
        public static Vector3[] Pickups => new[] {
            new Vector3(360f, 0.49f, -38f), new Vector3(409f, 0.49f, -40f), new Vector3(380f, 0.49f, -8f)
        };
        public static Vector3[] Dropoffs => new[] {
            new Vector3(290f, 0.45f, -50f), new Vector3(310f, 0.49f, -70f), new Vector3(330f, 0.49f, -60f)
        };
    }
}
