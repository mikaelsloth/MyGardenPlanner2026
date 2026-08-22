USE MyGardenPlanner2026;
GO

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'mgp_admin_user')
    DROP USER mgp_admin_user;
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'mgp_admin_user')
    DROP LOGIN mgp_admin_user;
GO

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'mgp_app_user')
    DROP USER mgp_app_user;
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'mgp_app_user')
    DROP LOGIN mgp_app_user;
GO

-- DROP SCHEMA admin;  -- kun efter migration Down er kørt (tabeller flyttet tilbage til dbo)