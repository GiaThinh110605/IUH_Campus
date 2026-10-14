using UnityEngine;

namespace IUHCampus.World
{
    /// <summary>
    /// Ensures 3D debug/editor labels are only visible in the Editor / Scene view
    /// and are automatically disabled when entering Play Mode / gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public class IUHEditorOnlyLabels : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
