CREATE TABLE IF NOT EXISTS AuditLog (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    Category VARCHAR(100) NOT NULL DEFAULT 'System',
    Action VARCHAR(200) NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    EntityId CHAR(36) NOT NULL,
    Severity INT NOT NULL DEFAULT 0,
    PerformedByUserId CHAR(36) NOT NULL,
    PerformedByName VARCHAR(200) NULL,
    Details VARCHAR(4000) NULL,
    MetadataJson TEXT NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    INDEX IX_AuditLog_TenantId (TenantId),
    INDEX IX_AuditLog_Category (Category),
    INDEX IX_AuditLog_EntityType (EntityType),
    INDEX IX_AuditLog_Action (Action),
    INDEX IX_AuditLog_EntityId (EntityId),
    INDEX IX_AuditLog_CreatedAtUtc (CreatedAtUtc),
    INDEX IX_AuditLog_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Configurations (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    `Key` VARCHAR(200) NOT NULL,
    Value VARCHAR(2000) NOT NULL,
    Description VARCHAR(500) NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAtUtc DATETIME NULL,
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    UNIQUE KEY UK_Configurations_TenantId_Key (TenantId, `Key`),
    INDEX IX_Configurations_TenantId (TenantId),
    INDEX IX_Configurations_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CondominiumSettings (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    VisitDurationMinutes INT NOT NULL DEFAULT 120,
    RequireShiftHandoverNotes BOOLEAN NOT NULL DEFAULT TRUE,
    DefaultShiftLengthHours INT NOT NULL DEFAULT 8,
    EmergencyContactPhone VARCHAR(50) NULL,
    AllowedVisitorStartHour VARCHAR(10) NOT NULL DEFAULT '06:00',
    AllowedVisitorEndHour VARCHAR(10) NOT NULL DEFAULT '22:00',
    AutoCheckoutAtMidnight BOOLEAN NOT NULL DEFAULT TRUE,
    MaxActiveVisitorsPerUnit INT NOT NULL DEFAULT 5,
    PhotoRequiredVisitors BOOLEAN NOT NULL DEFAULT TRUE,
    PhotoRequiredProviders BOOLEAN NOT NULL DEFAULT TRUE,
    PhotoRequiredResidents BOOLEAN NOT NULL DEFAULT FALSE,
    AllowOverrideOnRefusal BOOLEAN NOT NULL DEFAULT TRUE,
    OverdueVisitAlertMinutes INT NOT NULL DEFAULT 15,
    CreatedAtUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAtUtc DATETIME NULL,
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    UNIQUE KEY UK_CondominiumSettings_TenantId (TenantId),
    INDEX IX_CondominiumSettings_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Ensure migration on existing databases where AuditLog already existed
DROP PROCEDURE IF EXISTS upgrade_audit_log;
DELIMITER //
CREATE PROCEDURE upgrade_audit_log()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'Category'
    ) THEN
        ALTER TABLE AuditLog ADD COLUMN Category VARCHAR(100) NOT NULL DEFAULT 'System' AFTER TenantId;
        ALTER TABLE AuditLog ADD INDEX IX_AuditLog_Category (Category);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'Severity'
    ) THEN
        ALTER TABLE AuditLog ADD COLUMN Severity INT NOT NULL DEFAULT 0 AFTER EntityId;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'MetadataJson'
    ) THEN
        ALTER TABLE AuditLog ADD COLUMN MetadataJson TEXT NULL AFTER Details;
    END IF;
END //
DELIMITER ;
CALL upgrade_audit_log();
DROP PROCEDURE IF EXISTS upgrade_audit_log;