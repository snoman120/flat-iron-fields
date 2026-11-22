using UnityEngine;

namespace FlatIronFields.Tractors
{
    public enum TractorUpgradeStat { Speed, FuelEfficiency, CargoCapacity, Automation }

    [CreateAssetMenu(fileName = "TractorUpgradeDefinition", menuName = "FlatIronFields/Tractors/TractorUpgradeDefinition")]
    public sealed class TractorUpgradeDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public TractorUpgradeStat Stat;
        public int Value;
        public int Cost;
        public int UnlockedAtLevel = 1;
    }
}
