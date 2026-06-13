-- 00-schema.sql
-- Minimal schema for the Residents smoke test and JWT issuance.
-- This file runs first (00-prefix) during MySQL container initialization.

CREATE TABLE IF NOT EXISTS Users (
    Id                 CHAR(36)     NOT NULL PRIMARY KEY,
    Email              VARCHAR(256) NOT NULL UNIQUE,
    PasswordHash       VARCHAR(512) NOT NULL,
    DisplayName        VARCHAR(200) NOT NULL,
    Active             TINYINT(1)   NOT NULL DEFAULT 1,
    MustChangePassword TINYINT(1)   NOT NULL DEFAULT 0,
    Roles              VARCHAR(500) NOT NULL DEFAULT '',
    CreatedAtUtc       DATETIME     NOT NULL DEFAULT UTC_TIMESTAMP(),
    UpdatedAtUtc       DATETIME     NULL,
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_Active (Active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;