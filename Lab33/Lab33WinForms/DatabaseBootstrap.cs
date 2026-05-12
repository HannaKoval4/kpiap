using Microsoft.Data.SqlClient;

namespace Lab33WinForms;

internal static class DatabaseBootstrap
{
    internal static readonly object DbSync = new();

    internal const string DatabaseName = "Lab33FromLab29";

    internal static string MasterConnectionString { get; } =
        "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";

    internal static string DatabaseConnectionString { get; } =
        $"Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Database={DatabaseName};";

    internal static void EnsureReady()
    {
        lock (DbSync)
        {
            using var master = new SqlConnection(MasterConnectionString);
            master.Open();
            using (var createDb = master.CreateCommand())
            {
                createDb.CommandText = $@"
IF DB_ID(N'{DatabaseName}') IS NULL
BEGIN
    CREATE DATABASE [{DatabaseName}];
END";
                createDb.ExecuteNonQuery();
            }

            using var db = new SqlConnection(DatabaseConnectionString);
            db.Open();
            using var cmd = db.CreateCommand();
            cmd.CommandText = @"
IF OBJECT_ID(N'dbo.Drinks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Drinks
    (
        id INT IDENTITY PRIMARY KEY,
        name NVARCHAR(200) NOT NULL,
        description NVARCHAR(500) NULL
    );
END

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        id INT IDENTITY PRIMARY KEY,
        username VARCHAR(50) NOT NULL,
        password VARCHAR(50) NOT NULL
    );
END";
            cmd.ExecuteNonQuery();

            SeedIfEmpty(db);
        }
    }

    private static void SeedIfEmpty(SqlConnection db)
    {
        int drinkCount;
        using (var c = db.CreateCommand())
        {
            c.CommandText = "SELECT COUNT(*) FROM dbo.Drinks;";
            drinkCount = Convert.ToInt32(c.ExecuteScalar());
        }

        if (drinkCount == 0)
        {
            using var insert = db.CreateCommand();
            insert.CommandText = @"
INSERT INTO dbo.Drinks (name, description) VALUES
(N'Лабораторная 29 (основа)', N'WPF: диалог печати PrintDialog, сочетания Ctrl+P и F9'),
(N'Печать документа', N'То же назначение, что в исходной лаб. 29 — вывод на принтер'),
(N'Расширение лаб. 33', N'Та же таблица записей в БД, что в примере методички, но предметная область — лаб. 29');";
            insert.ExecuteNonQuery();
        }

        int userCount;
        using (var c = db.CreateCommand())
        {
            c.CommandText = "SELECT COUNT(*) FROM dbo.Users;";
            userCount = Convert.ToInt32(c.ExecuteScalar());
        }

        if (userCount == 0)
        {
            using var insert = db.CreateCommand();
            insert.CommandText = @"
INSERT INTO dbo.Users (username, password) VALUES
('Sinister', '2281337'),
('AizenSoloverse', '2905');";
            insert.ExecuteNonQuery();
        }
    }
}
