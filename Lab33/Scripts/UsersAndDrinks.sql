IF DB_ID(N'Lab33Coffeeshop') IS NULL
    CREATE DATABASE [Lab33Coffeeshop];
GO

USE [Lab33Coffeeshop];
GO

IF OBJECT_ID(N'dbo.Drinks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Drinks
    (
        id INT IDENTITY PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        description NVARCHAR(500) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        id INT IDENTITY PRIMARY KEY,
        username VARCHAR(50) NOT NULL,
        password VARCHAR(50) NOT NULL
    );
END
GO

INSERT INTO dbo.Users (username, password)
SELECT v.username, v.password
FROM (VALUES
    (N'Sinister', N'2281337'),
    (N'AizenSoloverse', N'2905')
) AS v(username, password)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.username = v.username);
GO
