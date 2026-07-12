import { test, expect } from '@playwright/test';
import {
  demoAdminEmail,
  demoAdminPassword,
  loginAsDemoUser,
  selectFirstApartmentOption,
  submitCreateAndWait,
  uniqueSuffix,
} from './helpers/demo-auth';

test.describe('Visit check-in and check-out', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsDemoUser(page, demoAdminEmail, demoAdminPassword);
    await page.goto('/visits');
    await expect(page.getByRole('heading', { name: 'Visits' })).toBeVisible();
  });

  test('admin can create a visit, check in, and check out', async ({ page }) => {
    const suffix = uniqueSuffix();
    const visitorName = `E2E Visitor ${suffix}`;
    const document = `DOC${suffix}`;

    await page.getByRole('button', { name: 'Add visit' }).click();
    await expect(page.getByRole('heading', { name: 'Add visit' })).toBeVisible();

    await page.locator('input[formControlName="visitorName"]').fill(visitorName);
    await page.locator('input[formControlName="visitorDocument"]').fill(document);
    await selectFirstApartmentOption(page, 'vs-apartment');
    await page.locator('input[formControlName="purpose"]').fill('E2E delivery');

    await submitCreateAndWait(page, '/api/v1/visits', 'Create');
    await expect(page.getByRole('heading', { name: 'Add visit' })).toBeHidden();

    const row = page.getByRole('row').filter({ hasText: visitorName });
    await expect(row).toBeVisible({ timeout: 15_000 });
    await expect(row.getByText('Pending')).toBeVisible();

    const checkInResponse = page.waitForResponse(
      (response) =>
        response.url().includes('/checkin') &&
        response.request().method() === 'POST' &&
        response.status() >= 200 &&
        response.status() < 300,
      { timeout: 15_000 },
    );
    await row.getByRole('button', { name: 'Check in' }).click();
    await checkInResponse;
    await expect(row.getByText('On-site')).toBeVisible({ timeout: 15_000 });
    await expect(row.locator('td').nth(4)).not.toHaveText('—');

    const checkOutResponse = page.waitForResponse(
      (response) =>
        response.url().includes('/checkout') &&
        response.request().method() === 'POST' &&
        response.status() >= 200 &&
        response.status() < 300,
      { timeout: 15_000 },
    );
    await row.getByRole('button', { name: 'Check out' }).click();
    await checkOutResponse;
    await expect(row.getByText('Checked out')).toBeVisible({ timeout: 15_000 });
    await expect(row.locator('td').nth(5)).not.toHaveText('—');
    await expect(row.getByRole('button', { name: 'Check in' })).toHaveCount(0);
    await expect(row.getByRole('button', { name: 'Check out' })).toHaveCount(0);
  });
});
