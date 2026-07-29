import { test, expect } from '@playwright/test';

/**
 * Requires the .NET API to be running at localhost:5244 with a database.
 * The Angular dev server is started automatically by Playwright (see playwright.config.ts).
 */

const API = 'http://localhost:5244';

test.describe('Run lifecycle', () => {
  let instrumentId: string;

  test.beforeAll(async ({ request }) => {
    const res = await request.post(`${API}/instruments`, {
      data: { name: 'E2E Instrument', type: 'Chromatograph', serialNumber: `E2E-${Date.now()}` },
    });
    expect(res.ok()).toBeTruthy();
    const body = await res.json();
    // Create instrument returns InstrumentHealthResponse with instrumentId (not id).
    instrumentId = body.instrumentId;
  });

  test('happy path: create, queue, start, complete and verify timeline', async ({ page, request }) => {
    const sampleId = `SAMPLE-${Date.now()}`;

    const createRes = await request.post(`${API}/runs`, {
      data: { instrumentId, sampleId, methodName: 'HPLC-Standard' },
    });
    expect(createRes.ok()).toBeTruthy();
    const run = await createRes.json();

    await page.goto(`/runs/${run.id}`);
    await expect(page.locator('mat-card-title').first()).toContainText('Run Details');

    await page.getByRole('button', { name: /queue/i }).click();
    await expect(page.locator('.badge-queued')).toBeVisible();

    await page.getByRole('button', { name: /start/i }).click();
    await expect(page.locator('.badge-running')).toBeVisible();

    await page.getByRole('button', { name: /complete/i }).click();
    await expect(page.locator('.badge-completed')).toBeVisible();

    const timeline = page.locator('mat-list-item');
    // Create does not emit a RunEvent; queue/start/complete each do.
    await expect(timeline).toHaveCount(3);
  });

  test('409 conflict: starting a Created run is rejected by the API', async ({ request }) => {
    const sampleId = `CONFLICT-${Date.now()}`;

    const createRes = await request.post(`${API}/runs`, {
      data: { instrumentId, sampleId },
    });
    const run = await createRes.json();

    const startRes = await request.post(`${API}/runs/${run.id}/start`);
    expect(startRes.status()).toBe(409);
    const errorBody = await startRes.text();
    expect(errorBody).toContain('cannot start');
  });
});
