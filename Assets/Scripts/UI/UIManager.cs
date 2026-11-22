using FlatIronFields.Core;
using TMPro;
using UnityEngine;

namespace FlatIronFields.UI
{
    public enum MenuId { Seeds, Sell, Land, Tractor, Skills, Help }

    public sealed class UIManager : MonoBehaviour
    {
        public TextMeshProUGUI DayText;
        public TextMeshProUGUI SeasonText;
        public TextMeshProUGUI WeatherText;
        public TextMeshProUGUI CoinsText;
        public TextMeshProUGUI XpText;
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI SkillPointsText;
        public TextMeshProUGUI CropStatusText;
        public TextMeshProUGUI TractorStatusText;
        public TextMeshProUGUI MessageText;
        public TextMeshProUGUI MenuText;
        public GameObject MenuPanel;

        private GameState state;
        private string lastMessage = string.Empty;

        public void Initialize(GameState value)
        {
            state = value;
            CloseMenu();
            RenderHud(state);
        }

        public void RenderHud(GameState value)
        {
            if (value != null) { state = value; }
            if (state == null) { return; }

            Set(DayText, "Day: " + state.Day);
            Set(SeasonText, "Season: " + state.Season);
            Set(WeatherText, "Weather: " + state.Weather);
            Set(CoinsText, "Coins: " + state.Coins);
            Set(XpText, "XP: " + (state.Player != null ? state.Player.ExperiencePoints : 0));
            Set(LevelText, "Level: " + (state.Player != null ? state.Player.Level : 1));
            Set(SkillPointsText, "Skill Points: " + (state.Player != null ? state.Player.SkillPoints : 0));

            Set(CropStatusText, BuildCropStatus());
            Set(TractorStatusText, BuildTractorStatus());
            Set(MessageText, lastMessage);
        }

        public void OpenMenu(MenuId menu)
        {
            if (MenuPanel != null) { MenuPanel.SetActive(true); }
            Set(MenuText, MenuContents(menu));
        }

        public void CloseMenu()
        {
            if (MenuPanel != null) { MenuPanel.SetActive(false); }
            Set(MenuText, string.Empty);
        }

        public void Message(string text)
        {
            if (string.IsNullOrEmpty(text)) { return; }
            lastMessage = text;
            Set(MessageText, text);
        }

        private string BuildCropStatus()
        {
            if (state.Farm == null || state.Farm.Plots == null || state.Farm.Plots.Count == 0)
            {
                return "Farm: no plots";
            }
            Plot plot = state.Farm.Plots[0];
            return "Crop: " + (string.IsNullOrEmpty(plot.CropId) ? state.SelectedCropId : plot.CropId)
                   + " | Stage: " + plot.StageIndex
                   + " | State: " + plot.State
                   + " | Water: " + plot.Water
                   + (plot.Ready ? " | READY" : string.Empty);
        }

        private string BuildTractorStatus()
        {
            if (state.Tractor == null) { return "Tractor: none"; }
            return "Tractor: " + (string.IsNullOrEmpty(state.Tractor.TractorId) ? "none" : state.Tractor.TractorId)
                   + " | Fuel: " + state.Tractor.Fuel
                   + " | Cargo: " + state.Tractor.CargoCount
                   + " | Attachment: " + state.Tractor.SelectedAttachmentId;
        }

        private static string MenuContents(MenuId menu)
        {
            switch (menu)
            {
                case MenuId.Seeds: return "SEEDS\nQ/E: select crop\nBuy seeds from the shop panel.";
                case MenuId.Sell: return "SELL\nHarvest crops, then sell by quality.";
                case MenuId.Land: return "LAND\nBuy land to expand the farm.";
                case MenuId.Tractor: return "TRACTOR\nT cycles attachment. WASD drives.";
                case MenuId.Skills: return "SKILLS\nInvest points at level up.";
                case MenuId.Help: return "HELP\nWASD drive | Space use tool | 1 Shovel | 2 Water | 3 Rake/Plant | 4 Hoe | 5 Scythe | 6 Cart\nQ/E crop | T attachment | N next day | B/U/M/V/K menus";
                default: return string.Empty;
            }
        }

        private static void Set(TextMeshProUGUI text, string value)
        {
            if (text != null) { text.text = value; }
        }
    }
}
