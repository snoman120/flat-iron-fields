using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using UnityEngine;

namespace FlatIronFields.Farming
{
    public sealed class FarmGrid
    {
        private static readonly FarmShape[] Shapes =
        {
            new FarmShape { Plots = 5,  Cols = 5,  Rows = 1, TileCols = 10, TileRows = 20 },
            new FarmShape { Plots = 10, Cols = 5,  Rows = 2, TileCols = 10, TileRows = 20 },
            new FarmShape { Plots = 20, Cols = 10, Rows = 2, TileCols = 20, TileRows = 20 },
            new FarmShape { Plots = 30, Cols = 10, Rows = 3, TileCols = 30, TileRows = 20 },
            new FarmShape { Plots = 50, Cols = 10, Rows = 5, TileCols = 50, TileRows = 20 }
        };

        private readonly CropSystem cropSystem = new CropSystem();

        public int CurrentStage { get; private set; }

        public FarmShape GetShape(int expansionStage)
        {
            int index = Mathf.Clamp(expansionStage, 0, Shapes.Length - 1);
            return Shapes[index];
        }

        public List<Plot> CreateFarm(int expansionStage)
        {
            FarmShape shape = GetShape(expansionStage);
            CurrentStage = Mathf.Clamp(expansionStage, 0, Shapes.Length - 1);
            var plots = new List<Plot>();
            for (int i = 0; i < shape.Plots; i++)
            {
                plots.Add(new Plot
                {
                    Id = i + 1,
                    GridPosition = new Vector2Int(i % shape.Cols, i / shape.Cols),
                    Type = PlotType.Regular,
                    State = PlotState.Wild
                });
            }
            return plots;
        }

        public bool Expand(int expansionStage)
        {
            if (expansionStage < 0 || expansionStage >= Shapes.Length) { return false; }
            CurrentStage = expansionStage;
            return true;
        }

        public bool Till(Plot plot)
        {
            if (plot == null || plot.State != PlotState.Wild) { return false; }
            plot.State = PlotState.Tilled;
            plot.Weeds = 0;
            return true;
        }

        public bool Cultivate(Plot plot)
        {
            if (plot == null || plot.State != PlotState.Tilled) { return false; }
            plot.State = PlotState.Cultivated;
            return true;
        }

        public bool ClearWeeds(Plot plot)
        {
            if (plot == null || plot.Weeds <= 0) { return false; }
            plot.Weeds = 0;
            return true;
        }

        public bool Water(Plot plot)
        {
            if (plot == null || plot.State == PlotState.Wild) { return false; }
            plot.Water = Mathf.Min(4, plot.Water + 2);
            if (plot.State == PlotState.Tilled || plot.State == PlotState.Cultivated)
            {
                plot.State = PlotState.Watered;
            }
            return true;
        }

        public bool Plant(Plot plot, string cropId)
        {
            if (plot == null || string.IsNullOrEmpty(cropId)) { return false; }
            if (CropDatabase.GetCrop(cropId) == null) { return false; }
            if (plot.State != PlotState.Tilled && plot.State != PlotState.Cultivated && plot.State != PlotState.Watered)
            {
                return false;
            }

            plot.CropId = cropId;
            plot.StageIndex = 0;
            plot.State = PlotState.Planted;
            plot.Ready = false;
            plot.Weeds = 0;
            return true;
        }

        public bool Fertilize(Plot plot)
        {
            if (plot == null || plot.State == PlotState.Wild) { return false; }
            plot.QualityBonus = Mathf.Min(4, plot.QualityBonus + 1);
            return true;
        }

        public CropYield Harvest(Plot plot)
        {
            if (plot == null || !plot.Ready || plot.State != PlotState.Ready) { return null; }
            CropYield result = cropSystem.YieldFor(plot);
            plot.State = PlotState.Harvested;
            plot.Ready = false;
            plot.StageIndex = 0;
            return result;
        }
    }
}
