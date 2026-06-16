import { test, expect } from '@playwright/test';
import {
  demoAdminEmail,
  demoAdminPassword,
  generateValidCpf,
  loginAsDemoUser,
  submitCreateAndWait,
  uniqueSuffix,
} from './helpers/demo-auth';

test.describe('Apartment edit residents', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsDemoUser(page, demoAdminEmail, demoAdminPassword);
    await page.goto('/apartments');
    await expect(page.getByRole('heading', { name: 'Apartments' })).toBeVisible();
  });

  test('admin can add and deactivate a resident from apartment edit modal', async ({ page }) => {
    const suffix = uniqueSuffix();
    const block = `R${suffix.slice(0, 2)}`;
    const unit = `U${suffix}`;
    const label = `${block}-${unit}`;
    const residentName = `E2E Apt Resident ${suffix}`;
    const validCpf = generateValidCpf(Number(suffix));

    await page.getByRole('button', { name: 'Add apartment' }).click();
    await page.locator('#ap-block').fill(block);
    await page.locator('#ap-unit').fill(unit);
    await submitCreateAndWait(page, '/api/v1/apartments', 'Add apartment', { useLast: true });
    await expect(page.getByRole('heading', { name: 'Add apartment' })).toBeHidden();

    const row = page.getByRole('row').filter({ hasText: label });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit apartment' }).click();
    await expect(page.getByRole('heading', { name: 'Edit apartment' })).toBeVisible();

    const editModal = page.locator('.ce-modal').filter({ hasText: 'Edit apartment' });
    await editModal.getByRole('button', { name: '+ Add resident' }).click();

    await editModal.locator('#ar-name').fill(residentName);
    await editModal.locator('#ar-cpf').fill(validCpf);
    await editModal.locator('#ar-phone').fill('11999990002');

    const createResponse = page.waitForResponse(
      (response) =>
        response.url().includes('/api/v1/residents') &&
        response.request().method() === 'POST' &&
        response.status() >= 200 &&
        response.status() < 300,
      { timeout: 15_000 },
    );
    await editModal.getByRole('button', { name: 'Add resident', exact: true }).click();
    await createResponse;

    const residentRow = editModal.locator('.resident-row').filter({ hasText: residentName });
    await expect(residentRow).toBeVisible({ timeout: 15_000 });

    await residentRow.getByRole('button', { name: 'Deactivate resident' }).click();
    await expect(editModal.locator('.confirm-deactivate-inline')).toBeVisible();

    const deactivateResponse = page.waitForResponse(
      (response) =>
        response.url().includes('/api/v1/residents/') &&
        response.request().method() === 'PUT' &&
        response.status() >= 200 &&
        response.status() < 300,
      { timeout: 15_000 },
    );
    await editModal.locator('.confirm-deactivate-inline').getByRole('button', { name: 'Deactivate', exact: true }).click();
    await deactivateResponse;

    await expect(residentRow).toBeHidden({ timeout: 15_000 });
    await expect(editModal.getByText('No active residents in this apartment.')).toBeVisible();
  });
});
