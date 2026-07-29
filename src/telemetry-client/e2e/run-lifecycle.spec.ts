import { test, expect } from '@playwright/test';

/**
 * Requires the .NET API at localhost:5244 and the Angular app at localhost:4200
 * (Docker Compose client or ng serve with proxy).
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

  test('409 conflict: invalid transition shows conflict snackbar in the UI', async ({ page, request }) => {
    const sampleId = `CONFLICT-${Date.now()}`;

    const createRes = await request.post(`${API}/runs`, {
      data: { instrumentId, sampleId },
    });
    const run = await createRes.json();

    await page.goto(`/runs/${run.id}`);
    await expect(page.locator('.badge-created')).toBeVisible();

    const conflictMessage = 'Run is in state Queued; cannot queue. Only Created runs can be queued.';
    await page.route(`**/api/runs/${run.id}/queue**`, async (route) => {
      await route.fulfill({
        status: 409,
        contentType: 'application/json',
        body: JSON.stringify({ error: conflictMessage }),
      });
    });

    await page.getByRole('button', { name: /queue/i }).click();

    const snackbar = page.locator('.mat-mdc-snack-bar-container');
    await expect(snackbar).toBeVisible();
    await expect(snackbar).toContainText(/cannot queue/i);
    await expect(snackbar).toHaveClass(/snackbar-conflict/);
  });
});
