(function () {
  window.FIF = window.FIF || {};
  var F = window.FIF;
  var canvas = null, ctx = null, lastTime = 0, running = false, lastSave = 0;

  function config() { return F.config || {}; }
  function list(v) { return v && v.length ? v : []; }
  function str(v) { return v === null || v === undefined ? '' : String(v); }
  function num(v, d) { var n = Number(v); return isFinite(n) ? n : (d || 0); }
  function now() { return window.performance && performance.now ? performance.now() : Date.now(); }
  function msg(text) { if (F.ui && F.ui.message) { F.ui.message(text); } }
  function callFn(mod, name, args) {
    if (mod && typeof mod[name] === 'function') {
      try { return mod[name].apply(mod, args || []); }
      catch (err) { msg(name + ' error: ' + err.message); }
    }
    return null;
  }
  function firstCrop() { var a = list(config().CROPS); return a.length ? a[0].id : null; }
  function makePlots() {
    var plots = [], i;
    if (F.plots && F.plots.createFarm) { plots = list(F.plots.createFarm(0)); }
    if (!plots.length) {
      for (i = 0; i < 5; i += 1) {
        plots.push({ id: i, x: i * 2, y: 0, type: 'regular', state: 'empty', cropId: null,
          stage: 0, water: 0, qualityBonus: 0, ready: false, days: 0, tileW: 2, tileH: 20 });
      }
    }
    return plots;
  }
  function fallbackState() {
    var cfg = config(), types = list(cfg.TRACTOR_TYPES), attachments = list(cfg.ATTACHMENTS);
    var tools = F.ui && F.ui.tools ? F.ui.tools() : [], toolIds = [], seeds = {}, crops = list(cfg.CROPS), i;
    for (i = 0; i < tools.length; i += 1) { toolIds.push(tools[i].id); }
    if (!toolIds.length) { toolIds = ['shovel', 'wateringCan', 'rake', 'hoe', 'scythe', 'cart']; }
    for (i = 0; i < crops.length; i += 1) { seeds[crops[i].id] = 5; }
    return {
      day: 1, seasonIndex: 0, coins: 100, selectedTool: 'shovel', selectedCrop: firstCrop(),
      inventory: { seeds: seeds, crops: {}, tools: toolIds },
      farm: { plots: makePlots(), expansionStage: 0 },
      tractor: { type: types.length ? types[0].id : 'basic', x: 0, y: 0,
        attachment: attachments.length ? attachments[0].id : 'tillage', fuel: 100, cargo: {} },
      player: { xp: 0, level: 1, skillPoints: 0, skills: {}, unlocked: {} },
      messages: [], stats: { planted: 0, harvested: 0, sold: 0, earned: 0 }
    };
  }
  function ensureState(st) {
    var tools, i, base;
    if (!st) { return fallbackState(); }
    if (!st.inventory) { st.inventory = {}; }
    if (!st.inventory.seeds) { st.inventory.seeds = {}; }
    if (!st.inventory.crops) { st.inventory.crops = {}; }
    if (!st.inventory.tools || !st.inventory.tools.length) {
      tools = F.ui && F.ui.tools ? F.ui.tools() : [];
      st.inventory.tools = [];
      for (i = 0; i < tools.length; i += 1) { st.inventory.tools.push(tools[i].id); }
    }
    if (!st.farm) { st.farm = {}; }
    if (!st.farm.plots || !st.farm.plots.length) { st.farm.plots = makePlots(); }
    if (typeof st.farm.expansionStage !== 'number') { st.farm.expansionStage = 0; }
    if (!st.tractor) { st.tractor = fallbackState().tractor; }
    if (!st.tractor.cargo || typeof st.tractor.cargo !== 'object') { st.tractor.cargo = {}; }
    if (!st.player) { st.player = fallbackState().player; }
    if (!st.messages || !st.messages.length) { st.messages = []; }
    if (!st.stats) { st.stats = fallbackState().stats; }
    if (!st.selectedTool) { st.selectedTool = 'shovel'; }
    if (!st.selectedCrop) { st.selectedCrop = firstCrop(); }
    if (typeof st.day !== 'number') { st.day = 1; }
    if (typeof st.seasonIndex !== 'number') { st.seasonIndex = 0; }
    if (typeof st.coins !== 'number') { st.coins = 100; }
    base = config().SEASONS;
    if (st.seasonIndex < 0 || st.seasonIndex >= list(base).length) { st.seasonIndex = 0; }
    return st;
  }
  function state() {
    if (!F.state) { F.state = fallbackState(); }
    F.state = ensureState(F.state);
    return F.state;
  }
  function toolAction(id) {
    var t = str(id).toLowerCase();
    if (t.indexOf('water') >= 0) { return 'water'; }
    if (t.indexOf('seed') >= 0 || t.indexOf('plant') >= 0 || t.indexOf('rake') >= 0) { return 'plant'; }
    if (t.indexOf('scythe') >= 0 || t.indexOf('harvest') >= 0) { return 'harvest'; }
    if (t.indexOf('cart') >= 0 || t.indexOf('sell') >= 0) { return 'sell'; }
    return 'till';
  }
  function seedCount(st, id) { var seeds = st && st.inventory && st.inventory.seeds; return seeds && id ? num(seeds[id], 0) : 0; }
  function takeSeed(st, id) {
    var seeds = st && st.inventory && st.inventory.seeds;
    if (seeds && typeof seeds[id] === 'number') { seeds[id] = Math.max(0, num(seeds[id], 0) - 1); }
  }
  function plotInvoke(name, args, fallback) {
    if (F.plots && typeof F.plots[name] === 'function') { return callFn(F.plots, name, args); }
    if (fallback) { fallback(); }
    return null;
  }
  function addCropToInventory(id, qty, quality) {
    var inv = state().inventory, key = id || 'crop', amount = Math.max(1, num(qty, 1)), q = quality || 'Normal';
    if (!inv.crops[key]) { inv.crops[key] = {}; }
    if (typeof inv.crops[key] === 'number') { inv.crops[key] = { Normal: inv.crops[key] }; }
    inv.crops[key][q] = num(inv.crops[key][q], 0) + amount;
  }
  function useTool(plot) {
    if (!plot) { return; }
    var st = state(), action = toolAction(st.selectedTool);
    var hasPlots = !!(F.plots && typeof F.plots[action === 'sell' ? 'harvest' : action] === 'function');
    var watered, crop, planted, yieldData, earned, cropList, i, base, tilled;
    if (action === 'water') {
      watered = plotInvoke('water', [plot], function () {
        plot.water = Math.min(4, num(plot.water, 0) + 2);
        if (plot.state === 'tilled') { plot.state = 'watered'; }
      });
      msg(watered === false ? 'Cannot water that plot.' : 'Watered plot (' + num(plot.water, 0) + '/4).');
    } else if (action === 'plant') {
      crop = st.selectedCrop || firstCrop();
      if (!crop) { msg('No crop selected.'); return; }
      if (seedCount(st, crop) <= 0) { msg('No ' + crop + ' seeds. Buy seeds in the Seeds menu.'); return; }
      planted = plotInvoke('plant', [plot, crop], function () {
        plot.cropId = crop; plot.state = 'planted'; plot.stage = 0; plot.days = 0;
      });
      if (planted === false) { msg('Cannot plant there or crop is out of season.'); return; }
      takeSeed(st, crop);
      msg('Planted ' + crop + '.');
    } else if (action === 'harvest' || action === 'sell') {
      yieldData = plotInvoke('harvest', [plot], function () {
        if (!plot.cropId || !plot.ready) { return null; }
        var out = { cropId: plot.cropId, quantity: 1, quality: 'Normal' };
        plot.cropId = null; plot.ready = false; plot.stage = 0; plot.state = 'harvested';
        return out;
      });
      if (!yieldData || !yieldData.cropId) { msg(action === 'sell' ? 'Cart: no ready crop.' : 'Nothing ready to harvest.'); return; }
      addCropToInventory(yieldData.cropId, yieldData.quantity, yieldData.quality);
      if (action === 'harvest') { if (!hasPlots) { msg('Harvested ' + (yieldData.quantity || 1) + ' ' + yieldData.cropId + '.'); } return; }
      earned = 0;
      if (F.economy && F.economy.sell) { earned = callFn(F.economy, 'sell', [yieldData.cropId, yieldData.quantity || 1, yieldData.quality || 'Normal']); }
      if (!earned) {
        base = 5; cropList = list(config().CROPS);
        for (i = 0; i < cropList.length; i += 1) { if (cropList[i].id === yieldData.cropId) { base = num(cropList[i].salePrice, 5); } }
        earned = base * num(yieldData.quantity, 1);
        st.coins = num(st.coins, 0) + earned;
        if (F.progression && F.progression.addXP) { callFn(F.progression, 'addXP', [Math.max(1, num(yieldData.quantity, 1)), 'cart sale']); }
      }
      msg('Cart sale: +' + num(earned, 0) + ' coins.');
    } else {
      tilled = plotInvoke('till', [plot], function () {
        plot.state = 'tilled'; plot.weeds = 0; plot.stage = 0; plot.ready = false;
      });
      msg(tilled === false ? 'Plot is already prepared.' : 'Prepared soil.');
    }
  }
  function advancePlot(plot) {
    if (!plot || !plot.cropId) { return; }
    if (F.crops && F.crops.advance) { callFn(F.crops, 'advance', [plot, 1]); return; }
    if (plot.state !== 'planted' && plot.state !== 'ready' || plot.ready || !(num(plot.water, 0) > 0)) { return; }
    var crops = list(config().CROPS), growthDays = 7, ratio, i;
    for (i = 0; i < crops.length; i += 1) { if (crops[i].id === plot.cropId) { growthDays = num(crops[i].growthDays, 7); } }
    plot.water = num(plot.water, 0) - 1;
    plot.days = num(plot.days, 0) + 1;
    ratio = plot.days / Math.max(1, growthDays);
    plot.stage = ratio >= 1 ? 4 : (ratio >= 0.75 ? 3 : (ratio >= 0.5 ? 2 : (ratio >= 0.25 ? 1 : 0)));
    plot.state = plot.stage >= 4 ? 'ready' : 'planted';
    plot.ready = plot.stage >= 4;
  }
  function newDay() {
    var st = state(), seasons = list(config().SEASONS), plots = list(st.farm.plots), i;
    st.day = num(st.day, 1) + 1;
    if (seasons.length) { st.seasonIndex = Math.floor((st.day - 1) / 12) % seasons.length; }
    for (i = 0; i < plots.length; i += 1) { advancePlot(plots[i]); }
    if (F.progression && F.progression.checkUnlocks) { callFn(F.progression, 'checkUnlocks', []); }
    saveGame(true);
    msg('Day ' + st.day + ' begins.');
  }
  function saveGame(silent) {
    var st = state(), ok = false;
    if (typeof F.save === 'function') { callFn(F, 'save', []); ok = true; }
    else { try { window.localStorage.setItem('fif-save', JSON.stringify(st)); ok = true; } catch (err) { ok = false; } }
    lastSave = now();
    if (!silent) { msg(ok ? 'Game saved.' : 'Save unavailable in this browser.'); }
  }
  function loadGame() {
    if (typeof F.load === 'function') {
      var loaded = callFn(F, 'load', []);
      if (loaded && !F.state) { F.state = loaded; }
    } else {
      try { var raw = window.localStorage.getItem('fif-save'); if (raw) { F.state = JSON.parse(raw); } }
      catch (err) { msg('Load failed.'); }
    }
    state();
    msg('Game loaded.');
  }
  function resetGame() {
    if (typeof F.reset === 'function') { callFn(F, 'reset', []); }
    else { F.state = fallbackState(); }
    state();
    msg('New farm started.');
  }
  function cycleCrop(dir) {
    var crops = list(config().CROPS), st = state(), idx = 0, i;
    if (!crops.length) { return; }
    for (i = 0; i < crops.length; i += 1) { if (crops[i].id === st.selectedCrop) { idx = i; break; } }
    idx = (idx + dir + crops.length) % crops.length;
    st.selectedCrop = crops[idx].id;
    msg('Crop: ' + crops[idx].name);
  }
  function cycleAttachment() {
    var attachments = list(config().ATTACHMENTS), st = state(), current = str(st.tractor.attachment), idx = 0, id, i;
    if (!attachments.length) { return; }
    for (i = 0; i < attachments.length; i += 1) { if (attachments[i].id === current) { idx = i; break; } }
    idx = (idx + 1) % attachments.length;
    id = attachments[idx].id;
    if (F.tractors && F.tractors.attach) { callFn(F.tractors, 'attach', [id]); }
    st.tractor.attachment = id;
    msg('Attachment: ' + (attachments[idx].name || id));
  }
  function hit(key) { return F.input && F.input.pressed ? F.input.pressed(key) : false; }
  function handleKeys(st) {
    var tools = F.ui && F.ui.tools ? F.ui.tools() : [], i, plot;
    for (i = 0; i < 6; i += 1) {
      if (hit(String(i + 1)) && tools[i]) { st.selectedTool = tools[i].id; msg('Tool: ' + tools[i].name); }
    }
    if (hit('q')) { cycleCrop(-1); }
    if (hit('e')) { cycleCrop(1); }
    if (hit('t')) { cycleAttachment(); }
    if (hit('n')) { newDay(); }
    if (hit('s') && now() - lastSave > 500) { saveGame(false); }
    if (hit('l')) { loadGame(); }
    if (hit('r')) { resetGame(); }
    if (hit('m') && F.ui && F.ui.toggleMenu) { F.ui.toggleMenu(); }
    if (hit('b') && F.ui && F.ui.openMenu) { F.ui.openMenu('seeds'); }
    if (hit('u') && F.ui && F.ui.openMenu) { F.ui.openMenu('tractor'); }
    if (hit(' ') || hit('space')) {
      plot = F.ui && F.ui.plotFromTractor ? F.ui.plotFromTractor(st) : null;
      if (plot) { useTool(plot); } else { msg('No plot under tractor.'); }
    }
  }
  function clamp(v, lo, hi) { if (v < lo) { return lo; } if (v > hi) { return hi; } return v; }
  function moveTractorFallback(dt, st) {
    var keys = F.input && F.input.keys ? F.input.keys : {}, dx = (keys.right ? 1 : 0) - (keys.left ? 1 : 0), dy = (keys.down ? 1 : 0) - (keys.up ? 1 : 0);
    if (!dx && !dy) { return; }
    var cols = Math.max(1, list(st.farm.plots).length);
    st.tractor.x = clamp(num(st.tractor.x, 0) + dx * 4 * dt, 0, cols - 1);
    st.tractor.y = clamp(num(st.tractor.y, 0) + dy * 4 * dt, 0, 8);
  }
  function update(dt) {
    var st = state();
    handleKeys(st);
    if (F.tractors && F.tractors.update) { callFn(F.tractors, 'update', [dt, F.input && F.input.keys ? F.input.keys : {}, st]); }
    else { moveTractorFallback(dt, st); }
  }
  function render() {
    if (!ctx) { return; }
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    if (F.ui && F.ui.render) { F.ui.render(ctx, state()); }
    else { ctx.fillStyle = '#f3e5c0'; ctx.font = '16px monospace'; ctx.fillText('FIF.ui is not ready.', 20, 20); }
  }
  function loop(timestamp) {
    if (!running) { return; }
    var t = num(timestamp, now()), dt = (t - lastTime) / 1000;
    if (!isFinite(dt) || dt < 0) { dt = 0; }
    if (dt > 0.05) { dt = 0.05; }
    lastTime = t;
    update(dt); render();
    window.requestAnimationFrame(loop);
  }
  function bindCanvas() {
    if (!canvas) { return; }
    canvas.addEventListener('click', function (e) {
      var rect = canvas.getBoundingClientRect(), x, y;
      if (!rect.width || !rect.height) { return; }
      x = (e.clientX - rect.left) * (canvas.width / rect.width);
      y = (e.clientY - rect.top) * (canvas.height / rect.height);
      if (F.ui && F.ui.handleClick) { F.ui.handleClick(x, y, state()); }
    }, false);
  }
  function bindHelp() {
    var close = document.getElementById('help-close'), toggle = document.getElementById('help-toggle'), overlay = document.getElementById('help-overlay');
    if (close && overlay) { close.onclick = function () { overlay.style.display = 'none'; }; }
    if (toggle && overlay) {
      toggle.onclick = function () { overlay.style.display = overlay.style.display === 'none' ? 'block' : 'none'; };
    }
  }
  function start() {
    if (running) { return; }
    canvas = document.getElementById('game') || document.getElementById('gameCanvas');
    if (!canvas || !canvas.getContext) { return; }
    ctx = canvas.getContext('2d');
    if (F.input && F.input.init) { F.input.init(); }
    if (F.ui && F.ui.init) { F.ui.init(canvas); }
    if (typeof F.load === 'function') {
      var loaded = callFn(F, 'load', []);
      if (loaded && !F.state) { F.state = loaded; }
    }
    if (!F.state) {
      if (typeof F.reset === 'function') { callFn(F, 'reset', []); }
      else { F.state = fallbackState(); }
    }
    state(); bindCanvas(); bindHelp();
    lastTime = now(); running = true;
    msg('Welcome to Flat Iron Fields.');
    window.requestAnimationFrame(loop);
  }
  F.game = { start: start, newDay: newDay, update: update, render: render, useTool: useTool };
  if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', start, false); }
  else { start(); }
}());
