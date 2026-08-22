USE master;
GO
ALTER SERVER ROLE dbcreator DROP MEMBER [DOMAIN\dinbruger];
DROP LOGIN [DOMAIN\dinbruger];
GO