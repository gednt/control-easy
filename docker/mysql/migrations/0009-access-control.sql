-- 0009-access-control.sql
-- Idempotent live-migration: access control module tables + identity-document + vehicle owner link + visit destination snapshot.

CREATE TABLE IF NOT EXISTS access_credentials (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    subject_type TINYINT NOT NULL,
    subject_id CHAR(36) NOT NULL,
    method TINYINT NOT NULL,
    secret_verifier VARBINARY(64) NOT NULL,
    key_version INT NOT NULL,
    status TINYINT NOT NULL,
    valid_from_utc DATETIME(6) NOT NULL,
    expires_at_utc DATETIME(6) NULL,
    replaced_by_credential_id CHAR(36) NULL,
    issued_by_profile_id CHAR(36) NOT NULL,
    created_at_utc DATETIME(6) NOT NULL,
    updated_at_utc DATETIME(6) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_access_credentials_subject_active (tenant_id, subject_type, subject_id, status),
    KEY ix_access_credentials_tenant_subject (tenant_id, subject_type, subject_id),
    KEY ix_access_credentials_status (tenant_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS credential_lifecycle_actions (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    credential_id CHAR(36) NOT NULL,
    action TINYINT NOT NULL,
    previous_status TINYINT NOT NULL,
    resulting_status TINYINT NOT NULL,
    actor_profile_id CHAR(36) NOT NULL,
    reason_code VARCHAR(64) NULL,
    reason_text VARCHAR(500) NULL,
    occurred_at_utc DATETIME(6) NOT NULL,
    correlation_id CHAR(36) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_credential_lifecycle_tenant_credential (tenant_id, credential_id, occurred_at_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS access_events (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    subject_type TINYINT NOT NULL,
    subject_id CHAR(36) NOT NULL,
    direction TINYINT NOT NULL,
    access_method TINYINT NOT NULL,
    credential_id CHAR(36) NULL,
    lookup_audit_id CHAR(36) NULL,
    scan_attempt_id CHAR(36) NOT NULL,
    performed_by_profile_id CHAR(36) NOT NULL,
    gatehouse_id CHAR(36) NULL,
    occurred_at_utc DATETIME(6) NOT NULL,
    correlation_id CHAR(36) NOT NULL,
    duplicate_of_access_event_id CHAR(36) NULL,
    duplicate_confirmed TINYINT NOT NULL DEFAULT 0,
    policy_outcome TINYINT NOT NULL,
    destination_apartment_id CHAR(36) NOT NULL,
    destination_block VARCHAR(64) NOT NULL,
    destination_unit VARCHAR(64) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_access_events_scan_attempt (tenant_id, scan_attempt_id),
    KEY ix_access_events_tenant_time (tenant_id, occurred_at_utc),
    KEY ix_access_events_subject_time (tenant_id, subject_type, subject_id, occurred_at_utc),
    KEY ix_access_events_performed_by (tenant_id, performed_by_profile_id, occurred_at_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS refused_scan_attempts (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    scan_attempt_id CHAR(36) NOT NULL,
    credential_fingerprint VARBINARY(32) NULL,
    direction TINYINT NULL,
    failure_code VARCHAR(64) NOT NULL,
    performed_by_profile_id CHAR(36) NOT NULL,
    gatehouse_id CHAR(36) NULL,
    occurred_at_utc DATETIME(6) NOT NULL,
    correlation_id CHAR(36) NOT NULL,
    related_credential_id CHAR(36) NULL,
    related_subject_id CHAR(36) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_refused_scan_attempt (tenant_id, scan_attempt_id),
    KEY ix_refused_attempts_time (tenant_id, occurred_at_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS access_lookup_audits (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    criterion_type TINYINT NOT NULL,
    result_count_band TINYINT NOT NULL,
    selected_subject_type TINYINT NULL,
    selected_subject_id CHAR(36) NULL,
    performed_by_profile_id CHAR(36) NOT NULL,
    occurred_at_utc DATETIME(6) NOT NULL,
    correlation_id CHAR(36) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_access_lookup_audits_time (tenant_id, occurred_at_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Resident identity documents.
CREATE TABLE IF NOT EXISTS resident_identity_documents (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    resident_id CHAR(36) NOT NULL,
    document_type VARCHAR(32) NOT NULL,
    normalized_value VARCHAR(64) NOT NULL,
    active TINYINT NOT NULL DEFAULT 1,
    created_at_utc DATETIME(6) NOT NULL,
    updated_at_utc DATETIME(6) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_resident_id_doc (tenant_id, document_type, normalized_value),
    KEY ix_resident_id_doc_resident (tenant_id, resident_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Optional resident-owner link on vehicles.
SET @s = (SELECT COLUMN_NAME FROM information_schema.COLUMNS
          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'vehicles' AND COLUMN_NAME = 'owner_resident_id');
SET @sql = IF(@s IS NULL, 'ALTER TABLE vehicles ADD COLUMN owner_resident_id CHAR(36) NULL AFTER apartment_id, ADD KEY ix_vehicles_owner_resident (tenant_id, owner_resident_id)', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Visit destination snapshot fields.
SET @s = (SELECT COLUMN_NAME FROM information_schema.COLUMNS
          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'visits' AND COLUMN_NAME = 'destination_block');
SET @sql = IF(@s IS NULL, 'ALTER TABLE visits ADD COLUMN destination_block VARCHAR(64) NULL, ADD COLUMN destination_unit VARCHAR(64) NULL', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
