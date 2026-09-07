using UnityEngine;
using UnityEditor;

namespace IUHCampus.Editor
{
    public static class IUHBuildAutoExec
    {
        [MenuItem("IUH Campus/Rebuild All Scenes and Tour Cameras")]
        public static void RebuildAll()
        {
            Debug.Log("[IUH] Rebuilding Right Buildings Cluster (G-I-C)...");
            IUHRightClusterBuilder.BuildRightClusterScene();

            Debug.Log("[IUH] Rebuilding Complete Master Campus...");
            IUHCampusMasterBuilder.BuildMasterCampus();

            Debug.Log("[IUH] All scenes successfully rebuilt with Interactive Tour Controller!");
        }
    }
}
