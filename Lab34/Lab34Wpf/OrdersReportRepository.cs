using System.Data;
using Microsoft.Data.SqlClient;

namespace Lab34Wpf;

internal static class OrdersReportRepository
{
    internal static DataTable LoadOrdersForReportDate(DateTime reportDate)
    {
        lock (DatabaseBootstrap.DbSync)
        {
            var dataTable = new DataTable();
            using var connection = new SqlConnection(DatabaseBootstrap.DatabaseConnectionString);
            connection.Open();
            const string sql =
                "SELECT id, id_drink, quantity, total FROM dbo.Orders WHERE CAST(date_time AS DATE) = @d;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@d", SqlDbType.Date).Value = reportDate.Date;
            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(dataTable);
            return dataTable;
        }
    }
}
