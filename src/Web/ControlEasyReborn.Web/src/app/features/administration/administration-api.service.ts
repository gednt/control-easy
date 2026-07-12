import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditLogResponse {
  id: string;
  tenantId: string;
  action: string;
  entityType: string;
  entityId: string;
  performedByName: string | null;
  details: string | null;
  createdAtUtc: string;
}

export interface ConfigurationResponse {
  id: string;
  tenantId: string;
  key: string;
  value: string;
  description: string | null;
  createdAtUtc: string;
}

export interface CreateConfigurationRequest {
  key: string;
  value: string;
  description?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AdministrationApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/administration';

  listAuditLogs(skip = 0, take = 50): Observable<AuditLogResponse[]> {
    const params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());
    return this.http.get<AuditLogResponse[]>(`${this.baseUrl}/audit-logs`, { params });
  }

  listConfigurations(skip = 0, take = 50): Observable<ConfigurationResponse[]> {
    const params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());
    return this.http.get<ConfigurationResponse[]>(`${this.baseUrl}/configurations`, { params });
  }

  createConfiguration(request: CreateConfigurationRequest): Observable<ConfigurationResponse> {
    return this.http.post<ConfigurationResponse>(`${this.baseUrl}/configurations`, request);
  }
}
