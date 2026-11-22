using FlatIronFields.Core;
using UnityEngine;

namespace FlatIronFields.Economy
{
    [CreateAssetMenu(fileName = "RecipeDefinition", menuName = "FlatIronFields/Economy/RecipeDefinition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string InputCropId;
        public Quality InputQuality;
        public int InputQuantity;
        public string OutputCropId;
        public Quality OutputQuality;
        public int OutputQuantity;
        public int CoinCost;
    }
}
