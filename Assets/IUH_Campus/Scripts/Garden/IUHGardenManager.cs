using System;
using System.Collections.Generic;
using IUHCampus.Guardian;
using UnityEngine;

namespace IUHCampus.Garden
{
    public sealed class IUHGardenManager : MonoBehaviour
    {
        const string SaveKey = "IUH_Garden_v2";
        List<GardenBed> beds;
        bool applying;
        public static IUHGardenManager Instance { get; private set; }
        public Func<DateTime> Clock { get; set; } = () => DateTime.UtcNow;
        public int BedCount => GardenRules.BedCount;
        public event Action<int> OnBedChanged;

        void Awake()
        {
            Instance = this;
            Load();
        }

        void OnEnable() => InvokeRepeating(nameof(Tick), 1f, 1f);
        void OnDisable() => CancelInvoke(nameof(Tick));

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public GardenBed GetBed(int index) => index >= 0 && index < BedCount ? beds?[index] : null;

        public void Load()
        {
            beds = GardenSave.Deserialize(PlayerPrefs.GetString(SaveKey));
            Tick();
            for (int i = 0; i < BedCount; i++) OnBedChanged?.Invoke(i);
        }

        public void Save()
        {
            PlayerPrefs.SetString(SaveKey, GardenSave.Serialize(beds));
            PlayerPrefs.Save();
        }

        void Tick()
        {
            var now = Clock();
            for (int i = 0; i < BedCount; i++)
            {
                if (beds[i].Advance(now)) Commit(i);
            }
        }

        public bool TryPlant(int i)
        {
            var bed = GetBed(i);
            return bed != null && bed.CanPlant() && Purchase(i, GardenRules.SeedCost, bed.Plant);
        }

        public bool TryFertilize(int i)
        {
            var bed = GetBed(i);
            return bed != null && bed.CanFertilize() && Purchase(i, GardenRules.FertilizerCost, bed.Fertilize);
        }

        bool Purchase(int i, int cost, Func<bool> action)
        {
            var inventory = IUHGuardianInventory.Instance;
            if (applying || inventory == null) return false;
            applying = true;
            try
            {
                if (!inventory.TrySpendGold(cost) || !action()) return false;
                Commit(i);
                return true;
            }
            finally { applying = false; }
        }

        public bool TryWater(int i)
        {
            var bed = GetBed(i);
            if (applying || bed == null || !bed.Water(Clock())) return false;
            Commit(i);
            return true;
        }

        public bool TryHarvest(int i)
        {
            var bed = GetBed(i);
            if (applying || bed == null || !bed.Harvest(out int gold)) return false;
            var inventory = IUHGuardianInventory.Instance;
            if (inventory != null) inventory.AddGold(gold);
            else Debug.LogWarning("[IUH Garden] Harvest completed without an inventory.");
            Commit(i);
            return true;
        }

        void Commit(int i)
        {
            Save();
            OnBedChanged?.Invoke(i);
        }
    }
}
