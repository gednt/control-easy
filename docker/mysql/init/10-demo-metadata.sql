-- 10-demo-metadata.sql
-- Platform table for demo seed idempotency (no tenant_id — exempt from backfill).

CREATE TABLE IF NOT EXISTS DemoMetadata (
    Id           INT          NOT NULL PRIMARY KEY DEFAULT 1,
    SeedVersion  INT          NOT NULL DEFAULT 0,
    SeededAtUtc  DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CHECK (Id = 1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
