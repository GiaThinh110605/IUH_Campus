using UnityEngine;

namespace IUHCampus.Interaction
{
    [DisallowMultipleComponent]
    public class WorldObjectID : MonoBehaviour
    {
        [Tooltip("Unique, stable identifier used for persistence, quest states, and cross-system references.")]
        public string objectId = "";

        [Tooltip("Optional user-friendly display name.")]
        public string displayName = "";

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = gameObject.name.ToUpper().Replace(" ", "_");
            }
        }
    }
}
