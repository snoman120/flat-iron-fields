using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Economy;
using FlatIronFields.Farming;
using FlatIronFields.Progression;
using FlatIronFields.Tractors;
using NUnit.Framework;
using UnityEngine;

namespace FlatIronFields.Tests
{
    public sealed class Phase2Tests
    {
        private CropDefinition crop;
        private GameState state;
        private EconomySystem economy;
        private ProgressionSystem progression;
        private TractorUpgradeSystem upgrades;
        private QuestSystem quests;

        [SetUp]
        public void SetUp()
        {
            crop = ScriptableObject.CreateInstance<CropDefinition>();
            crop.Id = "cabbage";
            crop.DisplayName = "Cabbage";
            crop.Seasons = new[] { Season.Spring };
            crop.GrowthDays = 5;
            crop.YieldMin = 1;
            crop.YieldMax = 3;
            crop.SeedCost = 8;
            crop.SalePrice = 18;
            crop.UnlockedAtLevel = 1;
            CropDatabase.Initialize(new[] { crop });
            TractorUpgradeDatabase.Initialize();
            QuestDatabase.Initialize();

            state = new GameState { Coins = 1000 };
            economy = new EconomySystem(state);
            progression = new ProgressionSystem(state);
            economy.SetProgressionSystem(progression);

            upgrades = new TractorUpgradeSystem(state);
            upgrades.SetEconomySystem(economy);
            upgrades.SetProgressionSystem(progression);

            quests = new QuestSystem(state);
            quests.SetEconomySystem(economy);
            quests.SetProgressionSystem(progression);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(crop);
            CropDatabase.Clear();
            TractorUpgradeDatabase.Clear();
            QuestDatabase.Clear();
        }

        [Test]
        public void Weather_DailyWaterDeltaAndWinterDormancy()
        {
            var weather = new WeatherSystem();
            Assert.AreEqual(-1, weather.DailyWaterDelta(Weather.Sunny));
            Assert.AreEqual(0, weather.DailyWaterDelta(Weather.Cloudy));
            Assert.AreEqual(2, weather.DailyWaterDelta(Weather.Rainy));
            Assert.IsTrue(weather.CanGrow(Season.Spring, 1));
            Assert.IsFalse(weather.CanGrow(Season.Winter, 1));
            Assert.IsTrue(weather.CanGrow(Season.Winter, 2));
        }

        [Test]
        public void Weather_RainWatersAndGrowsPlantedPlot()
        {
            var grid = new FarmGrid();
            Plot plot = MakePlantedPlot(grid);
            state.Farm.Plots = new List<Plot> { plot };
            state.Weather = Weather.Rainy;

            new WeatherSystem().ApplyDay(state, grid, new CropSystem());

            Assert.AreEqual(1, plot.Water);
            Assert.AreEqual(1, plot.StageIndex);
        }

        [Test]
        public void Weather_DryingSunStopsGrowth()
        {
            var grid = new FarmGrid();
            Plot plot = MakePlantedPlot(grid);
            plot.Water = 1;
            state.Farm.Plots = new List<Plot> { plot };
            state.Weather = Weather.Sunny;

            new WeatherSystem().ApplyDay(state, grid, new CropSystem());

            Assert.AreEqual(0, plot.Water);
            Assert.AreEqual(0, plot.StageIndex);
        }

        [Test]
        public void TractorUpgrades_PurchaseAppliesStatModifiers()
        {
            Assert.IsTrue(upgrades.Buy("speed_1"));
            Assert.AreEqual(1.2f, upgrades.SpeedMultiplier(), 0.001f);

            Assert.IsTrue(upgrades.Buy("fuel_1"));
            Assert.AreEqual(0.8f, upgrades.FuelEfficiencyMultiplier(), 0.001f);

            progression.AddXp(100, "test");
            Assert.IsTrue(upgrades.Buy("cargo_1"));
            Assert.AreEqual(8, upgrades.CargoBonus());
        }

        [Test]
        public void TractorUpgrades_RejectDuplicateAndUnaffordable()
        {
            state.Coins = 0;
            Assert.IsFalse(upgrades.Buy("speed_1"));

            state.Coins = 1000;
            Assert.IsTrue(upgrades.Buy("speed_1"));
            Assert.IsFalse(upgrades.Buy("speed_1"));
            Assert.AreEqual(1, upgrades.Owned.Count);
        }

        [Test]
        public void Quests_ProgressAndClaimAwardsRewards()
        {
            quests.Record(QuestGoal.Harvest, 3);
            Assert.IsTrue(quests.IsComplete("first_harvest"));

            int coins = state.Coins;
            int xp = state.Player.ExperiencePoints;
            Assert.IsTrue(quests.Claim("first_harvest"));
            Assert.AreEqual(coins + 40, state.Coins);
            Assert.AreEqual(xp + 20, state.Player.ExperiencePoints);
            Assert.IsFalse(quests.Claim("first_harvest"));
        }

        [Test]
        public void Quests_IncompleteCannotBeClaimed()
        {
            quests.Record(QuestGoal.Sell, 2);
            Assert.IsFalse(quests.IsComplete("market_day"));
            Assert.IsFalse(quests.Claim("market_day"));
        }

        private Plot MakePlantedPlot(FarmGrid grid)
        {
            Plot plot = grid.CreateFarm(0)[0];
            Assert.IsTrue(grid.Till(plot));
            Assert.IsTrue(grid.Plant(plot, "cabbage"));
            return plot;
        }
    }
}
