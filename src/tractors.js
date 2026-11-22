/*
 * Flat Iron Fields - Tractor System
 * Owner: Systems Agent
 * Classic script. Attaches to window.FIF.
 */
(function () {
  var FIF = window.FIF = window.FIF || {};

  var DEFAULT_TYPES = [
    { id: 'basic', name: 'Basic Tractor', speed: 110, capacity: 20, cost: 250, unlockedAtLevel: 1 },
    { id: 'crop', name: 'Crop Tractor', speed: 130, capacity: 30, cost: 600, unlockedAtLevel: 2 },
    { id: 'heavy', name: 'Heavy Duty', speed: 80, capacity: 60, cost: 1200, unlockedAtLevel: 3 },
    { id: 'specialized', name: 'Specialized', speed: 100, capacity: 40, cost: 1800, unlockedAtLevel: 4 }
  ];

  var DEFAULT_ATTACHMENTS = [
    { id: 'tillage', name: 'Tillage Attachment', description: 'Prepares soil for planting.' },
    { id: 'planter', name: 'Planter Attachment', description: 'Plants selected seeds automatically.' },
    { id: 'harvester', name: 'Harvester Attachment', description: 'Harvests mature crops into cargo.' },
    { id: 'transporter', name: 'Transporter Attachment', description: 'Unloads cargo into the inventory.' },
    { id: 'watering', name: 'Watering Attachment', description: 'Irrigates crops automatically.' },
    { id: 'fertilizer', name: 'Fertilizer Attachment', description: 'Adds a quality bonus to a plot.' }
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

  function mergeDefaults(list, defaults) {
    var out = asArray(list), i, j, found;
    for (i = 0; i < defaults.length; i++) {
      found = false;
      for (j = 0; j < out.length; j++) if (out[j] && out[j].id === defaults[i].id) { found = true; break; }
      if (!found) out.push(defaults[i]);
    }
    return out;
  }

  function message(text) {
    if (FIF.ui && typeof FIF.ui.message === 'function') { FIF.ui.message(text); return; }
    if (FIF.state && FIF.state.messages && isArray(FIF.state.messages)) FIF.state.messages.push(String(text));
  }

  function ensureTractor() {
    if (!FIF.state) return null;
    if (!FIF.state.tractor || typeof FIF.state.tractor !== 'object' || isArray(FIF.state.tractor)) {
      FIF.state.tractor = {};
    }
    return FIF.state.tractor;
  }

  function types() {
    var c = FIF.config || {};
    return mergeDefaults(c.TRACTOR_TYPES, DEFAULT_TYPES);
  }

  function attachments() {
    var c = FIF.config || {};
    return mergeDefaults(c.ATTACHMENTS, DEFAULT_ATTACHMENTS);
  }

  function findType(typeId) {
    var list = types();
    for (var i = 0; i < list.length; i++) if (list[i] && list[i].id === typeId) return list[i];
    return DEFAULT_TYPES[0];
  }

  function findAttachment(id) {
    var list = attachments();
    for (var i = 0; i < list.length; i++) if (list[i] && list[i].id === id) return list[i];
    return null;
  }

  function tileSize() {
    var size = num(FIF.config && FIF.config.TILE_SIZE, 32);
    return size >= 8 ? size : 32;
  }

  function farmBounds() {
    var cols = 20;
    var rows = 20;
    var shape = null;
    if (FIF.plots && typeof FIF.plots.plotShape === 'function') {
      try {
        var stage = (FIF.state && FIF.state.farm) ? FIF.state.farm.expansionStage : 0;
        shape = FIF.plots.plotShape(stage);
      } catch (error) { shape = null; }
    }
    if (shape) {
      cols = num(shape.tileCols, num(shape.cols, cols));
      rows = num(shape.tileRows, num(shape.rows, rows));
    }
    var size = tileSize();
    return { width: Math.max(1, cols) * size, height: Math.max(1, rows) * size };
  }

  function create(typeId) {
    var tractor = ensureTractor();
    if (!tractor) return null;
    var bounds = farmBounds();
    tractor.type = findType(typeId || tractor.type || 'basic').id;
    tractor.x = num(tractor.x, bounds.width / 2);
    tractor.y = num(tractor.y, bounds.height / 2);
    if (!findAttachment(tractor.attachment)) tractor.attachment = 'tillage';
    tractor.fuel = num(tractor.fuel, 100);
    if (tractor.fuel <= 0) tractor.fuel = 100;
    if (!tractor.cargo || typeof tractor.cargo !== 'object') tractor.cargo = {};
    return tractor;
  }

  function attach(attachmentId) {
    var tractor = ensureTractor();
    if (!tractor) return null;
    var attachment = findAttachment(attachmentId);
    if (!attachment) { message('Unknown tractor attachment.'); return tractor.attachment || null; }
    tractor.attachment = attachment.id;
    message(attachment.name + ' equipped.');
    return attachment.id;
  }

  function held(keys, name) {
    if (!keys) return false;
    var lower = String(name).toLowerCase();
    var base = lower.replace('arrow', '');
    var checks = [name, lower, lower.toUpperCase(), 'arrow' + base,
      'Arrow' + base.charAt(0).toUpperCase() + base.slice(1),
      lower.charAt(0).toUpperCase() + lower.slice(1)];
    for (var i = 0; i < checks.length; i++) if (keys[checks[i]]) return true;
    return false;
  }

  /* cargo is {cropId: {quality: count}}; legacy number/array shapes also work. */
  function cargoCount(cargo) {
    if (!cargo) return 0;
    var total = 0;
    var key;
    var q;
    if (isArray(cargo)) {
      for (var i = 0; i < cargo.length; i++) total += num(cargo[i] && cargo[i].quantity, 0);
      return total;
    }
    for (key in cargo) {
      if (!Object.prototype.hasOwnProperty.call(cargo, key)) continue;
      if (typeof cargo[key] === 'number') total += num(cargo[key], 0);
      else if (cargo[key]) {
        for (q in cargo[key]) {
          if (Object.prototype.hasOwnProperty.call(cargo[key], q)) total += num(cargo[key][q], 0);
        }
      }
    }
    return total;
  }

  function addCargo(tractor, cropId, quantity, quality) {
    if (!tractor || !cropId) return 0;
    var room = Math.max(1, num(findType(tractor.type).capacity, 20)) - cargoCount(tractor.cargo);
    var amount = Math.min(room, Math.floor(num(quantity, 0)));
    if (amount <= 0) { message('Tractor cargo is full.'); return 0; }
    if (!tractor.cargo || typeof tractor.cargo !== 'object') tractor.cargo = {};
    var q = quality || 'Normal';
    if (isArray(tractor.cargo)) {
      var found = null;
      for (var i = 0; i < tractor.cargo.length; i++) {
        if (tractor.cargo[i] && tractor.cargo[i].cropId === cropId &&
            tractor.cargo[i].quality === q) found = tractor.cargo[i];
      }
      if (found) found.quantity = num(found.quantity, 0) + amount;
      else tractor.cargo.push({ cropId: cropId, quality: q, quantity: amount });
    } else {
      if (typeof tractor.cargo[cropId] !== 'object' || !tractor.cargo[cropId]) {
        tractor.cargo[cropId] = {};
      }
      tractor.cargo[cropId][q] = num(tractor.cargo[cropId][q], 0) + amount;
    }
    return amount;
  }

  /* Matches FIF.ui.plotFromTractor: pixel position / TILE_SIZE, rounded. */
  function plotAt(x, y, state) {
    if (!state || !state.farm || !isArray(state.farm.plots)) return null;
    var plots = state.farm.plots;
    var size = tileSize();
    var col = Math.round(num(x, 0) / size);
    var row = Math.round(num(y, 0) / size);
    for (var i = 0; i < plots.length; i++) {
      var plot = plots[i];
      if (!plot) continue;
      if (num(plot.x, -1) === col && num(plot.y, -1) === row) return plot;
    }
    for (var j = 0; j < plots.length; j++) {
      if (plots[j] && num(plots[j].x, -1) === Math.round(num(x, 0)) &&
          num(plots[j].y, -1) === Math.round(num(y, 0))) return plots[j];
    }
    return null;
  }

  function update(dt, keys, state) {
    var current = state || FIF.state;
    var tractor = (current && current.tractor) ? current.tractor : ensureTractor();
    if (!tractor) return null;
    var seconds = Math.min(0.25, Math.max(0, num(dt, 0)));
    var type = findType(tractor.type);
    var input = keys || (FIF.input && FIF.input.keys) || {};
    var dx = 0;
    var dy = 0;
    if (held(input, 'w') || held(input, 'arrowup') || held(input, 'up')) dy -= 1;
    if (held(input, 's') || held(input, 'arrowdown') || held(input, 'down')) dy += 1;
    if (held(input, 'a') || held(input, 'arrowleft') || held(input, 'left')) dx -= 1;
    if (held(input, 'd') || held(input, 'arrowright') || held(input, 'right')) dx += 1;

    var moving = (dx !== 0 || dy !== 0);
    var fuel = num(tractor.fuel, 0);
    if (moving && fuel > 0 && seconds > 0) {
      var length = Math.sqrt(dx * dx + dy * dy);
      var speed = num(type.speed, 100);
      tractor.x = num(tractor.x, 0) + (dx / length) * speed * seconds;
      tractor.y = num(tractor.y, 0) + (dy / length) * speed * seconds;
      tractor.fuel = Math.max(0, fuel - seconds * 1.5);
    }
    var bounds = farmBounds();
    tractor.x = Math.max(0, Math.min(bounds.width, num(tractor.x, 0)));
    tractor.y = Math.max(0, Math.min(bounds.height, num(tractor.y, 0)));

    if (moving && fuel <= 0 && !tractor.fuelWarning) {
      message('Tractor is out of fuel.');
      tractor.fuelWarning = true;
    }
    if (!moving || fuel > 0) tractor.fuelWarning = false;

    if (moving && fuel > 0 && seconds > 0) {
      var plot = plotAt(tractor.x, tractor.y, current);
      if (plot && plot.id !== tractor.lastAutoPlot) {
        tractor.lastAutoPlot = plot.id;
        applyAttachment(plot, tractor.attachment, current);
      }
    }
    return tractor;
  }

  function callPlots(name, plot) {
    if (!FIF.plots || typeof FIF.plots[name] !== 'function') return false;
    try { return FIF.plots[name](plot) !== false; } catch (error) { return false; }
  }

  function seedCount(seeds, cropId) {
    if (!seeds || !cropId || !seeds[cropId]) return 0;
    if (typeof seeds[cropId] === 'number') return num(seeds[cropId], 0);
    return num(seeds[cropId].quantity !== undefined ? seeds[cropId].quantity : seeds[cropId].count, 0);
  }

  function takeSeed(seeds, cropId) {
    if (!seeds || !cropId || !seeds[cropId]) return;
    if (typeof seeds[cropId] === 'number') seeds[cropId] = Math.max(0, num(seeds[cropId], 0) - 1);
    else {
      var bucket = seeds[cropId];
      if (bucket.quantity !== undefined) bucket.quantity = Math.max(0, num(bucket.quantity, 0) - 1);
      else bucket.count = Math.max(0, num(bucket.count, 0) - 1);
    }
  }

  function plantWithTractor(plot, state) {
    var cropId = (state && state.selectedCrop) || (FIF.state && FIF.state.selectedCrop) || plot.cropId;
    if (!cropId || !FIF.plots || typeof FIF.plots.plant !== 'function') return false;
    if (FIF.crops && typeof FIF.crops.isInSeason === 'function') {
      var season = (state && state.seasonIndex) || (FIF.state && FIF.state.seasonIndex) || 0;
      if (!FIF.crops.isInSeason(cropId, season)) return false;
    }
    var seeds = (state && state.inventory && state.inventory.seeds) ||
      (FIF.state && FIF.state.inventory && FIF.state.inventory.seeds);
    if (seeds && seedCount(seeds, cropId) <= 0) { message('No ' + cropId + ' seeds.'); return false; }
    var result = false;
    try { result = FIF.plots.plant(plot, cropId); } catch (error) { return false; }
    if (result === false) return false;
    takeSeed(seeds, cropId);
    return true;
  }

  function deposit(inventory, cropId, quality, amount) {
    if (!inventory || !cropId || amount <= 0) return 0;
    if (!inventory.crops || typeof inventory.crops !== 'object') inventory.crops = {};
    var bucket = inventory.crops[cropId];
    if (typeof bucket === 'number') bucket = inventory.crops[cropId] = { Normal: bucket };
    if (!bucket || typeof bucket !== 'object') bucket = inventory.crops[cropId] = {};
    var q = quality || 'Normal';
    bucket[q] = num(bucket[q], 0) + amount;
    return amount;
  }

  function harvestWithTractor(plot, state, tractor) {
    if (!FIF.plots || typeof FIF.plots.harvest !== 'function') return false;
    var cropId = plot.cropId;
    var result = null;
    try { result = FIF.plots.harvest(plot); } catch (error) { return false; }
    if (result === false) return false;
    var quantity = 1;
    var quality = 'Normal';
    if (!result && FIF.crops && typeof FIF.crops.yieldFor === 'function') {
      try { result = FIF.crops.yieldFor(plot); } catch (error) { result = null; }
    }
    if (result && typeof result === 'object') {
      cropId = result.cropId || cropId;
      quantity = num(result.quantity !== undefined ? result.quantity : result.count, 1);
      quality = result.quality || quality;
    }
    if (!cropId || quantity <= 0) return false;
    addCargo(tractor, cropId, quantity, quality);
    return true;
  }

  function transport(state, tractor) {
    var inventory = (state && state.inventory) || (FIF.state && FIF.state.inventory);
    if (!inventory || !tractor || !tractor.cargo) return false;
    var moved = 0;
    var key;
    var q;
    if (isArray(tractor.cargo)) {
      for (var i = 0; i < tractor.cargo.length; i++) {
        var entry = tractor.cargo[i];
        if (entry && entry.cropId) {
          moved += deposit(inventory, entry.cropId, entry.quality, num(entry.quantity, 0));
        }
      }
    } else {
      for (key in tractor.cargo) {
        if (!Object.prototype.hasOwnProperty.call(tractor.cargo, key)) continue;
        if (typeof tractor.cargo[key] === 'number') {
          moved += deposit(inventory, key, 'Normal', num(tractor.cargo[key], 0));
        } else if (tractor.cargo[key]) {
          for (q in tractor.cargo[key]) {
            if (!Object.prototype.hasOwnProperty.call(tractor.cargo[key], q)) continue;
            moved += deposit(inventory, key, q, num(tractor.cargo[key][q], 0));
          }
        }
      }
    }
    tractor.cargo = {};
    if (moved > 0) message('Unloaded ' + moved + ' crop(s) into inventory.');
    return moved > 0;
  }

  function applyAttachment(plot, attachmentId, state) {
    if (!plot || typeof plot !== 'object') return false;
    var current = state || FIF.state;
    var tractor = (current && current.tractor) ? current.tractor : ensureTractor();
    var id = attachmentId || (tractor && tractor.attachment) || 'tillage';
    if (!findAttachment(id)) return false;
    if (tractor && num(tractor.fuel, 0) <= 0) {
      message('Tractor needs fuel.');
      return false;
    }
    if (tractor) tractor.fuel = Math.max(0, num(tractor.fuel, 0) - 0.2);

    var done = false;
    if (id === 'tillage') done = callPlots('till', plot);
    else if (id === 'watering') done = callPlots('water', plot);
    else if (id === 'planter') done = plantWithTractor(plot, current);
    else if (id === 'harvester') done = harvestWithTractor(plot, current, tractor);
    else if (id === 'transporter') done = transport(current, tractor);
    else if (id === 'fertilizer') {
      var bonus = num(plot.qualityBonus, 0);
      if (bonus >= 3) message('Plot is already fertilized.');
      else {
        plot.qualityBonus = bonus + 1;
        done = true;
      }
    }
    if (done && FIF.progression && typeof FIF.progression.addXP === 'function') {
      FIF.progression.addXP(2, id);
    }
    return done;
  }

  FIF.tractors = {
    create: create,
    attach: attach,
    update: update,
    applyAttachment: applyAttachment,
    types: types,
    attachments: attachments
  };
}());
