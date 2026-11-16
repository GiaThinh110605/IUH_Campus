using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryBeacon : MonoBehaviour
    {
        Vector3 target;
        public static IUHStoryBeacon Create(Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Story Beacon";
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.transform.localScale = new Vector3(1.2f, 12.5f, 1.2f);
            var beacon = go.AddComponent<IUHStoryBeacon>();
            go.SetActive(false);
            return beacon;
        }

        public void MoveTo(Vector3? worldPos)
        {
            gameObject.SetActive(worldPos.HasValue);
            if (!worldPos.HasValue)
                return;
            target = worldPos.Value;
            transform.position = target + Vector3.up * 12.5f;
        }

        public float DistanceTo(Vector3 from)
        {
            from.y = target.y;
            return Vector3.Distance(from, target);
        }
    }
}
