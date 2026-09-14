// Auto-generated placeholder. Real DTOs are produced by ng-openapi-gen at
// build time from the OpenAPI document. Do not add domain types here.
export interface AccessCredentialSummary {
  id: string;
  subjectType: 'resident' | 'vehicle';
  subjectId: string;
  method: 'qr';
  status: 'active' | 'replaced' | 'revoked' | 'expired' | 'inactive';
  validFromUtc: string;
  expiresAtUtc: string | null;
  createdAtUtc: string;
}

export interface AccessEventSummary {
  id: string;
  subjectType: 'resident' | 'vehicle';
  subjectId: string;
  direction: 'entrance' | 'exit';
  accessMethod: 'qr' | 'manual_lookup';
  occurredAtUtc: string;
  performedByProfileId: string;
  destinationApartmentId: string;
  destinationBlock: string;
  destinationUnit: string;
}

export interface RefusedScanSummary {
  id: string;
  scanAttemptId: string;
  failureCode: string;
  occurredAtUtc: string;
  performedByProfileId: string;
}
