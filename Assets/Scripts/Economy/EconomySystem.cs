using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Crops;
using FlatIronFields.Farming;
using FlatIronFields.Progression;
using FlatIronFields.Tractors;
using UnityEngine;

namespace FlatIronFields.Economy
{
    public sealed class EconomySystem
    {
        private readonly GameState state;
        private ProgressionSystem progression;
        private FarmGrid farmGrid;

        public EconomySystem(GameState state)
        {
            this.state = state ?? new GameState();
        }

        public void SetProgressionSystem(ProgressionSystem value) { progression = value; }
        public void SetFarmGrid(FarmGrid value) { farmGrid = value; }

        public int Price(string cropId, Quality quality)
        {
            CropDefinition crop = CropDatabase.GetCrop(cropId);
            if (crop == null) { return 0; }

            float multiplier;
            switch (quality)
            {
                case Quality.Poor: multiplier = 0.5f; break;
                case Quality.Good: multiplier = 1.25f; break;
                case Quality.Perfect: multiplier = 1.5f; break;
                default: multiplier = 1f; break;
            }
            return Mathf.Max(1, Mathf.RoundToInt(crop.SalePrice * multiplier));
        }

        public int Sell(string cropId, int quantity, Quality quality)
        {
            if (string.IsNullOrEmpty(cropId) || quantity <= 0 || state.Inventory == null) { return 0; }

            Dictionary<Quality, int> bucket;
            if (!state.Inventory.Crops.TryGetValue(cropId, out bucket)) { return 0; }

            int available;
            if (!bucket.TryGetValue(quality, out available) || available <= 0) { return 0; }

            int sold = Mathf.Min(available, quantity);
            int earned = Price(cropId, quality) * sold;
            bucket[quality] = available - sold;
            AddCoins(earned);
            if (progression != null) { progression.AddXp(Mathf.Max(1, earned / 10), "sale"); }
            return earned;
        }

        public bool BuySeed(string cropId, int quantity)
        {
            if (string.IsNullOrEmpty(cropId) || quantity <= 0) { return false; }
            CropDefinition crop = CropDatabase.GetCrop(cropId);
            if (crop == null) { return false; }

            int cost = crop.SeedCost * quantity;
            if (!TrySpend(cost)) { return false; }

            int owned;
            state.Inventory.Seeds.TryGetValue(cropId, out owned);
            state.Inventory.Seeds[cropId] = owned + quantity;
            if (progression != null) { progression.AddXp(2, "buy_seed"); }
            return true;
        }

        public bool BuyPlot()
        {
            if (state.Farm == null || state.Farm.ExpansionStage >= 4) { return false; }
            int nextStage = state.Farm.ExpansionStage + 1;
            if (nextStage < 0 || nextStage > 4) { return false; }
            int cost = 200 * nextStage;
            if (!TrySpend(cost)) { return false; }

            if (farmGrid != null)
            {
                farmGrid.Expand(nextStage);
                state.Farm.Plots = farmGrid.CreateFarm(nextStage);
            }
            state.Farm.ExpansionStage = nextStage;
            if (progression != null) { progression.AddXp(25, "expansion"); }
            return true;
        }

        public bool BuyTractor(string tractorId)
        {
            TractorDefinition tractor = TractorDatabase.GetTractor(tractorId);
            if (tractor == null) { return false; }
            if (progression != null && progression.Level < tractor.UnlockedAtLevel) { return false; }
            if (!TrySpend(tractor.Cost)) { return false; }

            if (state.Tractor == null) { state.Tractor = new TractorState(); }
            state.Tractor.TractorId = tractor.Id;
            state.Tractor.Fuel = 100;
            if (progression != null) { progression.AddXp(20, "tractor_purchase"); }
            return true;
        }

        public bool Craft(string recipeId)
        {
            RecipeDefinition recipe = RecipeDatabase.GetRecipe(recipeId);
            if (recipe == null) { return false; }

            Dictionary<Quality, int> bucket;
            if (!state.Inventory.Crops.TryGetValue(recipe.InputCropId, out bucket)) { return false; }
            int available;
            if (!bucket.TryGetValue(recipe.InputQuality, out available) || available < recipe.InputQuantity) { return false; }
            if (!TrySpend(recipe.CoinCost)) { return false; }

            bucket[recipe.InputQuality] = available - recipe.InputQuantity;
            Dictionary<Quality, int> output;
            if (!state.Inventory.Crops.TryGetValue(recipe.OutputCropId, out output))
            {
                output = new Dictionary<Quality, int>();
                state.Inventory.Crops[recipe.OutputCropId] = output;
            }
            int outputCount;
            output.TryGetValue(recipe.OutputQuality, out outputCount);
            output[recipe.OutputQuality] = outputCount + recipe.OutputQuantity;

            if (progression != null) { progression.AddXp(10, "craft"); }
            return true;
        }

        private bool TrySpend(int cost)
        {
            if (cost < 0 || state.Coins < cost) { return false; }
            state.Coins -= cost;
            state.Player.TotalExpenses += cost;
            return true;
        }

        private void AddCoins(int amount)
        {
            state.Coins += amount;
            state.Player.TotalIncome += amount;
        }
    }
}
