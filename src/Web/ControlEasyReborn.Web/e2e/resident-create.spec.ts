import { test, expect } from '@playwright/test';
import {
  demoAdminEmail,
  demoAdminPassword,
  generateValidCpf,
  loginAsDemoUser,
  searchResidents,
  selectFirstApartmentOption,
  submitCreateAndWait,
  uniqueSuffix,
} from './helpers/demo-auth';

test.describe('Resident create with apartment', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsDemoUser(page, demoAdminEmail, demoAdminPassword);
    await page.goto('/residents');
    await expect(page.getByRole('heading', { name: 'Residents' })).toBeVisible();
  });

  test('admin can create a resident linked to an apartment', async ({ page }) => {
    const suffix = uniqueSuffix();
    const residentName = `E2E Resident ${suffix}`;
    const validCpf = generateValidCpf(Number(suffix));

    await page.getByRole('button', { name: 'Add resident' }).click();
    await expect(page.getByRole('heading', { name: 'Add new resident' })).toBeVisible();

    await page.locator('#ar-name').fill(residentName);
    await page.locator('#ar-cpf').fill(validCpf);
    const apartmentLabel = await selectFirstApartmentOption(page, 'ar-apartment');
    await page.locator('#ar-phone').fill('11999990001');

    await submitCreateAndWait(page, '/api/v1/residents', 'Add resident', { useLast: true });
    await expect(page.getByRole('heading', { name: `Add photos for ${residentName}` })).toBeVisible();
    await expect(page.locator('ce-photo-panel')).toBeVisible();
    await page.getByRole('button', { name: 'Done' }).click();
    await expect(page.getByRole('heading', { name: `Add photos for ${residentName}` })).toBeHidden();

    await searchResidents(page, residentName);

    const row = page.getByRole('row').filter({ hasText: residentName });
    await expect(row).toBeVisible({ timeout: 15_000 });
    await expect(row.getByRole('cell', { name: apartmentLabel })).toBeVisible();
  });
});
