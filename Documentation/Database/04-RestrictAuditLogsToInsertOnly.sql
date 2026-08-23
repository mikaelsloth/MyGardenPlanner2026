USE MyGardenPlanner2026;
GO

-- mgp_admin_user har fået fuld CRUD på admin-schema i script 01 (GRANT SELECT, INSERT,
-- UPDATE, DELETE ON SCHEMA::admin). Her strammes specifikt op på AuditLogs: selv
-- admin-brugeren kan kun INSERT og SELECT, aldrig ændre eller slette en logrække.
DENY UPDATE, DELETE ON admin.AuditLogs TO mgp_admin_user;
GO

-- mgp_app_user har kun SELECT på admin-schema generelt (script 01). Her tillades
-- eksplicit INSERT på AuditLogs specifikt, hvis app-laget nogensinde skal skrive logs
-- direkte (uden om admin-context) — ellers har den fortsat ingen skriveadgang.
DENY UPDATE, DELETE ON admin.AuditLogs TO mgp_app_user;
GRANT INSERT, SELECT ON admin.AuditLogs TO mgp_app_user;
GO