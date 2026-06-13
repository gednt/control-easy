CREATE TABLE IF NOT EXISTS Residents (
    Id          CHAR(36)     NOT NULL PRIMARY KEY,
    TenantId    CHAR(36)     NOT NULL,
    Name        VARCHAR(200) NOT NULL,
    Cpf         VARCHAR(14)  NOT NULL,
    Email       VARCHAR(256) NULL,
    Phone       VARCHAR(20)  NULL,
    ApartmentId CHAR(36)     NULL,
    Active      TINYINT(1)   NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME    NOT NULL DEFAULT UTC_TIMESTAMP(),
    UpdatedAtUtc DATETIME   NULL,
    INDEX IX_Residents_TenantId (TenantId),
    INDEX IX_Residents_Cpf (Cpf),
    INDEX IX_Residents_Name (Name),
    INDEX IX_Residents_Active (Active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;