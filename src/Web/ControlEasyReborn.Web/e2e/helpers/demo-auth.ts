import { expect, Page } from '@playwright/test';

export const demoAdminEmail = process.env['E2E_ADMIN_EMAIL'] ?? 'admin@controleasy.app';
export const demoAdminPassword = process.env['E2E_ADMIN_PASSWORD'] ?? 'demo123';
export const demoPorteiroEmail = process.env['E2E_PORTEIRO_EMAIL'] ?? 'porteiro@controleasy.app';
export const demoPorteiroPassword = process.env['E2E_PORTEIRO_PASSWORD'] ?? 'demo123';

export async function loginAsDemoUser(
  page: Page,
  email = demoAdminEmail,
  password = demoAdminPassword,
): Promise<void> {
  await page.goto('/login');
  await page.getByRole('textbox', { name: 'Email' }).fill(email);
  await page.getByRole('textbox', { name: 'Password' }).fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await expect(page).not.toHaveURL(/\/login$/, { timeout: 15_000 });
  await dismissDemoBannerIfVisible(page);
}

export async function selectFirstApartmentOption(page: Page, selectId: string): Promise<string> {
  const select = page.locator(`#${selectId}`);
  await expect(select).toBeVisible({ timeout: 15_000 });
  await expect(select.locator('option')).not.toHaveCount(1, { timeout: 15_000 });

  const firstValue = await select.locator('option').nth(1).getAttribute('value');
  const firstLabel = await select.locator('option').nth(1).textContent();
  if (!firstValue) {
    throw new Error(`No apartment options available for #${selectId}`);
  }

  await select.selectOption(firstValue);
  return firstLabel?.trim() ?? firstValue;
}

export function uniqueSuffix(): string {
  return Date.now().toString().slice(-6);
}

export function generateValidCpf(seed: number): string {
  const nums = new Array<number>(9);
  let value = 100_000_000 + (seed % 899_999_999);
  for (let i = 8; i >= 0; i--) {
    nums[i] = value % 10;
    value = Math.floor(value / 10);
  }
  if (nums.every((n) => n === nums[0])) {
    nums[8] = (nums[8]! + 1) % 10;
  }

  const computeCheckDigit = (digits: number[], weightStart: number): number => {
    const sum = digits.reduce((acc, digit, index) => acc + digit * (weightStart - index), 0);
    const remainder = sum % 11;
    return remainder < 2 ? 0 : 11 - remainder;
  };

  const check1 = computeCheckDigit(nums, 10);
  const withFirst = [...nums, check1];
  const check2 = computeCheckDigit(withFirst, 11);
  return `${nums.join('')}${check1}${check2}`;
}

export async function dismissDemoBannerIfVisible(page: Page): Promise<void> {
  const dismiss = page.getByRole('button', { name: 'Dismiss demo banner' });
  if (await dismiss.isVisible().catch(() => false)) {
    await dismiss.click();
  }
}

export async function searchResidents(page: Page, term: string): Promise<void> {
  const search = page.getByRole('searchbox', { name: 'Search by name, apartment, or CPF...' });
  await search.fill(term);
  await page.waitForTimeout(400);
}

export async function submitCreateAndWait(
  page: Page,
  apiPath: string,
  submitButtonName: string,
  options?: { useLast?: boolean },
): Promise<void> {
  const button = options?.useLast
    ? page.getByRole('button', { name: submitButtonName }).last()
    : page.getByRole('button', { name: submitButtonName });

  const responsePromise = page.waitForResponse(
    (response) =>
      response.url().includes(apiPath) &&
      response.request().method() === 'POST' &&
      response.status() >= 200 &&
      response.status() < 300,
    { timeout: 15_000 },
  );
  await button.click();
  await responsePromise;
}
