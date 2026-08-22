USE MyGardenPlanner2026;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'admin')
    EXEC('CREATE SCHEMA admin AUTHORIZATION dbo');
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'mgp_app_user')
    CREATE LOGIN mgp_app_user WITH PASSWORD = 'CHANGE_ME_APP_STRONG_PW!';
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'mgp_app_user')
    CREATE USER mgp_app_user FOR LOGIN mgp_app_user;
GO
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO mgp_app_user;
GRANT SELECT ON SCHEMA::admin TO mgp_app_user;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'mgp_admin_user')
    CREATE LOGIN mgp_admin_user WITH PASSWORD = 'CHANGE_ME_ADMIN_STRONG_PW!';
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'mgp_admin_user')
    CREATE USER mgp_admin_user FOR LOGIN mgp_admin_user;
GO
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::admin TO mgp_admin_user;
GO