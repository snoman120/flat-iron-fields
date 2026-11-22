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
    public sealed class EconomyTests
    {
        private GameState state;
        private EconomySystem economy;
        private CropDefinition crop;
        private TractorDefinition tractor;
        private RecipeDefinition recipe;

        [SetUp]
        public void SetUp()
        {
            crop = ScriptableObject.CreateInstance<CropDefinition>();
            crop.Id = "cabbage";
            crop.DisplayName = "Cabbage";
            crop.Seasons = new[] { Season.Spring };
            crop.GrowthDays = 4;
            crop.YieldMin = 1;
            crop.YieldMax = 3;
            crop.SeedCost = 8;
            crop.SalePrice = 20;
            crop.UnlockedAtLevel = 1;
            CropDatabase.Initialize(new[] { crop });

            tractor = ScriptableObject.CreateInstance<TractorDefinition>();
            tractor.Id = "basic";
            tractor.DisplayName = "Basic Tractor";
            tractor.Speed = 3f;
            tractor.CargoCapacity = 10;
            tractor.Cost = 100;
            tractor.UnlockedAtLevel = 1;
            TractorDatabase.Initialize(new[] { tractor });

            recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            recipe.Id = "better_cabbage";
            recipe.InputCropId = "cabbage";
            recipe.InputQuality = Quality.Normal;
            recipe.InputQuantity = 2;
            recipe.OutputCropId = "cabbage";
            recipe.OutputQuality = Quality.Good;
            recipe.OutputQuantity = 1;
            recipe.CoinCost = 5;
            RecipeDatabase.Initialize(new[] { recipe });

            state = new GameState { Coins = 100 };
            var grid = new FarmGrid();
            state.Farm.Plots = grid.CreateFarm(0);
            economy = new EconomySystem(state);
            economy.SetFarmGrid(grid);
            economy.SetProgressionSystem(new ProgressionSystem(state));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(crop);
            Object.DestroyImmediate(tractor);
            Object.DestroyImmediate(recipe);
            CropDatabase.Clear();
            TractorDatabase.Clear();
            RecipeDatabase.Clear();
        }

        [Test]
        public void Price_ScalesWithQuality()
        {
            Assert.Less(economy.Price("cabbage", Quality.Poor), economy.Price("cabbage", Quality.Normal));
            Assert.Less(economy.Price("cabbage", Quality.Normal), economy.Price("cabbage", Quality.Good));
            Assert.Less(economy.Price("cabbage", Quality.Good), economy.Price("cabbage", Quality.Perfect));
        }

        [Test]
        public void Sell_RemovesInventoryAndAddsCoins()
        {
            state.Inventory.Crops["cabbage"] = new Dictionary<Quality, int> { { Quality.Normal, 3 } };
            int earned = economy.Sell("cabbage", 2, Quality.Normal);
            Assert.Greater(earned, 0);
            Assert.AreEqual(100 + earned, state.Coins);
            Assert.AreEqual(1, state.Inventory.Crops["cabbage"][Quality.Normal]);
        }

        [Test]
        public void BuySeed_SpendsCoinsAndGrantsSeed()
        {
            Assert.IsTrue(economy.BuySeed("cabbage", 2));
            Assert.AreEqual(84, state.Coins);
            Assert.AreEqual(2, state.Inventory.Seeds["cabbage"]);
        }

        [Test]
        public void BuyPlot_AdvancesExpansion()
        {
            Assert.IsTrue(economy.BuyPlot());
            Assert.AreEqual(1, state.Farm.ExpansionStage);
            Assert.AreEqual(10, state.Farm.Plots.Count);
            Assert.AreEqual(200, state.Coins);
        }

        [Test]
        public void Purchases_NeverAllowNegativeCoins()
        {
            state.Coins = 0;
            Assert.IsFalse(economy.BuySeed("cabbage", 1));
            Assert.IsFalse(economy.BuyPlot());
            Assert.IsFalse(economy.BuyTractor("basic"));
            Assert.AreEqual(0, state.Coins);
        }

        [Test]
        public void Craft_ConsumesInputsAndProducesOutput()
        {
            state.Inventory.Crops["cabbage"] = new Dictionary<Quality, int> { { Quality.Normal, 3 } };
            Assert.IsTrue(economy.Craft("better_cabbage"));
            Assert.AreEqual(1, state.Inventory.Crops["cabbage"][Quality.Normal]);
            Assert.AreEqual(1, state.Inventory.Crops["cabbage"][Quality.Good]);
            Assert.AreEqual(95, state.Coins);
        }
    }
}
