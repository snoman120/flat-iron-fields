/* Flat Iron Fields - global game state and persistence.
 * Classic script. Owned by Core Systems agent.
 */
window.FIF = window.FIF || {};

(function (FIF) {
  var SAVE_KEY = 'fif_save_v1';

  function isObject(value) {
    return !!value && typeof value === 'object' && !Array.isArray(value);
  }

  function defaultState() {
    var plots = [];
    var config = FIF.config;
    var skills = {};
    var i;

    if (config && Array.isArray(config.SKILLS)) {
      for (i = 0; i < config.SKILLS.length; i++) {
        skills[config.SKILLS[i].id] = 0;
      }
    }

    if (FIF.plots && typeof FIF.plots.createFarm === 'function') {
      plots = FIF.plots.createFarm(0);
    }

    return {
      day: 1,
      seasonIndex: 0,
      coins: 100,
      selectedTool: 'shovel',
      selectedCrop: 'carrot',
      weather: 'clear',
      inventory: {
        seeds: { carrot: 5, cabbage: 3, grass: 3, wheat: 3 },
        crops: {},
        tools: ['shovel', 'wateringCan', 'rake', 'hoe', 'scythe', 'cart']
      },
      farm: {
        plots: plots,
        expansionStage: 0
      },
      tractor: {
        type: 'basic',
        x: 0,
        y: 0,
        attachment: 'tillage',
        fuel: 100,
        cargo: {}
      },
      player: {
        xp: 0,
        level: 1,
        skillPoints: 0,
        skills: skills,
        unlocked: []
      },
      messages: [],
      stats: { planted: 0, harvested: 0, sold: 0, earned: 0 }
    };
  }

  function replaceState(next) {
    var key;
    for (key in FIF.state) {
      if (Object.prototype.hasOwnProperty.call(FIF.state, key)) {
        delete FIF.state[key];
      }
    }
    for (key in next) {
      if (Object.prototype.hasOwnProperty.call(next, key)) {
        FIF.state[key] = next[key];
      }
    }
    return FIF.state;
  }

  // Rebuild the state in place so other modules keep valid references.
  FIF.reset = function () {
    return replaceState(defaultState());
  };

  FIF.state = defaultState();

  FIF.save = function () {
    try {
      if (typeof localStorage === 'undefined' || !localStorage) {
        return false;
      }
      var key = (FIF.config && FIF.config.SAVE_KEY) || SAVE_KEY;
      localStorage.setItem(key, JSON.stringify(FIF.state));
      return true;
    } catch (err) {
      return false;
    }
  };

  FIF.load = function () {
    var raw;
    var data;
    var next;
    var config = FIF.config;
    var key = (config && config.SAVE_KEY) || SAVE_KEY;

    try {
      if (typeof localStorage === 'undefined' || !localStorage) {
        return false;
      }
      raw = localStorage.getItem(key);
    } catch (err) {
      return false;
    }

    if (!raw) {
      return false;
    }

    try {
      data = JSON.parse(raw);
    } catch (err) {
      return false;
    }

    if (!isObject(data)) {
      return false;
    }

    next = defaultState();

    if (typeof data.day === 'number') { next.day = data.day; }
    if (typeof data.seasonIndex === 'number') { next.seasonIndex = data.seasonIndex; }
    if (typeof data.coins === 'number') { next.coins = data.coins; }
    if (typeof data.selectedTool === 'string') { next.selectedTool = data.selectedTool; }
    if (typeof data.selectedCrop === 'string') { next.selectedCrop = data.selectedCrop; }
    if (typeof data.weather === 'string') { next.weather = data.weather; }

    if (isObject(data.inventory)) {
      if (isObject(data.inventory.seeds)) { next.inventory.seeds = data.inventory.seeds; }
      if (isObject(data.inventory.crops)) { next.inventory.crops = data.inventory.crops; }
      if (Array.isArray(data.inventory.tools)) { next.inventory.tools = data.inventory.tools; }
    }

    if (isObject(data.farm)) {
      if (Array.isArray(data.farm.plots) && data.farm.plots.length > 0) {
        next.farm.plots = data.farm.plots;
      }
      if (typeof data.farm.expansionStage === 'number') {
        next.farm.expansionStage = data.farm.expansionStage;
      }
    }

    if (isObject(data.tractor)) {
      if (typeof data.tractor.type === 'string') { next.tractor.type = data.tractor.type; }
      if (typeof data.tractor.x === 'number') { next.tractor.x = data.tractor.x; }
      if (typeof data.tractor.y === 'number') { next.tractor.y = data.tractor.y; }
      if (typeof data.tractor.attachment === 'string') { next.tractor.attachment = data.tractor.attachment; }
      if (typeof data.tractor.fuel === 'number') { next.tractor.fuel = data.tractor.fuel; }
      if (isObject(data.tractor.cargo)) { next.tractor.cargo = data.tractor.cargo; }
    }

    if (isObject(data.player)) {
      if (typeof data.player.xp === 'number') { next.player.xp = data.player.xp; }
      if (typeof data.player.level === 'number') { next.player.level = data.player.level; }
      if (typeof data.player.skillPoints === 'number') { next.player.skillPoints = data.player.skillPoints; }
      if (isObject(data.player.skills)) { next.player.skills = data.player.skills; }
      if (Array.isArray(data.player.unlocked)) { next.player.unlocked = data.player.unlocked; }
    }

    if (Array.isArray(data.messages)) { next.messages = data.messages.slice(-50); }
    if (isObject(data.stats)) { next.stats = data.stats; }

    replaceState(next);
    return true;
  };
})(window.FIF);
