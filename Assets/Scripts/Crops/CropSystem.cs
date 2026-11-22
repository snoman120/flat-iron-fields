using System.Collections.Generic;
using FlatIronFields.Core;
using UnityEngine;

namespace FlatIronFields.Crops
{
    public sealed class CropSystem
    {
        private static readonly List<CropGrowthStage> Stages = new List<CropGrowthStage>
        {
            new CropGrowthStage { Name = "Seed", Days = 1 },
            new CropGrowthStage { Name = "Sprout", Days = 1 },
            new CropGrowthStage { Name = "Grow", Days = 1 },
            new CropGrowthStage { Name = "Bloom", Days = 1 },
            new CropGrowthStage { Name = "Harvest", Days = 1 }
        };

        public bool IsInSeason(string cropId, Season season)
        {
            CropDefinition crop = CropDatabase.GetCrop(cropId);
            if (crop == null || crop.Seasons == null) { return false; }
            for (int i = 0; i < crop.Seasons.Length; i++)
            {
                if (crop.Seasons[i] == season) { return true; }
            }
            return false;
        }

        public IReadOnlyList<CropGrowthStage> GrowthStages()
        {
            return Stages;
        }

        public bool Advance(Plot plot, int days)
        {
            if (plot == null || days <= 0) { return false; }
            if (CropDatabase.GetCrop(plot.CropId) == null) { return false; }

            bool changed = false;
            for (int day = 0; day < days; day++)
            {
                if (plot.Ready || plot.State == PlotState.Ready) { break; }
                if (plot.Water <= 0) { break; }

                plot.Water--;
                plot.StageIndex = Mathf.Min(plot.StageIndex + 1, Stages.Count - 1);

                if (plot.StageIndex >= Stages.Count - 1)
                {
                    plot.State = PlotState.Ready;
                    plot.Ready = true;
                }
                else if (plot.State != PlotState.Planted)
                {
                    plot.State = PlotState.Planted;
                }
                changed = true;
            }
            return changed;
        }

        public Quality QualityFor(Plot plot)
        {
            if (plot == null) { return Quality.Poor; }

            int score = plot.QualityBonus;
            if (plot.Water >= 2) { score++; }
            score -= Mathf.Max(0, plot.Weeds);

            if (score <= 0) { return Quality.Poor; }
            if (score <= 2) { return Quality.Normal; }
            if (score <= 4) { return Quality.Good; }
            return Quality.Perfect;
        }

        public CropYield YieldFor(Plot plot)
        {
            var result = new CropYield();
            if (plot == null) { return result; }

            CropDefinition crop = CropDatabase.GetCrop(plot.CropId);
            if (crop == null) { return result; }

            int min = Mathf.Max(0, crop.YieldMin);
            int max = Mathf.Max(min, crop.YieldMax);
            int bonus = Mathf.Clamp(plot.QualityBonus, 0, 4);
            int quantity = Mathf.Clamp(min + bonus, min, max);

            result.CropId = crop.Id;
            result.Min = min;
            result.Max = max;
            result.Quality = QualityFor(plot);
            result.Quantity = quantity;
            return result;
        }
    }
}
