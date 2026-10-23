using UnityEngine;

namespace IUHCampus.Minigames.GreenCampus
{
    public class IUHGreenLitter : MonoBehaviour
    {
        public string itemId;
        public string displayName;
        public string zoneId;
        public WasteCategory category;
        public Transform visual;
        public GameObject marker;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private Transform homeParent;
        private bool cached;

        public GreenItemDefinition Definition => new GreenItemDefinition(itemId, zoneId, category);
        public void CacheHome()
        {
            if (cached) return;
            cached = true;
            homePosition = transform.position;
            homeRotation = transform.rotation;
            homeParent = transform.parent;
        }
        public void SetSorted(bool sorted)
        {
            CacheHome();
            if (!sorted) ReturnHome();
            gameObject.SetActive(!sorted);
        }
        public void Carry(Transform player)
        {
            CacheHome();
            transform.SetParent(player, true);
            transform.localPosition = new Vector3(0.35f, 1.0f, 0.5f);
            transform.localRotation = Quaternion.identity;
            if (marker != null) marker.SetActive(false);
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
        }
        public void ReturnHome()
        {
            CacheHome();
            transform.SetParent(homeParent, true);
            transform.SetPositionAndRotation(homePosition, homeRotation);
            if (marker != null) marker.SetActive(true);
            foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = true;
        }
    }

}
