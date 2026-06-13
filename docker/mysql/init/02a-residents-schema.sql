CREATE TABLE IF NOT EXISTS Residents (
    Id          CHAR(36)     NOT NULL PRIMARY KEY,
    TenantId    CHAR(36)     NOT NULL,
    Name        VARCHAR(200) NOT NULL,
    Cpf         VARCHAR(14)  NOT NULL,
    Email       VARCHAR(256) NULL,
    Phone       VARCHAR(20)  NULL,
    ApartmentId CHAR(36)     NULL,
    Active      TINYINT(1)   NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAtUtc DATETIME   NULL,
    tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    INDEX IX_Residents_TenantId (TenantId),
    INDEX IX_Residents_Cpf (Cpf),
    INDEX IX_Residents_Name (Name),
    INDEX IX_Residents_Active (Active),
    INDEX IX_Residents_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;