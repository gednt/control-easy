-- 03-tenant-backfill.sql
-- Adds tenant_id to every business table and backfills with the default tenant.
-- Tables listed here MUST be kept in sync with scripts/generate-tenant-backfill.sql.
-- CI (NetArchTest rule in ArchitectureTests project) enforces this.
-- The default tenant GUID is 00000000-0000-0000-0000-000000000001.

SET @defaultTenantId = '00000000-0000-0000-0000-000000000001';

-- Users table
ALTER TABLE Users ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Users SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Users MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Users_tenant_id ON Users (tenant_id);

-- Residents table
ALTER TABLE Residents ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Residents SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Residents MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Residents_tenant_id ON Residents (tenant_id);

-- [Add new business tables here in the same pattern]
-- Pattern:
-- ALTER TABLE <TableName> ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
-- UPDATE <TableName> SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
-- ALTER TABLE <TableName> MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
-- CREATE INDEX IF NOT EXISTS IX_<TableName>_tenant_id ON <TableName> (tenant_id);