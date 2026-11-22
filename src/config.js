/* Flat Iron Fields - Core configuration.
 * Classic script. Owned by Core Systems agent.
 */
window.FIF = window.FIF || {};

(function (FIF) {
  var TILE_SIZE = 32;

  var SEASONS = ['Spring', 'Summer', 'Autumn', 'Winter'];

  var QUALITIES = ['Poor', 'Normal', 'Good', 'Perfect'];

  // All crops from the GDD, deduplicated (Grape/Grapes, Cherry/Cherries,
  // Strawberry/Strawberries collapsed). Two GDD "secret / discovery" crops
  // are included to cover the 25+ crop requirement.
  var CROPS = [
    { id: 'grass', name: 'Grass', seasons: SEASONS, growthDays: 3, yieldMin: 1, yieldMax: 3, seedCost: 2, salePrice: 4, multipleHarvest: true, unlockedAtLevel: 1 },
    { id: 'cabbage', name: 'Cabbage', seasons: ['Spring', 'Autumn'], growthDays: 7, yieldMin: 1, yieldMax: 3, seedCost: 8, salePrice: 18, multipleHarvest: false, unlockedAtLevel: 1 },
    { id: 'wheat', name: 'Wheat', seasons: ['Summer', 'Autumn'], growthDays: 8, yieldMin: 2, yieldMax: 4, seedCost: 10, salePrice: 16, multipleHarvest: false, unlockedAtLevel: 1 },
    { id: 'carrot', name: 'Carrot', seasons: ['Spring', 'Autumn'], growthDays: 6, yieldMin: 2, yieldMax: 4, seedCost: 6, salePrice: 14, multipleHarvest: false, unlockedAtLevel: 1 },
    { id: 'sweetPotato', name: 'Sweet Potato', seasons: ['Summer', 'Autumn'], growthDays: 9, yieldMin: 2, yieldMax: 5, seedCost: 12, salePrice: 24, multipleHarvest: true, unlockedAtLevel: 1 },
    { id: 'potato', name: 'Potato', seasons: ['Spring', 'Autumn'], growthDays: 8, yieldMin: 2, yieldMax: 4, seedCost: 9, salePrice: 20, multipleHarvest: false, unlockedAtLevel: 1 },
    { id: 'corn', name: 'Corn', seasons: ['Summer'], growthDays: 10, yieldMin: 1, yieldMax: 3, seedCost: 14, salePrice: 32, multipleHarvest: true, unlockedAtLevel: 2 },
    { id: 'pepper', name: 'Pepper', seasons: ['Summer'], growthDays: 9, yieldMin: 2, yieldMax: 4, seedCost: 15, salePrice: 30, multipleHarvest: true, unlockedAtLevel: 2 },
    { id: 'tomato', name: 'Tomato', seasons: ['Summer'], growthDays: 9, yieldMin: 2, yieldMax: 5, seedCost: 14, salePrice: 28, multipleHarvest: true, unlockedAtLevel: 2 },
    { id: 'watermelon', name: 'Watermelon', seasons: ['Summer'], growthDays: 12, yieldMin: 1, yieldMax: 2, seedCost: 22, salePrice: 60, multipleHarvest: false, unlockedAtLevel: 3 },
    { id: 'pumpkin', name: 'Pumpkin', seasons: ['Autumn'], growthDays: 12, yieldMin: 1, yieldMax: 3, seedCost: 20, salePrice: 55, multipleHarvest: false, unlockedAtLevel: 3 },
    { id: 'onion', name: 'Onion', seasons: ['Spring', 'Autumn'], growthDays: 7, yieldMin: 2, yieldMax: 4, seedCost: 8, salePrice: 18, multipleHarvest: false, unlockedAtLevel: 1 },
    { id: 'garlic', name: 'Garlic', seasons: ['Autumn', 'Winter'], growthDays: 8, yieldMin: 1, yieldMax: 3, seedCost: 12, salePrice: 26, multipleHarvest: false, unlockedAtLevel: 2 },
    { id: 'strawberry', name: 'Strawberry', seasons: ['Spring'], growthDays: 8, yieldMin: 2, yieldMax: 4, seedCost: 16, salePrice: 34, multipleHarvest: true, unlockedAtLevel: 2 },
    { id: 'apple', name: 'Apple', seasons: ['Autumn'], growthDays: 14, yieldMin: 2, yieldMax: 5, seedCost: 24, salePrice: 40, multipleHarvest: true, unlockedAtLevel: 3 },
    { id: 'peach', name: 'Peach', seasons: ['Summer'], growthDays: 13, yieldMin: 2, yieldMax: 4, seedCost: 24, salePrice: 42, multipleHarvest: true, unlockedAtLevel: 3 },
    { id: 'plum', name: 'Plum', seasons: ['Summer'], growthDays: 12, yieldMin: 2, yieldMax: 4, seedCost: 20, salePrice: 36, multipleHarvest: true, unlockedAtLevel: 3 },
    { id: 'grape', name: 'Grape', seasons: ['Autumn'], growthDays: 11, yieldMin: 2, yieldMax: 5, seedCost: 22, salePrice: 38, multipleHarvest: true, unlockedAtLevel: 3 },
    { id: 'cherry', name: 'Cherry', seasons: ['Spring'], growthDays: 13, yieldMin: 2, yieldMax: 4, seedCost: 26, salePrice: 46, multipleHarvest: true, unlockedAtLevel: 4 },
    { id: 'orange', name: 'Orange', seasons: ['Winter'], growthDays: 14, yieldMin: 2, yieldMax: 4, seedCost: 26, salePrice: 48, multipleHarvest: true, unlockedAtLevel: 4 },
    { id: 'lemon', name: 'Lemon', seasons: ['Winter'], growthDays: 14, yieldMin: 2, yieldMax: 4, seedCost: 26, salePrice: 48, multipleHarvest: true, unlockedAtLevel: 4 },
    { id: 'blueberry', name: 'Blueberry', seasons: ['Summer'], growthDays: 10, yieldMin: 2, yieldMax: 4, seedCost: 18, salePrice: 36, multipleHarvest: true, unlockedAtLevel: 3 },
    { id: 'blackberry', name: 'Blackberry', seasons: ['Autumn'], growthDays: 10, yieldMin: 2, yieldMax: 4, seedCost: 18, salePrice: 36, multipleHarvest: true, unlockedAtLevel: 4 },
    { id: 'raspberry', name: 'Raspberry', seasons: ['Summer', 'Autumn'], growthDays: 10, yieldMin: 2, yieldMax: 4, seedCost: 18, salePrice: 36, multipleHarvest: true, unlockedAtLevel: 4 },
    { id: 'goldenApple', name: 'Golden Apple', seasons: ['Winter'], growthDays: 16, yieldMin: 1, yieldMax: 2, seedCost: 50, salePrice: 150, multipleHarvest: false, unlockedAtLevel: 5 },
    { id: 'moonberry', name: 'Moonberry', seasons: ['Winter'], growthDays: 15, yieldMin: 1, yieldMax: 3, seedCost: 45, salePrice: 120, multipleHarvest: true, unlockedAtLevel: 6 }
  ];

  var TRACTOR_TYPES = [
    { id: 'basic', name: 'Basic Tractor', speed: 110, capacity: 20, cost: 0, unlockedAtLevel: 1 },
    { id: 'crop', name: 'Crop Tractor', speed: 95, capacity: 30, cost: 350, unlockedAtLevel: 2 },
    { id: 'heavy', name: 'Heavy Duty', speed: 70, capacity: 60, cost: 900, unlockedAtLevel: 4 },
    { id: 'specialized', name: 'Specialized', speed: 120, capacity: 40, cost: 1500, unlockedAtLevel: 5 }
  ];

  var ATTACHMENTS = [
    { id: 'tillage', tool: 'shovel', name: 'Tillage Attachment', description: 'Prepares soil for planting.' },
    { id: 'planter', tool: 'seeds', name: 'Planter Attachment', description: 'Plants selected seeds automatically.' },
    { id: 'harvester', tool: 'scythe', name: 'Harvester Attachment', description: 'Harvests ripe crops quickly.' },
    { id: 'transporter', tool: 'cart', name: 'Transporter Attachment', description: 'Moves crops into tractor cargo.' },
    { id: 'watering', tool: 'wateringCan', name: 'Watering Attachment', description: 'Irrigates crops automatically.' },
    { id: 'fertilizer', tool: 'fertilizer', name: 'Fertilizer Attachment', description: 'Adds nutrients and quality bonus.' }
  ];

  var SKILLS = [
    { id: 'planting', name: 'Planting Efficiency', description: 'Plant more crops with fewer seeds.' },
    { id: 'harvesting', name: 'Harvesting Speed', description: 'Harvest more from every crop.' },
    { id: 'cropSelection', name: 'Crop Selection', description: 'Unlock better crop choices.' },
    { id: 'resourceManagement', name: 'Resource Management', description: 'Save water and fuel.' },
    { id: 'tractorOperation', name: 'Tractor Operation', description: 'Improve tractor speed and capacity.' },
    { id: 'farmExpansion', name: 'Farm Expansion', description: 'Reduce the cost of new land.' }
  ];

  // XP needed to reach level 1..6: Noob, Beginner, Intermediate,
  // Advanced, Expert, Master.
  var LEVELS = [0, 100, 250, 500, 900, 1500];

  var LEVEL_NAMES = ['Noob', 'Beginner', 'Intermediate', 'Advanced', 'Expert', 'Master'];

  var TOOLS = [
    { id: 'shovel', name: 'Shovel' },
    { id: 'wateringCan', name: 'Watering Can' },
    { id: 'rake', name: 'Rake' },
    { id: 'hoe', name: 'Hoe' },
    { id: 'scythe', name: 'Scythe' },
    { id: 'cart', name: 'Cart' }
  ];

  var PLOT_COSTS = [0, 250, 600, 1200, 2500];

  FIF.config = {
    TILE_SIZE: TILE_SIZE,
    SEASONS: SEASONS,
    QUALITIES: QUALITIES,
    CROPS: CROPS,
    TRACTOR_TYPES: TRACTOR_TYPES,
    ATTACHMENTS: ATTACHMENTS,
    SKILLS: SKILLS,
    LEVELS: LEVELS,
    LEVEL_NAMES: LEVEL_NAMES,
    TOOLS: TOOLS,
    PLOT_COSTS: PLOT_COSTS,
    SAVE_KEY: 'fif_save_v1'
  };
})(window.FIF);
