(function () {
  window.FIF = window.FIF || {};
  var F = window.FIF;
  var canvas = null, menu = 'seeds', buttons = [], seedPage = 0, lastFarm = null;
  var farmBox = { x: 16, y: 104, w: 920, h: 508 };
  var panelBox = { x: 950, y: 104, w: 314, h: 508 };
  var logBox = { x: 16, y: 624, w: 1248, h: 80 };
  var TOOLS = [
    { id: 'shovel', name: '1 Shovel' }, { id: 'wateringCan', name: '2 Water Can' },
    { id: 'rake', name: '3 Rake/Seed' }, { id: 'hoe', name: '4 Hoe' },
    { id: 'scythe', name: '5 Scythe' }, { id: 'cart', name: '6 Cart' }
  ];
  function c() { return F.config || {}; }
  function s() { return F.state || {}; }
  function list(v) { return v && v.length ? v : []; }
  function str(v) { return v === null || v === undefined ? '' : String(v); }
  function num(v, d) { var n = Number(v); return isFinite(n) ? n : (d || 0); }
  function cropName(id) { var a = list(c().CROPS), i; for (i = 0; i < a.length; i += 1) { if (a[i].id === id) { return a[i].name || id; } } return str(id) || 'None'; }
  function seasonName(st) { var a = list(c().SEASONS), i = num(st && st.seasonIndex, 0); if (i < 0 || i >= a.length) { i = 0; } return a[i] || 'Spring'; }
  function byId(key, id) { var a = list(c()[key]), i; for (i = 0; i < a.length; i += 1) { if (a[i].id === id) { return a[i].name || id; } } return str(id) || 'None'; }
  function tractorName(id) { return byId('TRACTOR_TYPES', id); }
  function attachmentName(id) { return byId('ATTACHMENTS', id); }
  function toolAction(id) {
    var t = str(id).toLowerCase();
    if (t.indexOf('water') >= 0) { return 'water'; }
    if (t.indexOf('seed') >= 0 || t.indexOf('plant') >= 0 || t.indexOf('rake') >= 0) { return 'plant'; }
    if (t.indexOf('scythe') >= 0 || t.indexOf('harvest') >= 0) { return 'harvest'; }
    if (t.indexOf('cart') >= 0 || t.indexOf('sell') >= 0) { return 'sell'; }
    return 'till';
  }
  function toolName(id) { var i; for (i = 0; i < TOOLS.length; i += 1) { if (TOOLS[i].id === id) { return TOOLS[i].name; } } return '1 ' + str(id || 'Shovel'); }
  function unlockCount(player) {
    var u = player && player.unlocked, groups = ['crops', 'tractors', 'decorations'], n = 0, i, k, g;
    if (!u) { return 0; }
    if (Object.prototype.toString.call(u) === '[object Array]') { return u.length; }
    if (typeof u !== 'object') { return 0; }
    for (i = 0; i < groups.length; i += 1) {
      g = u[groups[i]];
      if (g && typeof g === 'object') { for (k in g) { if (Object.prototype.hasOwnProperty.call(g, k) && g[k]) { n += 1; } } }
    }
    if (!n) { for (k in u) { if (Object.prototype.hasOwnProperty.call(u, k) && u[k] && groups.indexOf(k) < 0 && k.indexOf(':') < 0) { n += 1; } } }
    return n;
  }
  function cargoCount(st) {
    var cargo = st && st.tractor && st.tractor.cargo, total = 0, i, k, q, v;
    if (typeof cargo === 'number') { return cargo; }
    if (Object.prototype.toString.call(cargo) === '[object Array]') {
      for (i = 0; i < cargo.length; i += 1) { total += num(cargo[i] && cargo[i].quantity, 0); }
      return total;
    }
    if (cargo && typeof cargo === 'object') {
      for (k in cargo) {
        if (!Object.prototype.hasOwnProperty.call(cargo, k)) { continue; }
        v = cargo[k];
        if (typeof v === 'number') { total += v; }
        else if (v && typeof v === 'object') {
          for (q in v) { if (Object.prototype.hasOwnProperty.call(v, q) && typeof v[q] === 'number') { total += v[q]; } }
        }
      }
    }
    return total;
  }
  function msgText(m) { return m && typeof m === 'object' && m.text !== undefined ? str(m.text) : str(m); }
  function message(text) {
    if (!text) { return; }
    var st = s(), value = str(text), last;
    if (!st.messages || !st.messages.length) { st.messages = []; }
    last = st.messages[st.messages.length - 1];
    if (msgText(last) === value) { return; }
    st.messages.push(value);
    while (st.messages.length > 40) { st.messages.shift(); }
  }
  function invoke(mod, name, args, ok) {
    if (mod && typeof mod[name] === 'function') {
      try {
        var result = mod[name].apply(mod, args || []);
        if (typeof result === 'string') { message(result); }
        else if (ok) { message(ok); }
        return result;
      } catch (err) { message(name + ' failed: ' + err.message); return null; }
    }
    message(name + ' is not ready.');
    return null;
  }
  function button(ctx, x, y, w, h, label, action, args, active) {
    buttons.push({ x: x, y: y, w: w, h: h, action: action, args: args || [] });
    ctx.fillStyle = active ? '#557a3c' : '#4a3524';
    ctx.fillRect(x, y, w, h);
    ctx.strokeStyle = active ? '#f2d98f' : '#b59361';
    ctx.strokeRect(x + 0.5, y + 0.5, w - 1, h - 1);
    ctx.fillStyle = '#f6e7c1';
    ctx.font = '11px "Courier New", monospace';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(label, x + w / 2, y + h / 2 + 1);
  }
  function panel(ctx, box, title) {
    ctx.fillStyle = '#241c14';
    ctx.fillRect(box.x, box.y, box.w, box.h);
    ctx.strokeStyle = '#8c6c43';
    ctx.strokeRect(box.x + 0.5, box.y + 0.5, box.w - 1, box.h - 1);
    ctx.fillStyle = '#d8bf8a';
    ctx.font = 'bold 13px "Courier New", monospace';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'alphabetic';
    ctx.fillText(title, box.x + 10, box.y + 20);
  }
  function drawBackground(ctx) {
    var x, y;
    ctx.fillStyle = '#1b1712';
    ctx.fillRect(0, 0, 1280, 720);
    ctx.fillStyle = '#243126';
    for (y = 0; y < 720; y += 24) { for (x = (y / 24 % 2) * 12; x < 1280; x += 24) { ctx.fillRect(x, y, 12, 12); } }
  }
  function drawHud(ctx, st) {
    var player = st.player || {}, tractor = st.tractor || {};
    ctx.fillStyle = '#2a2118';
    ctx.fillRect(0, 0, 1280, 96);
    ctx.strokeStyle = '#8c6c43';
    ctx.beginPath(); ctx.moveTo(0, 96.5); ctx.lineTo(1280, 96.5); ctx.stroke();
    ctx.textAlign = 'left'; ctx.textBaseline = 'alphabetic';
    ctx.fillStyle = '#f3d993'; ctx.font = 'bold 20px "Courier New", monospace';
    ctx.fillText('FLAT IRON FIELDS', 16, 26);
    ctx.fillStyle = '#efe0bb'; ctx.font = '12px "Courier New", monospace';
    ctx.fillText('Day ' + num(st.day, 1) + '  ' + seasonName(st) + '  |  Coins ' + num(st.coins, 0) +
      '  |  XP ' + num(player.xp, 0) + '  |  Level ' + num(player.level, 1) +
      '  |  Skill Points ' + num(player.skillPoints, 0), 16, 48);
    ctx.fillText('Tool: ' + toolName(st.selectedTool) + '  |  Crop: ' + cropName(st.selectedCrop) +
      '  |  Tractor: ' + tractorName(tractor.type) + '  |  Attach: ' + attachmentName(tractor.attachment) +
      '  |  Cargo: ' + cargoCount(st), 16, 68);
    ctx.fillStyle = '#bfa77c';
    ctx.fillText('Click = tool  |  Space = tractor plot  |  1-6 tools  Q/E crop  T attach  N day  B seeds  U upgrades  M menu', 16, 87);
    button(ctx, 1080, 12, 140, 28, 'Next Day (N)', function () { if (F.game && F.game.newDay) { F.game.newDay(); } }, [], false);
  }
  function plotColor(p) {
    var stateName = str(p.state).toLowerCase();
    if (p.ready || stateName === 'ready') { return '#c9a23f'; }
    if (stateName === 'harvested') { return '#5b432c'; }
    if (stateName === 'watered' || (num(p.water, 0) > 0 && stateName !== 'planted')) { return '#3e6f8a'; }
    if (stateName === 'tilled') { return '#6b4a2f'; }
    if (stateName === 'planted') { return num(p.stage, 0) >= 2 ? '#6f8f3d' : '#54703a'; }
    return '#3f5a30';
  }
  function drawFarm(ctx, st) {
    var farm = st.farm || {}, plots = list(farm.plots), shape = null, cols, rows, tileCols, tileRows;
    var maxX = 0, maxY = 0, i, p, tw, th, px, py, pw, ph, water, ww;
    if (F.plots && F.plots.plotShape) { shape = F.plots.plotShape(farm.expansionStage || 0); }
    cols = shape && shape.cols ? num(shape.cols, 1) : 0;
    rows = shape && shape.rows ? num(shape.rows, 1) : 0;
    tileCols = shape && shape.tileCols ? num(shape.tileCols, cols || 1) : (cols || 1);
    tileRows = shape && shape.tileRows ? num(shape.tileRows, rows || 1) : (rows || 1);
    for (i = 0; i < plots.length; i += 1) {
      if (num(plots[i].x, 0) > maxX) { maxX = num(plots[i].x, 0); }
      if (num(plots[i].y, 0) > maxY) { maxY = num(plots[i].y, 0); }
    }
    if (cols < 1) { cols = Math.max(1, maxX + 1); }
    if (rows < 1) { rows = Math.max(1, maxY + 1); }
    if (tileCols < cols) { tileCols = cols; }
    if (tileRows < rows) { tileRows = rows; }
    panel(ctx, farmBox, 'FARM - Stage ' + num(farm.expansionStage, 0) + ' - ' + plots.length + ' plots');
    var scaleX = (farmBox.w - 32) / tileCols, scaleY = (farmBox.h - 70) / tileRows;
    if (scaleX < 2) { scaleX = 2; }
    if (scaleY < 2) { scaleY = 2; }
    var ox = farmBox.x + 16, oy = farmBox.y + 44, defW = Math.max(1, Math.floor(tileCols / cols)), defH = Math.max(1, Math.floor(tileRows / rows));
    lastFarm = { ox: ox, oy: oy, scaleX: scaleX, scaleY: scaleY, cols: cols, rows: rows, tileCols: tileCols, tileRows: tileRows, defW: defW, defH: defH, plots: plots };
    var stages = F.crops && F.crops.growthStages ? F.crops.growthStages() : ['Seed', 'Sprout', 'Grow', 'Bloom', 'Harvest'];
    for (i = 0; i < plots.length; i += 1) {
      p = plots[i] || {};
      tw = num(p.tileW, defW); th = num(p.tileH, defH);
      px = ox + num(p.x, 0) * tw * scaleX; py = oy + num(p.y, 0) * th * scaleY;
      pw = Math.max(8, tw * scaleX - 2); ph = Math.max(8, th * scaleY - 2);
      ctx.fillStyle = plotColor(p); ctx.fillRect(px, py, pw, ph);
      ctx.strokeStyle = str(p.type).indexOf('fence') >= 0 ? '#ded3b5' : (str(p.type).indexOf('raised') >= 0 ? '#d2a679' : '#78a24d');
      ctx.strokeRect(px + 0.5, py + 0.5, pw - 1, ph - 1);
      if (p.cropId && pw >= 18 && ph >= 18) {
        ctx.fillStyle = p.ready ? '#ffe27a' : '#cbea8a';
        ctx.font = 'bold 11px "Courier New", monospace';
        ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
        ctx.fillText(str(typeof p.stage === 'number' ? stages[p.stage] : (p.stage || stages[0])).charAt(0), px + pw / 2, py + ph / 2 + 1);
      }
      water = num(p.water, 0);
      if (water > 0) {
        ww = Math.max(2, Math.min(pw - 4, (water / 4) * (pw - 4)));
        ctx.fillStyle = '#69b6d8'; ctx.fillRect(px + 2, py + ph - 7, ww, 3);
        if (ph >= 44) { ctx.fillStyle = '#cfefff'; ctx.font = '10px "Courier New", monospace'; ctx.textAlign = 'center'; ctx.fillText('W' + water, px + pw / 2, py + ph - 12); }
      }
    }
    drawTractor(ctx, st, ox, oy, scaleX, scaleY);
  }
  function drawTractor(ctx, st, ox, oy, scaleX, scaleY) {
    var t = st.tractor || {}, tile = num(c().TILE_SIZE, 32), rawX = num(t.x, 0), rawY = num(t.y, 0);
    var tx = rawX / tile, ty = rawY / tile, px, py, w, colors = { basic: '#d96b3c', crop: '#e2b53c', heavy: '#7d9b4f', specialized: '#a675c0' };
    if (lastFarm && rawX === Math.floor(rawX) && rawY === Math.floor(rawY) && rawX >= 0 && rawX < lastFarm.cols && rawY >= 0 && rawY < lastFarm.rows) {
      tx = (rawX + 0.5) * num(lastFarm.plots[0] && lastFarm.plots[0].tileW, lastFarm.defW);
      ty = (rawY + 0.5) * num(lastFarm.plots[0] && lastFarm.plots[0].tileH, lastFarm.defH);
    }
    px = ox + tx * scaleX; py = oy + ty * scaleY; w = Math.max(12, Math.min(26, scaleX * 2));
    ctx.fillStyle = '#1b1712';
    ctx.fillRect(px - w / 2 + 1, py + w * 0.22, w * 0.24, w * 0.24);
    ctx.fillRect(px + w * 0.27, py + w * 0.22, w * 0.24, w * 0.24);
    ctx.fillStyle = colors[str(t.type).toLowerCase()] || '#d96b3c';
    ctx.fillRect(px - w / 2, py - w * 0.26, w, w * 0.6);
    ctx.fillStyle = '#f2d98f'; ctx.fillRect(px - w * 0.32, py - w * 0.5, w * 0.54, w * 0.32);
    ctx.strokeStyle = '#f6e7c1'; ctx.strokeRect(px - w / 2 + 0.5, py - w * 0.26 + 0.5, w - 1, w - 1);
  }
  function drawPanel(ctx, st) {
    var tabs = ['Seeds', 'Sell', 'Land', 'Tractor', 'Skills'], id, i, x, y, w;
    panel(ctx, panelBox, 'MANAGEMENT');
    for (i = 0; i < tabs.length; i += 1) {
      id = tabs[i].toLowerCase();
      button(ctx, panelBox.x + 8 + i * 60, panelBox.y + 28, 58, 24, tabs[i], openMenu, [id], menu === id);
    }
    x = panelBox.x + 10; y = panelBox.y + 64; w = panelBox.w - 20;
    if (menu === 'seeds') { drawSeeds(ctx, st, x, y, w); }
    else if (menu === 'sell') { drawSell(ctx, st, x, y, w); }
    else if (menu === 'land') { drawLand(ctx, st, x, y, w); }
    else if (menu === 'tractor') { drawTractorMenu(ctx, st, x, y, w); }
    else if (menu === 'skills') { drawSkills(ctx, st, x, y, w); }
    else { ctx.fillStyle = '#bfa77c'; ctx.font = '12px "Courier New", monospace'; ctx.fillText('Menu closed. Press M, B, or U.', x, y + 12); }
  }
  function seedCount(st, id) {
    var seeds = st && st.inventory && st.inventory.seeds || {}, v = seeds[id];
    if (typeof v === 'number') { return v; }
    if (v && typeof v === 'object') { return num(v.quantity || v.qty || v.count, 0); }
    return 0;
  }
  function takeCrops(inv, id, quality, qty) {
    var bucket = inv && inv.crops, v = bucket && bucket[id], alt = id + ':' + quality;
    if (!bucket) { return false; }
    if (v && typeof v === 'object' && typeof v[quality] === 'number') { if (v[quality] < qty) { return false; } v[quality] -= qty; return true; }
    if (typeof v === 'number') { if (v < qty) { return false; } bucket[id] -= qty; return true; }
    if (typeof bucket[alt] === 'number') { if (bucket[alt] < qty) { return false; } bucket[alt] -= qty; return true; }
    if (v && typeof v === 'object' && typeof v.quantity === 'number') { if (v.quantity < qty) { return false; } v.quantity -= qty; return true; }
    return false;
  }
  function sellCrop(id, quality, qty) {
    var earned = 0, price, crops, i, st = s();
    qty = Math.max(1, Math.floor(num(qty, 1)));
    if (F.economy && F.economy.sell) { earned = invoke(F.economy, 'sell', [id, qty, quality], null); }
    if (earned) { return earned; }
    price = F.economy && F.economy.price ? F.economy.price(id, quality) : 0;
    crops = list(c().CROPS);
    if (!price) { for (i = 0; i < crops.length; i += 1) { if (crops[i].id === id) { price = num(crops[i].salePrice, 0); } } }
    if (!takeCrops(st.inventory, id, quality, qty)) { message('Not enough ' + id + ' to sell.'); return 0; }
    earned = num(price, 0) * qty;
    st.coins = num(st.coins, 0) + earned; if (F.progression && F.progression.addXP) { F.progression.addXP(Math.max(1, qty), 'sale'); }
    message('Sold ' + qty + ' ' + id + ' for ' + earned + ' coins.');
    return earned;
  }
  function drawSeeds(ctx, st, x, y) {
    var crops = list(c().CROPS), per = 8, maxPage = Math.max(0, Math.ceil(crops.length / per) - 1), i, idx, crop, ry, mark;
    if (seedPage > maxPage) { seedPage = maxPage; }
    ctx.font = '11px "Courier New", monospace'; ctx.fillStyle = '#bfa77c';
    ctx.fillText('Buy seeds / choose crop  (Q/E cycles)', x, y + 4);
    for (i = 0; i < per && seedPage * per + i < crops.length; i += 1) {
      idx = seedPage * per + i; crop = crops[idx] || {}; ry = y + 14 + i * 30;
      mark = F.crops && F.crops.isInSeason && F.crops.isInSeason(crop.id, st.seasonIndex) ? '*' : '';
      button(ctx, x, ry, 150, 24, crop.name + mark, function (id) { s().selectedCrop = id; message('Crop selected: ' + cropName(id)); }, [crop.id], st.selectedCrop === crop.id);
      button(ctx, x + 154, ry, 62, 24, 'Buy 1 ' + (crop.seedCost || 0), function (id) { invoke(F.economy, 'buySeed', [id, 1], 'Bought seed.'); }, [crop.id], false);
      button(ctx, x + 220, ry, 62, 24, 'Buy 5', function (id) { invoke(F.economy, 'buySeed', [id, 5], 'Bought seeds.'); }, [crop.id], false);
    }
    button(ctx, x, y + 268, 68, 24, '< Prev', function () { seedPage = Math.max(0, seedPage - 1); }, [], false);
    ctx.fillStyle = '#d8bf8a'; ctx.textAlign = 'center'; ctx.fillText('Page ' + (seedPage + 1) + '/' + (maxPage + 1), x + 145, y + 284);
    button(ctx, x + 214, y + 268, 68, 24, 'Next >', function () { seedPage = Math.min(maxPage, seedPage + 1); }, [], false);
    ctx.fillStyle = '#bfa77c'; ctx.textAlign = 'left';
    ctx.fillText('* in season. Seeds: ' + seedCount(st, st.selectedCrop), x, y + 312);
  }
  function cropEntries(inv) {
    var out = [], bucket = inv && inv.crops || {}, id, v, q, quality;
    for (id in bucket) {
      if (!Object.prototype.hasOwnProperty.call(bucket, id)) { continue; }
      v = bucket[id];
      if (typeof v === 'number') { if (v > 0) { out.push({ id: id, qty: v, quality: 'Normal' }); } }
      else if (v && typeof v === 'object') {
        if (v.quantity !== undefined || v.qty !== undefined || v.count !== undefined) {
          q = num(v.quantity || v.qty || v.count, 0);
          if (q > 0) { out.push({ id: id, qty: q, quality: v.quality || 'Normal' }); }
        } else {
          for (quality in v) { if (Object.prototype.hasOwnProperty.call(v, quality) && typeof v[quality] === 'number' && v[quality] > 0) { out.push({ id: id, qty: v[quality], quality: quality }); } }
        }
      }
    }
    return out;
  }
  function drawSell(ctx, st, x, y) {
    var entries = cropEntries(st.inventory), i, e, ry, price;
    ctx.font = '11px "Courier New", monospace'; ctx.fillStyle = '#bfa77c';
    ctx.fillText('Sell harvested crops at the farm store.', x, y + 4);
    if (!entries.length) { ctx.fillStyle = '#8e7d5f'; ctx.fillText('No crops. Harvest a ready plot first.', x, y + 30); return; }
    for (i = 0; i < Math.min(8, entries.length); i += 1) {
      e = entries[i]; ry = y + 14 + i * 30;
      price = F.economy && F.economy.price ? F.economy.price(e.id, e.quality) : '?';
      ctx.fillStyle = '#f0e2be'; ctx.textAlign = 'left';
      ctx.fillText(cropName(e.id) + ' x' + e.qty + ' [' + e.quality + '] ' + price + 'c', x, ry + 16);
      button(ctx, x + 190, ry, 44, 24, 'Sell 1', function (id, quality) { sellCrop(id, quality, 1); }, [e.id, e.quality], false);
      button(ctx, x + 238, ry, 44, 24, 'All', function (id, quality, qty) { sellCrop(id, quality, qty); }, [e.id, e.quality, e.qty], false);
    }
  }
  function drawLand(ctx, st, x, y, w) {
    var farm = st.farm || {}, costs = list(c().PLOT_COSTS), nextCost = costs[num(farm.expansionStage, 0) + 1];
    if (nextCost && typeof nextCost === 'object') { nextCost = nextCost.cost; }
    ctx.font = '12px "Courier New", monospace'; ctx.fillStyle = '#e7d5a8';
    ctx.fillText('Expansion Stage: ' + num(farm.expansionStage, 0), x, y + 8);
    ctx.fillText('Owned plots: ' + list(farm.plots).length, x, y + 30);
    ctx.fillStyle = '#bfa77c'; ctx.fillText('Next land cost: ' + (nextCost || 0) + ' coins', x, y + 56);
    button(ctx, x, y + 76, w, 30, 'Buy land / next plot', function () { invoke(F.economy, 'buyPlot', [], 'Land purchased.'); }, [], false);
    button(ctx, x, y + 116, w, 30, 'New Day (advance growth)', function () { if (F.game && F.game.newDay) { F.game.newDay(); } }, [], false);
  }
  function drawTractorMenu(ctx, st, x, y) {
    var types = list(c().TRACTOR_TYPES), attachments = list(c().ATTACHMENTS), player = st.player || {};
    var i, type, ry, j, att, col, row, bx, by, ay = y + 140;
    ctx.font = '11px "Courier New", monospace'; ctx.fillStyle = '#bfa77c';
    ctx.fillText('Tractors (buy / use)', x, y + 4);
    for (i = 0; i < Math.min(4, types.length); i += 1) {
      type = types[i] || {}; ry = y + 12 + i * 30;
      ctx.fillStyle = '#f0e2be';
      ctx.fillText(type.name + '  ' + (type.cost || 0) + 'c  Lv' + (type.unlockedAtLevel || 1), x, ry + 16);
      button(ctx, x + 190, ry, 44, 24, 'Buy', function (id) { invoke(F.economy, 'buyTractor', [id], 'Tractor purchased.'); }, [type.id], false);
      button(ctx, x + 238, ry, 44, 24, 'Use', function (id) { if (F.tractors && F.tractors.create) { invoke(F.tractors, 'create', [id], 'Tractor ready.'); } s().tractor = s().tractor || {}; s().tractor.type = id; message('Tractor selected: ' + tractorName(id)); }, [type.id], st.tractor && st.tractor.type === type.id);
    }
    ctx.fillStyle = '#bfa77c'; ctx.fillText('Attachments (T cycles)', x, ay);
    for (j = 0; j < Math.min(6, attachments.length); j += 1) {
      att = attachments[j] || {}; col = j % 2; row = Math.floor(j / 2); bx = x + col * 148; by = ay + 12 + row * 30;
      button(ctx, bx, by, 144, 24, att.name || att.id, function (id) { if (F.tractors && F.tractors.attach) { invoke(F.tractors, 'attach', [id], 'Attachment fitted.'); } s().tractor = s().tractor || {}; s().tractor.attachment = id; message('Attachment: ' + attachmentName(id)); }, [att.id], st.tractor && st.tractor.attachment === att.id);
    }
    ctx.fillStyle = '#8e7d5f';
    ctx.fillText('Fuel: ' + num(st.tractor && st.tractor.fuel, 0) + '  Cargo: ' + cargoCount(st) + '  Level: ' + num(player.level, 1), x, ay + 120);
  }
  function drawSkills(ctx, st, x, y, w) {
    var skills = list(c().SKILLS), player = st.player || {}, i, skill, ry, level;
    ctx.font = '11px "Courier New", monospace'; ctx.fillStyle = '#bfa77c';
    ctx.fillText('Skill points: ' + num(player.skillPoints, 0) + '  XP: ' + num(player.xp, 0) + '  Unlocks: ' + unlockCount(player), x, y + 4);
    for (i = 0; i < Math.min(7, skills.length); i += 1) {
      skill = skills[i] || {}; ry = y + 16 + i * 34;
      level = player.skills && player.skills[skill.id] ? player.skills[skill.id] : 0;
      ctx.fillStyle = '#f0e2be'; ctx.fillText(skill.name + ' Lv' + level, x, ry + 10);
      ctx.fillStyle = '#8e7d5f'; ctx.fillText(str(skill.description || '').slice(0, 30), x, ry + 23);
      button(ctx, x + 238, ry, 44, 24, 'Invest', function (id) { invoke(F.progression, 'investSkill', [id], 'Skill improved.'); }, [skill.id], false);
    }
    button(ctx, x, y + 282, w, 26, 'Check unlocks', function () { invoke(F.progression, 'checkUnlocks', [], 'Unlocks checked.'); }, [], false);
  }
  function drawLog(ctx, st) {
    var msgs = list(st.messages), start = Math.max(0, msgs.length - 4), i;
    panel(ctx, logBox, 'LOG');
    ctx.font = '12px "Courier New", monospace'; ctx.fillStyle = '#d8c9a3'; ctx.textAlign = 'left';
    if (!msgs.length) { ctx.fillStyle = '#8e7d5f'; ctx.fillText('Welcome. Till, water, plant, then advance the day.', logBox.x + 10, logBox.y + 38); }
    for (i = 0; i < 4 && start + i < msgs.length; i += 1) { ctx.fillText('> ' + msgText(msgs[start + i]).slice(0, 150), logBox.x + 10, logBox.y + 38 + i * 14); }
    ctx.fillStyle = '#8e7d5f'; ctx.textAlign = 'right';
    ctx.fillText('S save  L load  R reset  M menu  ? help', logBox.x + logBox.w - 10, logBox.y + 38);
  }
  function render(ctx, st) {
    if (!ctx) { return; }
    st = st || s(); buttons = [];
    drawBackground(ctx); drawHud(ctx, st); drawFarm(ctx, st); drawPanel(ctx, st); drawLog(ctx, st);
  }
  function plotAtTile(tx, ty) {
    var i, p, tw, th, gx, gy;
    if (!lastFarm) { return null; }
    for (i = 0; i < lastFarm.plots.length; i += 1) {
      p = lastFarm.plots[i] || {}; tw = num(p.tileW, lastFarm.defW); th = num(p.tileH, lastFarm.defH);
      gx = num(p.x, 0) * tw; gy = num(p.y, 0) * th;
      if (tx >= gx && tx < gx + tw && ty >= gy && ty < gy + th) { return p; }
    }
    return null;
  }
  function plotAtPoint(x, y) { return lastFarm ? plotAtTile((x - lastFarm.ox) / lastFarm.scaleX, (y - lastFarm.oy) / lastFarm.scaleY) : null; }
  function plotFromTractor(st) {
    st = st || s();
    var plots = list(st.farm && st.farm.plots), t = st.tractor || {}, tile = num(c().TILE_SIZE, 32);
    var rawX = num(t.x, 0), rawY = num(t.y, 0);
    if (!lastFarm) { return plots.length ? plots[0] : null; }
    if (rawX === Math.floor(rawX) && rawY === Math.floor(rawY) && rawX >= 0 && rawX < lastFarm.cols && rawY >= 0 && rawY < lastFarm.rows) {
      return plotAtTile((rawX + 0.5) * lastFarm.defW, (rawY + 0.5) * lastFarm.defH);
    }
    return plotAtTile(rawX / tile, rawY / tile);
  }
  function handleClick(x, y, st) {
    var i, b, plot;
    st = st || s();
    for (i = buttons.length - 1; i >= 0; i -= 1) {
      b = buttons[i];
      if (x >= b.x && x <= b.x + b.w && y >= b.y && y <= b.y + b.h) {
        if (typeof b.action === 'function') { b.action.apply(null, b.args || []); }
        return true;
      }
    }
    plot = plotAtPoint(x, y);
    if (plot && F.game && F.game.useTool) { F.game.useTool(plot); return true; }
    return false;
  }
  function init(el) { canvas = el || document.getElementById('game'); buttons = []; }
  function openMenu(name) { menu = name || 'seeds'; }
  function toggleMenu() { menu = menu ? null : 'seeds'; }
  F.ui = {
    init: init, render: render, handleClick: handleClick, message: message,
    openMenu: openMenu, toggleMenu: toggleMenu, plotFromTractor: plotFromTractor,
    tools: function () { return TOOLS.slice(); }
  };
}());
