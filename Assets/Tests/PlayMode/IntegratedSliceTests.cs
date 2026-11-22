using System.Collections.Generic;
using FlatIronFields.Core;
using FlatIronFields.Game;
using FlatIronFields.UI;
using NUnit.Framework;
using UnityEngine;

namespace FlatIronFields.Tests
{
    public sealed class IntegratedSliceTests
    {
        private GameObject controllerObject;

        [SetUp]
        public void SetUp()
        {
            controllerObject = new GameObject("TestGameController");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(controllerObject);
        }

        [Test]
        public void ManualLoop_TillWaterPlantGrowHarvestSell()
        {
            GameController controller = controllerObject.AddComponent<GameController>();
            GameState state = controller.State;
            Assert.AreEqual(5, state.Farm.Plots.Count);

            state.SelectedCropId = "cabbage";
            state.Inventory.Seeds["cabbage"] = 2;
            Plot plot = state.Farm.Plots[0];

            state.SelectedTool = ToolId.Shovel;
            controller.UseTool(plot);
            state.SelectedTool = ToolId.WateringCan;
            controller.UseTool(plot);
            state.SelectedTool = ToolId.Rake;
            controller.UseTool(plot);
            Assert.AreEqual(PlotState.Planted, plot.State);

            for (int day = 0; day < 4; day++)
            {
                if (plot.Water == 0) { controller.Farm.Water(plot); }
                controller.NewDay();
            }
            Assert.IsTrue(plot.Ready);

            state.SelectedTool = ToolId.Scythe;
            controller.UseTool(plot);
            Dictionary<Quality, int> bucket;
            Assert.IsTrue(state.Inventory.Crops.TryGetValue("cabbage", out bucket));
            Assert.Greater(InventoryCount(state.Inventory.Crops, "cabbage"), 0);

            Quality sellQuality = Quality.Poor;
            foreach (KeyValuePair<Quality, int> pair in bucket)
            {
                if (pair.Value > 0) { sellQuality = pair.Key; break; }
            }

            int coinsBefore = state.Coins;
            int earned = controller.Economy.Sell("cabbage", 1, sellQuality);
            Assert.Greater(earned, 0);
            Assert.AreEqual(coinsBefore + earned, state.Coins);
        }

        [Test]
        public void Tractor_MovementAndAllSixAttachments()
        {
            GameController controller = controllerObject.AddComponent<GameController>();
            GameState state = controller.State;
            state.SelectedCropId = "cabbage";
            state.Inventory.Seeds["cabbage"] = 2;

            var input = new InputState { Horizontal = 1f, Vertical = 0f };
            int fuelBefore = state.Tractor.Fuel;
            Vector2Int positionBefore = state.Tractor.Position;
            controller.Tractor.UpdateMovement(1f, input);
            Assert.AreNotEqual(positionBefore, state.Tractor.Position);
            Assert.Less(state.Tractor.Fuel, fuelBefore);

            Plot plot = state.Farm.Plots[0];
            Assert.IsTrue(controller.Tractor.ApplyAttachment(plot, "tillage"));
            Assert.IsTrue(controller.Tractor.ApplyAttachment(plot, "planter"));
            Assert.IsTrue(controller.Tractor.ApplyAttachment(plot, "watering"));
            Assert.IsTrue(controller.Tractor.ApplyAttachment(plot, "fertilizer"));

            plot.StageIndex = 4;
            plot.Ready = true;
            plot.State = PlotState.Ready;
            Assert.IsTrue(controller.Tractor.ApplyAttachment(plot, "harvester"));
            Assert.Greater(controller.Tractor.CargoCount(), 0);

            int inventoryBefore = 0;
            Dictionary<Quality, int> bucket;
            if (state.Inventory.Crops.TryGetValue("cabbage", out bucket))
            {
                foreach (KeyValuePair<Quality, int> pair in bucket) { inventoryBefore += pair.Value; }
            }
            Assert.IsTrue(controller.Tractor.ApplyAttachment(plot, "transporter"));
            Assert.AreEqual(0, controller.Tractor.CargoCount());
            Assert.Greater(InventoryCount(state.Inventory.Crops, "cabbage"), inventoryBefore);
        }

        [Test]
        public void SaveLoadReset_RoundTripsState()
        {
            GameController controller = controllerObject.AddComponent<GameController>();
            controller.State.Day = 12;
            controller.State.Coins = 345;
            controller.SaveGame();

            controller.State.Day = 1;
            controller.State.Coins = 0;
            controller.LoadGame();
            Assert.AreEqual(12, controller.State.Day);
            Assert.AreEqual(345, controller.State.Coins);

            controller.ResetGame();
            Assert.AreEqual(1, controller.State.Day);
            Assert.AreEqual(100, controller.State.Coins);
        }

        [Test]
        public void UIManager_OpenCloseMenusWithoutErrors()
        {
            var uiObject = new GameObject("TestUIManager");
            try
            {
                UIManager ui = uiObject.AddComponent<UIManager>();
                GameController controller = controllerObject.AddComponent<GameController>();
                ui.Initialize(controller.State);
                ui.RenderHud(controller.State);
                ui.OpenMenu(MenuId.Seeds);
                ui.OpenMenu(MenuId.Help);
                ui.Message("test message");
                ui.CloseMenu();
                Assert.Pass();
            }
            finally
            {
                Object.DestroyImmediate(uiObject);
            }
        }

        private static int InventoryCount(Dictionary<string, Dictionary<Quality, int>> crops, string cropId)
        {
            Dictionary<Quality, int> bucket;
            if (!crops.TryGetValue(cropId, out bucket)) { return 0; }
            int total = 0;
            foreach (KeyValuePair<Quality, int> pair in bucket) { total += pair.Value; }
            return total;
        }
    }
}
