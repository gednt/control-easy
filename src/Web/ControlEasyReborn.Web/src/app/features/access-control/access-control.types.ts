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

export interface ScanResult {
  decision:
    | 'recorded'
    | 'duplicate_confirmation_required'
    | 'policy_action_required'
    | 'refused'
    | 'unavailable';
  accessEventId: string;
  subjectType: 'resident' | 'vehicle';
  subjectId: string;
  credentialId: string | null;
  lookupAuditId: string | null;
  accessMethod: 'qr' | 'manual_lookup';
  direction: 'entrance' | 'exit';
  policyOutcome: 'permit' | 'requires_action' | 'refused';
  destinationApartmentId: string;
  destinationBlock: string;
  destinationUnit: string;
}

export interface ScanRefusal {
  failureCode:
    | 'invalid_credential'
    | 'credential_inactive'
    | 'subject_inactive'
    | 'destination_required'
    | 'destination_inactive'
    | 'not_authorized'
    | 'policy_action_required'
    | 'duplicate_confirmation_required'
    | 'vehicle_inactive'
    | 'search_too_broad'
    | 'manual_event_orphan_lookup_id';
  decision: ScanResult['decision'];
}

export type ManualLookupType = 'cpf' | 'identity_document' | 'name' | 'apartment' | 'block';

export interface ManualLookupRequest {
  criterion: {
    type: ManualLookupType;
    value: string;
    unit: string | null;
  };
}

export interface ManualLookupResult {
  subjectType: 'resident' | 'vehicle';
  subjectId: string;
  displayName: string;
  maskedDocument: string | null;
  destinationBlock: string | null;
  destinationUnit: string | null;
}

export interface ManualLookupResponse {
  lookupId: string;
  results: ManualLookupResult[];
  narrowHint: string | null;
}

export interface ManualAccessRequest {
  lookupId: string;
  subjectType: 'resident' | 'vehicle';
  subjectId: string;
  direction: 'entrance' | 'exit';
}

export interface ManualAccessResponse {
  decision: ScanResult['decision'];
  accessEventId: string;
  accessMethod: 'manual_lookup';
  subjectType: 'resident' | 'vehicle';
  subjectId: string;
  direction: 'entrance' | 'exit';
  destinationApartmentId: string;
  destinationBlock: string;
  destinationUnit: string;
}
