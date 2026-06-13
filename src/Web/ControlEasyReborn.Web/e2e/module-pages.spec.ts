import { test, expect } from '@playwright/test';

const pages = [
  { path: '/residents', title: 'Residents', createLabel: '+ Add resident' },
  { path: '/visits', title: 'Visits', createLabel: '+ Add visit' },
  { path: '/vehicles', title: 'Vehicles', createLabel: '+ Add vehicle' },
  { path: '/service-providers', title: 'Service Providers', createLabel: '+ Add provider' },
  { path: '/administration', title: 'Administration', createLabel: '+ Add configuration' },
] as const;

test.describe('Module primary pages', () => {
  for (const pageDef of pages) {
    test(`${pageDef.title} page renders heading and create action`, async ({ page }) => {
      await page.goto(pageDef.path);

      await expect(page.getByRole('heading', { name: pageDef.title })).toBeVisible();
      await expect(page.getByRole('button', { name: pageDef.createLabel })).toBeVisible();
    });
  }
});
