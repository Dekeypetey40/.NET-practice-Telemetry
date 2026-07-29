import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  retries: 0,
  use: {
    baseURL: 'http://localhost:4200',
    headless: true,
    screenshot: 'only-on-failure',
  },
  webServer: [
    {
      command: 'npx ng serve --port 4200',
      url: 'http://localhost:4200',
      reuseExistingServer: true,
      timeout: 120_000,
    },
  ],
});
