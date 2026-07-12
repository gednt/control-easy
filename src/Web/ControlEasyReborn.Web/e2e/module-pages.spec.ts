import { test, expect } from '@playwright/test';

const pages = [
  { path: '/residents', title: 'Residents', createLabel: '+ Add resident' },
  { path: '/visits', title: 'Visits', createLabel: '+ Add visit' },
  { path: '/vehicles', title: 'Vehicles', createLabel: '+ Add vehicle' },
  { path: '/service-providers', title: 'Service Providers', createLabel: '+ Add provider' },
  { path: '/administration', title: 'Administration', createLabel: '+ Add configuration' },
] as const;

test.beforeEach(async ({ page }) => {
  await page.addInitScript(() => {
    localStorage.setItem('ce.access_token', 'e2e-token');
    localStorage.setItem('ce.refresh_token', 'e2e-refresh-token');
    localStorage.setItem('ce.tenant_id', '00000000-0000-0000-0000-000000000001');
    localStorage.setItem('ce.profile_id', '00000000-0000-0000-0000-000000000002');
    localStorage.setItem('ce.roles', JSON.stringify(['TenantAdmin']));
    localStorage.setItem(
      'ce.permissions',
      JSON.stringify([
        'residents.read',
        'residents.write',
        'visits.read',
        'visits.write',
        'vehicles.read',
        'vehicles.write',
        'serviceproviders.read',
        'serviceproviders.write',
        'administration.read',
        'administration.write',
      ]),
    );
  });
});

test.describe('Module primary pages', () => {
  for (const pageDef of pages) {
    test(`${pageDef.title} page renders heading and create action`, async ({ page }) => {
      await page.goto(pageDef.path);

      await expect(page.getByRole('heading', { name: pageDef.title })).toBeVisible();
      await expect(page.getByRole('button', { name: pageDef.createLabel })).toBeVisible();
    });
  }
});
