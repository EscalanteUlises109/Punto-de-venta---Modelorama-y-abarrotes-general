

window.appCharts = (function () {
  const registry = {};
  const retries = {}; 
  const MAX_RETRIES = 12; 

  function ensureChartAvailable() {
    return typeof window.Chart !== 'undefined';
  }

  function attempt(ctxId, config) {
    const ctx = document.getElementById(ctxId);
    if (!ctx) {
      console.warn('charts.js: canvas not found:', ctxId);
      return;
    }

    if (!config || !config.data) {
      console.warn('charts.js: invalid config for', ctxId);
      return;
    }

    if (!ensureChartAvailable()) {
      
      retries[ctxId] = (retries[ctxId] || 0) + 1;
      if (retries[ctxId] <= MAX_RETRIES) {
        setTimeout(() => attempt(ctxId, config), 250);
      } else {
        console.warn('charts.js: Chart.js not available, giving up for', ctxId);
      }
      return;
    }

    const existing = registry[ctxId];
    if (existing) {
      try {
        existing.data = config.data;
        existing.options = config.options || existing.options;
        existing.update();
      } catch (err) {
        console.warn('charts.js: update failed, recreating', ctxId, err);
        try { existing.destroy(); } catch (_) {}
        registry[ctxId] = new Chart(ctx, config);
      }
      return;
    }

    try {
      const chart = new Chart(ctx, config);
      registry[ctxId] = chart;
    } catch (err) {
      console.error('charts.js: create chart failed', err);
    }
  }

  function upsert(ctxId, config) {
    attempt(ctxId, config);
  }

  function destroy(ctxId) {
    const existing = registry[ctxId];
    if (existing) {
      try { existing.destroy(); } catch (_) {}
      delete registry[ctxId];
    }
  }

  return { upsert, destroy };
})();

