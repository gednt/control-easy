-- 02-tenants-seed.sql
-- Creates the Tenants table and inserts the default tenant row.

CREATE TABLE IF NOT EXISTS Tenants (
    Id          CHAR(36)     NOT NULL PRIMARY KEY,
    Slug        VARCHAR(32)  NOT NULL UNIQUE,
    DisplayName VARCHAR(120) NOT NULL,
    Status      INT          NOT NULL DEFAULT 0,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT UTC_TIMESTAMP(6),
    INDEX IX_Tenants_Slug (Slug),
    INDEX IX_Tenants_Status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO Tenants (Id, Slug, DisplayName, Status, CreatedAtUtc)
VALUES ('00000000-0000-0000-0000-000000000001', 'default', 'Condominio Padrao', 0, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE DisplayName = VALUES(DisplayName);