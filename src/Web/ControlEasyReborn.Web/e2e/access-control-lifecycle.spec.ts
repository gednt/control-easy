import { test, expect, type APIRequestContext } from '@playwright/test';
import {
  demoAdminEmail,
  demoAdminPassword,
  loginAsDemoUser,
} from './helpers/demo-auth';

/**
 * Phase 8/9 (T070) E2E: Access-control credential lifecycle through the
 * JSON API. Because the in-app QR / credentials pages are not yet wired
 * into the Angular router, this spec drives the documented lifecycle
 * (issue → scan entrance → scan exit → revoke → refused scan) via the
 * browser session's authenticated request context. The same browser
 * session is used to confirm no raw QR ever appears in a list/read
 * response (Constitution: redaction-first).
 *
 * All assertions run against the worktree host
 * (E2E_BASE_URL=https://ce-feat-qr-entrance-exit-access.localhost).
 */

test.describe('Access Control lifecycle (API)', () => {
  let requestCtx: APIRequestContext;
  let residentId: string;
  let credentialId: string;
  let issuedQrPayload: string;

  test.beforeEach(async ({ page, context }) => {
    await loginAsDemoUser(page, demoAdminEmail, demoAdminPassword);
    requestCtx = context.request;

    // Pick an active resident from the demo seed to bind the credential to.
    const residentsRes = await requestCtx.get('/api/v1/residents', {
      params: { status: 'active', take: '5' },
    });
    expect(residentsRes.status(), 'residents list').toBeLessThan(300);
    const residents = (await residentsRes.json()) as Array<{ id: string }>;
    expect(residents.length, 'demo resident').toBeGreaterThan(0);
    residentId = residents[0]!.id;
  });

  test('issue → scan entrance → scan exit → revoke → refused scan', async () => {
    // 1. Issue a QR credential for the active resident.
    const issueRes = await requestCtx.post('/api/v1/access-credentials', {
      data: {
        subjectType: 'resident',
        subjectId: residentId,
      },
    });
    expect(issueRes.status(), 'issue credential').toBe(200);
    const issued = (await issueRes.json()) as {
      credentialId: string;
      qrPayload: string;
    };
    expect(issued.credentialId).toBeTruthy();
    expect(issued.qrPayload).toBeTruthy();
    credentialId = issued.credentialId;
    issuedQrPayload = issued.qrPayload;

    // 2. Scan for entrance.
    const entranceRes = await requestCtx.post('/api/v1/access-events/scans', {
      data: {
        qrPayload: issuedQrPayload,
        direction: 'entrance',
        scanAttemptId: crypto.randomUUID(),
      },
    });
    expect(entranceRes.status(), 'scan entrance').toBe(200);
    const entrance = (await entranceRes.json()) as {
      decision: string;
      destinationBlock: string;
      destinationUnit: string;
    };
    expect(entrance.decision).toBe('recorded');
    expect(entrance.destinationBlock).toBeTruthy();
    expect(entrance.destinationUnit).toBeTruthy();

    // 3. Scan for exit (new attempt id to avoid idempotency reuse).
    const exitRes = await requestCtx.post('/api/v1/access-events/scans', {
      data: {
        qrPayload: issuedQrPayload,
        direction: 'exit',
        scanAttemptId: crypto.randomUUID(),
      },
    });
    expect(exitRes.status(), 'scan exit').toBe(200);
    const exit = (await exitRes.json()) as { decision: string };
    expect(exit.decision).toBe('recorded');

    // 4. Revoke the credential.
    const revokeRes = await requestCtx.post(
      `/api/v1/access-credentials/${credentialId}/revoke`,
      { data: { reasonCode: 'lost' } },
    );
    expect(revokeRes.status(), 'revoke credential').toBeLessThan(300);
    const revoked = (await revokeRes.json()) as { resultingStatus: string };
    expect(revoked.resultingStatus).toBe('revoked');

    // 5. A subsequent scan must be refused with credential_inactive.
    const refusedRes = await requestCtx.post('/api/v1/access-events/scans', {
      data: {
        qrPayload: issuedQrPayload,
        direction: 'entrance',
        scanAttemptId: crypto.randomUUID(),
      },
    });
    expect(refusedRes.status(), 'refused scan').toBe(422);
    const refusedBody = (await refusedRes.json()) as { failureCode?: string };
    expect(refusedBody.failureCode).toBe('credential_inactive');
  });

  test('credential list never exposes the raw QR payload', async () => {
    // Issue a credential first (cleanup-safe — we revoke at the end).
    const issueRes = await requestCtx.post('/api/v1/access-credentials', {
      data: {
        subjectType: 'resident',
        subjectId: residentId,
      },
    });
    expect(issueRes.status()).toBe(200);
    const issued = (await issueRes.json()) as {
      credentialId: string;
      qrPayload: string;
    };
    credentialId = issued.credentialId;
    issuedQrPayload = issued.qrPayload;

    // List credentials.
    const listRes = await requestCtx.get('/api/v1/access-credentials');
    expect(listRes.status()).toBe(200);
    const list = (await listRes.json()) as Array<Record<string, unknown>>;
    const ourCred = list.find(c => c['id'] === issued.credentialId);
    expect(ourCred, 'credential present in list').toBeDefined();
    const serialized = JSON.stringify(ourCred);
    expect(serialized, 'raw QR not in list').not.toContain(issuedQrPayload);

    test.afterAll(async () => {
      if (credentialId) {
        await requestCtx.post(`/api/v1/access-credentials/${credentialId}/revoke`, {
          data: { reasonCode: 'cleanup' },
        });
      }
    });
  });
});
