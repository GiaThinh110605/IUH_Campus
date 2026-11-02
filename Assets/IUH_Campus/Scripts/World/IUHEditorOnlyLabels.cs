using UnityEngine;

namespace IUHCampus.World
{

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
