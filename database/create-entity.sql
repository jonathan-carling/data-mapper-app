-- Creates the database named by the sqlcmd variable DatabaseName, then the Entity table.
--   sqlcmd -C -S localhost,14333 -U sa -P <password> -v DatabaseName=Development -i database/create-entity.sql
IF DB_ID(N'$(DatabaseName)') IS NULL
    CREATE DATABASE [$(DatabaseName)];
GO

USE [$(DatabaseName)];
GO

IF OBJECT_ID(N'dbo.Entity', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Entity
    (
        Id      int           IDENTITY(1,1) NOT NULL CONSTRAINT PK_Entity PRIMARY KEY,
        Name    nvarchar(200) NOT NULL,
        Created datetime2     NOT NULL,
        Updated datetime2     NOT NULL
    );
END
GO
