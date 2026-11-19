using System;
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

        public int BedIndex => m_BedIndex;

        public void Refresh(GardenBed bed, DateTime nowUtc)
        {
            if (bed == null) return;
            m_SeedVisual.SetActive(bed.Stage == GardenStage.Seed);
            m_SproutVisual.SetActive(bed.Stage == GardenStage.Sprout);
            m_BloomVisual.SetActive(bed.Stage == GardenStage.Bloom);
            m_FertilizedFx.SetActive(bed.Fertilized);
            m_Soil.sharedMaterial = bed.IsWatered && m_WetSoil != null ? m_WetSoil : m_DrySoil;
            if (soilTint == null) soilTint = new MaterialPropertyBlock();
            soilTint.SetColor("_BaseColor", bed.IsWatered && m_WetSoil == null ? Color.white * .55f : Color.white);
            m_Soil.SetPropertyBlock(soilTint);
            SetPrompt(bed, nowUtc, false);
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
