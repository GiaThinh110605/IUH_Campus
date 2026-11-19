using System;
using System.Collections;
using UnityEngine;

namespace IUHCampus.Garden
{
    public sealed class IUHGardenBedView : MonoBehaviour
    {
        [SerializeField] int m_BedIndex;
        [SerializeField] GameObject m_SeedVisual;
        [SerializeField] GameObject m_SproutVisual;
        [SerializeField] GameObject m_BloomVisual;
        [SerializeField] Renderer m_Soil;
        [SerializeField] Material m_DrySoil;
        [SerializeField] Material m_WetSoil;
        [SerializeField] GameObject m_FertilizedFx;
        [SerializeField] TextMesh m_Label;
        MaterialPropertyBlock soilTint;
        BoxCollider footprint;
        GardenStage? displayedStage;
        Coroutine growth;

        public int BedIndex => m_BedIndex;
        public Bounds InteractionBounds => footprint.bounds;

        void Awake() => footprint = GetComponent<BoxCollider>();

        public void Refresh(GardenBed bed, DateTime nowUtc)
        {
            if (bed == null) return;
            if (displayedStage != bed.Stage)
            {
                bool animate = displayedStage.HasValue;
                displayedStage = bed.Stage;
                if (growth != null) StopCoroutine(growth);
                ResetVisual(m_SeedVisual, bed.Stage == GardenStage.Seed);
                ResetVisual(m_SproutVisual, bed.Stage == GardenStage.Sprout);
                ResetVisual(m_BloomVisual, bed.Stage == GardenStage.Bloom);
                var active = bed.Stage == GardenStage.Seed ? m_SeedVisual :
                    bed.Stage == GardenStage.Sprout ? m_SproutVisual :
                    bed.Stage == GardenStage.Bloom ? m_BloomVisual : null;
                growth = animate && active != null ? StartCoroutine(Grow(active.transform)) : null;
            }
            m_FertilizedFx.SetActive(bed.Fertilized);
            m_Soil.sharedMaterial = bed.IsWatered && m_WetSoil != null ? m_WetSoil : m_DrySoil;
            if (soilTint == null) soilTint = new MaterialPropertyBlock();
            var color = m_Soil.sharedMaterial.GetColor("_BaseColor");
            soilTint.SetColor("_BaseColor", bed.IsWatered && m_WetSoil == null
                ? new Color(color.r * .78f, color.g * .78f, color.b * .78f, color.a)
                : color);
            m_Soil.SetPropertyBlock(soilTint);
            SetPrompt(bed, nowUtc, false);
        }

        static void ResetVisual(GameObject stage, bool visible)
        {
            stage.transform.localScale = Vector3.one;
            stage.SetActive(visible);
        }

        IEnumerator Grow(Transform stage)
        {
            const float duration = .6f;
            const float startScale = .3f;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                stage.localScale = Vector3.one * Mathf.SmoothStep(startScale, 1, elapsed / duration);
                yield return null;
            }

            stage.localScale = Vector3.one;
            growth = null;
        }

        public void SetPrompt(GardenBed bed, DateTime nowUtc, bool near)
        {
            string text = near ? "[E] Chăm hoa" : Status(bed, nowUtc);
            if (m_Label.text != text) m_Label.text = text;
            if (m_Label.gameObject.activeSelf != (text.Length > 0))
                m_Label.gameObject.SetActive(text.Length > 0);
        }

        public static string Status(GardenBed bed, DateTime nowUtc)
        {
            if (bed.Stage == GardenStage.Empty) return "";
            if (bed.CanHarvest()) return "Hái được!";
            int? seconds = bed.SecondsRemaining(nowUtc);
            return seconds.HasValue ? $"{seconds.Value / 60}:{seconds.Value % 60:00}" : "Cần tưới";
        }
    }
}
