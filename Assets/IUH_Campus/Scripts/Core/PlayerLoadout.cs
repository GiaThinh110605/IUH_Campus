using System;
using UnityEngine;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.Core
{
    /// <summary>
    /// Holds the player's selected uniform and vehicle loadout for Mini-Game 1 (Grab Campus) and campus exploration.
    /// Persists across sessions via PlayerPrefs.
    /// Default selection is 'Sinh viên IUH' (DefaultIUH) if not previously set by player.
    /// </summary>
    [System.Serializable]
    public class PlayerLoadout
    {
        private const string PREF_KEY_THEME = "IUH_PlayerLoadout_Theme";

        public IUHCustomizationManager.BrandTheme theme = IUHCustomizationManager.BrandTheme.DefaultIUH;
        public string outfitName = "Sinh Viên IUH";
        public string vehicleName = "Xe máy Sinh Viên";

        private static PlayerLoadout s_Current;
        public static PlayerLoadout Current
        {
            get
            {
                if (s_Current == null)
                {
                    s_Current = new PlayerLoadout();
                    s_Current.Load();
                }
                return s_Current;
            }
        }

        public void Load()
        {
            // Default to Sinh viên IUH (DefaultIUH = 3) if no key saved
            int savedVal = PlayerPrefs.GetInt(PREF_KEY_THEME, (int)IUHCustomizationManager.BrandTheme.DefaultIUH);
            theme = (IUHCustomizationManager.BrandTheme)Mathf.Clamp(savedVal, 0, 3);
            UpdateNames();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(PREF_KEY_THEME, (int)theme);
            PlayerPrefs.SetInt("IUH_Selected_Theme", (int)theme); // Backwards compatibility
            PlayerPrefs.Save();
            UpdateNames();
        }

        public void SetTheme(IUHCustomizationManager.BrandTheme newTheme)
        {
            theme = newTheme;
            Save();

            if (IUHCustomizationManager.Instance != null)
            {
                IUHCustomizationManager.Instance.ApplyTheme(newTheme);
            }
        }

        private void UpdateNames()
        {
            switch (theme)
            {
                case IUHCustomizationManager.BrandTheme.XanhSM:
                    outfitName = "XANH SM";
                    vehicleName = "Xe máy điện Xanh SM";
                    break;
                case IUHCustomizationManager.BrandTheme.Grab:
                    outfitName = "Grab";
                    vehicleName = "Xe máy Grab";
                    break;
                case IUHCustomizationManager.BrandTheme.Be:
                    outfitName = "be";
                    vehicleName = "Xe máy be";
                    break;
                case IUHCustomizationManager.BrandTheme.DefaultIUH:
                default:
                    outfitName = "Sinh Viên IUH";
                    vehicleName = "Xe máy Sinh Viên IUH";
                    break;
            }
        }
    }
}
