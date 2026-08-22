USE master;
GO

SELECT SUSER_SNAME(); -- bekræft dit login-navn først

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'DOMAIN\dinbruger')
    CREATE LOGIN [DOMAIN\dinbruger] FROM WINDOWS;
GO

ALTER SERVER ROLE dbcreator ADD MEMBER [DOMAIN\dinbruger];
GO