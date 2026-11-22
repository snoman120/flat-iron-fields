using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Economy;
using FlatIronFields.Farming;
using FlatIronFields.Progression;
using UnityEngine;

namespace FlatIronFields.Tractors
{
    public sealed class TractorController
    {
        private readonly GameState state;
        private readonly FarmGrid grid;
        private readonly EconomySystem economy;
        private readonly ProgressionSystem progression;
        private TractorDefinition definition;
        private TractorUpgradeSystem upgrades;
        private Vector2 position;
        private float fuelTimer;

        public TractorController(GameState state, FarmGrid grid, EconomySystem economy, ProgressionSystem progression)
        {
            this.state = state ?? new GameState();
            this.grid = grid;
            this.economy = economy;
            this.progression = progression;
        }

        public void SetUpgradeSystem(TractorUpgradeSystem value) { upgrades = value; }

        public void Initialize(TractorDefinition value)
        {
            definition = value;
            if (state.Tractor == null) { state.Tractor = new TractorState(); }
            state.Tractor.TractorId = value != null ? value.Id : string.Empty;
            state.Tractor.Fuel = value != null ? value.FuelCapacity : 100;
            state.Tractor.Cargo.Clear();
            state.Tractor.CargoCount = 0;
            state.Tractor.SelectedAttachmentId = "tillage";
            position = state.Tractor.Position;
        }

        public void Attach(string attachmentId)
        {
            if (string.IsNullOrEmpty(attachmentId)) { return; }
            if (AttachmentDatabase.GetAttachment(attachmentId) == null) { return; }
            state.Tractor.SelectedAttachmentId = attachmentId;
        }

        public void UpdateMovement(float deltaTime, InputState input)
        {
            if (input == null || definition == null || state.Tractor == null) { return; }
            if (Mathf.Abs(input.Horizontal) + Mathf.Abs(input.Vertical) < 0.001f) { return; }

            float speedBonus = upgrades != null ? upgrades.SpeedMultiplier() : 1f;
            Vector2 direction = new Vector2(input.Horizontal, input.Vertical).normalized;
            position += direction * definition.Speed * speedBonus * deltaTime;
            state.Tractor.Position = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

            float efficiency = upgrades != null ? upgrades.FuelEfficiencyMultiplier() : 1f;
            fuelTimer += deltaTime * efficiency;
            if (fuelTimer >= 1f)
            {
                fuelTimer -= 1f;
                state.Tractor.Fuel = Mathf.Max(0, state.Tractor.Fuel - 1);
            }
        }

        public bool ApplyAttachment(Plot plot, string attachmentId)
        {
            if (plot == null || string.IsNullOrEmpty(attachmentId) || state.Tractor == null) { return false; }
            if (grid == null) { return false; }

            AttachmentDefinition attachment = AttachmentDatabase.GetAttachment(attachmentId);
            int cost = attachment != null ? Mathf.Max(0, attachment.FuelCost) : 1;
            if (state.Tractor.Fuel < cost) { return false; }

            bool applied;
            switch (attachmentId)
            {
                case "tillage":
                    applied = grid.Till(plot);
                    break;
                case "planter":
                    applied = PlantWithTractor(plot);
                    break;
                case "harvester":
                    applied = HarvestWithTractor(plot);
                    break;
                case "transporter":
                    applied = TransferCargo();
                    break;
                case "watering":
                    applied = grid.Water(plot);
                    break;
                case "fertilizer":
                    applied = grid.Fertilize(plot);
                    break;
                default:
                    applied = false;
                    break;
            }

            if (applied)
            {
                state.Tractor.Fuel -= cost;
                if (progression != null) { progression.AddXp(2, "tractor_" + attachmentId); }
            }
            return applied;
        }

        public int CargoCount()
        {
            return state.Tractor != null ? state.Tractor.CargoCount : 0;
        }

        private bool PlantWithTractor(Plot plot)
        {
            string cropId = state.SelectedCropId;
            if (string.IsNullOrEmpty(cropId)) { return false; }
            int seeds;
            if (!state.Inventory.Seeds.TryGetValue(cropId, out seeds) || seeds <= 0) { return false; }
            if (!grid.Plant(plot, cropId)) { return false; }
            state.Inventory.Seeds[cropId] = seeds - 1;
            return true;
        }

        private bool HarvestWithTractor(Plot plot)
        {
            CropYield yield = grid.Harvest(plot);
            if (yield == null || yield.Quantity <= 0) { return false; }
            AddCargo(yield.CropId, yield.Quality, yield.Quantity);
            return true;
        }

        private void AddCargo(string cropId, Quality quality, int quantity)
        {
            int capacity = definition != null ? Mathf.Max(0, definition.CargoCapacity) : 0;
            if (upgrades != null) { capacity += upgrades.CargoBonus(); }
            for (int i = 0; i < quantity && state.Tractor.CargoCount < capacity; i++)
            {
                Dictionary<Quality, int> bucket;
                if (!state.Tractor.Cargo.TryGetValue(cropId, out bucket))
                {
                    bucket = new Dictionary<Quality, int>();
                    state.Tractor.Cargo[cropId] = bucket;
                }
                int count;
                bucket.TryGetValue(quality, out count);
                bucket[quality] = count + 1;
                state.Tractor.CargoCount++;
            }
        }

        private bool TransferCargo()
        {
            if (state.Tractor.Cargo.Count == 0) { return false; }
            foreach (KeyValuePair<string, Dictionary<Quality, int>> entry in state.Tractor.Cargo)
            {
                Dictionary<Quality, int> target;
                if (!state.Inventory.Crops.TryGetValue(entry.Key, out target))
                {
                    target = new Dictionary<Quality, int>();
                    state.Inventory.Crops[entry.Key] = target;
                }
                foreach (KeyValuePair<Quality, int> qualityEntry in entry.Value)
                {
                    int count;
                    target.TryGetValue(qualityEntry.Key, out count);
                    target[qualityEntry.Key] = count + qualityEntry.Value;
                }
            }
            state.Tractor.Cargo.Clear();
            state.Tractor.CargoCount = 0;
            return true;
        }
    }
}
