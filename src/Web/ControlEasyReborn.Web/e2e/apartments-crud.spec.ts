import { test, expect } from '@playwright/test';
import { demoAdminEmail, demoAdminPassword, loginAsDemoUser, submitCreateAndWait, uniqueSuffix } from './helpers/demo-auth';

test.describe('Apartments CRUD', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsDemoUser(page, demoAdminEmail, demoAdminPassword);
    await page.goto('/apartments');
    await expect(page.getByRole('heading', { name: 'Apartments' })).toBeVisible();
  });

  test('admin can create and deactivate an apartment', async ({ page }) => {
    const suffix = uniqueSuffix();
    const block = `Z${suffix.slice(0, 2)}`;
    const unit = `U${suffix}`;
    const label = `${block}-${unit}`;

    await page.getByRole('button', { name: 'Add apartment' }).click();
    await expect(page.getByRole('heading', { name: 'Add apartment' })).toBeVisible();

    await page.locator('#ap-block').fill(block);
    await page.locator('#ap-unit').fill(unit);
    await submitCreateAndWait(page, '/api/v1/apartments', 'Add apartment', { useLast: true });
    await expect(page.getByRole('heading', { name: 'Add apartment' })).toBeHidden();

    const row = page.getByRole('row').filter({ hasText: label });
    await expect(row).toBeVisible();

    await row.getByRole('button', { name: 'Deactivate apartment' }).click();
    await expect(page.getByRole('heading', { name: 'Deactivate apartment' })).toBeVisible();
    const dialog = page.locator('.ce-modal').filter({ hasText: 'Deactivate apartment' });
    const deactivateResponse = page.waitForResponse(
      (response) =>
        response.url().includes('/api/v1/apartments/') &&
        response.request().method() === 'PUT' &&
        response.status() >= 200 &&
        response.status() < 300,
      { timeout: 15_000 },
    );
    await dialog.getByRole('button', { name: 'Deactivate', exact: true }).click();
    await deactivateResponse;

    await expect(row.getByText('Inactive')).toBeVisible({ timeout: 15_000 });
  });
});
