/* Accessibility widget JS */
(function () {
  'use strict';

  const state = {
    fontScale: 1,
    bgMode: 'normal',
    textColor: '',
    fontFamily: '',
    bgColor: '',
    bgOpacity: 0.88,
    blurLevel: 'medium'
  };

  // Utilities to apply styles
  function applyFontScale(scale) {
    document.documentElement.style.fontSize = (scale * 100) + '%';
  }

  // Apply background: supports presets ('dark','light','normal') or custom hex/color with opacity
  function applyBackground(modeOrColor) {
    var root = document.documentElement;
    var overlay = document.getElementById('site-overlay');

    function hexToRgba(hex, opacity) {
      if (!hex) return null;
      hex = hex.replace('#','');
      if (hex.length === 3) hex = hex.split('').map(h=>h+h).join('');
      var r = parseInt(hex.substring(0,2),16);
      var g = parseInt(hex.substring(2,4),16);
      var b = parseInt(hex.substring(4,6),16);
      return 'rgba('+r+','+g+','+b+','+opacity+')';
    }

    function setOverlayColor(color, opacity) {
      if (!overlay) return;
      if (!color) { overlay.style.backgroundColor = 'transparent'; return; }
      var out = color;
      if (/^#([A-Fa-f0-9]{3,8})$/.test(color)) {
        out = hexToRgba(color, opacity);
      } else if (/^rgb\(/.test(color)) {
        // convert to rgba
        out = color.replace('rgb(', 'rgba(').replace(')', ',' + opacity + ')');
      } else if (/^rgba\(/.test(color)) {
        out = color;
      } else {
        // named colors - still apply opacity by using rgba overlay with computed color isn't trivial; use as-is
        out = color;
      }
      overlay.style.backgroundColor = out;
    }

    if (!overlay) {
      // fallback: apply CSS classes on root as before
      if (modeOrColor === 'dark') { root.classList.remove('acc-high-contrast-light'); root.classList.add('acc-high-contrast-dark'); state.bgMode = 'dark'; }
      else if (modeOrColor === 'light') { root.classList.remove('acc-high-contrast-dark'); root.classList.add('acc-high-contrast-light'); state.bgMode = 'light'; }
      else { root.classList.remove('acc-high-contrast-dark'); root.classList.remove('acc-high-contrast-light'); state.bgMode = 'normal'; }
      saveState();
      return;
    }

    // apply presets
    if (modeOrColor === 'dark') {
      state.bgMode = 'dark';
      setOverlayColor('#000000', state.bgOpacity || 0.88);
    } else if (modeOrColor === 'light') {
      state.bgMode = 'light';
      setOverlayColor('#ffffff', state.bgOpacity || 0.94);
    } else if (modeOrColor === 'normal') {
      state.bgMode = 'normal';
      setOverlayColor(null, 0);
    } else {
      // custom color string
      state.bgMode = 'custom';
      state.bgColor = modeOrColor;
      setOverlayColor(state.bgColor, state.bgOpacity || 0.88);
    }

    // blur class
    root.classList.remove('acc-blur-strong','acc-blur-medium','acc-blur-light');
    if (state.blurLevel === 'strong') root.classList.add('acc-blur-strong');
    else if (state.blurLevel === 'light') root.classList.add('acc-blur-light');
    else root.classList.add('acc-blur-medium');

    saveState();
  }

  function applyTextColor(color) {
    if (color) {
      document.documentElement.style.setProperty('--acc-text-color', color);
      document.body.classList.add('acc-text-override');
    } else {
      document.documentElement.style.removeProperty('--acc-text-color');
      document.body.classList.remove('acc-text-override');
    }
  }

  function applyFontFamily(family) {
    if (family) document.documentElement.style.fontFamily = family;
    else document.documentElement.style.fontFamily = '';
  }

  // Persist (optional)
  function saveState() {
    try { localStorage.setItem('accState', JSON.stringify(state)); } catch { }
  }
  function loadState() {
    try {
      const v = JSON.parse(localStorage.getItem('accState') || '{}');
      if (v.fontScale) state.fontScale = v.fontScale;
      if (v.bgMode) state.bgMode = v.bgMode;
      if (v.textColor) state.textColor = v.textColor;
      if (v.fontFamily) state.fontFamily = v.fontFamily;
      if (v.bgColor) state.bgColor = v.bgColor;
      if (v.bgOpacity) state.bgOpacity = v.bgOpacity;
      if (v.blurLevel) state.blurLevel = v.blurLevel;
    } catch { }
  }

  // Draggable logic for panel
  function makeDraggable(panel, handle) {
    let active = false, startX = 0, startY = 0, origX = 0, origY = 0;
    function pointerDown(e) {
      active = true;
      startX = e.clientX || (e.touches && e.touches[0].clientX);
      startY = e.clientY || (e.touches && e.touches[0].clientY);
      const rect = panel.getBoundingClientRect();
      origX = rect.left;
      origY = rect.top;
      document.addEventListener('pointermove', pointerMove);
      document.addEventListener('pointerup', pointerUp);
    }
    function pointerMove(e) {
      if (!active) return;
      const cx = e.clientX || (e.touches && e.touches[0].clientX);
      const cy = e.clientY || (e.touches && e.touches[0].clientY);
      const dx = cx - startX;
      const dy = cy - startY;
      panel.style.right = 'auto';
      panel.style.left = (origX + dx) + 'px';
      panel.style.top = (origY + dy) + 'px';
    }
    function pointerUp() {
      active = false;
      document.removeEventListener('pointermove', pointerMove);
      document.removeEventListener('pointerup', pointerUp);
    }
    handle.addEventListener('pointerdown', pointerDown, { passive: true });
  }

  // Initialize widget
  function init() {
    loadState();
    const btn = document.getElementById('acc-toggle-btn');
    const panel = document.getElementById('acc-panel');
    const close = document.getElementById('acc-close');

    if (!btn || !panel) return;

    // restore
    applyFontScale(state.fontScale);
    applyBackground(state.bgMode);
    if (state.textColor) applyTextColor(state.textColor);
    if (state.fontFamily) applyFontFamily(state.fontFamily);

    btn.addEventListener('click', function () {
      panel.classList.toggle('hidden');
      if (!panel.classList.contains('hidden')) {
        panel.setAttribute('aria-hidden', 'false');
        panel.querySelector('[data-focus-first]')?.focus();
      } else {
        panel.setAttribute('aria-hidden', 'true');
      }
    });

    btn.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); btn.click(); }
    });

    close?.addEventListener('click', function () { panel.classList.add('hidden'); panel.setAttribute('aria-hidden','true'); btn.focus(); });

    // Controls
    const inc = document.getElementById('acc-font-inc');
    const dec = document.getElementById('acc-font-dec');
    const bgSelect = document.getElementById('acc-bg-select');
    const bgColorInput = document.getElementById('acc-bg-color');
    const bgOpacityInput = document.getElementById('acc-bg-opacity');
    const blurSelect = document.getElementById('acc-blur-select');
    const textColorSelect = document.getElementById('acc-text-color');
    const fontSelect = document.getElementById('acc-font-family');

    inc?.addEventListener('click', function () { state.fontScale = Math.min(1.6, +(state.fontScale) + 0.1); applyFontScale(state.fontScale); saveState(); });
    dec?.addEventListener('click', function () { state.fontScale = Math.max(0.8, +(state.fontScale) - 0.1); applyFontScale(state.fontScale); saveState(); });

    bgSelect?.addEventListener('change', function (e) {
      const v = e.target.value;
      if (v === 'custom') {
        bgColorInput?.focus();
        // apply stored custom if present
        applyBackground(state.bgColor || '#000000');
      } else {
        state.bgColor = '';
        applyBackground(v);
      }
      saveState();
    });

    bgColorInput?.addEventListener('input', function (e) {
      state.bgColor = e.target.value;
      applyBackground(state.bgColor);
      saveState();
    });

    bgOpacityInput?.addEventListener('input', function (e) {
      var val = parseFloat(e.target.value);
      state.bgOpacity = isNaN(val) ? 0.88 : (val/100);
      // reapply
      applyBackground(state.bgMode === 'custom' ? (state.bgColor || '#000') : state.bgMode);
      saveState();
    });

    blurSelect?.addEventListener('change', function (e) {
      state.blurLevel = e.target.value;
      applyBackground(state.bgMode === 'custom' ? (state.bgColor || null) : state.bgMode);
      saveState();
    });

    textColorSelect?.addEventListener('change', function (e) { state.textColor = e.target.value || ''; applyTextColor(state.textColor); saveState(); });
    fontSelect?.addEventListener('change', function (e) { state.fontFamily = e.target.value || ''; applyFontFamily(state.fontFamily); saveState(); });

    // draggable
    const handle = panel.querySelector('.acc-header');
    makeDraggable(panel, handle);

    // Listen for changes in other windows/tabs and reapply immediately
    window.addEventListener('storage', function (e) {
      if (e.key === 'accState') {
        try {
          const v = JSON.parse(e.newValue || '{}');
          if (v.fontScale) { state.fontScale = v.fontScale; applyFontScale(state.fontScale); }
          if (v.bgMode !== undefined) { state.bgMode = v.bgMode; }
          if (v.bgColor !== undefined) { state.bgColor = v.bgColor; }
          if (v.bgOpacity !== undefined) { state.bgOpacity = v.bgOpacity; }
          if (v.blurLevel !== undefined) { state.blurLevel = v.blurLevel; }
          // reapply background after syncing
          applyBackground(state.bgMode === 'custom' ? (state.bgColor || null) : state.bgMode);
          if (v.textColor !== undefined) { state.textColor = v.textColor; applyTextColor(state.textColor); }
          if (v.fontFamily !== undefined) { state.fontFamily = v.fontFamily; applyFontFamily(state.fontFamily); }
        } catch { }
      }
    });
  }

  // DOM ready
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();

})();
