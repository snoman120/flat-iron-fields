using System.Collections.Generic;
using UnityEngine;

namespace FlatIronFields.Tractors
{
    public static class TractorUpgradeDatabase
    {
        private static readonly List<TractorUpgradeDefinition> Fallback = new List<TractorUpgradeDefinition>
        {
            Make("speed_1", "Speed Tune I", TractorUpgradeStat.Speed, 20, 120, 1),
            Make("speed_2", "Speed Tune II", TractorUpgradeStat.Speed, 35, 260, 3),
            Make("fuel_1", "Fuel Economy I", TractorUpgradeStat.FuelEfficiency, 20, 130, 1),
            Make("fuel_2", "Fuel Economy II", TractorUpgradeStat.FuelEfficiency, 35, 270, 4),
            Make("cargo_1", "Cargo Rack I", TractorUpgradeStat.CargoCapacity, 8, 140, 2),
            Make("cargo_2", "Cargo Rack II", TractorUpgradeStat.CargoCapacity, 16, 280, 5),
            Make("auto_1", "Auto Steer", TractorUpgradeStat.Automation, 1, 300, 6)
        };

        private static List<TractorUpgradeDefinition> definitions = new List<TractorUpgradeDefinition>();

        public static void Initialize(IEnumerable<TractorUpgradeDefinition> assets = null)
        {
            definitions = new List<TractorUpgradeDefinition>();
            if (assets != null)
            {
                foreach (TractorUpgradeDefinition asset in assets)
                {
                    if (asset != null && !string.IsNullOrEmpty(asset.Id)) { definitions.Add(asset); }
                }
            }
            if (definitions.Count == 0) { definitions = new List<TractorUpgradeDefinition>(Fallback); }
        }

        public static void Clear()
        {
            definitions = new List<TractorUpgradeDefinition>();
        }

        public static TractorUpgradeDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) { return null; }
            foreach (TractorUpgradeDefinition upgrade in definitions)
            {
                if (upgrade != null && upgrade.Id == id) { return upgrade; }
            }
            return null;
        }

        public static IReadOnlyList<TractorUpgradeDefinition> All()
        {
            return definitions;
        }

        private static TractorUpgradeDefinition Make(string id, string display, TractorUpgradeStat stat, int value, int cost, int level)
        {
            var upgrade = ScriptableObject.CreateInstance<TractorUpgradeDefinition>();
            upgrade.Id = id;
            upgrade.DisplayName = display;
            upgrade.Stat = stat;
            upgrade.Value = value;
            upgrade.Cost = cost;
            upgrade.UnlockedAtLevel = level;
            return upgrade;
        }
    }
}
