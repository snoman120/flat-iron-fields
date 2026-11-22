using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Economy;
using FlatIronFields.Progression;
using UnityEngine;

namespace FlatIronFields.Tractors
{
    /// <summary>
    /// Owns purchased tractor upgrades and exposes their aggregated stat modifiers.
    /// Purchases go through EconomySystem so currency never moves outside it.
    /// </summary>
    public sealed class TractorUpgradeSystem
    {
        private readonly GameState state;
        private EconomySystem economy;
        private ProgressionSystem progression;

        public TractorUpgradeSystem(GameState state)
        {
            this.state = state ?? new GameState();
        }

        public void SetEconomySystem(EconomySystem value) { economy = value; }
        public void SetProgressionSystem(ProgressionSystem value) { progression = value; }

        public IReadOnlyList<string> Owned { get { return state.Player.TractorUpgrades; } }

        public bool Has(string upgradeId)
        {
            return !string.IsNullOrEmpty(upgradeId) && state.Player.TractorUpgrades.Contains(upgradeId);
        }

        public bool CanBuy(string upgradeId)
        {
            TractorUpgradeDefinition upgrade = TractorUpgradeDatabase.Get(upgradeId);
            if (upgrade == null || Has(upgradeId)) { return false; }
            if (progression != null && progression.Level < upgrade.UnlockedAtLevel) { return false; }
            return economy != null && economy.CanAfford(upgrade.Cost);
        }

        public bool Buy(string upgradeId)
        {
            TractorUpgradeDefinition upgrade = TractorUpgradeDatabase.Get(upgradeId);
            if (upgrade == null || Has(upgradeId)) { return false; }
            if (progression != null && progression.Level < upgrade.UnlockedAtLevel) { return false; }
            if (economy == null || !economy.TrySpend(upgrade.Cost)) { return false; }

            state.Player.TractorUpgrades.Add(upgradeId);
            if (progression != null) { progression.AddXp(15, "tractor_upgrade"); }
            return true;
        }

        public float SpeedMultiplier()
        {
            float percent = SumPercent(TractorUpgradeStat.Speed);
            return 1f + percent / 100f;
        }

        public float FuelEfficiencyMultiplier()
        {
            float percent = SumPercent(TractorUpgradeStat.FuelEfficiency);
            return Mathf.Max(0.25f, 1f - percent / 100f);
        }

        public int CargoBonus()
        {
            return SumFlat(TractorUpgradeStat.CargoCapacity);
        }

        public int AutomationLevel()
        {
            return SumFlat(TractorUpgradeStat.Automation);
        }

        private int SumFlat(TractorUpgradeStat stat)
        {
            int total = 0;
            foreach (string id in state.Player.TractorUpgrades)
            {
                TractorUpgradeDefinition upgrade = TractorUpgradeDatabase.Get(id);
                if (upgrade != null && upgrade.Stat == stat) { total += upgrade.Value; }
            }
            return total;
        }

        private int SumPercent(TractorUpgradeStat stat)
        {
            return SumFlat(stat);
        }
    }
}
