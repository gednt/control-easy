import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test('showcase page a11y scan', async ({ page }) => {
  await page.goto('/design-system/showcase');
  await page.waitForTimeout(500);

  const results = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
    .analyze();

  const violations = results.violations.filter(
    v => v.impact === 'serious' || v.impact === 'critical',
  );

  if (violations.length > 0) {
    for (const violation of violations) {
      console.error(
        `[${violation.impact}] ${violation.id}: ${violation.description}`,
      );
      for (const node of violation.nodes) {
        console.error(`  - ${node.html}`);
      }
    }
  }

  expect(violations).toHaveLength(0);
});