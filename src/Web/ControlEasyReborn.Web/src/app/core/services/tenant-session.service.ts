import { Injectable, inject, signal, computed } from '@angular/core';
import { SecurityApiService, SessionResponse, SessionTenantResponse } from './security-api.service';

@Injectable({ providedIn: 'root' })
export class TenantSessionService {
  private readonly api = inject(SecurityApiService);

  readonly loaded = signal(false);
  readonly loading = signal(false);
  readonly tenantId = signal<string | null>(null);
  readonly tenantSlug = signal<string | null>(null);
  readonly tenantDisplayName = signal<string | null>(null);
  readonly userDisplayName = signal<string | null>(null);
  readonly switchableTenants = signal<SessionTenantResponse[]>([]);

  readonly canSwitchTenant = computed(() => this.switchableTenants().length > 1);

  load(): void {
    if (this.loading()) return;
    this.loading.set(true);

    this.api.getSession().subscribe({
      next: (session) => {
        this.applySession(session);
        this.loading.set(false);
        this.loaded.set(true);
      },
      error: () => {
        this.clear();
        this.loading.set(false);
        this.loaded.set(true);
      },
    });
  }

  applySession(session: SessionResponse): void {
    this.tenantId.set(session.tenantId);
    this.tenantSlug.set(session.tenantSlug);
    this.tenantDisplayName.set(session.tenantDisplayName);
    this.userDisplayName.set(session.userDisplayName);
    this.switchableTenants.set(session.switchableTenants);
  }

  clear(): void {
    this.tenantId.set(null);
    this.tenantSlug.set(null);
    this.tenantDisplayName.set(null);
    this.userDisplayName.set(null);
    this.switchableTenants.set([]);
    this.loaded.set(false);
  }
}
