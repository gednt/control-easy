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

-- AttendantProfiles table
ALTER TABLE AttendantProfiles ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE AttendantProfiles SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE AttendantProfiles MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_AttendantProfiles_tenant_id ON AttendantProfiles (tenant_id);

-- Shifts table
ALTER TABLE Shifts ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Shifts SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Shifts MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Shifts_tenant_id ON Shifts (tenant_id);

-- Gatehouses table
ALTER TABLE Gatehouses ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Gatehouses SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Gatehouses MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Gatehouses_tenant_id ON Gatehouses (tenant_id);

-- Vehicles table
ALTER TABLE Vehicles ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Vehicles SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Vehicles MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Vehicles_tenant_id ON Vehicles (tenant_id);

-- Visits table
ALTER TABLE Visits ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Visits SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Visits MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Visits_tenant_id ON Visits (tenant_id);

-- ServiceProviders table
ALTER TABLE ServiceProviders ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE ServiceProviders SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE ServiceProviders MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_ServiceProviders_tenant_id ON ServiceProviders (tenant_id);

-- AuditLog table
ALTER TABLE AuditLog ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE AuditLog SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE AuditLog MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_AuditLog_tenant_id ON AuditLog (tenant_id);

-- Configurations table
ALTER TABLE Configurations ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
UPDATE Configurations SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
ALTER TABLE Configurations MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Configurations_tenant_id ON Configurations (tenant_id);

-- [Add new business tables here in the same pattern]
-- Pattern:
-- ALTER TABLE <TableName> ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;
-- UPDATE <TableName> SET tenant_id = @defaultTenantId WHERE tenant_id IS NULL;
-- ALTER TABLE <TableName> MODIFY COLUMN tenant_id CHAR(36) NOT NULL;
-- CREATE INDEX IF NOT EXISTS IX_<TableName>_tenant_id ON <TableName> (tenant_id);