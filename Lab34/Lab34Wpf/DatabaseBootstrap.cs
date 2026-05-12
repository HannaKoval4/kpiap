using Microsoft.Data.SqlClient;

namespace Lab34Wpf;

internal static class DatabaseBootstrap
{
    internal static readonly object DbSync = new();

    internal const string DatabaseName = "Lab34PrintLab29";

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
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        id INT IDENTITY PRIMARY KEY,
        username VARCHAR(50) NOT NULL,
        password VARCHAR(50) NOT NULL
    );
END

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        id_drink INT NOT NULL,
        quantity INT NOT NULL,
        total DECIMAL(18,2) NOT NULL,
        date_time DATETIME2(0) NOT NULL
    );
END";
            cmd.ExecuteNonQuery();

            SeedUsersIfEmpty(db);
            SeedOrdersIfEmpty(db);
        }
    }

    private static void SeedUsersIfEmpty(SqlConnection db)
    {
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

    private static void SeedOrdersIfEmpty(SqlConnection db)
    {
        int count;
        using (var c = db.CreateCommand())
        {
            c.CommandText = "SELECT COUNT(*) FROM dbo.Orders;";
            count = Convert.ToInt32(c.ExecuteScalar());
        }

        if (count != 0)
            return;

        using var insert = db.CreateCommand();
        insert.CommandText = @"
INSERT INTO dbo.Orders (id_drink, quantity, total, date_time) VALUES
(101, 500,  1200.00, '2024-02-14T09:10:00'),
(202, 120,   840.50, '2024-02-14T11:25:00'),
(303, 80,    640.00, '2024-02-14T15:40:00'),
(101, 1000, 2100.00, '2024-02-15T10:00:00');";
        insert.ExecuteNonQuery();
    }
}
