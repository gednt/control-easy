-- 09b-consent-schema.sql
-- Consent audit log (append-only, enforced by triggers) and per-tenant consent policy.
-- recorded_at uses DATETIME(3) for millisecond precision
-- CHECK constraint (MySQL 8.0.16+): entered_with_consent entries require a non-null PhotoId
-- Triggers use single-statement bodies (no BEGIN/END) so naive semicolon-split script runners work

CREATE TABLE IF NOT EXISTS ConsentAuditLog (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    EntryState VARCHAR(40) NOT NULL,
    OverrideReason VARCHAR(40) NULL,
    PhotoId CHAR(36) NULL,
    SubjectType VARCHAR(40) NOT NULL,
    SubjectName VARCHAR(200) NULL,
    SubjectDocument VARCHAR(20) NULL,
    ApartmentId CHAR(36) NULL,
    PerformedByProfileId CHAR(36) NULL,
    RecordedAt DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    CHECK (EntryState <> 'entered_with_consent' OR PhotoId IS NOT NULL),
    INDEX IX_ConsentAuditLog_TenantId (TenantId),
    INDEX IX_ConsentAuditLog_tenant_id (tenant_id),
    INDEX IX_ConsentAuditLog_EntryState (EntryState),
    INDEX IX_ConsentAuditLog_SubjectType (SubjectType),
    INDEX IX_ConsentAuditLog_PhotoId (PhotoId),
    INDEX IX_ConsentAuditLog_ApartmentId (ApartmentId),
    INDEX IX_ConsentAuditLog_RecordedAt (RecordedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TRIGGER IF EXISTS trg_consent_audit_log_no_update;

CREATE TRIGGER trg_consent_audit_log_no_update
BEFORE UPDATE ON ConsentAuditLog
FOR EACH ROW
SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ConsentAuditLog is append-only: UPDATE is not permitted';

DROP TRIGGER IF EXISTS trg_consent_audit_log_no_delete;

CREATE TRIGGER trg_consent_audit_log_no_delete
BEFORE DELETE ON ConsentAuditLog
FOR EACH ROW
SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'ConsentAuditLog is append-only: DELETE is not permitted';

CREATE TABLE IF NOT EXISTS TenantConsentPolicy (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    SubjectCategory VARCHAR(40) NOT NULL,
    PhotoRequired TINYINT(1) NOT NULL DEFAULT 0,
    DwellTimeLimitMinutes INT NULL,
    UpdatedByProfileId CHAR(36) NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAtUtc DATETIME NULL,
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    UNIQUE KEY UK_TenantConsentPolicy_TenantId_Category (TenantId, SubjectCategory),
    INDEX IX_TenantConsentPolicy_TenantId (TenantId),
    INDEX IX_TenantConsentPolicy_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;