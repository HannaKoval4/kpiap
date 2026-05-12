using Microsoft.Data.SqlClient;

namespace Lab33WinForms;

internal sealed class UserService
{
    private readonly string _connectionString;

    internal UserService(string connectionString) => _connectionString = connectionString;

    internal bool AuthenticateUser(string username, string password)
    {
        lock (DatabaseBootstrap.DbSync)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Users WHERE username = @username AND password = @password";
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            var count = (int)command.ExecuteScalar()!;
            return count > 0;
        }
    }

    internal bool RegisterUser(string username, string password)
    {
        lock (DatabaseBootstrap.DbSync)
        {
            if (IsUserExistsCore(username))
                return false;

            const string sql = "INSERT INTO dbo.Users (username, password) VALUES (@username, @password)";
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            return command.ExecuteNonQuery() > 0;
        }
    }

    private bool IsUserExistsCore(string username)
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Users WHERE username = @username";
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);
        var count = (int)command.ExecuteScalar()!;
        return count > 0;
    }
}
