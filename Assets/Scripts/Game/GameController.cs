using System;
using System.Collections.Generic;
using System.IO;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Economy;
using FlatIronFields.Farming;
using FlatIronFields.Input;
using FlatIronFields.Progression;
using FlatIronFields.Tractors;
using FlatIronFields.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlatIronFields.Game
{
    public sealed class GameController : MonoBehaviour
    {
        private static readonly string[] AttachmentIds = { "tillage", "planter", "harvester", "transporter", "watering", "fertilizer" };

        public GameState State { get; private set; }
        public FarmGrid Farm { get { return farmGrid; } }
        public EconomySystem Economy { get { return economy; } }
        public ProgressionSystem Progression { get { return progression; } }
        public TractorController Tractor { get { return tractor; } }
        public InputRouter Input;
        public UIManager UI;

        private CropSystem cropSystem;
        private FarmGrid farmGrid;
        private EconomySystem economy;
        private ProgressionSystem progression;
        private TractorController tractor;

        private void Awake()
        {
            InitializeSystems();
        }

        private void Start()
        {
            if (UI != null) { UI.Initialize(State); }
        }

        private void Update()
        {
            HandleInput();
            if (tractor != null && Input != null)
            {
                tractor.UpdateMovement(Time.deltaTime, Input.Current);
            }
            if (UI != null) { UI.RenderHud(State); }
        }

        public void StartRun()
        {
            SceneManager.LoadScene("Farm");
        }

        public void NewDay()
        {
            if (State == null) { return; }
            State.Day++;
            if (State.Day % 28 == 1) { State.Season = (Season)(((int)State.Season + 1) % 4); }
            State.Weather = (Weather)(((int)State.Weather + 1) % 3);

            if (farmGrid == null || cropSystem == null || State.Farm == null) { return; }
            foreach (Plot plot in State.Farm.Plots)
            {
                if (plot == null || string.IsNullOrEmpty(plot.CropId) || plot.Ready) { continue; }
                if (!cropSystem.IsInSeason(plot.CropId, State.Season)) { continue; }
                cropSystem.Advance(plot, 1);
            }
            Message("Day " + State.Day + " — " + State.Season);
        }

        public void UseTool(Plot plot)
        {
            if (plot == null || farmGrid == null || State == null) { return; }
            bool done;
            switch (State.SelectedTool)
            {
                case ToolId.Shovel: done = farmGrid.Till(plot); break;
                case ToolId.Rake: done = CultivateOrPlant(plot); break;
                case ToolId.WateringCan: done = farmGrid.Water(plot); break;
                case ToolId.Hoe: done = farmGrid.ClearWeeds(plot); break;
                case ToolId.Scythe: done = HarvestToInventory(plot); break;
                case ToolId.Cart: done = tractor != null && tractor.ApplyAttachment(plot, "transporter"); break;
                default: done = false; break;
            }
            if (done && progression != null) { progression.AddXp(2, "tool"); }
        }

        public void SaveGame()
        {
            if (State == null) { return; }
            SaveData data = ToSaveData(State);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "flat-iron-fields.json"), json);
            Message("Game saved.");
        }

        public void LoadGame()
        {
            string path = Path.Combine(Application.persistentDataPath, "flat-iron-fields.json");
            if (!File.Exists(path)) { Message("No save found."); return; }
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            if (data == null) { Message("Save invalid."); return; }
            State = FromSaveData(data);
            RewireSystems();
            if (UI != null) { UI.Initialize(State); }
            Message("Game loaded.");
        }

        public void ResetGame()
        {
            InitializeSystems();
            if (UI != null) { UI.Initialize(State); }
            Message("Game reset.");
        }

        private void InitializeSystems()
        {
            CropDatabase.Initialize();
            TractorDatabase.Initialize();
            AttachmentDatabase.Initialize();
            SkillDatabase.Initialize();
            RecipeDatabase.Initialize();

            State = new GameState();
            farmGrid = new FarmGrid();
            State.Farm.Plots = farmGrid.CreateFarm(0);
            cropSystem = new CropSystem();
            economy = new EconomySystem(State);
            progression = new ProgressionSystem(State);
            economy.SetProgressionSystem(progression);
            economy.SetFarmGrid(farmGrid);
            tractor = new TractorController(State, farmGrid, economy, progression);

            TractorDefinition definition = TractorDatabase.GetTractor("basic");
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<TractorDefinition>();
                definition.Id = "basic";
                definition.DisplayName = "Basic Tractor";
                definition.Speed = 3f;
                definition.CargoCapacity = 10;
                definition.FuelCapacity = 100;
            }
            tractor.Initialize(definition);

            IReadOnlyList<CropDefinition> crops = CropDatabase.All();
            State.SelectedCropId = crops.Count > 0 ? crops[0].Id : "cabbage";
            if (State.Tractor != null) { State.Tractor.SelectedAttachmentId = "tillage"; }
        }

        private void RewireSystems()
        {
            farmGrid = new FarmGrid();
            cropSystem = new CropSystem();
            economy = new EconomySystem(State);
            progression = new ProgressionSystem(State);
            economy.SetProgressionSystem(progression);
            economy.SetFarmGrid(farmGrid);
            tractor = new TractorController(State, farmGrid, economy, progression);
            TractorDefinition definition = TractorDatabase.GetTractor(State.Tractor != null ? State.Tractor.TractorId : "basic");
            if (definition == null) { definition = ScriptableObject.CreateInstance<TractorDefinition>(); definition.Id = "basic"; definition.Speed = 3f; definition.CargoCapacity = 10; }
            tractor.Initialize(definition);
        }

        private void HandleInput()
        {
            if (Input == null || State == null) { return; }

            if (Input.WasPressed(ActionId.CycleCropNext)) { CycleCrop(1); }
            if (Input.WasPressed(ActionId.CycleCropPrev)) { CycleCrop(-1); }
            if (Input.WasPressed(ActionId.CycleAttachment)) { CycleAttachment(); }
            if (Input.WasPressed(ActionId.NextDay)) { NewDay(); }
            if (Input.WasPressed(ActionId.Interact)) { UseTool(PlotUnderTractor()); }

            if (Input.WasPressed(ActionId.Tool1)) { State.SelectedTool = ToolId.Shovel; }
            if (Input.WasPressed(ActionId.Tool2)) { State.SelectedTool = ToolId.WateringCan; }
            if (Input.WasPressed(ActionId.Tool3)) { State.SelectedTool = ToolId.Rake; }
            if (Input.WasPressed(ActionId.Tool4)) { State.SelectedTool = ToolId.Hoe; }
            if (Input.WasPressed(ActionId.Tool5)) { State.SelectedTool = ToolId.Scythe; }
            if (Input.WasPressed(ActionId.Tool6)) { State.SelectedTool = ToolId.Cart; }

            if (UI == null) { return; }
            if (Input.WasPressed(ActionId.OpenSeeds)) { UI.OpenMenu(MenuId.Seeds); }
            if (Input.WasPressed(ActionId.OpenSell)) { UI.OpenMenu(MenuId.Sell); }
            if (Input.WasPressed(ActionId.OpenLand)) { UI.OpenMenu(MenuId.Land); }
            if (Input.WasPressed(ActionId.OpenTractor)) { UI.OpenMenu(MenuId.Tractor); }
            if (Input.WasPressed(ActionId.OpenSkills)) { UI.OpenMenu(MenuId.Skills); }
            if (Input.WasPressed(ActionId.ShowHelp)) { UI.OpenMenu(MenuId.Help); }
        }

        private void CycleCrop(int direction)
        {
            IReadOnlyList<CropDefinition> crops = CropDatabase.All();
            if (crops.Count == 0) { return; }
            int index = 0;
            for (int i = 0; i < crops.Count; i++)
            {
                if (crops[i].Id == State.SelectedCropId) { index = i; break; }
            }
            index = (index + direction + crops.Count) % crops.Count;
            State.SelectedCropId = crops[index].Id;
            Message("Selected crop: " + crops[index].DisplayName);
        }

        private void CycleAttachment()
        {
            if (State.Tractor == null) { return; }
            int index = 0;
            for (int i = 0; i < AttachmentIds.Length; i++)
            {
                if (AttachmentIds[i] == State.Tractor.SelectedAttachmentId) { index = i; break; }
            }
            State.Tractor.SelectedAttachmentId = AttachmentIds[(index + 1) % AttachmentIds.Length];
            Message("Attachment: " + State.Tractor.SelectedAttachmentId);
        }

        private Plot PlotUnderTractor()
        {
            if (State.Farm == null || State.Farm.Plots == null || State.Farm.Plots.Count == 0 || State.Tractor == null)
            {
                return null;
            }
            Vector2Int position = State.Tractor.Position;
            foreach (Plot plot in State.Farm.Plots)
            {
                if (plot != null && plot.GridPosition == position) { return plot; }
            }
            return State.Farm.Plots[0];
        }

        private bool CultivateOrPlant(Plot plot)
        {
            if (plot.State == PlotState.Tilled) { return farmGrid.Cultivate(plot); }
            if (plot.State != PlotState.Cultivated && plot.State != PlotState.Watered) { return false; }

            string cropId = State.SelectedCropId;
            int seeds;
            if (string.IsNullOrEmpty(cropId) || !State.Inventory.Seeds.TryGetValue(cropId, out seeds) || seeds <= 0)
            {
                Message("No " + cropId + " seeds. Buy seeds first.");
                return false;
            }
            if (!farmGrid.Plant(plot, cropId)) { return false; }
            State.Inventory.Seeds[cropId] = seeds - 1;
            Message("Planted " + cropId + ".");
            return true;
        }

        private bool HarvestToInventory(Plot plot)
        {
            CropYield yield = farmGrid.Harvest(plot);
            if (yield == null || yield.Quantity <= 0) { return false; }
            Dictionary<Quality, int> bucket;
            if (!State.Inventory.Crops.TryGetValue(yield.CropId, out bucket))
            {
                bucket = new Dictionary<Quality, int>();
                State.Inventory.Crops[yield.CropId] = bucket;
            }
            int count;
            bucket.TryGetValue(yield.Quality, out count);
            bucket[yield.Quality] = count + yield.Quantity;
            if (progression != null) { progression.AddXp(5, "harvest"); }
            Message("Harvested " + yield.Quantity + " " + yield.CropId + " (" + yield.Quality + ")");
            return true;
        }

        private void Message(string text)
        {
            if (State.Messages == null) { State.Messages = new List<string>(); }
            State.Messages.Add(text);
            if (UI != null) { UI.Message(text); }
        }

        private static SaveData ToSaveData(GameState state)
        {
            var data = new SaveData
            {
                Day = state.Day,
                Season = (int)state.Season,
                Weather = (int)state.Weather,
                Coins = state.Coins,
                SelectedTool = (int)state.SelectedTool,
                SelectedCropId = state.SelectedCropId,
                Xp = state.Player.ExperiencePoints,
                Level = state.Player.Level,
                SkillPoints = state.Player.SkillPoints,
                Skills = state.Player.Skills,
                TractorId = state.Tractor.TractorId,
                Fuel = state.Tractor.Fuel,
                AttachmentId = state.Tractor.SelectedAttachmentId
            };

            data.Plots = new List<PlotSave>();
            foreach (Plot plot in state.Farm.Plots)
            {
                data.Plots.Add(new PlotSave
                {
                    Id = plot.Id, GridX = plot.GridPosition.x, GridY = plot.GridPosition.y,
                    Type = (int)plot.Type, State = (int)plot.State, CropId = plot.CropId,
                    StageIndex = plot.StageIndex, Water = plot.Water, QualityBonus = plot.QualityBonus,
                    Ready = plot.Ready, Weeds = plot.Weeds
                });
            }

            data.Seeds = new List<SeedSave>();
            foreach (KeyValuePair<string, int> pair in state.Inventory.Seeds)
            {
                data.Seeds.Add(new SeedSave { CropId = pair.Key, Count = pair.Value });
            }

            data.Crops = new List<CropSave>();
            foreach (KeyValuePair<string, Dictionary<Quality, int>> crop in state.Inventory.Crops)
            {
                foreach (KeyValuePair<Quality, int> quality in crop.Value)
                {
                    data.Crops.Add(new CropSave { CropId = crop.Key, Quality = (int)quality.Key, Count = quality.Value });
                }
            }
            return data;
        }

        private static GameState FromSaveData(SaveData data)
        {
            var state = new GameState
            {
                Day = data.Day,
                Season = (Season)data.Season,
                Weather = (Weather)data.Weather,
                Coins = data.Coins,
                SelectedTool = (ToolId)data.SelectedTool,
                SelectedCropId = data.SelectedCropId
            };
            state.Player.ExperiencePoints = data.Xp;
            state.Player.Level = data.Level;
            state.Player.SkillPoints = data.SkillPoints;
            state.Player.Skills = data.Skills ?? new List<string>();
            state.Tractor.TractorId = data.TractorId;
            state.Tractor.Fuel = data.Fuel;
            state.Tractor.SelectedAttachmentId = data.AttachmentId;

            if (data.Plots != null)
            {
                state.Farm.Plots = new List<Plot>();
                foreach (PlotSave plot in data.Plots)
                {
                    state.Farm.Plots.Add(new Plot
                    {
                        Id = plot.Id, GridPosition = new Vector2Int(plot.GridX, plot.GridY),
                        Type = (PlotType)plot.Type, State = (PlotState)plot.State, CropId = plot.CropId,
                        StageIndex = plot.StageIndex, Water = plot.Water, QualityBonus = plot.QualityBonus,
                        Ready = plot.Ready, Weeds = plot.Weeds
                    });
                }
            }
            if (data.Seeds != null)
            {
                foreach (SeedSave seed in data.Seeds) { state.Inventory.Seeds[seed.CropId] = seed.Count; }
            }
            if (data.Crops != null)
            {
                foreach (CropSave crop in data.Crops)
                {
                    Dictionary<Quality, int> bucket;
                    if (!state.Inventory.Crops.TryGetValue(crop.CropId, out bucket))
                    {
                        bucket = new Dictionary<Quality, int>();
                        state.Inventory.Crops[crop.CropId] = bucket;
                    }
                    bucket[(Quality)crop.Quality] = crop.Count;
                }
            }
            return state;
        }

        [Serializable] private sealed class SaveData
        {
            public int Day;
            public int Season;
            public int Weather;
            public int Coins;
            public int SelectedTool;
            public string SelectedCropId;
            public int Xp;
            public int Level;
            public int SkillPoints;
            public List<string> Skills = new List<string>();
            public string TractorId;
            public int Fuel;
            public string AttachmentId;
            public List<PlotSave> Plots = new List<PlotSave>();
            public List<SeedSave> Seeds = new List<SeedSave>();
            public List<CropSave> Crops = new List<CropSave>();
        }

        [Serializable] private sealed class PlotSave
        {
            public int Id; public int GridX; public int GridY; public int Type; public int State;
            public string CropId; public int StageIndex; public int Water; public int QualityBonus;
            public bool Ready; public int Weeds;
        }

        [Serializable] private sealed class SeedSave
        {
            public string CropId; public int Count;
        }

        [Serializable] private sealed class CropSave
        {
            public string CropId; public int Quality; public int Count;
        }
    }
}
