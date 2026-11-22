using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Progression;
using FlatIronFields.Tractors;
using NUnit.Framework;
using UnityEngine;

namespace FlatIronFields.Tests
{
    public sealed class ProgressionTests
    {
        private GameState state;
        private ProgressionSystem progression;
        private CropDefinition crop;
        private SkillDefinition skill;
        private TractorDefinition tractor;

        [SetUp]
        public void SetUp()
        {
            crop = ScriptableObject.CreateInstance<CropDefinition>();
            crop.Id = "cabbage";
            crop.DisplayName = "Cabbage";
            crop.Seasons = new[] { Season.Spring };
            crop.UnlockedAtLevel = 1;

            skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.Id = "planting";
            skill.DisplayName = "Planting";
            skill.UnlockedAtLevel = 1;

            tractor = ScriptableObject.CreateInstance<TractorDefinition>();
            tractor.Id = "basic";
            tractor.DisplayName = "Basic Tractor";
            tractor.UnlockedAtLevel = 1;

            CropDatabase.Initialize(new[] { crop });
            SkillDatabase.Initialize(new[] { skill });
            TractorDatabase.Initialize(new[] { tractor });

            state = new GameState();
            progression = new ProgressionSystem(state);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(crop);
            Object.DestroyImmediate(skill);
            Object.DestroyImmediate(tractor);
            CropDatabase.Clear();
            SkillDatabase.Clear();
            TractorDatabase.Clear();
        }

        [Test]
        public void LevelForXp_UsesSixThresholds()
        {
            Assert.AreEqual(1, progression.LevelForXp(0));
            Assert.AreEqual(2, progression.LevelForXp(100));
            Assert.AreEqual(3, progression.LevelForXp(250));
            Assert.AreEqual(4, progression.LevelForXp(500));
            Assert.AreEqual(5, progression.LevelForXp(900));
            Assert.AreEqual(6, progression.LevelForXp(1500));
        }

        [Test]
        public void AddXp_GrantsSkillPointOnLevelUp()
        {
            progression.AddXp(100, "test");
            Assert.AreEqual(2, state.Player.Level);
            Assert.AreEqual(1, state.Player.SkillPoints);
            Assert.AreEqual("Beginner", ProgressionSystem.LevelName(state.Player.Level));
        }

        [Test]
        public void InvestSkill_ConsumesPointAndAddsSkill()
        {
            progression.AddXp(100, "test");
            Assert.IsTrue(progression.InvestSkill("planting"));
            Assert.AreEqual(0, state.Player.SkillPoints);
            Assert.IsTrue(state.Player.Skills.Contains("planting"));
            Assert.IsFalse(progression.InvestSkill("planting"));
        }

        [Test]
        public void CheckUnlocks_RegistersCropsAndTractors()
        {
            progression.CheckUnlocks();
            Assert.IsTrue(state.Player.UnlockedIds.Contains("crop:cabbage"));
            Assert.IsTrue(state.Player.UnlockedIds.Contains("tractor:basic"));
        }
    }
}
