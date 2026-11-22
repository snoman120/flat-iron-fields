using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Economy;
using UnityEngine;

namespace FlatIronFields.Progression
{
    /// <summary>
    /// Tracks quest progress from normal gameplay events and pays rewards through
    /// the economy and progression services.
    /// </summary>
    public sealed class QuestSystem
    {
        private readonly GameState state;
        private EconomySystem economy;
        private ProgressionSystem progression;

        public QuestSystem(GameState state)
        {
            this.state = state ?? new GameState();
        }

        public void SetEconomySystem(EconomySystem value) { economy = value; }
        public void SetProgressionSystem(ProgressionSystem value) { progression = value; }

        public IReadOnlyList<QuestDefinition> Active
        {
            get
            {
                var result = new List<QuestDefinition>();
                foreach (QuestDefinition quest in QuestDatabase.All())
                {
                    if (quest != null && !IsClaimed(quest.Id)) { result.Add(quest); }
                }
                return result;
            }
        }

        public int Progress(string questId)
        {
            int value;
            if (string.IsNullOrEmpty(questId) || state.Player.QuestProgress == null) { return 0; }
            return state.Player.QuestProgress.TryGetValue(questId, out value) ? value : 0;
        }

        public bool IsComplete(string questId)
        {
            QuestDefinition quest = QuestDatabase.Get(questId);
            if (quest == null) { return false; }
            return Progress(questId) >= Mathf.Max(1, quest.TargetCount);
        }

        public bool IsClaimed(string questId)
        {
            return !string.IsNullOrEmpty(questId) && state.Player.ClaimedQuests.Contains(questId);
        }

        public void Record(QuestGoal goal, int amount)
        {
            if (amount <= 0 || state.Player.QuestProgress == null) { return; }
            foreach (QuestDefinition quest in QuestDatabase.All())
            {
                if (quest == null || quest.Goal != goal || IsClaimed(quest.Id)) { continue; }
                int current = Progress(quest.Id);
                if (current >= quest.TargetCount) { continue; }
                state.Player.QuestProgress[quest.Id] = current + amount;
            }
        }

        public bool Claim(string questId)
        {
            QuestDefinition quest = QuestDatabase.Get(questId);
            if (quest == null || IsClaimed(questId) || !IsComplete(questId)) { return false; }

            state.Player.ClaimedQuests.Add(questId);
            if (economy != null && quest.CoinReward > 0) { economy.AddReward(quest.CoinReward); }
            if (progression != null && quest.XpReward > 0) { progression.AddXp(quest.XpReward, "quest"); }
            return true;
        }
    }
}
