import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, throwError, switchMap, tap, catchError } from 'rxjs';
import { SecurityApiService, LoginResponse, TenantLookupResponse } from './security-api.service';

export type { LoginResponse, TenantLookupResponse } from './security-api.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'ce.access_token';
  private readonly refreshKey = 'ce.refresh_token';
  private readonly emailKey = 'ce.email';
  private readonly tenantIdKey = 'ce.tenant_id';
  private readonly profileIdKey = 'ce.profile_id';
  private readonly rolesKey = 'ce.roles';
  private readonly permissionsKey = 'ce.permissions';

  private readonly api: SecurityApiService;
  private readonly router: Router;

  readonly accessToken = signal<string | null>(this.loadFromStorage(this.tokenKey));
  readonly refreshTokenValue = signal<string | null>(this.loadFromStorage(this.refreshKey));
  readonly isAuthenticated = computed(() => this.accessToken() !== null);
  readonly tenantId = signal<string | null>(this.loadFromStorage(this.tenantIdKey));
  readonly profileId = signal<string | null>(this.loadFromStorage(this.profileIdKey));
  readonly roles = signal<string[]>(this.loadJsonFromStorage(this.rolesKey));
  readonly permissions = signal<string[]>(this.loadJsonFromStorage(this.permissionsKey));
  readonly userDisplayName = signal<string | null>(null);
  readonly mustChangePassword = signal(false);

  readonly isPlatformAdmin = computed(() => this.roles().includes('PlatformAdmin'));
  readonly isTenantAdmin = computed(() => this.roles().includes('TenantAdmin'));

  constructor(api: SecurityApiService, router: Router) {
    this.api = api;
    this.router = router;
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.api.login({ email, password }).pipe(
      tap((response) => {
        this.setTokens(response.token, response.refreshToken);
        this.tenantId.set(response.tenantId);
        this.profileId.set(response.profileId);
        this.roles.set(response.roles);
        this.permissions.set(response.permissions);
        this.mustChangePassword.set(response.mustChangePassword);
        this.saveToStorage(this.tenantIdKey, response.tenantId);
        this.saveToStorage(this.profileIdKey, response.profileId);
        this.saveJsonToStorage(this.rolesKey, response.roles);
        this.saveJsonToStorage(this.permissionsKey, response.permissions);
      }),
    );
  }

  refreshAuth(): Observable<string | null> {
    const refresh = this.refreshTokenValue();
    if (!refresh) {
      this.logout();
      return of(null);
    }
    return this.api.refresh(refresh).pipe(
      tap((response) => {
        this.setTokens(response.token, response.refreshToken);
      }),
      switchMap(() => of(this.accessToken())),
      catchError(() => {
        this.logout();
        return of(null);
      }),
    );
  }

  switchTenant(tenantId: string): Observable<void> {
    return this.api.switchTenant(tenantId).pipe(
      tap((response) => {
        this.setTokens(response.token, response.refreshToken);
        this.tenantId.set(response.tenantId);
        this.profileId.set(response.profileId);
        this.roles.set(response.roles);
        this.permissions.set(response.permissions);
        this.saveToStorage(this.tenantIdKey, response.tenantId);
        this.saveToStorage(this.profileIdKey, response.profileId);
        this.saveJsonToStorage(this.rolesKey, response.roles);
        this.saveJsonToStorage(this.permissionsKey, response.permissions);
      }),
      switchMap(() => of(void 0)),
    );
  }

  lookupTenants(email: string): Observable<TenantLookupResponse[]> {
    return this.api.lookupTenants(email);
  }

  logout(): void {
    this.accessToken.set(null);
    this.refreshTokenValue.set(null);
    this.tenantId.set(null);
    this.profileId.set(null);
    this.roles.set([]);
    this.permissions.set([]);
    this.mustChangePassword.set(false);
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.refreshKey);
      localStorage.removeItem(this.tenantIdKey);
      localStorage.removeItem(this.profileIdKey);
      localStorage.removeItem(this.rolesKey);
      localStorage.removeItem(this.permissionsKey);
    }
    this.router.navigate(['/login']);
  }

  setToken(token: string): void {
    this.accessToken.set(token);
    this.saveToStorage(this.tokenKey, token);
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

  private setTokens(accessToken: string, refreshToken: string): void {
    this.accessToken.set(accessToken);
    this.refreshTokenValue.set(refreshToken);
    this.saveToStorage(this.tokenKey, accessToken);
    this.saveToStorage(this.refreshKey, refreshToken);
  }

  private loadFromStorage(key: string): string | null {
    return typeof localStorage !== 'undefined' ? localStorage.getItem(key) : null;
  }

  private saveToStorage(key: string, value: string): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(key, value);
    }
  }

  private loadJsonFromStorage(key: string): string[] {
    if (typeof localStorage === 'undefined') return [];
    const raw = localStorage.getItem(key);
    if (!raw) return [];
    try {
      return JSON.parse(raw);
    } catch {
      return [];
    }
  }

  private saveJsonToStorage(key: string, value: string[]): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(key, JSON.stringify(value));
    }
  }
}