using UnityEngine;

namespace IUHCampus.Minigames.GreenCampus
{
    public class IUHGreenBillboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
        }
    }
}
