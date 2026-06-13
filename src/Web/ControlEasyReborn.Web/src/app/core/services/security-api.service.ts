import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  tenantId: string;
  profileId: string;
  roles: string[];
  permissions: string[];
  mustChangePassword: boolean;
}

export interface RefreshResponse {
  token: string;
  refreshToken: string;
}

export interface TenantLookupResponse {
  tenantId: string;
  slug: string;
  displayName: string;
  userDisplayName: string;
}

export interface TenantSwitchRequest {
  tenantId: string;
}

export interface TenantSwitchResponse {
  token: string;
  refreshToken: string;
  tenantId: string;
  profileId: string;
  roles: string[];
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class SecurityApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/security';

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, request);
  }

  refresh(refreshToken: string): Observable<RefreshResponse> {
    return this.http.post<RefreshResponse>(`${this.baseUrl}/auth/refresh`, { refreshToken });
  }

  lookupTenants(email: string): Observable<TenantLookupResponse[]> {
    const params = new HttpParams().set('email', email);
    return this.http.get<TenantLookupResponse[]>(`${this.baseUrl}/tenants`, { params });
  }

  switchTenant(tenantId: string): Observable<TenantSwitchResponse> {
    return this.http.post<TenantSwitchResponse>(`${this.baseUrl}/tenant-switch`, { tenantId });
  }
}