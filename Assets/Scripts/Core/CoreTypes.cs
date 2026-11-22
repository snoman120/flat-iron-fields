using System.Collections.Generic;
using UnityEngine;

namespace FlatIronFields.Core
{
    public enum Season { Spring, Summer, Autumn, Winter }

    public enum Weather { Sunny, Rainy, Cloudy }

    public enum ToolId { Shovel, WateringCan, Rake, Hoe, Scythe, Cart }

    public enum PlotType { Regular, Raised, Fenced }

    public enum PlotState { Wild, Tilled, Cultivated, Watered, Planted, Ready, Harvested }

    public enum Quality { Poor, Normal, Good, Perfect }

    public sealed class GameState
    {
        public int Day { get; set; } = 1;
        public Season Season { get; set; } = Season.Spring;
        public Weather Weather { get; set; } = Weather.Sunny;
        public int Coins { get; set; } = 100;
        public ToolId SelectedTool { get; set; } = ToolId.Shovel;
        public string SelectedCropId { get; set; } = string.Empty;
        public Inventory Inventory { get; set; } = new Inventory();
        public FarmState Farm { get; set; } = new FarmState();
        public TractorState Tractor { get; set; } = new TractorState();
        public PlayerState Player { get; set; } = new PlayerState();
        public List<string> Messages { get; set; } = new List<string>();
    }

    public sealed class Inventory
    {
        public Dictionary<string, int> Seeds { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, Dictionary<Quality, int>> Crops { get; set; } = new Dictionary<string, Dictionary<Quality, int>>();
        public List<ToolId> Tools { get; set; } = new List<ToolId> { ToolId.Shovel, ToolId.WateringCan, ToolId.Rake, ToolId.Hoe, ToolId.Scythe, ToolId.Cart };
    }

    public sealed class Plot
    {
        public int Id { get; set; }
        public Vector2Int GridPosition { get; set; }
        public PlotType Type { get; set; } = PlotType.Regular;
        public PlotState State { get; set; } = PlotState.Wild;
        public string CropId { get; set; } = string.Empty;
        public int StageIndex { get; set; }
        public int Water { get; set; }
        public int QualityBonus { get; set; }
        public bool Ready { get; set; }
        public int Weeds { get; set; }
    }

    public sealed class FarmState
    {
        public List<Plot> Plots { get; set; } = new List<Plot>();
        public int ExpansionStage { get; set; }
    }

    public sealed class TractorState
    {
        public string TractorId { get; set; } = string.Empty;
        public string SelectedAttachmentId { get; set; } = string.Empty;
        public int Fuel { get; set; } = 100;
        public Vector2Int Position { get; set; }
        public int CargoCount { get; set; }
        public Dictionary<string, Dictionary<Quality, int>> Cargo { get; set; } = new Dictionary<string, Dictionary<Quality, int>>();
    }

    public sealed class PlayerState
    {
        public int ExperiencePoints { get; set; }
        public int Level { get; set; } = 1;
        public int SkillPoints { get; set; }
        public int TotalIncome { get; set; }
        public int TotalExpenses { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> UnlockedIds { get; set; } = new List<string>();
        public List<string> TractorUpgrades { get; set; } = new List<string>();
        public Dictionary<string, int> QuestProgress { get; set; } = new Dictionary<string, int>();
        public List<string> ClaimedQuests { get; set; } = new List<string>();
    }

    public sealed class InputState
    {
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
        public bool Interact { get; set; }
        public Vector2 Move { get { return new Vector2(Horizontal, Vertical); } }
    }

    public sealed class CropYield
    {
        public string CropId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public Quality Quality { get; set; } = Quality.Normal;
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public sealed class CropGrowthStage
    {
        public string Name { get; set; } = string.Empty;
        public int Days { get; set; } = 1;
    }

    public sealed class FarmShape
    {
        public int Plots { get; set; }
        public int Cols { get; set; }
        public int Rows { get; set; }
        public int TileCols { get; set; }
        public int TileRows { get; set; }
    }
}
