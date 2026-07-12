import type { FullConfig } from '@playwright/test';

async function globalSetup(config: FullConfig): Promise<void> {
  const baseURL = config.projects[0]?.use?.baseURL ?? 'http://localhost:8080';
  const email = process.env['E2E_ADMIN_EMAIL'] ?? 'admin@controleasy.app';
  const password = process.env['E2E_ADMIN_PASSWORD'] ?? 'demo123';

  for (let attempt = 0; attempt < 90; attempt++) {
    try {
      const response = await fetch(`${baseURL}/api/v1/security/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      if (response.ok) {
        return;
      }
    } catch {
      // stack still starting
    }
    await new Promise((resolve) => setTimeout(resolve, 2000));
  }

  throw new Error(`Demo stack login not ready at ${baseURL} after 3 minutes.`);
}

export default globalSetup;
