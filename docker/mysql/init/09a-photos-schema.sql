-- 09a-photos-schema.sql
-- Photos table for the Gatehouse Photo & Consent Ledger (v2.0 Phase 11).
-- Thumbnails are produced client-side in Phase 12 (ThumbnailPath nullable).

CREATE TABLE IF NOT EXISTS Photos (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ThumbnailPath VARCHAR(500) NULL,
    MimeType VARCHAR(100) NOT NULL,
    SizeBytes BIGINT NOT NULL,
    CapturedAtUtc DATETIME NULL,
    EntityType VARCHAR(40) NULL,
    EntityId VARCHAR(100) NULL,
    CreatedAtUtc DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    DeletedAtUtc DATETIME NULL,
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    INDEX IX_Photos_TenantId (TenantId),
    INDEX IX_Photos_tenant_id (tenant_id),
    INDEX IX_Photos_DeletedAtUtc (DeletedAtUtc),
    INDEX IX_Photos_Entity (EntityType, EntityId),
    INDEX IX_Photos_CreatedAtUtc (CreatedAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
