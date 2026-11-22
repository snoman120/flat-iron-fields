/*
 * Flat Iron Fields - Economy System
 * Owner: Systems Agent
 * Classic script. Attaches to window.FIF.
 */
(function () {
  var FIF = window.FIF = window.FIF || {};

  var QUALITY_MULTIPLIER = {
    Poor: 0.6,
    Normal: 1.0,
    Good: 1.4,
    Perfect: 1.8
  };

  var DEFAULT_QUALITIES = ['Poor', 'Normal', 'Good', 'Perfect'];

  /* Land price indexed by the expansion stage being purchased. */
  var LAND_COSTS = [0, 200, 600, 1500, 3000];

  var DEFAULT_RECIPES = {
    fertilizer: { coins: 10, crops: { grass: 2 }, output: 'fertilizer', amount: 1 },
    fuel: { coins: 15, crops: {}, output: 'fuel', amount: 1 },
    feed: { coins: 5, crops: { wheat: 2 }, output: 'feed', amount: 1 }
  };

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

  function cropList() {
    if (FIF.config && FIF.config.CROPS) return asArray(FIF.config.CROPS);
    return [];
  }

  function findCrop(cropId) {
    var list = cropList();
    for (var i = 0; i < list.length; i++) if (list[i] && list[i].id === cropId) return list[i];
    return null;
  }

  function tractorList() {
    if (FIF.config && FIF.config.TRACTOR_TYPES) return asArray(FIF.config.TRACTOR_TYPES);
    return [];
  }

  function findTractor(typeId) {
    var list = tractorList();
    for (var i = 0; i < list.length; i++) if (list[i] && list[i].id === typeId) return list[i];
    return null;
  }

  function qualityList() {
    if (FIF.config && FIF.config.QUALITIES && FIF.config.QUALITIES.length) return FIF.config.QUALITIES;
    return DEFAULT_QUALITIES;
  }

  function normalizeQuality(quality) {
    var list = qualityList();
    for (var i = 0; i < list.length; i++) if (list[i] === quality) return quality;
    return 'Normal';
  }

  function multiplier(quality) {
    var m = QUALITY_MULTIPLIER[normalizeQuality(quality)];
    return typeof m === 'number' ? m : 1;
  }

  function inventory() {
    if (!FIF.state || !FIF.state.inventory) return null;
    return FIF.state.inventory;
  }

  /*
   * Locates a numeric counter inside a seeds/crops map. Supports plain
   * number maps, object buckets with quantity, quality-keyed maps, and
   * arrays of {cropId, quantity} entries so parallel agents can integrate.
   */
  function locate(bucket, key, quality, create) {
    if (!bucket || !key) return null;
    if (isArray(bucket)) {
      for (var i = 0; i < bucket.length; i++) {
        var entry = bucket[i];
        if (entry && entry.cropId === key && (!quality || !entry.quality || entry.quality === quality)) return entry;
      }
      if (create) {
        var added = { cropId: key, quantity: 0 };
        if (quality) added.quality = quality;
        bucket.push(added);
        return added;
      }
      return null;
    }
    var direct = bucket[key];
    if (direct && typeof direct === 'object' && !isArray(direct)) {
      if (direct.quantity !== undefined || direct.count !== undefined) {
        if (!quality || !direct.quality || direct.quality === quality) return direct;
      } else if (!quality) {
        /* crops[cropId][quality] map: read across all qualities. */
        return { bucket: direct, nested: true };
      } else {
        if (typeof direct[quality] !== 'number') direct[quality] = 0;
        return { bucket: direct, key: quality, numeric: true };
      }
    }
    if (typeof direct === 'number') return { bucket: bucket, key: key, numeric: true };
    if (!quality) {
      if (create) {
        bucket[key] = 0;
        return { bucket: bucket, key: key, numeric: true };
      }
      return null;
    }
    var qkey = key + ':' + quality;
    var alt = bucket[qkey];
    if (alt && typeof alt === 'object') return alt;
    if (typeof alt === 'number') return { bucket: bucket, key: qkey, numeric: true };
    if (create) {
      bucket[qkey] = 0;
      return { bucket: bucket, key: qkey, numeric: true };
    }
    return null;
  }

  function readCount(loc) {
    if (!loc) return 0;
    if (loc.numeric) return Math.max(0, Math.floor(num(loc.bucket[loc.key], 0)));
    if (loc.nested) {
      var sum = 0;
      for (var q in loc.bucket) {
        if (Object.prototype.hasOwnProperty.call(loc.bucket, q)) sum += num(loc.bucket[q], 0);
      }
      return Math.max(0, Math.floor(sum));
    }
    return Math.max(0, Math.floor(num(loc.quantity !== undefined ? loc.quantity : loc.count, 0)));
  }

  function writeCount(loc, value) {
    if (!loc) return 0;
    var next = Math.max(0, Math.floor(num(value, 0)));
    if (loc.numeric) loc.bucket[loc.key] = next;
    else if (loc.nested) {
      var total = 0;
      var q;
      for (q in loc.bucket) {
        if (Object.prototype.hasOwnProperty.call(loc.bucket, q)) total += num(loc.bucket[q], 0);
      }
      var toRemove = Math.max(0, total - next);
      for (q in loc.bucket) {
        if (!Object.prototype.hasOwnProperty.call(loc.bucket, q)) continue;
        var have = num(loc.bucket[q], 0);
        var used = Math.min(have, toRemove);
        loc.bucket[q] = have - used;
        toRemove -= used;
        if (toRemove <= 0) break;
      }
    } else if (loc.quantity !== undefined) loc.quantity = next;
    else loc.count = next;
    return next;
  }

  function addToBucket(bucket, key, quality, amount) {
    if (!bucket || !key || amount <= 0) return 0;
    var loc = locate(bucket, key, quality, true);
    if (!loc) return 0;
    writeCount(loc, readCount(loc) + amount);
    return amount;
  }

  function takeFromBucket(bucket, key, quality, amount) {
    if (amount <= 0) return true;
    var loc = locate(bucket, key, quality, false);
    var have = readCount(loc);
    if (have < amount) return false;
    writeCount(loc, have - amount);
    return true;
  }

  function addCoins(amount) {
    if (!FIF.state) return 0;
    FIF.state.coins = Math.max(0, Math.floor(num(FIF.state.coins, 0) + num(amount, 0)));
    return FIF.state.coins;
  }

  function canAfford(cost) {
    if (!FIF.state) return false;
    return num(FIF.state.coins, 0) >= cost;
  }

  function grantXP(amount, reason) {
    if (FIF.progression && typeof FIF.progression.addXP === 'function') {
      FIF.progression.addXP(amount, reason);
    }
  }

  function price(cropId, quality) {
    var crop = findCrop(cropId);
    var base = crop ? num(crop.salePrice, 0) : 0;
    if (base <= 0) return 0;
    return Math.max(1, Math.floor(base * multiplier(quality) + 0.5));
  }

  function sell(cropId, quantity, quality) {
    var qty = Math.floor(num(quantity, 1));
    if (!cropId || qty <= 0 || !findCrop(cropId)) {
      message('Cannot sell that crop.');
      return 0;
    }
    var q = normalizeQuality(quality);
    var inv = inventory();
    if (!inv) return 0;
    var have = readCount(locate(inv.crops, cropId, q, false));
    /* The cart tool sells a freshly harvested yield that never enters inventory. */
    var directSale = (have === 0 && !inv.crops[cropId]);
    if (have < qty && !directSale) {
      message('Not enough ' + cropId + ' to sell.');
      return 0;
    }
    var earned = price(cropId, q) * qty;
    if (!directSale) takeFromBucket(inv.crops, cropId, q, qty);
    addCoins(earned);
    if (FIF.state.stats) {
      FIF.state.stats.sold = num(FIF.state.stats.sold, 0) + qty;
      FIF.state.stats.earned = num(FIF.state.stats.earned, 0) + earned;
    }
    grantXP(Math.max(1, qty), 'sale');
    message('Sold ' + qty + ' ' + cropId + ' for ' + earned + ' coins.');
    return earned;
  }

  function buySeed(cropId, quantity) {
    var qty = Math.floor(num(quantity, 1));
    var crop = findCrop(cropId);
    if (!crop || qty <= 0) {
      message('That seed is not available.');
      return false;
    }
    var cost = Math.max(1, Math.floor(num(crop.seedCost, 5))) * qty;
    if (!canAfford(cost)) {
      message('Not enough coins for ' + cropId + ' seeds.');
      return false;
    }
    var inv = inventory();
    if (!inv) return false;
    addCoins(-cost);
    addToBucket(inv.seeds, cropId, null, qty);
    message('Bought ' + qty + ' ' + cropId + ' seed(s) for ' + cost + ' coins.');
    return true;
  }

  function maxExpansionStage() {
    if (FIF.config && FIF.config.PLOT_COSTS && FIF.config.PLOT_COSTS.length) {
      return Math.max(1, FIF.config.PLOT_COSTS.length - 1);
    }
    return 4;
  }

  function landCost(stage) {
    var source = (FIF.config && FIF.config.PLOT_COSTS) || LAND_COSTS;
    var value = asArray(source)[stage];
    if (value && value.cost !== undefined) return Math.max(0, Math.floor(num(value.cost, 0)));
    if (value !== undefined) return Math.max(0, Math.floor(num(value, 0)));
    return 1000 + (stage - 4) * 1000;
  }

  function buyPlot() {
    if (!FIF.state || !FIF.state.farm) return false;
    var current = Math.floor(num(FIF.state.farm.expansionStage, 0));
    var next = current + 1;
    if (next > maxExpansionStage()) {
      message('Farm is fully expanded.');
      return false;
    }
    var cost = landCost(next);
    if (!canAfford(cost)) {
      message('Not enough coins to expand the farm (' + cost + ').');
      return false;
    }
    addCoins(-cost);
    if (FIF.plots && typeof FIF.plots.expand === 'function') FIF.plots.expand(next);
    FIF.state.farm.expansionStage = next;
    grantXP(25, 'expansion');
    message('Farm expanded to stage ' + next + ' for ' + cost + ' coins.');
    return true;
  }

  function buyTractor(typeId) {
    var type = findTractor(typeId);
    if (!type) {
      message('Unknown tractor type.');
      return false;
    }
    var level = 1;
    if (FIF.progression && typeof FIF.progression.levelForXP === 'function') {
      var xp = (FIF.state && FIF.state.player) ? FIF.state.player.xp : 0;
      level = num(FIF.progression.levelForXP(xp), 1);
    } else if (FIF.state && FIF.state.player) {
      level = num(FIF.state.player.level, 1);
    }
    var required = num(type.unlockedAtLevel, 1);
    if (level < required) {
      message('Tractor unlocks at level ' + required + '.');
      return false;
    }
    var cost = Math.max(0, Math.floor(num(type.cost, 0)));
    if (!canAfford(cost)) {
      message('Not enough coins for the ' + type.name + '.');
      return false;
    }
    addCoins(-cost);
    if (FIF.tractors && typeof FIF.tractors.create === 'function') FIF.tractors.create(type.id);
    else if (FIF.state.tractor) FIF.state.tractor.type = type.id;
    message('Bought the ' + type.name + ' for ' + cost + ' coins.');
    return true;
  }

  function recipes() {
    if (FIF.config && FIF.config.RECIPES) return FIF.config.RECIPES;
    return DEFAULT_RECIPES;
  }

  function craft(itemId) {
    var all = recipes();
    var recipe = all && all[itemId];
    if (!recipe || typeof recipe !== 'object') {
      message('Unknown recipe.');
      return false;
    }
    var inv = inventory();
    if (!inv) return false;
    var cost = Math.max(0, Math.floor(num(recipe.coins, 0)));
    if (!canAfford(cost)) {
      message('Not enough coins to craft ' + itemId + '.');
      return false;
    }
    var needed = recipe.crops || {};
    for (var id in needed) {
      if (!Object.prototype.hasOwnProperty.call(needed, id)) continue;
      var count = Math.floor(num(needed[id], 0));
      if (count > 0 && readCount(locate(inv.crops, id, null, false)) < count) {
        message('Missing materials for ' + itemId + '.');
        return false;
      }
    }
    addCoins(-cost);
    for (var cropId in needed) {
      if (Object.prototype.hasOwnProperty.call(needed, cropId)) {
        takeFromBucket(inv.crops, cropId, null, Math.floor(num(needed[cropId], 0)));
      }
    }
    var output = recipe.output || itemId;
    var amount = Math.max(1, Math.floor(num(recipe.amount, 1)));
    var target = inv.items;
    if (!target || typeof target !== 'object' || isArray(target)) {
      inv.items = {};
      target = inv.items;
    }
    addToBucket(target, output, null, amount);
    grantXP(5, 'craft');
    message('Crafted ' + amount + ' ' + output + '.');
    return true;
  }

  FIF.economy = {
    price: price,
    sell: sell,
    buySeed: buySeed,
    buyPlot: buyPlot,
    buyTractor: buyTractor,
    craft: craft
  };
}());
