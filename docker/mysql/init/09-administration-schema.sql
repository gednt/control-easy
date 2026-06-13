CREATE TABLE IF NOT EXISTS AuditLog (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    Action VARCHAR(200) NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    EntityId CHAR(36) NOT NULL,
    PerformedByUserId CHAR(36) NOT NULL,
    PerformedByName VARCHAR(200) NULL,
    Details VARCHAR(4000) NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT UTC_TIMESTAMP(),
    INDEX IX_AuditLog_TenantId (TenantId),
    INDEX IX_AuditLog_EntityType (EntityType),
    INDEX IX_AuditLog_Action (Action),
    INDEX IX_AuditLog_EntityId (EntityId),
    INDEX IX_AuditLog_CreatedAtUtc (CreatedAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Configurations (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    `Key` VARCHAR(200) NOT NULL,
    Value VARCHAR(2000) NOT NULL,
    Description VARCHAR(500) NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT UTC_TIMESTAMP(),
    UpdatedAtUtc DATETIME NULL,
    UNIQUE KEY UK_Configurations_TenantId_Key (TenantId, `Key`),
    INDEX IX_Configurations_TenantId (TenantId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;