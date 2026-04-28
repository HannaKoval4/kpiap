using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Data.SqlClient;

class Program
{
    private static readonly object DbSync = new();
    private static readonly object ConsoleSync = new();

    static void Main()
    {
        string dbName = "Lab32CrudDb";
        string masterCs = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";
        string dbCs = $@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Database={dbName};";

        try
        {
            EnsureDatabase(masterCs, dbName);
            EnsureSchema(dbCs);
            ClearTable(dbCs);
            Seed(dbCs);

            WriteLineSafe("БД готова. Запуск потоков (вставка/модификация/удаление)...");

            var startGate = new ManualResetEventSlim(false);
            var threads = new List<Thread>
            {
                new(() => Inserter(dbCs, startGate)) { IsBackground = true, Name = "Inserter" },
                new(() => Updater(dbCs, startGate)) { IsBackground = true, Name = "Updater" },
                new(() => Deleter(dbCs, startGate)) { IsBackground = true, Name = "Deleter" },
            };

            foreach (var t in threads) t.Start();
            Thread.Sleep(250);
            startGate.Set();
            foreach (var t in threads) t.Join();

            WriteLineSafe("");
            WriteLineSafe("Поиск (Name содержит 'Item'):");
            foreach (var row in SearchByName(dbCs, "Item"))
                WriteLineSafe(RowToString(row));

            WriteLineSafe("");
            WriteLineSafe("Сортировка по Price DESC (TOP 10):");
            foreach (var row in GetSorted(dbCs, "Price", descending: true, top: 10))
                WriteLineSafe(RowToString(row));

            WriteLineSafe("");
            WriteLineSafe("Итоговое количество строк: " + GetCount(dbCs));
        }
        catch (SqlException ex)
        {
            WriteLineSafe("Ошибка SQL Server / LocalDB.");
            WriteLineSafe(ex.Message);
            WriteLineSafe("");
            WriteLineSafe("Проверьте, установлен ли SQL Server Express LocalDB (Visual Studio Installer).");
        }
        catch (Exception ex)
        {
            WriteLineSafe("Ошибка.");
            WriteLineSafe(ex.Message);
        }

        WriteLineSafe("");
        WriteLineSafe("Нажмите Enter для выхода...");
        Console.ReadLine();
    }

    private static void Inserter(string cs, ManualResetEventSlim gate)
    {
        gate.Wait();
        for (int i = 1; i <= 10; i++)
        {
            lock (DbSync)
            {
                int id = Insert(cs, $"Item {i}", price: 10 + i);
                WriteLineSafe($"[{Thread.CurrentThread.Name}] INSERT id={id}");
            }
            Thread.Sleep(70);
        }
    }

    private static void Updater(string cs, ManualResetEventSlim gate)
    {
        gate.Wait();
        for (int step = 1; step <= 10; step++)
        {
            lock (DbSync)
            {
                var ids = GetIds(cs);
                if (ids.Count > 0)
                {
                    int id = ids[step % ids.Count];
                    bool ok = UpdatePrice(cs, id, delta: 1.25m);
                    WriteLineSafe($"[{Thread.CurrentThread.Name}] UPDATE id={id} ok={ok}");
                }
            }
            Thread.Sleep(95);
        }
    }

    private static void Deleter(string cs, ManualResetEventSlim gate)
    {
        gate.Wait();
        for (int step = 1; step <= 10; step++)
        {
            lock (DbSync)
            {
                var ids = GetIds(cs);
                if (ids.Count > 0 && step % 2 == 0)
                {
                    int id = ids[0];
                    bool ok = Delete(cs, id);
                    WriteLineSafe($"[{Thread.CurrentThread.Name}] DELETE id={id} ok={ok}");
                }
            }
            Thread.Sleep(120);
        }
    }

    private static void EnsureDatabase(string masterConnectionString, string dbName)
    {
        using var cn = new SqlConnection(masterConnectionString);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = $@"
IF DB_ID(N'{dbName}') IS NULL
BEGIN
    CREATE DATABASE [{dbName}];
END";
        cmd.ExecuteNonQuery();
    }

    private static void EnsureSchema(string cs)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
IF OBJECT_ID(N'dbo.Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Items
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(120) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2(3) NOT NULL
    );
END";
        cmd.ExecuteNonQuery();
    }

    private static void ClearTable(string cs)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Items;";
        cmd.ExecuteNonQuery();
    }

    private static void Seed(string cs)
    {
        for (int i = 1; i <= 6; i++)
            Insert(cs, $"Seed {i}", price: 5 + i);
    }

    private static int Insert(string cs, string name, decimal price)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.Items(Name, Price, CreatedAt)
OUTPUT INSERTED.Id
VALUES (@name, @price, SYSUTCDATETIME());";
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@price", price);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static bool UpdatePrice(string cs, int id, decimal delta)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = "UPDATE dbo.Items SET Price = Price + @delta WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@delta", delta);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() == 1;
    }

    private static bool Delete(string cs, int id)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Items WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() == 1;
    }

    private static int GetCount(string cs)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM dbo.Items;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static List<int> GetIds(string cs)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT Id FROM dbo.Items ORDER BY Id;";
        using var r = cmd.ExecuteReader();

        var ids = new List<int>();
        while (r.Read())
            ids.Add(r.GetInt32(0));
        return ids;
    }

    private static IEnumerable<ItemRow> SearchByName(string cs, string contains)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
SELECT Id, Name, Price, CreatedAt
FROM dbo.Items
WHERE Name LIKE @p
ORDER BY Id;";
        cmd.Parameters.AddWithValue("@p", "%" + contains + "%");

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            yield return new ItemRow(
                r.GetInt32(0),
                r.GetString(1),
                r.GetDecimal(2),
                r.GetDateTime(3)
            );
        }
    }

    private static IEnumerable<ItemRow> GetSorted(string cs, string sortBy, bool descending, int top)
    {
        string orderBy = sortBy switch
        {
            "Id" => "Id",
            "Name" => "Name",
            "Price" => "Price",
            "CreatedAt" => "CreatedAt",
            _ => "Id"
        };

        string dir = descending ? "DESC" : "ASC";

        using var cn = new SqlConnection(cs);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = $@"
SELECT TOP (@top) Id, Name, Price, CreatedAt
FROM dbo.Items
ORDER BY {orderBy} {dir};";
        cmd.Parameters.AddWithValue("@top", top);

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            yield return new ItemRow(
                r.GetInt32(0),
                r.GetString(1),
                r.GetDecimal(2),
                r.GetDateTime(3)
            );
        }
    }

    private static string RowToString(ItemRow r)
        => $"{r.Id}: {r.Name} | {r.Price:0.00} | {r.CreatedAt:O}";

    private static void WriteLineSafe(string text)
    {
        lock (ConsoleSync)
        {
            Console.WriteLine(text);
        }
    }

    private readonly record struct ItemRow(int Id, string Name, decimal Price, DateTime CreatedAt);
}
