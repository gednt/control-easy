import { test, expect } from '@playwright/test';

const viewports = [
  { name: 'mobile', width: 375, height: 812 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'desktop', width: 1440, height: 900 },
] as const;

const themes = ['light', 'dark'] as const;

for (const viewport of viewports) {
  for (const theme of themes) {
    test(`showcase ${viewport.name} ${theme}`, async ({ page }) => {
      await page.setViewportSize({ width: viewport.width, height: viewport.height });
      await page.goto('/design-system/showcase');

      const select = page.locator('.theme-switcher select');
      if (await select.isVisible()) {
        await select.selectOption(theme);
      }

      await page.waitForTimeout(500);

      await expect(page).toHaveScreenshot(
        `showcase-${viewport.name}-${theme}.png`,
        { maxDiffPixelRatio: 0.001 },
      );
    });
  }
});