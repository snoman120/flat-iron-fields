using UnityEngine;

namespace FlatIronFields.Progression
{
    [CreateAssetMenu(fileName = "SkillDefinition", menuName = "FlatIronFields/Progression/SkillDefinition")]
    public sealed class SkillDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public int UnlockedAtLevel;
    }
}
