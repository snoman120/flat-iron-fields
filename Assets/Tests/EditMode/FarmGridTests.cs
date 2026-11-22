using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Farming;
using NUnit.Framework;
using UnityEngine;

namespace FlatIronFields.Tests
{
    public sealed class FarmGridTests
    {
        private CropDefinition crop;
        private FarmGrid grid;

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
            grid = new FarmGrid();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(crop);
            CropDatabase.Clear();
        }

        [Test]
        public void GetShape_ReturnsCanonicalShapes()
        {
            int[] plots = { 5, 10, 20, 30, 50 };
            int[] cols = { 5, 5, 10, 10, 10 };
            int[] rows = { 1, 2, 2, 3, 5 };
            int[] tileCols = { 10, 10, 20, 30, 50 };

            for (int i = 0; i < plots.Length; i++)
            {
                FarmShape shape = grid.GetShape(i);
                Assert.AreEqual(plots[i], shape.Plots);
                Assert.AreEqual(cols[i], shape.Cols);
                Assert.AreEqual(rows[i], shape.Rows);
                Assert.AreEqual(tileCols[i], shape.TileCols);
                Assert.AreEqual(20, shape.TileRows);
            }
        }

        [Test]
        public void CreateFarm_StageZeroHasFiveWildPlots()
        {
            List<Plot> plots = grid.CreateFarm(0);
            Assert.AreEqual(5, plots.Count);
            Assert.AreEqual(PlotState.Wild, plots[0].State);
            Assert.AreEqual(new Vector2Int(0, 0), plots[0].GridPosition);
        }

        [Test]
        public void ManualLoop_TillWaterPlantGrowHarvest()
        {
            Plot plot = grid.CreateFarm(0)[0];
            Assert.IsTrue(grid.Till(plot));
            Assert.AreEqual(PlotState.Tilled, plot.State);
            Assert.IsTrue(grid.Water(plot));
            Assert.IsTrue(grid.Plant(plot, "cabbage"));
            Assert.IsTrue(grid.Water(plot));

            var cropSystem = new CropSystem();
            Assert.IsTrue(cropSystem.Advance(plot, 4));
            Assert.IsTrue(plot.Ready);

            CropYield yield = grid.Harvest(plot);
            Assert.IsNotNull(yield);
            Assert.AreEqual("cabbage", yield.CropId);
            Assert.AreEqual(PlotState.Harvested, plot.State);
        }

        [Test]
        public void Plant_RejectsUnknownCropAndWrongState()
        {
            Plot plot = grid.CreateFarm(0)[0];
            Assert.IsFalse(grid.Plant(plot, "cabbage"));
            Assert.IsFalse(grid.Plant(plot, "unknown"));
            grid.Till(plot);
            Assert.IsFalse(grid.Plant(plot, "unknown"));
        }

        [Test]
        public void Expand_AcceptsValidStagesAndClampsInvalid()
        {
            Assert.IsTrue(grid.Expand(4));
            Assert.AreEqual(4, grid.CurrentStage);
            Assert.IsFalse(grid.Expand(5));
            Assert.IsFalse(grid.Expand(-1));
            Assert.AreEqual(5, grid.GetShape(-1).Plots);
            Assert.AreEqual(50, grid.GetShape(99).Plots);
        }
    }
}
