-- Apply once to existing MySQL 8 databases created before entity-bound photos.
-- Fresh databases receive the same columns from init/09a-photos-schema.sql.
SET @schema_name = DATABASE();

SET @has_entity_type = (
    SELECT COUNT(*) FROM information_schema.columns
    WHERE table_schema = @schema_name AND table_name = 'Photos' AND column_name = 'EntityType'
);
SET @statement = IF(@has_entity_type = 0, 'ALTER TABLE Photos ADD COLUMN EntityType VARCHAR(40) NULL', 'SELECT 1');
PREPARE migration_statement FROM @statement;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @has_entity_id = (
    SELECT COUNT(*) FROM information_schema.columns
    WHERE table_schema = @schema_name AND table_name = 'Photos' AND column_name = 'EntityId'
);
SET @statement = IF(@has_entity_id = 0, 'ALTER TABLE Photos ADD COLUMN EntityId VARCHAR(100) NULL', 'SELECT 1');
PREPARE migration_statement FROM @statement;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @has_entity_index = (
    SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @schema_name AND table_name = 'Photos' AND index_name = 'IX_Photos_Entity'
);
SET @statement = IF(@has_entity_index = 0, 'CREATE INDEX IX_Photos_Entity ON Photos (EntityType, EntityId)', 'SELECT 1');
PREPARE migration_statement FROM @statement;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;
