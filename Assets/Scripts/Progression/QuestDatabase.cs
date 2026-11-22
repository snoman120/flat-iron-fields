using System.Collections.Generic;
using FlatIronFields.Core;
using UnityEngine;

namespace FlatIronFields.Progression
{
    public static class QuestDatabase
    {
        private static readonly List<QuestDefinition> Fallback = new List<QuestDefinition>
        {
            Make("first_harvest", "First Harvest", "Harvest 3 crops.", QuestGoal.Harvest, 3, 40, 20),
            Make("market_day", "Market Day", "Sell 5 crops.", QuestGoal.Sell, 5, 60, 30),
            Make("homestead", "Homestead", "Expand the farm once.", QuestGoal.Expand, 1, 100, 40),
            Make("mechanised", "Mechanised", "Use 6 tractor attachment actions.", QuestGoal.Attachments, 6, 120, 50)
        };

        private static List<QuestDefinition> definitions = new List<QuestDefinition>();

        public static void Initialize(IEnumerable<QuestDefinition> assets = null)
        {
            definitions = new List<QuestDefinition>();
            if (assets != null)
            {
                foreach (QuestDefinition asset in assets)
                {
                    if (asset != null && !string.IsNullOrEmpty(asset.Id)) { definitions.Add(asset); }
                }
            }
            if (definitions.Count == 0) { definitions = new List<QuestDefinition>(Fallback); }
        }

        public static void Clear() { definitions = new List<QuestDefinition>(); }

        public static QuestDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) { return null; }
            foreach (QuestDefinition quest in definitions)
            {
                if (quest != null && quest.Id == id) { return quest; }
            }
            return null;
        }

        public static IReadOnlyList<QuestDefinition> All() { return definitions; }

        private static QuestDefinition Make(string id, string display, string description, QuestGoal goal, int target, int coins, int xp)
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.Id = id;
            quest.DisplayName = display;
            quest.Description = description;
            quest.Goal = goal;
            quest.TargetCount = target;
            quest.CoinReward = coins;
            quest.XpReward = xp;
            return quest;
        }
    }
}
