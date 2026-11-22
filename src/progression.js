/*
 * Flat Iron Fields - Player Progression System
 * Owner: Systems Agent
 * Classic script. Attaches to window.FIF.
 */
(function () {
  var FIF = window.FIF = window.FIF || {};

  var LEVEL_NAMES = ['Noob', 'Beginner', 'Intermediate', 'Advanced', 'Expert', 'Master'];
  var DEFAULT_LEVELS = [0, 100, 250, 500, 900, 1500];

  var DEFAULT_SKILLS = [
    { id: 'planting', name: 'Planting Efficiency', description: 'Plant more crops with fewer actions.' },
    { id: 'harvesting', name: 'Harvesting Speed', description: 'Harvest crops faster.' },
    { id: 'cropSelection', name: 'Crop Selection', description: 'Grow a wider variety of crops.' },
    { id: 'resourceManagement', name: 'Resource Management', description: 'Get more from every resource.' },
    { id: 'tractorOperation', name: 'Tractor Operation', description: 'Improve tractor speed and fuel use.' },
    { id: 'farmExpansion', name: 'Farm Expansion', description: 'Expand the farm further.' }
  ];

  var DEFAULT_DECORATIONS = [
    { id: 'fence', name: 'Wooden Fence', unlockLevel: 1 },
    { id: 'scarecrow', name: 'Scarecrow', unlockLevel: 2 },
    { id: 'lamp', name: 'Field Lamp', unlockLevel: 3 },
    { id: 'flowerbed', name: 'Flower Bed', unlockLevel: 4 },
    { id: 'fountain', name: 'Fountain', unlockLevel: 5 },
    { id: 'statue', name: 'Golden Statue', unlockLevel: 6 }
  ];

  var DEFAULT_TRACTORS = [
    { id: 'basic', name: 'Basic Tractor', unlockedAtLevel: 1 },
    { id: 'crop', name: 'Crop Tractor', unlockedAtLevel: 2 },
    { id: 'heavy', name: 'Heavy Duty', unlockedAtLevel: 3 },
    { id: 'specialized', name: 'Specialized', unlockedAtLevel: 4 }
  ];

  var DEFAULT_CROPS = [
    'grass', 'cabbage', 'wheat', 'carrot', 'sweetPotato', 'potato', 'corn',
    'pepper', 'tomato', 'watermelon', 'pumpkin', 'onion', 'garlic',
    'strawberry', 'apple', 'peach', 'plum', 'grape', 'cherry', 'orange'
  ];

  function num(value, fallback) {
    var n = Number(value);
    if (!isFinite(n)) return fallback === undefined ? 0 : fallback;
    return n;
  }

  function isArray(value) {
    return Object.prototype.toString.call(value) === '[object Array]';
  }

  function asArray(value) {
    if (!value) return [];
    if (isArray(value)) return value;
    var result = [];
    for (var key in value) {
      if (Object.prototype.hasOwnProperty.call(value, key) && value[key]) result.push(value[key]);
    }
    return result;
  }

  function message(text) {
    if (FIF.ui && typeof FIF.ui.message === 'function') {
      FIF.ui.message(text);
      return;
    }
    if (FIF.state && FIF.state.messages && isArray(FIF.state.messages)) {
      FIF.state.messages.push(String(text));
    }
  }

  function player() {
    if (!FIF.state) return null;
    if (!FIF.state.player || typeof FIF.state.player !== 'object') {
      FIF.state.player = { xp: 0, level: 1, skillPoints: 0, skills: {}, unlocked: {} };
    }
    return FIF.state.player;
  }

  function mergeDefaults(list, defaults) {
    var out = asArray(list);
    for (var i = 0; i < defaults.length; i++) {
      var found = false;
      for (var j = 0; j < out.length; j++) {
        if (out[j] && out[j].id === defaults[i].id) {
          found = true;
          break;
        }
      }
      if (!found) out.push(defaults[i]);
    }
    return out;
  }

  function levels() {
    if (FIF.config && FIF.config.LEVELS && FIF.config.LEVELS.length) return asArray(FIF.config.LEVELS);
    return DEFAULT_LEVELS;
  }

  function skills() {
    if (FIF.config && FIF.config.SKILLS && FIF.config.SKILLS.length) {
      return asArray(FIF.config.SKILLS);
    }
    return DEFAULT_SKILLS;
  }

  function levelForXP(xp) {
    var list = levels();
    var value = Math.max(0, num(xp, 0));
    var level = 1;
    for (var i = 0; i < list.length; i++) {
      if (value >= num(list[i], 0)) level = i + 1;
    }
    return Math.min(level, list.length);
  }

  function levelName(level) {
    var names = (FIF.config && FIF.config.LEVEL_NAMES) ? FIF.config.LEVEL_NAMES : LEVEL_NAMES;
    var index = Math.max(1, Math.floor(num(level, 1))) - 1;
    if (names[index]) return names[index];
    if (LEVEL_NAMES[index]) return LEVEL_NAMES[index];
    return 'Level ' + level;
  }

  function unlockedObject() {
    var p = player();
    if (!p) return null;
    if (!p.unlocked || typeof p.unlocked !== 'object') p.unlocked = {};
    var u = p.unlocked;
    if (!u.crops || typeof u.crops !== 'object' || isArray(u.crops)) u.crops = {};
    if (!u.tractors || typeof u.tractors !== 'object' || isArray(u.tractors)) u.tractors = {};
    if (!u.decorations || typeof u.decorations !== 'object' || isArray(u.decorations)) u.decorations = {};
    return u;
  }

  function markUnlock(u, group, id) {
    if (!u || !id) return false;
    if (!u[group] || typeof u[group] !== 'object') u[group] = {};
    if (u[group][id]) return false;
    u[group][id] = true;
    u[id] = true;
    u[group + ':' + id] = true;
    if (isArray(u) && u.indexOf && u.indexOf(id) < 0) u.push(id);
    return true;
  }

  function unlockName(entry) {
    if (!entry) return 'item';
    return entry.name || entry.id || 'item';
  }

  function checkUnlocks() {
    var p = player();
    var u = unlockedObject();
    if (!p || !u) return null;
    var level = Math.floor(num(p.level, 0));
    if (level < 1) level = levelForXP(p.xp);

    var tractors = (FIF.config && FIF.config.TRACTOR_TYPES) ? asArray(FIF.config.TRACTOR_TYPES) : [];
    if (!tractors.length) tractors = DEFAULT_TRACTORS;
    for (var i = 0; i < tractors.length; i++) {
      var tractor = tractors[i];
      if (!tractor || !tractor.id) continue;
      if (level >= num(tractor.unlockedAtLevel !== undefined ? tractor.unlockedAtLevel : 1, 1) &&
          markUnlock(u, 'tractors', tractor.id)) {
        message('Unlocked ' + unlockName(tractor) + '.');
      }
    }

    var cropList = (FIF.config && FIF.config.CROPS) ? asArray(FIF.config.CROPS) : [];
    if (!cropList.length) {
      for (var c = 0; c < DEFAULT_CROPS.length; c++) cropList.push({ id: DEFAULT_CROPS[c] });
    }
    for (var j = 0; j < cropList.length; j++) {
      var crop = cropList[j];
      if (!crop || !crop.id) continue;
      var need = crop.unlockedAtLevel !== undefined ? crop.unlockedAtLevel : (1 + Math.floor(j / 4));
      need = Math.max(1, Math.min(6, num(need, 1)));
      if (level >= need && markUnlock(u, 'crops', crop.id)) {
        message('New crop available: ' + unlockName(crop) + '.');
      }
    }

    var decorations = (FIF.config && FIF.config.DECORATIONS) ? asArray(FIF.config.DECORATIONS) : DEFAULT_DECORATIONS;
    if (!decorations.length) decorations = DEFAULT_DECORATIONS;
    for (var d = 0; d < decorations.length; d++) {
      var deco = decorations[d];
      if (!deco || !deco.id) continue;
      if (level >= num(deco.unlockLevel !== undefined ? deco.unlockLevel : 1, 1) &&
          markUnlock(u, 'decorations', deco.id)) {
        message('Unlocked decoration: ' + unlockName(deco) + '.');
      }
    }
    return u;
  }

  function addXP(amount, reason) {
    var p = player();
    if (!p) return 0;
    var gain = Math.floor(num(amount, 0));
    if (gain <= 0) return num(p.xp, 0);
    var before = levelForXP(p.xp);
    p.xp = Math.max(0, num(p.xp, 0) + gain);
    var after = levelForXP(p.xp);
    p.level = after;
    if (after > before) {
      var points = after - before;
      p.skillPoints = Math.max(0, num(p.skillPoints, 0)) + points;
      message('Level up! ' + levelName(after) + ' (+' + points + ' skill point' +
        (points > 1 ? 's' : '') + ').');
      checkUnlocks();
    } else if (reason) {
      message('+' + gain + ' XP (' + reason + ')');
    }
    return p.xp;
  }

  function investSkill(skillId) {
    var p = player();
    if (!p) return false;
    var list = skills();
    var found = null;
    for (var i = 0; i < list.length; i++) {
      if (list[i] && list[i].id === skillId) found = list[i];
    }
    if (!found) {
      message('Unknown skill.');
      return false;
    }
    if (num(p.skillPoints, 0) <= 0) {
      message('No skill points available.');
      return false;
    }
    if (!p.skills || typeof p.skills !== 'object' || isArray(p.skills)) p.skills = {};
    var current = Math.floor(num(p.skills[skillId], 0));
    if (current >= 10) {
      message('Skill already maxed.');
      return false;
    }
    p.skills[skillId] = current + 1;
    p.skillPoints = num(p.skillPoints, 0) - 1;
    message(unlockName(found) + ' improved to rank ' + (current + 1) + '.');
    checkUnlocks();
    return true;
  }

  FIF.progression = {
    levelForXP: levelForXP,
    levelName: levelName,
    addXP: addXP,
    investSkill: investSkill,
    checkUnlocks: checkUnlocks,
    skills: skills,
    levels: levels
  };
}());
