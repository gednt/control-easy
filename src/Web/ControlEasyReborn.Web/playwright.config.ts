import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: ['./e2e', '../../tests/visual', '../../tests/a11y'],
  globalSetup: './e2e/global-setup.ts',
  timeout: 60_000,
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  reporter: 'list',
  use: {
    baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:8080',
    trace: 'on-first-retry',
  },
  snapshotDir: '../../tests/visual/__snapshots__',
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});