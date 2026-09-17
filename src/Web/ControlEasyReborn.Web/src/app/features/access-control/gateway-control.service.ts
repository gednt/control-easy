import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AccessCredentialSummary,
  AccessEventSummary,
  ManualAccessRequest,
  ManualAccessResponse,
  ManualLookupRequest,
  ManualLookupResponse,
  RefusedScanSummary,
  ScanResult,
} from './access-control.types';

/**
 * Placeholder service for the AccessControl SPA surface. Real wiring is
 * populated by ng-openapi-gen from the OpenAPI document at build time.
 */
@Injectable({ providedIn: 'root' })
export class GatewayControlService {
  private readonly http = inject(HttpClient);

  listCredentials(): Observable<AccessCredentialSummary[]> {
    return this.http.get<AccessCredentialSummary[]>('/api/v1/access-credentials');
  }

  recordScan(payload: {
    qrPayload: string;
    direction: 'entrance' | 'exit';
    scanAttemptId: string;
    gatehouseId?: string | null;
    confirmDuplicate?: boolean;
  }): Observable<ScanResult> {
    return this.http.post<ScanResult>('/api/v1/access-events/scans', payload);
  }

  searchSubject(criterion: ManualLookupRequest['criterion']): Observable<ManualLookupResponse> {
    return this.http.post<ManualLookupResponse>('/api/v1/access-subjects/search', { criterion });
  }

  recordManual(payload: ManualAccessRequest): Observable<ManualAccessResponse> {
    return this.http.post<ManualAccessResponse>('/api/v1/access-events/manual', payload);
  }

  listEvents(): Observable<AccessEventSummary[]> {
    return this.http.get<AccessEventSummary[]>('/api/v1/access-events');
  }

  listRefusedAttempts(): Observable<RefusedScanSummary[]> {
    return this.http.get<RefusedScanSummary[]>('/api/v1/access-events/refused-attempts');
  }
}
