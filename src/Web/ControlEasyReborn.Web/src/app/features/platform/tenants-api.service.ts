import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TenantResponse {
  id: string;
  slug: string;
  displayName: string;
  status: string;
  createdAtUtc: string;
}

export interface CreateTenantRequest {
  slug: string;
  displayName: string;
}

export interface CreateTenantAdminRequest {
  email: string;
  displayName: string;
  password: string;
}

export interface TenantAdminResponse {
  userId: string;
  email: string;
  displayName: string;
  tenantId: string;
  active: boolean;
  createdAtUtc: string;
}

export interface UpdateTenantAdminRequest {
  email: string;
  displayName: string;
}

export interface CreatePorteiroRequest {
  email: string;
  displayName: string;
  password: string;
}

export interface UpdatePorteiroRequest {
  email: string;
  displayName: string;
}

export interface PorteiroResponse {
  userId: string;
  profileId: string;
  email: string;
  displayName: string;
  tenantId: string;
  active: boolean;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class TenantsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/tenants';

  list(): Observable<TenantResponse[]> {
    return this.http.get<TenantResponse[]>(this.baseUrl);
  }

  create(request: CreateTenantRequest): Observable<TenantResponse> {
    return this.http.post<TenantResponse>(this.baseUrl, request);
  }

  suspend(id: string): Observable<TenantResponse> {
    return this.http.post<TenantResponse>(`${this.baseUrl}/${id}/suspend`, {});
  }

  resume(id: string): Observable<TenantResponse> {
    return this.http.post<TenantResponse>(`${this.baseUrl}/${id}/resume`, {});
  }

  createAdmin(tenantId: string, request: CreateTenantAdminRequest): Observable<TenantAdminResponse> {
    return this.http.post<TenantAdminResponse>(`${this.baseUrl}/${tenantId}/admins`, request);
  }

  listAdmins(tenantId: string): Observable<TenantAdminResponse[]> {
    return this.http.get<TenantAdminResponse[]>(`${this.baseUrl}/${tenantId}/admins`);
  }

  updateAdmin(tenantId: string, userId: string, request: UpdateTenantAdminRequest): Observable<TenantAdminResponse> {
    return this.http.put<TenantAdminResponse>(`${this.baseUrl}/${tenantId}/admins/${userId}`, request);
  }

  revokeAdmin(tenantId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${tenantId}/admins/${userId}/revoke`, {});
  }

  suspendAdmin(tenantId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${tenantId}/admins/${userId}/suspend`, {});
  }

  resumeAdmin(tenantId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${tenantId}/admins/${userId}/resume`, {});
  }

  deleteAdmin(tenantId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tenantId}/admins/${userId}`);
  }

  listPorteiros(tenantId: string): Observable<PorteiroResponse[]> {
    return this.http.get<PorteiroResponse[]>(`${this.baseUrl}/${tenantId}/porteiros`);
  }

  createPorteiro(tenantId: string, request: CreatePorteiroRequest): Observable<PorteiroResponse> {
    return this.http.post<PorteiroResponse>(`${this.baseUrl}/${tenantId}/porteiros`, request);
  }

  updatePorteiro(tenantId: string, userId: string, request: UpdatePorteiroRequest): Observable<PorteiroResponse> {
    return this.http.put<PorteiroResponse>(`${this.baseUrl}/${tenantId}/porteiros/${userId}`, request);
  }

  suspendPorteiro(tenantId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${tenantId}/porteiros/${userId}/suspend`, {});
  }

  resumePorteiro(tenantId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${tenantId}/porteiros/${userId}/resume`, {});
  }

  deletePorteiro(tenantId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tenantId}/porteiros/${userId}`);
  }
}
