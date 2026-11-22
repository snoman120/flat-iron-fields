using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Progression;
using FlatIronFields.Tractors;
using TMPro;
using UnityEngine;

namespace FlatIronFields.UI
{
    public enum MenuId { Seeds, Sell, Land, Tractor, Skills, Quests, Upgrades, Help }

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

        private string MenuContents(MenuId menu)
        {
            switch (menu)
            {
                case MenuId.Seeds: return "SEEDS\nQ/E: select crop\nBuy seeds from the shop panel.";
                case MenuId.Sell: return "SELL\nHarvest crops, then sell by quality.";
                case MenuId.Land: return "LAND\nBuy land to expand the farm.";
                case MenuId.Tractor: return "TRACTOR\nT cycles attachment. WASD drives.";
                case MenuId.Skills: return "SKILLS\nInvest points at level up.";
                case MenuId.Quests: return BuildQuestContents();
                case MenuId.Upgrades: return BuildUpgradeContents();
                case MenuId.Help: return "HELP\nWASD drive | Space use tool | 1 Shovel | 2 Water | 3 Rake/Plant | 4 Hoe | 5 Scythe | 6 Cart\nQ/E crop | T attachment | N next day | B/U/M/V/K menus | J quests | R upgrades";
                default: return string.Empty;
            }
        }

        private string BuildQuestContents()
        {
            var lines = new List<string> { "QUESTS" };
            foreach (QuestDefinition quest in QuestDatabase.All())
            {
                if (quest == null) { continue; }
                int progress = state != null && state.Player != null && state.Player.QuestProgress != null && state.Player.QuestProgress.ContainsKey(quest.Id)
                    ? state.Player.QuestProgress[quest.Id] : 0;
                int target = Mathf.Max(1, quest.TargetCount);
                string status = state != null && state.Player != null && state.Player.ClaimedQuests.Contains(quest.Id)
                    ? "CLAIMED" : progress >= target ? "READY" : "ACTIVE";
                lines.Add(quest.DisplayName + " " + Mathf.Min(progress, target) + "/" + target + " [" + status + "]");
            }
            if (lines.Count == 1) { lines.Add("No quests available."); }
            return string.Join("\n", lines);
        }

        private string BuildUpgradeContents()
        {
            var lines = new List<string> { "TRACTOR UPGRADES" };
            foreach (TractorUpgradeDefinition upgrade in TractorUpgradeDatabase.All())
            {
                if (upgrade == null) { continue; }
                bool owned = state != null && state.Player != null && state.Player.TractorUpgrades.Contains(upgrade.Id);
                lines.Add(upgrade.DisplayName + " [" + (owned ? "OWNED" : upgrade.Cost + "c") + "]");
            }
            if (lines.Count == 1) { lines.Add("No upgrades available."); }
            return string.Join("\n", lines);
        }

        private static void Set(TextMeshProUGUI text, string value)
        {
            if (text != null) { text.text = value; }
        }
    }
}
