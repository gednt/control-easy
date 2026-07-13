import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from '../../e2e/helpers/demo-auth';

const viewports = [
  { name: 'mobile', width: 375, height: 812 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'desktop', width: 1440, height: 900 },
];

const themes = ['light', 'dark'] as const;

for (const viewport of viewports) {
  for (const theme of themes) {
    test(`showcase-${viewport.name}-${theme}`, async ({ page }) => {
      await page.setViewportSize({ width: viewport.width, height: viewport.height });
      await loginAsDemoUser(page);
      await page.goto('/design-system/showcase');
      await expect(page.getByRole('heading', { name: 'Design System Showcase' })).toBeVisible();

      if (theme === 'dark') {
        await page.selectOption('.theme-switcher select', 'dark');
      }

      await page.waitForTimeout(500);

      await expect(page).toHaveScreenshot(`showcase-${viewport.name}-${theme}.png`, {
        maxDiffPixelRatio: 0.001,
      });
    });
  }
}
