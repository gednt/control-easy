-- 04-tenant-views.sql
-- WPF compatibility shim: legacy views hard-coded to the default tenant.
-- The WPF EF6 model is repointed at these views (EDMX regeneration).
-- Removed in task 4.4 (WPF decommission).

CREATE OR REPLACE VIEW Users_legacy AS
SELECT * FROM Users WHERE tenant_id = '00000000-0000-0000-0000-000000000001';

CREATE OR REPLACE VIEW Residents_legacy AS
SELECT * FROM Residents WHERE tenant_id = '00000000-0000-0000-0000-000000000001';

-- [Add new business table views here in the same pattern]
-- Pattern:
-- CREATE OR REPLACE VIEW <TableName>_legacy AS
-- SELECT * FROM <TableName> WHERE tenant_id = '00000000-0000-0000-0000-000000000001';