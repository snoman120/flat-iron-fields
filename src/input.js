(function () {
  window.FIF = window.FIF || {};
  var F = window.FIF;
  var keys = {};
  var pressedOnce = {};
  var initialized = false;

  function normalize(e) {
    var k = e && e.key;
    if (!k && typeof e.keyCode === 'number') {
      if (e.keyCode === 32) { k = ' '; }
      else if (e.keyCode >= 48 && e.keyCode <= 57) { k = String(e.keyCode - 48); }
      else if (e.keyCode >= 65 && e.keyCode <= 90) { k = String.fromCharCode(e.keyCode + 32); }
      else if (e.keyCode >= 37 && e.keyCode <= 40) {
        k = ['ArrowLeft', 'ArrowUp', 'ArrowRight', 'ArrowDown'][e.keyCode - 37];
      }
    }
    if (!k) { return ''; }
    if (k.length === 1) { return k.toLowerCase(); }
    return k;
  }

  function setKey(k, value) {
    keys[k] = value;
    if (k === ' ') { keys.space = value; }
    if (k === 'w' || k === 'ArrowUp') { keys.up = value; }
    if (k === 's' || k === 'ArrowDown') { keys.down = value; }
    if (k === 'a' || k === 'ArrowLeft') { keys.left = value; }
    if (k === 'd' || k === 'ArrowRight') { keys.right = value; }
  }

  function shouldBlock(k) {
    return k === ' ' || k === 'ArrowUp' || k === 'ArrowDown' || k === 'ArrowLeft' || k === 'ArrowRight';
  }

  function onKeyDown(e) {
    var k = normalize(e);
    if (!k) { return; }
    if (shouldBlock(k) && e.preventDefault) { e.preventDefault(); }
    if (!keys[k]) { pressedOnce[k] = true; }
    setKey(k, true);
  }

  function onKeyUp(e) {
    var k = normalize(e);
    if (!k) { return; }
    if (shouldBlock(k) && e.preventDefault) { e.preventDefault(); }
    setKey(k, false);
  }

  function clearAll() {
    var k;
    for (k in keys) {
      if (Object.prototype.hasOwnProperty.call(keys, k)) { keys[k] = false; }
    }
    for (k in pressedOnce) {
      if (Object.prototype.hasOwnProperty.call(pressedOnce, k)) { pressedOnce[k] = false; }
    }
  }

  function pressed(key) {
    var k = key;
    if (k === 'space') { k = ' '; }
    if (typeof k === 'string') { k = k.toLowerCase(); }
    if (!k) { return false; }
    if (pressedOnce[k]) {
      pressedOnce[k] = false;
      return true;
    }
    return false;
  }

  function init() {
    if (initialized) { return; }
    initialized = true;
    window.addEventListener('keydown', onKeyDown, false);
    window.addEventListener('keyup', onKeyUp, false);
    window.addEventListener('blur', clearAll, false);
  }

  F.input = {
    init: init,
    keys: keys,
    pressed: pressed
  };
}());
