-- Apply once to existing MySQL 8 databases created before apartment-attributed entry logs.
-- Fresh databases receive the same column from init/09b-consent-schema.sql.
SET @schema_name = DATABASE();

SET @has_apartment_id = (
    SELECT COUNT(*) FROM information_schema.columns
    WHERE table_schema = @schema_name AND table_name = 'ConsentAuditLog' AND column_name = 'ApartmentId'
);
SET @statement = IF(@has_apartment_id = 0, 'ALTER TABLE ConsentAuditLog ADD COLUMN ApartmentId CHAR(36) NULL', 'SELECT 1');
PREPARE migration_statement FROM @statement;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @has_apartment_index = (
    SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @schema_name AND table_name = 'ConsentAuditLog' AND index_name = 'IX_ConsentAuditLog_ApartmentId'
);
SET @statement = IF(@has_apartment_index = 0, 'CREATE INDEX IX_ConsentAuditLog_ApartmentId ON ConsentAuditLog (ApartmentId)', 'SELECT 1');
PREPARE migration_statement FROM @statement;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;