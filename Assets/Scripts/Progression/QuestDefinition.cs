using UnityEngine;

namespace FlatIronFields.Progression
{
    public enum QuestGoal { Harvest, Sell, Expand, Attachments }

    [CreateAssetMenu(fileName = "QuestDefinition", menuName = "FlatIronFields/Progression/QuestDefinition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public QuestGoal Goal;
        public int TargetCount;
        public int CoinReward;
        public int XpReward;
    }
}
