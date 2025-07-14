-- Mark the Oracle migration as applied manually
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('20250714045047_Oracle10gFinalCompatible', '8.0.11');

-- Verify the record was inserted
SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";