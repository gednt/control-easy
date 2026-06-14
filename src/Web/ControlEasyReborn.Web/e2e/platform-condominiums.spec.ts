import { test, expect } from '@playwright/test';

const baseUrl = process.env['E2E_BASE_URL'] ?? 'http://localhost:8080';

test.describe('PlatformAdmin condominium registration', () => {
  test('platform admin registers a new condominium', async ({ page }) => {
    const slug = `e2e-tenant-${Date.now()}`;
    const displayName = `E2E Condominium ${Date.now()}`;

    await page.goto(`${baseUrl}/login`);
    await page.getByRole('textbox', { name: 'Email' }).fill('platform@controleasy.app');
    await page.getByRole('textbox', { name: 'Password' }).fill('demo123');
    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page).toHaveURL(`${baseUrl}/platform/condominiums`);
    await expect(page.getByRole('heading', { name: 'Condominiums' })).toBeVisible();

    await page.getByRole('button', { name: 'Register condominium' }).click();
    await page.getByLabel('Display name').fill(displayName);
    await page.getByLabel('Slug').fill(slug);
    await page.getByRole('button', { name: 'Register', exact: true }).click();

    await expect(page.getByText(displayName)).toBeVisible();
    await expect(page.getByText(slug)).toBeVisible();
  });
});
