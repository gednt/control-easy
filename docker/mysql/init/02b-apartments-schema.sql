CREATE TABLE IF NOT EXISTS Apartments (
    Id           CHAR(36)     NOT NULL PRIMARY KEY,
    TenantId     CHAR(36)     NOT NULL,
    Block        VARCHAR(50)  NOT NULL,
    Unit         VARCHAR(50)  NOT NULL,
    Active       TINYINT(1)   NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAtUtc DATETIME     NULL,
    tenant_id    CHAR(36)     NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    UNIQUE INDEX UX_Apartments_TenantId_Block_Unit (TenantId, Block, Unit),
    INDEX IX_Apartments_TenantId (TenantId),
    INDEX IX_Apartments_Active (Active),
    INDEX IX_Apartments_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
