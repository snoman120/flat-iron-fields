/* Flat Iron Fields - crop definitions and growth logic.
 * Classic script. Owned by Core Systems agent.
 */
window.FIF = window.FIF || {};

(function (FIF) {
  var STAGES = ['Seed', 'Sprout', 'Grow', 'Bloom', 'Harvest'];

  function config() {
    return FIF.config || {};
  }

  function cropList() {
    var cfg = config();
    return Array.isArray(cfg.CROPS) ? cfg.CROPS : [];
  }

  function getCrop(cropId) {
    var list = cropList();
    var i;
    if (!cropId) { return null; }
    for (i = 0; i < list.length; i++) {
      if (list[i] && list[i].id === cropId) { return list[i]; }
    }
    return null;
  }

  function seasonName(index) {
    var cfg = config();
    var seasons = Array.isArray(cfg.SEASONS) ? cfg.SEASONS : [];
    var n = index;

    if (typeof n !== 'number') {
      n = (FIF.state && FIF.state.seasonIndex) || 0;
    }
    if (n < 0 || n >= seasons.length) { return null; }
    return seasons[n];
  }

  function isInSeason(cropId, seasonIndex) {
    var crop = getCrop(cropId);
    var name = seasonName(seasonIndex);
    var seasons;
    var i;

    if (!crop || !name || !Array.isArray(crop.seasons)) { return false; }
    seasons = crop.seasons;
    for (i = 0; i < seasons.length; i++) {
      if (seasons[i] === name) { return true; }
      if (seasons[i] === seasonIndex) { return true; }
    }
    return false;
  }

  function growthStages() {
    return STAGES.slice();
  }

  function stageFor(days, growthDays) {
    var total = Number(growthDays) > 0 ? Number(growthDays) : 7;
    var ratio = (Number(days) || 0) / total;
    if (ratio >= 1) { return 4; }
    if (ratio >= 0.75) { return 3; }
    if (ratio >= 0.5) { return 2; }
    if (ratio >= 0.25) { return 1; }
    return 0;
  }

  // Advance one plot by a number of days. Water is consumed one per day;
  // a dry plot does not grow. Returns the plot (or null for bad input).
  function advance(plot, days) {
    var crop;
    var count;
    var i;
    if (!plot || typeof plot !== 'object') { return null; }
    if (!plot.cropId) { return plot; }

    crop = getCrop(plot.cropId);
    if (!crop) { return plot; }

    count = Math.floor(Number(days) || 0);
    if (count <= 0) { return plot; }

    if (plot.state !== 'planted' && plot.state !== 'ready') { return plot; }
    if (plot.ready) { return plot; }

    if (typeof plot.days !== 'number') { plot.days = 0; }

    for (i = 0; i < count; i++) {
      if (!(Number(plot.water) > 0)) {
        if (!plot.protected && Math.random() < 0.3) {
          plot.weeds = (Number(plot.weeds) || 0) + 1;
        }
        if (!plot.protected && Math.random() < 0.15) {
          plot.pests = (Number(plot.pests) || 0) + 1;
        }
        break;
      }
      plot.water = Number(plot.water) - 1;
      plot.days += 1;
      plot.stage = stageFor(plot.days, crop.growthDays);
      if (plot.stage >= 4) {
        plot.ready = true;
        plot.state = 'ready';
        break;
      }
    }
    return plot;
  }

  function qualityFor(plot) {
    var cfg = config();
    var qualities = Array.isArray(cfg.QUALITIES) ? cfg.QUALITIES : QUALITY_FALLBACK;
    var score;
    if (!plot || typeof plot !== 'object') { return qualities[1] || 'Normal'; }

    score = 45;
    score += (Number(plot.qualityBonus) || 0) * 5;
    if (Number(plot.water) > 0) { score += 10; }
    if (plot.type === 'raised') { score += 8; }
    score -= (Number(plot.pests) || 0) * 12;
    score -= (Number(plot.weeds) || 0) * 5;
    if (plot.protected) { score += 5; }

    if (score >= 80) { return qualities[3] || 'Perfect'; }
    if (score >= 55) { return qualities[2] || 'Good'; }
    if (score >= 30) { return qualities[1] || 'Normal'; }
    return qualities[0] || 'Poor';
  }

  var QUALITY_FALLBACK = ['Poor', 'Normal', 'Good', 'Perfect'];

  function yieldFor(plot) {
    var crop;
    var min;
    var max;
    var quantity;
    var level;
    if (!plot || typeof plot !== 'object' || !plot.cropId) {
      return { cropId: '', quantity: 0, quality: 'Normal' };
    }

    crop = getCrop(plot.cropId);
    if (!crop) {
      return { cropId: '', quantity: 0, quality: 'Normal' };
    }

    min = Math.max(1, Number(crop.yieldMin) || 1);
    max = Math.max(min, Number(crop.yieldMax) || min);
    quantity = min + Math.floor(Math.random() * (max - min + 1));

    if (plot.type === 'raised') { quantity += 1; }
    if (FIF.state && FIF.state.player && FIF.state.player.skills) {
      level = Number(FIF.state.player.skills.harvesting) || 0;
      quantity += Math.floor(level / 2);
    }

    return {
      cropId: crop.id,
      quantity: quantity,
      quality: qualityFor(plot)
    };
  }

  // Called by plots.harvest after yield has been collected.
  // Multi-harvest crops regrow from Bloom; others clear the plot.
  function afterHarvest(plot) {
    var crop;
    if (!plot || typeof plot !== 'object' || !plot.cropId) { return plot || null; }
    crop = getCrop(plot.cropId);
    if (!crop) { return plot; }

    if (crop.multipleHarvest && Number(plot.harvestsLeft) > 0) {
      plot.harvestsLeft = Number(plot.harvestsLeft) - 1;
      plot.days = Math.floor((Number(crop.growthDays) || 7) * 0.5);
      plot.stage = 2;
      plot.ready = false;
      plot.state = 'planted';
      plot.water = Math.max(Number(plot.water) || 0, 1);
      plot.qualityBonus = Math.max(0, (Number(plot.qualityBonus) || 0) - 1);
    } else {
      plot.cropId = null;
      plot.secondaryCropId = null;
      plot.stage = 0;
      plot.days = 0;
      plot.ready = false;
      plot.state = 'harvested';
      plot.harvestsLeft = 0;
    }
    return plot;
  }

  function label(cropId) {
    var crop = getCrop(cropId);
    return crop ? crop.name : '';
  }

  FIF.crops = {
    get: getCrop,
    label: label,
    isInSeason: isInSeason,
    growthStages: growthStages,
    stageFor: stageFor,
    advance: advance,
    qualityFor: qualityFor,
    yieldFor: yieldFor,
    afterHarvest: afterHarvest
  };
})(window.FIF);
