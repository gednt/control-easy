-- 12-access-control-schema.sql
-- AccessControl module tables. Fresh-schema DDL mirrored by migration 0009-access-control.sql.

CREATE TABLE IF NOT EXISTS AccessCredentials (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    SubjectType TINYINT NOT NULL,
    SubjectId CHAR(36) NOT NULL,
    Method TINYINT NOT NULL,
    SecretVerifier VARBINARY(64) NOT NULL,
    KeyVersion INT NOT NULL,
    Status TINYINT NOT NULL,
    ValidFromUtc DATETIME(6) NOT NULL,
    ExpiresAtUtc DATETIME(6) NULL,
    ReplacedByCredentialId CHAR(36) NULL,
    IssuedByProfileId CHAR(36) NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    tenant_id CHAR(36) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY uq_access_credentials_subject_active (tenant_id, SubjectType, SubjectId, Status),
    KEY ix_access_credentials_tenant_subject (tenant_id, SubjectType, SubjectId),
    KEY ix_access_credentials_status (tenant_id, Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CredentialLifecycleActions (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    CredentialId CHAR(36) NOT NULL,
    Action TINYINT NOT NULL,
    PreviousStatus TINYINT NOT NULL,
    ResultingStatus TINYINT NOT NULL,
    ActorProfileId CHAR(36) NOT NULL,
    ReasonCode VARCHAR(64) NULL,
    ReasonText VARCHAR(500) NULL,
    OccurredAtUtc DATETIME(6) NOT NULL,
    CorrelationId CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    PRIMARY KEY (Id),
    KEY ix_credential_lifecycle_tenant_credential (tenant_id, CredentialId, OccurredAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS AccessEvents (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    SubjectType TINYINT NOT NULL,
    SubjectId CHAR(36) NOT NULL,
    Direction TINYINT NOT NULL,
    AccessMethod TINYINT NOT NULL,
    CredentialId CHAR(36) NULL,
    LookupAuditId CHAR(36) NULL,
    ScanAttemptId CHAR(36) NOT NULL,
    PerformedByProfileId CHAR(36) NOT NULL,
    GatehouseId CHAR(36) NULL,
    OccurredAtUtc DATETIME(6) NOT NULL,
    CorrelationId CHAR(36) NOT NULL,
    DuplicateOfAccessEventId CHAR(36) NULL,
    DuplicateConfirmed TINYINT NOT NULL DEFAULT 0,
    PolicyOutcome TINYINT NOT NULL,
    DestinationApartmentId CHAR(36) NOT NULL,
    DestinationBlock VARCHAR(64) NOT NULL,
    DestinationUnit VARCHAR(64) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY uq_access_events_scan_attempt (tenant_id, ScanAttemptId),
    KEY ix_access_events_tenant_time (tenant_id, OccurredAtUtc),
    KEY ix_access_events_subject_time (tenant_id, SubjectType, SubjectId, OccurredAtUtc),
    KEY ix_access_events_performed_by (tenant_id, PerformedByProfileId, OccurredAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS RefusedScanAttempts (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    ScanAttemptId CHAR(36) NOT NULL,
    CredentialFingerprint VARBINARY(32) NULL,
    Direction TINYINT NULL,
    FailureCode VARCHAR(64) NOT NULL,
    PerformedByProfileId CHAR(36) NOT NULL,
    GatehouseId CHAR(36) NULL,
    OccurredAtUtc DATETIME(6) NOT NULL,
    CorrelationId CHAR(36) NOT NULL,
    RelatedCredentialId CHAR(36) NULL,
    RelatedSubjectId CHAR(36) NULL,
    tenant_id CHAR(36) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY uq_refused_scan_attempt (tenant_id, ScanAttemptId),
    KEY ix_refused_attempts_time (tenant_id, OccurredAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS AccessLookupAudits (
    Id CHAR(36) NOT NULL,
    TenantId CHAR(36) NOT NULL,
    CriterionType TINYINT NOT NULL,
    ResultCountBand TINYINT NOT NULL,
    SelectedSubjectType TINYINT NULL,
    SelectedSubjectId CHAR(36) NULL,
    PerformedByProfileId CHAR(36) NOT NULL,
    OccurredAtUtc DATETIME(6) NOT NULL,
    CorrelationId CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    PRIMARY KEY (Id),
    KEY ix_access_lookup_audits_time (tenant_id, OccurredAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
