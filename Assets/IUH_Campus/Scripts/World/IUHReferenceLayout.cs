using UnityEngine;

namespace IUHCampus.World
{
    // Shared world anchors for the illustrated exterior layout. Legacy scenes keep their own coordinates.
    public static class IUHReferenceLayout
    {
        public const string RootName = "IUH_REFERENCE_CAMPUS";
        public static bool IsActive => GameObject.Find(RootName) != null;
        public static Vector3 Entrance => new Vector3(409f, 0.65f, -28f);
        public static Vector3 GreenSpawn => new Vector3(375.8f, 0.65f, -78.5f);
        public static Vector3[] Pickups => new[] {
            new Vector3(409f, .49f, -57f), new Vector3(357f, .49f, -67f), new Vector3(409f, .49f, -37f)
        };
        public static Vector3[] Dropoffs => new[] {
            new Vector3(295f, .49f, -44f), new Vector3(393f, .49f, -136f), new Vector3(398f, .49f, -86f)
        };
    }
}
