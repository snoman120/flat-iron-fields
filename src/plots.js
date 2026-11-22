/* Flat Iron Fields - farm layout and plot state machine.
 * Classic script. Owned by Core Systems agent.
 */
window.FIF = window.FIF || {};

(function (FIF) {
  // Stage: number of plots, plot-grid columns/rows, farm size in tiles.
  var STAGES = [
    { plots: 5, cols: 5, rows: 1, tileCols: 10, tileRows: 20 },
    { plots: 10, cols: 10, rows: 1, tileCols: 10, tileRows: 20 },
    { plots: 20, cols: 10, rows: 2, tileCols: 20, tileRows: 20 },
    { plots: 30, cols: 15, rows: 2, tileCols: 30, tileRows: 20 },
    { plots: 50, cols: 25, rows: 2, tileCols: 50, tileRows: 20 }
  ];

  function clampStage(stage) {
    var n = Math.floor(Number(stage) || 0);
    if (n < 0) { n = 0; }
    if (n >= STAGES.length) { n = STAGES.length - 1; }
    return n;
  }

  function plotShape(stage) {
    var s = STAGES[clampStage(stage)];
    return {
      plots: s.plots,
      cols: s.cols,
      rows: s.rows,
      tileCols: s.tileCols,
      tileRows: s.tileRows
    };
  }

  function plotType(index) {
    if (index % 7 === 3) { return 'raised'; }
    if (index % 9 === 5) { return 'fence'; }
    return 'regular';
  }

  function createFarm(stage) {
    var shape = plotShape(stage);
    var result = [];
    var i;
    var type;

    for (i = 0; i < shape.plots; i++) {
      type = plotType(i);
      result.push({
        id: i,
        x: i % shape.cols,
        y: Math.floor(i / shape.cols),
        type: type,
        state: 'empty',
        cropId: null,
        secondaryCropId: null,
        stage: 0,
        water: 0,
        qualityBonus: 0,
        ready: false,
        days: 0,
        harvestsLeft: 0,
        pests: 0,
        weeds: 0,
        protected: type === 'fence',
        tileW: Math.floor(shape.tileCols / shape.cols),
        tileH: Math.floor(shape.tileRows / shape.rows)
      });
    }
    return result;
  }

  function notify(text) {
    var st = FIF.state;
    if (!st) { return; }
    if (!Array.isArray(st.messages)) { st.messages = []; }
    st.messages.push(text);
    if (st.messages.length > 50) { st.messages = st.messages.slice(-50); }
    if (FIF.ui && typeof FIF.ui.message === 'function') {
      FIF.ui.message(text);
    }
  }

  function till(plot) {
    if (!plot || typeof plot !== 'object') { return false; }
    if (plot.state !== 'empty' && plot.state !== 'harvested') { return false; }
    plot.state = 'tilled';
    plot.weeds = 0;
    plot.stage = 0;
    plot.ready = false;
    return true;
  }

  function water(plot) {
    if (!plot || typeof plot !== 'object') { return false; }
    if (plot.state !== 'tilled' && plot.state !== 'watered' &&
        plot.state !== 'planted' && plot.state !== 'ready') {
      return false;
    }
    if (plot.state === 'tilled') { plot.state = 'watered'; }
    plot.water = Math.min(4, (Number(plot.water) || 0) + 2);
    if (Number(plot.water) > 0) { plot.pests = 0; }
    return true;
  }

  function plant(plot, cropId) {
    var crop;
    var st = FIF.state;
    if (!plot || typeof plot !== 'object' || !cropId) { return false; }
    if (plot.state !== 'empty' && plot.state !== 'tilled' &&
        plot.state !== 'watered' && plot.state !== 'harvested') {
      return false;
    }
    if (!FIF.crops || typeof FIF.crops.get !== 'function') { return false; }
    crop = FIF.crops.get(cropId);
    if (!crop) { return false; }
    if (typeof FIF.crops.isInSeason === 'function' &&
        !FIF.crops.isInSeason(cropId, st && st.seasonIndex)) {
      notify(crop.name + ' is not in season.');
      return false;
    }

    plot.cropId = crop.id;
    plot.state = 'planted';
    plot.stage = 0;
    plot.days = 0;
    plot.ready = false;
    plot.harvestsLeft = crop.multipleHarvest ? 2 : 1;
    if (st && st.stats && typeof st.stats.planted === 'number') {
      st.stats.planted += 1;
    }
    return true;
  }

  function harvest(plot) {
    var result;
    var st = FIF.state;
    if (!plot || typeof plot !== 'object') { return null; }
    if (!plot.ready || !plot.cropId) { return null; }
    if (!FIF.crops || typeof FIF.crops.yieldFor !== 'function') { return null; }

    result = FIF.crops.yieldFor(plot);
    if (FIF.crops.afterHarvest) { FIF.crops.afterHarvest(plot); }
    if (st && st.stats && typeof st.stats.harvested === 'number') {
      st.stats.harvested += 1;
    }
    if (result && result.quantity > 0) {
      notify('Harvested ' + result.quantity + ' ' +
        (FIF.crops.label ? FIF.crops.label(result.cropId) : result.cropId) +
        ' (' + result.quality + ').');
    }
    return result;
  }

  function expand(stage) {
    var target = clampStage(stage);
    var st = FIF.state;
    var oldPlots;
    var newPlots;
    var i;
    var p;

    if (!st || !st.farm) { return []; }
    oldPlots = Array.isArray(st.farm.plots) ? st.farm.plots : [];
    newPlots = createFarm(target);

    for (i = 0; i < newPlots.length && i < oldPlots.length; i++) {
      p = oldPlots[i];
      if (p && typeof p === 'object') {
        newPlots[i].state = p.state || 'empty';
        newPlots[i].cropId = p.cropId || null;
        newPlots[i].stage = Number(p.stage) || 0;
        newPlots[i].water = Number(p.water) || 0;
        newPlots[i].qualityBonus = Number(p.qualityBonus) || 0;
        newPlots[i].ready = !!p.ready;
        newPlots[i].days = Number(p.days) || 0;
        newPlots[i].harvestsLeft = Number(p.harvestsLeft) || 0;
      }
    }

    st.farm.plots = newPlots;
    st.farm.expansionStage = target;
    notify('Farm expanded to stage ' + target + ': ' + newPlots.length + ' plots.');
    return newPlots;
  }

  function plotAt(x, y) {
    var st = FIF.state;
    var plots;
    var i;
    if (!st || !st.farm || !Array.isArray(st.farm.plots)) { return null; }
    plots = st.farm.plots;
    for (i = 0; i < plots.length; i++) {
      if (plots[i] && plots[i].x === x && plots[i].y === y) { return plots[i]; }
    }
    return null;
  }

  FIF.plots = {
    plotShape: plotShape,
    createFarm: createFarm,
    till: till,
    water: water,
    plant: plant,
    harvest: harvest,
    expand: expand,
    plotAt: plotAt
  };

  // state.js loads before this file, so seed the farm here if needed.
  if (FIF.state && FIF.state.farm &&
      (!Array.isArray(FIF.state.farm.plots) || FIF.state.farm.plots.length === 0)) {
    FIF.state.farm.plots = createFarm(FIF.state.farm.expansionStage || 0);
  }
})(window.FIF);
