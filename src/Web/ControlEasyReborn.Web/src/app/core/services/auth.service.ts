import { Injectable, signal, computed } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  tenants: TenantInfo[];
}

export interface TenantInfo {
  tenantId: string;
  slug: string;
  displayName: string;
  userDisplayName: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'ce.access_token';
  private readonly refreshKey = 'ce.refresh_token';
  private readonly emailKey = 'ce.email';

  readonly accessToken = signal<string | null>(this.loadToken());
  readonly isAuthenticated = computed(() => this.accessToken() !== null);
  readonly currentTenant = signal<TenantInfo | null>(null);

  login(_request: LoginRequest): Observable<LoginResponse> {
    return of({
      accessToken: 'placeholder-token',
      refreshToken: 'placeholder-refresh',
      tenants: [
        {
          tenantId: '00000000-0000-0000-0000-000000000001',
          slug: 'default',
          displayName: 'Condomínio Padrão',
          userDisplayName: 'Admin',
        },
      ],
    });
  }

  logout(): void {
    this.accessToken.set(null);
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.refreshKey);
    }
  }

  setToken(token: string): void {
    this.accessToken.set(token);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  getRememberedEmail(): string | null {
    return typeof localStorage !== 'undefined'
      ? localStorage.getItem(this.emailKey)
      : null;
  }

  setRememberedEmail(email: string, remember: boolean): void {
    if (typeof localStorage === 'undefined') return;
    if (remember) {
      localStorage.setItem(this.emailKey, email);
    } else {
      localStorage.removeItem(this.emailKey);
    }
  }

  private loadToken(): string | null {
    return typeof localStorage !== 'undefined'
      ? localStorage.getItem(this.tokenKey)
      : null;
  }
}