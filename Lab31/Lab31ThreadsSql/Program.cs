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
        string dbName = "Lab31ThreadDb";
        string masterConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";
        string dbConnectionString = $@"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Database={dbName};";

        try
        {
            EnsureDatabase(masterConnectionString, dbName);
            EnsureSchema(dbConnectionString);

            WriteLineSafe("БД готова. Запуск потоков...");

            var startGate = new ManualResetEventSlim(false);
            var threads = new List<Thread>();

            for (int i = 1; i <= 4; i++)
            {
                int threadId = i;
                var t = new Thread(() => Worker(threadId, dbConnectionString, startGate))
                {
                    IsBackground = true,
                    Name = $"T{threadId}"
                };
                threads.Add(t);
                t.Start();
            }

            Thread.Sleep(300);
            startGate.Set();

            foreach (var t in threads)
                t.Join();

            WriteLineSafe("");
            WriteLineSafe("Готово. Последние записи в таблице:");
            PrintLastRows(dbConnectionString, 12);
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

    private static void Worker(int threadId, string dbConnectionString, ManualResetEventSlim startGate)
    {
        startGate.Wait();

        for (int i = 1; i <= 8; i++)
        {
            lock (DbSync)
            {
                InsertRow(dbConnectionString, threadId, $"message {i}");
                int count = GetRowCount(dbConnectionString);
                WriteLineSafe($"[{Thread.CurrentThread.Name}] вставка {i}, всего строк = {count}");
            }

            Thread.Sleep(80 + threadId * 25);
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

    private static void EnsureSchema(string dbConnectionString)
    {
        using var cn = new SqlConnection(dbConnectionString);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
IF OBJECT_ID(N'dbo.ThreadLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ThreadLog
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ThreadId INT NOT NULL,
        Message NVARCHAR(200) NOT NULL,
        CreatedAt DATETIME2(3) NOT NULL
    );
END";
        cmd.ExecuteNonQuery();
    }

    private static void InsertRow(string dbConnectionString, int threadId, string message)
    {
        using var cn = new SqlConnection(dbConnectionString);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = "INSERT INTO dbo.ThreadLog(ThreadId, Message, CreatedAt) VALUES (@tid, @msg, SYSUTCDATETIME());";
        cmd.Parameters.AddWithValue("@tid", threadId);
        cmd.Parameters.AddWithValue("@msg", message);
        cmd.ExecuteNonQuery();
    }

    private static int GetRowCount(string dbConnectionString)
    {
        using var cn = new SqlConnection(dbConnectionString);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM dbo.ThreadLog;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static void PrintLastRows(string dbConnectionString, int take)
    {
        using var cn = new SqlConnection(dbConnectionString);
        cn.Open();

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
SELECT TOP (@take) Id, ThreadId, Message, CreatedAt
FROM dbo.ThreadLog
ORDER BY Id DESC;";
        cmd.Parameters.AddWithValue("@take", take);

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            int id = r.GetInt32(0);
            int tid = r.GetInt32(1);
            string msg = r.GetString(2);
            DateTime created = r.GetDateTime(3);

            WriteLineSafe($"{id}: T{tid} | {msg} | {created:O}");
        }
    }

    private static void WriteLineSafe(string text)
    {
        lock (ConsoleSync)
        {
            Console.WriteLine(text);
        }
    }
}
