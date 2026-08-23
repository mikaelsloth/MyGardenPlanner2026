USE MyGardenPlanner2026;
GO

REVOKE UPDATE, DELETE ON admin.AuditLogs FROM mgp_admin_user;
GO

REVOKE INSERT, SELECT ON admin.AuditLogs FROM mgp_app_user;
REVOKE UPDATE, DELETE ON admin.AuditLogs FROM mgp_app_user;
GO