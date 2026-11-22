using UnityEngine;

namespace FlatIronFields.Tractors
{
    [CreateAssetMenu(fileName = "AttachmentDefinition", menuName = "FlatIronFields/Tractors/AttachmentDefinition")]
    public sealed class AttachmentDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public int FuelCost = 1;
    }
}
