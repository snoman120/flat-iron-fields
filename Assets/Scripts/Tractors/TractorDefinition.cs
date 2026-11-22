using UnityEngine;

namespace FlatIronFields.Tractors
{
    [CreateAssetMenu(fileName = "TractorDefinition", menuName = "FlatIronFields/Tractors/TractorDefinition")]
    public sealed class TractorDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public float Speed;
        public int CargoCapacity;
        public int Cost;
        public int UnlockedAtLevel;
        public int FuelCapacity = 100;
    }
}
