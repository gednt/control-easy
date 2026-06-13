import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface DemoTenantInfo {
  slug: string;
  displayName: string;
}

export interface DemoInfoResponse {
  enabled: boolean;
  seedVersion?: number;
  tenants?: DemoTenantInfo[];
}

@Injectable({ providedIn: 'root' })
export class DemoInfoService {
  private readonly http = inject(HttpClient);

  readonly enabled = signal(false);
  readonly seedVersion = signal(0);
  readonly tenants = signal<DemoTenantInfo[]>([]);
  readonly loaded = signal(false);

  async load(): Promise<void> {
    try {
      const info = await firstValueFrom(this.http.get<DemoInfoResponse>('/api/v1/demo/info'));
      this.enabled.set(info.enabled);
      this.seedVersion.set(info.seedVersion ?? 0);
      this.tenants.set(info.tenants ?? []);
    } catch {
      this.enabled.set(false);
      this.seedVersion.set(0);
      this.tenants.set([]);
    } finally {
      this.loaded.set(true);
    }
  }
}

export function demoInfoInitializer(demoInfo: DemoInfoService): () => Promise<void> {
  return () => demoInfo.load();
}
