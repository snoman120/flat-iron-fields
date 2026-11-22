using FlatIronFields.Core;
using UnityEngine;

namespace FlatIronFields.Crops
{
    [CreateAssetMenu(fileName = "CropDefinition", menuName = "FlatIronFields/Crops/CropDefinition")]
    public sealed class CropDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Season[] Seasons;
        public int GrowthDays;
        public int YieldMin;
        public int YieldMax;
        public int SeedCost;
        public int SalePrice;
        public bool MultipleHarvest;
        public int UnlockedAtLevel;
    }
}
