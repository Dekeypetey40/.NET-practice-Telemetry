(function () {
  'use strict';

  const TERMINAL = ['Completed', 'Failed', 'Canceled'];

  function genId() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = (Math.random() * 16) | 0, v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  function shortId(id) {
    return typeof id === 'string' ? id.slice(0, 8) : (id || '').slice(0, 8);
  }

  const demoStore = { instruments: [], runs: [] };

  function demoCreateInstrument(name, type, serialNumber) {
    const id = genId();
    const instrument = {
      instrumentId: id,
      name: name || 'Instrument',
      type: type || 'Device',
      status: 'Unknown',
      lastHealthCheck: null,
      alarms: []
    };
    demoStore.instruments.push(instrument);
    return Promise.resolve(instrument);
  }

  function demoCreateRun(instrumentId, sampleId, methodName, methodVersion) {
    const id = genId();
    const run = {
      id,
      instrumentId,
      sampleId: sampleId || 'S-001',
      methodMetadataJson: methodName ? JSON.stringify({ methodName, methodVersion }) : '{}',
      currentState: 'Created',
      createdAt: new Date().toISOString(),
      startedAt: null,
      completedAt: null,
      actor: null,
      correlationId: null,
      events: [{ id: genId(), eventType: 'StateTransition', timestamp: new Date().toISOString(), data: 'Created', actor: null, correlationId: null }]
    };
    demoStore.runs.push(run);
    return Promise.resolve(run);
  }

  function demoTransition(runId, action) {
    const run = demoStore.runs.find(r => r.id === runId);
    if (!run) return Promise.reject(new Error('Run not found'));
    const now = new Date().toISOString();
    const transitions = {
      queue: { from: 'Created', to: 'Queued' },
      start: { from: 'Queued', to: 'Running' },
      complete: { from: 'Running', to: 'Completed' },
      fail: { from: 'Running', to: 'Failed' },
      cancel: { from: ['Created', 'Queued', 'Running'], to: 'Canceled' }
    };
    const t = transitions[action];
    if (!t) return Promise.reject(new Error('Unknown action'));
    const fromOk = Array.isArray(t.from) ? t.from.includes(run.currentState) : run.currentState === t.from;
    if (!fromOk) return Promise.reject(new Error(`Run is in state ${run.currentState}; cannot ${action}.`));
    run.currentState = t.to;
    if (t.to === 'Running') run.startedAt = now;
    if (TERMINAL.includes(t.to)) run.completedAt = now;
    run.events = run.events || [];
    run.events.push({
      id: genId(),
      eventType: 'StateTransition',
      timestamp: now,
      data: `${run.events[run.events.length - 1]?.data || run.currentState}→${t.to}`,
      actor: 'operator',
      correlationId: run.correlationId
    });
    return Promise.resolve(run);
  }

  function liveCreateInstrument(baseUrl, name, type, serialNumber) {
    return fetch(`${baseUrl}/instruments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, type, serialNumber })
    }).then(r => r.ok ? r.json() : Promise.reject(new Error(r.status + ' ' + r.statusText)));
  }

  function liveCreateRun(baseUrl, instrumentId, sampleId, methodName, methodVersion, correlationId) {
    const body = { instrumentId, sampleId };
    if (methodName) body.methodName = methodName;
    if (methodVersion) body.methodVersion = methodVersion;
    const headers = { 'Content-Type': 'application/json' };
    if (correlationId) headers['X-Correlation-Id'] = correlationId;
    return fetch(`${baseUrl}/runs`, {
      method: 'POST',
      headers,
      body: JSON.stringify(body)
    }).then(r => r.ok ? r.json() : Promise.reject(new Error(r.status + ' ' + r.statusText)));
  }

  function liveAction(baseUrl, runId, action, actor) {
    const path = action === 'queue' ? 'queue' : action === 'start' ? 'start' : action === 'complete' ? 'complete' : action === 'fail' ? 'fail' : 'cancel';
    const url = `${baseUrl}/runs/${runId}/${path}` + (actor ? `?actor=${encodeURIComponent(actor)}` : '');
    return fetch(url, { method: 'POST' }).then(r => r.ok ? r.json() : Promise.reject(new Error(r.status + ' ' + r.statusText)));
  }

  function liveGetTimeline(baseUrl, runId) {
    return fetch(`${baseUrl}/runs/${runId}/timeline`).then(r => r.ok ? r.json() : (r.status === 404 ? null : Promise.reject(new Error(r.status + ' ' + r.statusText))));
  }

  let mode = 'demo';
  let apiBaseUrl = '';
  let sessionInstruments = [];
  let sessionRuns = [];
  let selectedRunId = null;
  const correlationId = 'demo-' + genId();

  function getInstruments() {
    return mode === 'demo' ? demoStore.instruments : sessionInstruments;
  }

  function getRuns() {
    return mode === 'demo' ? demoStore.runs : sessionRuns;
  }

  function getRun(id) {
    if (mode === 'demo') return demoStore.runs.find(r => r.id === id);
    return sessionRuns.find(r => r.id === id);
  }

  function setStatus(msg, isError) {
    const el = document.getElementById('status');
    if (el) {
      el.textContent = msg || '';
      el.classList.toggle('error', !!isError);
    }
  }

  function renderStateDiagram(currentState) {
    const diagram = document.getElementById('stateDiagram');
    if (!diagram) return;
    const nodes = [
      { id: 'Created', terminal: false },
      { id: 'Queued', terminal: false },
      { id: 'Running', terminal: false },
      { id: 'Completed', terminal: true },
      { id: 'Failed', terminal: true },
      { id: 'Canceled', terminal: true }
    ];
    diagram.innerHTML = '';
    const line1 = document.createElement('div');
    nodes.forEach((n, i) => {
      if (i > 0) {
        const arr = document.createElement('span');
        arr.className = 'arrow';
        arr.textContent = ' → ';
        line1.appendChild(arr);
      }
      const span = document.createElement('span');
      span.className = 'node' + (n.terminal ? ' terminal' : '') + (currentState === n.id ? ' current' : '');
      span.textContent = n.id;
      line1.appendChild(span);
    });
    diagram.appendChild(line1);
    const line2 = document.createElement('p');
    line2.className = 'hint';
    line2.style.marginTop = '0.5rem';
    line2.textContent = 'Created → queue → Queued → start → Running → complete/fail/cancel → terminal.';
    diagram.appendChild(line2);
  }

  function renderInstrumentList() {
    const list = document.getElementById('instrumentList');
    const select = document.getElementById('runInstrumentId');
    if (!list || !select) return;
    const instruments = getInstruments();
    list.innerHTML = instruments.map(inst => {
      const id = inst.instrumentId || inst.id;
      return `<li data-id="${id}">${inst.name} (${inst.type}) <code>${shortId(id)}</code></li>`;
    }).join('') || '<li class="empty">None yet</li>';
    const opts = instruments.map(inst => {
      const id = inst.instrumentId || inst.id;
      return `<option value="${id}">${inst.name} – ${shortId(id)}</option>`;
    }).join('');
    select.innerHTML = '<option value="">Select instrument</option>' + opts;
  }

  function renderRunList() {
    const list = document.getElementById('runList');
    if (!list) return;
    const runs = getRuns();
    list.innerHTML = runs.map(run => {
      const id = run.id;
      const state = (run.currentState || 'Created').toLowerCase();
      return `<li data-id="${id}" class="${selectedRunId === id ? 'selected' : ''}">${run.sampleId} <span class="state-badge state-${state}">${run.currentState}</span> <code>${shortId(id)}</code></li>`;
    }).join('') || '<li class="empty">None yet</li>';
    list.querySelectorAll('li[data-id]').forEach(li => {
      li.addEventListener('click', () => {
        selectedRunId = li.dataset.id;
        renderRunList();
        renderRunDetail();
        renderStateDiagram(getRun(selectedRunId)?.currentState || null);
      });
    });
  }

  function allowedActions(state) {
    const s = (state || 'Created').toLowerCase();
    return {
      queue: s === 'created',
      start: s === 'queued',
      complete: s === 'running',
      fail: s === 'running',
      cancel: ['created', 'queued', 'running'].includes(s)
    };
  }

  function renderRunDetail() {
    const empty = document.getElementById('runDetailEmpty');
    const detail = document.getElementById('runDetail');
    const meta = document.getElementById('runMeta');
    const actions = document.getElementById('runActions');
    const timeline = document.getElementById('runTimeline');
    const bundleSection = document.getElementById('supportBundleSection');
    const bundleLink = document.getElementById('supportBundleLink');
    if (!detail || !empty) return;

    if (!selectedRunId) {
      empty.hidden = false;
      detail.hidden = true;
      return;
    }

    const run = getRun(selectedRunId);
    if (!run) {
      empty.hidden = false;
      detail.hidden = true;
      return;
    }

    empty.hidden = true;
    detail.hidden = false;
    meta.textContent = `Run ${shortId(run.id)} · Sample ${run.sampleId} · ${run.currentState}`;

    const act = allowedActions(run.currentState);
    actions.innerHTML = '';
    if (act.queue) {
      const b = document.createElement('button');
      b.className = 'primary';
      b.textContent = 'Queue';
      b.onclick = () => doAction('queue');
      actions.appendChild(b);
    }
    if (act.start) {
      const b = document.createElement('button');
      b.className = 'primary';
      b.textContent = 'Start';
      b.onclick = () => doAction('start');
      actions.appendChild(b);
    }
    if (act.complete) {
      const b = document.createElement('button');
      b.textContent = 'Complete';
      b.onclick = () => doAction('complete');
      actions.appendChild(b);
    }
    if (act.fail) {
      const b = document.createElement('button');
      b.textContent = 'Fail';
      b.onclick = () => doAction('fail');
      actions.appendChild(b);
    }
    if (act.cancel) {
      const b = document.createElement('button');
      b.textContent = 'Cancel';
      b.onclick = () => doAction('cancel');
      actions.appendChild(b);
    }

    const events = (run.events || []).slice().sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
    timeline.innerHTML = events.map(e => {
      const time = e.timestamp ? new Date(e.timestamp).toLocaleTimeString() : '';
      return `<li>${time} – ${e.eventType}${e.data ? ': ' + e.data : ''}${e.actor ? ' (by ' + e.actor + ')' : ''}</li>`;
    }).join('') || '<li>No events</li>';

    if (mode === 'live' && apiBaseUrl) {
      bundleSection.hidden = false;
      bundleLink.textContent = 'Download support bundle (ZIP)';
      bundleLink.onclick = function (e) {
        e.preventDefault();
        const base = apiBaseUrl.replace(/\/$/, '');
        fetch(`${base}/runs/${run.id}/support-bundle?lastLogEntries=100`, { method: 'POST' })
          .then(r => r.blob())
          .then(blob => {
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `support-bundle-${run.id}.zip`;
            a.click();
            URL.revokeObjectURL(url);
            setStatus('Support bundle downloaded.');
          })
          .catch(err => setStatus(err.message, true));
      };
    } else {
      bundleSection.hidden = true;
    }
  }

  function doAction(action) {
    if (!selectedRunId) return;
    setStatus('');

    if (mode === 'demo') {
      demoTransition(selectedRunId, action)
        .then(() => {
          renderRunList();
          renderRunDetail();
          renderStateDiagram(getRun(selectedRunId)?.currentState || null);
          setStatus(`Run ${action}d.`);
        })
        .catch(err => setStatus(err.message, true));
      return;
    }

    const base = apiBaseUrl.replace(/\/$/, '');
    liveAction(base, selectedRunId, action, 'operator')
      .then(updated => {
        const idx = sessionRuns.findIndex(r => r.id === selectedRunId);
        if (idx >= 0) sessionRuns[idx] = updated;
        return liveGetTimeline(base, selectedRunId);
      })
      .then(tl => {
        const run = getRun(selectedRunId);
        if (run && tl && tl.events) run.events = tl.events;
        renderRunList();
        renderRunDetail();
        renderStateDiagram(getRun(selectedRunId)?.currentState || null);
        setStatus(`Run ${action}d.`);
      })
      .catch(err => setStatus(err.message, true));
  }

  function onCreateInstrument() {
    const name = document.getElementById('instName')?.value?.trim() || 'Instrument';
    const type = document.getElementById('instType')?.value?.trim() || 'Device';
    const serial = document.getElementById('instSerial')?.value?.trim() || null;
    setStatus('');

    if (mode === 'demo') {
      demoCreateInstrument(name, type, serial).then(() => {
        renderInstrumentList();
        setStatus('Instrument created.');
      }).catch(err => setStatus(err.message, true));
      return;
    }

    const base = apiBaseUrl.replace(/\/$/, '');
    if (!base) { setStatus('Set API base URL first.', true); return; }
    liveCreateInstrument(base, name, type, serial)
      .then(inst => {
        sessionInstruments.push(inst);
        renderInstrumentList();
        setStatus('Instrument created.');
      })
      .catch(err => setStatus(err.message, true));
  }

  function onCreateRun() {
    const instrumentId = document.getElementById('runInstrumentId')?.value;
    const sampleId = document.getElementById('runSampleId')?.value?.trim() || 'S-001';
    const methodName = document.getElementById('runMethodName')?.value?.trim() || null;
    const methodVersion = document.getElementById('runMethodVersion')?.value?.trim() || null;
    setStatus('');

    if (!instrumentId) { setStatus('Select an instrument first.', true); return; }

    if (mode === 'demo') {
      demoCreateRun(instrumentId, sampleId, methodName, methodVersion).then(() => {
        renderRunList();
        setStatus('Run created.');
      }).catch(err => setStatus(err.message, true));
      return;
    }

    const base = apiBaseUrl.replace(/\/$/, '');
    if (!base) { setStatus('Set API base URL first.', true); return; }
    liveCreateRun(base, instrumentId, sampleId, methodName, methodVersion, correlationId)
      .then(run => {
        sessionRuns.push(run);
        renderRunList();
        setStatus('Run created.');
      })
      .catch(err => setStatus(err.message, true));
  }

  function onModeChange() {
    const demo = document.querySelector('input[name="mode"][value="demo"]');
    mode = demo?.checked ? 'demo' : 'live';
    const wrap = document.getElementById('apiUrlWrap');
    if (wrap) wrap.hidden = mode !== 'live';
    if (mode === 'live') {
      apiBaseUrl = (document.getElementById('apiUrl')?.value || '').trim();
    }
    renderInstrumentList();
    renderRunList();
    renderRunDetail();
    renderStateDiagram(selectedRunId ? getRun(selectedRunId)?.currentState : null);
  }

  function init() {
    document.querySelectorAll('input[name="mode"]').forEach(radio => {
      radio.addEventListener('change', onModeChange);
    });
    document.getElementById('apiUrl')?.addEventListener('input', function () {
      apiBaseUrl = this.value.trim();
    });
    document.getElementById('btnCreateInstrument')?.addEventListener('click', onCreateInstrument);
    document.getElementById('btnCreateRun')?.addEventListener('click', onCreateRun);

    document.getElementById('apiUrlWrap').hidden = true;
    renderStateDiagram(null);
    renderInstrumentList();
    renderRunList();
    renderRunDetail();
    setStatus('Use Demo mode to try without an API, or set Live API URL and create instruments and runs.');
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
