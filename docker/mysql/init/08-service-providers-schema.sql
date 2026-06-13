CREATE TABLE IF NOT EXISTS ServiceProviders (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId CHAR(36) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Document VARCHAR(20) NOT NULL,
    Phone VARCHAR(20) NULL,
    Email VARCHAR(256) NULL,
    ServiceType VARCHAR(100) NULL,
    Company VARCHAR(200) NULL,
    Active TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME NOT NULL DEFAULT UTC_TIMESTAMP(),
    UpdatedAtUtc DATETIME NULL,
    INDEX IX_ServiceProviders_TenantId (TenantId),
    INDEX IX_ServiceProviders_Document (Document),
    INDEX IX_ServiceProviders_Name (Name),
    INDEX IX_ServiceProviders_Active (Active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;