import { test, expect } from '@playwright/test';

const baseUrl = process.env['E2E_BASE_URL'] ?? 'http://localhost:8080';
const bootstrapEmail = process.env['BOOTSTRAP_EMAIL'] ?? 'platform-admin@controleasy.local';
const bootstrapPassword = process.env['BOOTSTRAP_PASSWORD'] ?? 'Bootstrap-ChangeMe1!';

test.describe('First-boot PlatformAdmin login', () => {
  test('bootstrap credentials reach change-password then dashboard', async ({ page, request }) => {
    const bootstrap = await request.get(`${baseUrl}/api/v1/security/bootstrap`);
    test.skip(!(await bootstrap.json()).pending, 'Bootstrap credentials are not pending');

    await page.goto(`${baseUrl}/login`);

    await expect(page.getByText('First boot — Platform Admin')).toBeVisible();
    await expect(page.getByRole('textbox', { name: 'Email' })).toHaveValue(bootstrapEmail);
    await expect(page.getByRole('textbox', { name: 'Password' })).toHaveValue(bootstrapPassword);

    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page).toHaveURL(`${baseUrl}/change-password`);
    await expect(page.getByRole('heading', { name: 'Change your password' })).toBeVisible();
    await expect(page.getByLabel('Current password')).toHaveValue(bootstrapPassword);

    const newPassword = `${bootstrapPassword}X1`;
    await page.getByLabel('New password', { exact: true }).fill(newPassword);
    await page.getByLabel('Confirm new password').fill(newPassword);
    await page.getByRole('button', { name: 'Update password' }).click();

    await expect(page).toHaveURL(`${baseUrl}/`);
    await expect(page.getByText('Getting started')).toBeVisible();
  });
});
