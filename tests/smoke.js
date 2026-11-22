(function () {
  'use strict';

  var results = [];
  var pass = 0;
  var fail = 0;

  function check(name, fn) {
    try {
      var value = fn();
      if (value === false) throw new Error('returned false');
      results.push('<div class="pass">PASS — ' + name + '</div>');
      pass++;
    } catch (err) {
      results.push('<div class="fail">FAIL — ' + name + ': ' + err.message + '</div>');
      fail++;
    }
  }

  function exists(obj, key) {
    return !!(obj && typeof obj[key] === 'function');
  }

  window.addEventListener('load', function () {
    var F = window.FIF;
    var out = document.getElementById('results');

    check('FIF namespace exists', function () { return !!F; });

    check('config data present', function () {
      return F && F.config && F.config.CROPS.length >= 25 && F.config.QUALITIES.length === 4 &&
        F.config.SEASONS.length === 4 && F.config.TRACTOR_TYPES.length >= 4 &&
        F.config.ATTACHMENTS.length >= 6 && F.config.SKILLS.length >= 6;
    });

    check('state shape present', function () {
      return F && F.state && F.state.farm && F.state.inventory && F.state.tractor && F.state.player;
    });

    check('crop API present', function () {
      return exists(F && F.crops, 'isInSeason') && exists(F && F.crops, 'growthStages') &&
        exists(F && F.crops, 'advance') && exists(F && F.crops, 'qualityFor') && exists(F && F.crops, 'yieldFor');
    });

    check('plot API present', function () {
      return exists(F && F.plots, 'createFarm') && exists(F && F.plots, 'plotShape') &&
        exists(F && F.plots, 'till') && exists(F && F.plots, 'water') &&
        exists(F && F.plots, 'plant') && exists(F && F.plots, 'harvest') && exists(F && F.plots, 'expand');
    });

    check('economy API present', function () {
      return exists(F && F.economy, 'price') && exists(F && F.economy, 'sell') &&
        exists(F && F.economy, 'buySeed') && exists(F && F.economy, 'buyPlot') &&
        exists(F && F.economy, 'buyTractor') && exists(F && F.economy, 'craft');
    });

    check('progression API present', function () {
      return exists(F && F.progression, 'levelForXP') && exists(F && F.progression, 'addXP') &&
        exists(F && F.progression, 'investSkill') && exists(F && F.progression, 'checkUnlocks');
    });

    check('tractor API present', function () {
      return exists(F && F.tractors, 'create') && exists(F && F.tractors, 'attach') &&
        exists(F && F.tractors, 'update') && exists(F && F.tractors, 'applyAttachment');
    });

    check('client API present', function () {
      return exists(F && F.input, 'init') && exists(F && F.input, 'pressed') &&
        exists(F && F.ui, 'init') && exists(F && F.ui, 'render') &&
        exists(F && F.ui, 'handleClick') && exists(F && F.ui, 'message') &&
        exists(F && F.game, 'start') && exists(F && F.game, 'newDay');
    });

    check('farm stages 0 and 4 produce 5 and 50 plots', function () {
      var a = F.plots.createFarm(0);
      var b = F.plots.createFarm(4);
      return a.length === 5 && b.length === 50;
    });

    check('till/water/plant/grow/harvest loop runs', function () {
      F.reset();
      var plot = F.state.farm.plots[0];
      var crop = F.config.CROPS[1];
      var tries = 0;
      var wasReady = false;
      var yieldData;

      F.plots.till(plot);
      F.plots.water(plot);
      if (!F.plots.plant(plot, crop.id)) { return false; }

      while (!plot.ready && tries < 12) {
        F.plots.water(plot);
        F.crops.advance(plot, 4);
        tries++;
      }
      wasReady = !!plot.ready;
      yieldData = F.plots.harvest(plot);
      return wasReady && !!yieldData && yieldData.quantity > 0;
    });

    check('economy sell increments coins', function () {
      F.reset();
      var before = F.state.coins;
      var crop = F.config.CROPS[0];
      var inv = F.state.inventory.crops;
      if (!inv[crop.id] || typeof inv[crop.id] !== 'object') { inv[crop.id] = {}; }
      inv[crop.id].Normal = (inv[crop.id].Normal || 0) + 2;
      F.economy.sell(crop.id, 1, 'Normal');
      return F.state.coins > before;
    });

    check('progression levels from XP', function () {
      return F.progression.levelForXP(0) === 1 && F.progression.levelForXP(999999) >= 6;
    });

    check('buySeed spends coins and adds seed', function () {
      F.reset();
      var beforeCoins = F.state.coins;
      var beforeSeeds = F.state.inventory.seeds.carrot || 0;
      if (!F.economy.buySeed('carrot', 1)) { return false; }
      return F.state.coins < beforeCoins && (F.state.inventory.seeds.carrot || 0) === beforeSeeds + 1;
    });

    check('buyPlot expands the farm', function () {
      F.reset();
      F.state.coins = 1000;
      var before = F.state.farm.plots.length;
      if (!F.economy.buyPlot()) { return false; }
      return F.state.farm.plots.length > before;
    });

    check('level up grants skill points and invest works', function () {
      F.reset();
      F.progression.addXP(150, 'test');
      if (F.state.player.level < 2) { return false; }
      var before = F.state.player.skillPoints;
      if (!F.progression.investSkill('planting')) { return false; }
      return F.state.player.skillPoints === before - 1 && F.state.player.skills.planting === 1;
    });

    check('tractor create and attach work', function () {
      var t = F.tractors.create('basic');
      F.tractors.attach('tillage');
      return !!t && F.state.tractor.attachment === 'tillage';
    });

    check('tractor attachments apply plot effects', function () {
      F.reset();
      var plot = F.state.farm.plots[0];
      var tilled = false;
      F.tractors.create('basic');
      tilled = F.tractors.applyAttachment(plot, 'tillage') && plot.state === 'tilled';
      F.tractors.applyAttachment(plot, 'watering');
      F.tractors.applyAttachment(plot, 'fertilizer');
      return tilled && plot.water > 0 && plot.qualityBonus > 0;
    });

    results.unshift('<div class="summary">' + pass + ' passed, ' + fail + ' failed</div>');
    out.innerHTML = results.join('');
  });
})();
