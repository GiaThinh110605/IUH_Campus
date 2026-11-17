using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryCorruptionZone : MonoBehaviour
    {
        const float Padding = 6f;
        const float PurifySeconds = 2f;
        Renderer fogRenderer;
        bool purifying;
        public static IUHStoryCorruptionZone Create(IReadOnlyList<Vector3> encounterPositions, Material fog)
        {
            var center = Vector3.zero;
            foreach (var position in encounterPositions)
                center += position;
            if (encounterPositions.Count > 0)
                center /= encounterPositions.Count;
            float radius = 0;
            foreach (var position in encounterPositions)
                radius = Mathf.Max(radius, Vector3.Distance(center, position));
            radius += Padding;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Story Corruption Zone";
            Destroy(go.GetComponent<Collider>());
            go.transform.position = center;
            go.transform.localScale = new Vector3(radius * 2, 3, radius * 2);
            var zone = go.AddComponent<IUHStoryCorruptionZone>();
            zone.fogRenderer = go.GetComponent<Renderer>();
            zone.fogRenderer.sharedMaterial = fog;
            return zone;
        }

        public void Purify()
        {
            if (purifying)
                return;
            purifying = true;
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 3, "KHU VỰC ĐÃ ĐƯỢC THANH TẨY", Color.cyan, 1.4f);
            StartCoroutine(Fade());
        }

        IEnumerator Fade()
        {
            var scale = transform.localScale;
            var color = fogRenderer.sharedMaterial.GetColor("_BaseColor");
            var block = new MaterialPropertyBlock();
            for (float time = 0; time < PurifySeconds; time += Time.deltaTime)
            {
                float fraction = time / PurifySeconds;
                transform.localScale = scale * (1 - fraction);
                var faded = color;
                faded.a *= 1 - fraction;
                block.SetColor("_BaseColor", faded);
                fogRenderer.SetPropertyBlock(block);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
