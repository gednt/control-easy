import { test, expect } from '@playwright/test';
import {
  demoAdminEmail,
  demoAdminPassword,
  loginAsDemoUser,
  selectFirstApartmentOption,
  submitCreateAndWait,
  uniqueSuffix,
} from './helpers/demo-auth';

test.describe('Vehicle create with apartment', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsDemoUser(page, demoAdminEmail, demoAdminPassword);
    await page.goto('/vehicles');
    await expect(page.getByRole('heading', { name: 'Vehicles' })).toBeVisible();
  });

  test('admin can create a vehicle linked to an apartment', async ({ page }) => {
    const suffix = uniqueSuffix();
    const plate = `E2E${suffix}`;
    const ownerName = `E2E Owner ${suffix}`;

    await page.getByRole('button', { name: 'Add vehicle' }).click();
    await expect(page.getByRole('heading', { name: 'Add vehicle' })).toBeVisible();

    await page.locator('input[formControlName="plate"]').fill(plate);
    await page.locator('input[formControlName="ownerName"]').fill(ownerName);
    const apartmentLabel = await selectFirstApartmentOption(page, 'vh-apartment');
    await page.locator('input[formControlName="brand"]').fill('Toyota');
    await page.locator('input[formControlName="model"]').fill('Corolla');

    await submitCreateAndWait(page, '/api/v1/vehicles', 'Create');
    await expect(page.getByRole('heading', { name: 'Add vehicle' })).toBeHidden();

    const row = page.getByRole('row').filter({ hasText: plate });
    await expect(row).toBeVisible({ timeout: 15_000 });
    await expect(row.getByRole('cell', { name: ownerName })).toBeVisible();
    await expect(row.getByRole('cell', { name: apartmentLabel })).toBeVisible();
  });
});
