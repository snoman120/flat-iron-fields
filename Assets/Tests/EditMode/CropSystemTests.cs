using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using NUnit.Framework;
using UnityEngine;

namespace FlatIronFields.Tests
{
    public sealed class CropSystemTests
    {
        private readonly List<CropDefinition> crops = new List<CropDefinition>();
        private CropSystem system;

        [SetUp]
        public void SetUp()
        {
            crops.Clear();
            crops.Add(MakeCrop("cabbage", new[] { Season.Spring, Season.Autumn }, 8, 1, 3, 18));
            crops.Add(MakeCrop("wheat", new[] { Season.Summer }, 10, 2, 4, 22));
            CropDatabase.Initialize(crops);
            system = new CropSystem();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (CropDefinition crop in crops) { Object.DestroyImmediate(crop); }
            CropDatabase.Clear();
        }

        [Test]
        public void GrowthStages_HasFiveExpectedStages()
        {
            IReadOnlyList<CropGrowthStage> stages = system.GrowthStages();
            Assert.AreEqual(5, stages.Count);
            Assert.AreEqual("Seed", stages[0].Name);
            Assert.AreEqual("Harvest", stages[4].Name);
        }

        [Test]
        public void IsInSeason_AcceptsValidAndRejectsInvalid()
        {
            Assert.IsTrue(system.IsInSeason("cabbage", Season.Spring));
            Assert.IsFalse(system.IsInSeason("cabbage", Season.Summer));
            Assert.IsFalse(system.IsInSeason("missing", Season.Spring));
        }

        [Test]
        public void Advance_RequiresWaterAndReachesReady()
        {
            var plot = new Plot { CropId = "cabbage", Water = 0, StageIndex = 0 };
            Assert.IsFalse(system.Advance(plot, 1));

            plot.Water = 4;
            Assert.IsTrue(system.Advance(plot, 1));
            Assert.AreEqual(1, plot.StageIndex);

            Assert.IsTrue(system.Advance(plot, 3));
            Assert.AreEqual(4, plot.StageIndex);
            Assert.IsTrue(plot.Ready);
            Assert.AreEqual(PlotState.Ready, plot.State);
        }

        [Test]
        public void QualityFor_UsesBonusAndWeeds()
        {
            var plot = new Plot { QualityBonus = 0, Weeds = 0, Water = 0 };
            Assert.AreEqual(Quality.Poor, system.QualityFor(plot));

            plot.QualityBonus = 2;
            Assert.AreEqual(Quality.Normal, system.QualityFor(plot));

            plot.QualityBonus = 4;
            plot.Water = 2;
            Assert.AreEqual(Quality.Perfect, system.QualityFor(plot));

            plot.Weeds = 5;
            Assert.AreEqual(Quality.Poor, system.QualityFor(plot));
        }

        [Test]
        public void YieldFor_UsesCropDefinitionAndQuality()
        {
            var plot = new Plot { CropId = "cabbage", QualityBonus = 2, Water = 2, Weeds = 0 };
            CropYield yield = system.YieldFor(plot);
            Assert.AreEqual("cabbage", yield.CropId);
            Assert.AreEqual(Quality.Good, yield.Quality);
            Assert.GreaterOrEqual(yield.Quantity, 1);
            Assert.LessOrEqual(yield.Quantity, 3);
        }

        private static CropDefinition MakeCrop(string id, Season[] seasons, int salePrice, int min, int max, int seedCost)
        {
            var crop = ScriptableObject.CreateInstance<CropDefinition>();
            crop.Id = id;
            crop.DisplayName = id;
            crop.Seasons = seasons;
            crop.GrowthDays = 5;
            crop.YieldMin = min;
            crop.YieldMax = max;
            crop.SeedCost = seedCost;
            crop.SalePrice = salePrice;
            crop.UnlockedAtLevel = 1;
            return crop;
        }
    }
}
