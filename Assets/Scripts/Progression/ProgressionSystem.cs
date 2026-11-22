using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Tractors;
using UnityEngine;

namespace FlatIronFields.Progression
{
    public sealed class ProgressionSystem
    {
        private static readonly int[] Thresholds = { 0, 100, 250, 500, 900, 1500 };
        private static readonly string[] LevelNames = { "Noob", "Beginner", "Intermediate", "Advanced", "Expert", "Master" };

        private readonly GameState state;

        public ProgressionSystem(GameState state)
        {
            this.state = state ?? new GameState();
        }

        public int Level { get { return state.Player.Level; } }

        public static string LevelName(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, LevelNames.Length - 1);
            return LevelNames[index];
        }

        public int LevelForXp(int xp)
        {
            int level = 1;
            for (int i = 1; i < Thresholds.Length; i++)
            {
                if (xp >= Thresholds[i]) { level = i + 1; }
            }
            return level;
        }

        public void AddXp(int amount, string reason)
        {
            if (amount <= 0 || state.Player == null) { return; }
            state.Player.ExperiencePoints += amount;

            int newLevel = LevelForXp(state.Player.ExperiencePoints);
            if (newLevel > state.Player.Level)
            {
                int gained = newLevel - state.Player.Level;
                state.Player.Level = newLevel;
                state.Player.SkillPoints += gained;
                Message("Level up: " + LevelName(newLevel) + "! +" + gained + " skill point.");
            }
            CheckUnlocks();
        }

        public bool InvestSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || state.Player.SkillPoints <= 0) { return false; }
            SkillDefinition skill = SkillDatabase.GetSkill(skillId);
            if (skill == null) { return false; }
            if (state.Player.Skills.Contains(skillId)) { return false; }

            state.Player.SkillPoints--;
            state.Player.Skills.Add(skillId);
            Message("Invested skill: " + skill.DisplayName);
            return true;
        }

        public bool HasSkill(string skillId)
        {
            return !string.IsNullOrEmpty(skillId) && state.Player.Skills.Contains(skillId);
        }

        public void CheckUnlocks()
        {
            foreach (CropDefinition crop in CropDatabase.All())
            {
                if (crop == null || string.IsNullOrEmpty(crop.Id)) { continue; }
                if (state.Player.Level < crop.UnlockedAtLevel) { continue; }
                string key = "crop:" + crop.Id;
                if (!state.Player.UnlockedIds.Contains(key))
                {
                    state.Player.UnlockedIds.Add(key);
                    Message("Unlocked crop: " + crop.DisplayName);
                }
            }

            foreach (TractorDefinition tractor in TractorDatabase.All())
            {
                if (tractor == null || string.IsNullOrEmpty(tractor.Id)) { continue; }
                if (state.Player.Level < tractor.UnlockedAtLevel) { continue; }
                string key = "tractor:" + tractor.Id;
                if (!state.Player.UnlockedIds.Contains(key))
                {
                    state.Player.UnlockedIds.Add(key);
                    Message("Unlocked tractor: " + tractor.DisplayName);
                }
            }
        }

        private void Message(string text)
        {
            if (state.Messages == null) { state.Messages = new List<string>(); }
            state.Messages.Add(text);
        }
    }
}
