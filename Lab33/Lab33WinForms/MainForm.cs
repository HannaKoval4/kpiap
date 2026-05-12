using System.Data;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace Lab33WinForms;

public partial class MainForm : Form
{
    private readonly string _connectionString = DatabaseBootstrap.DatabaseConnectionString;
    private DataTable? _drinksTable;

    public MainForm()
    {
        InitializeComponent();
        buttonSave.Enabled = false;
        buttonSearch.Enabled = false;
    }

    public Button GetSaveButton() => buttonSave;
    public Button GetSearchButton() => buttonSearch;

    private void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            DatabaseBootstrap.EnsureReady();
        }
        catch (SqlException ex)
        {
            MessageBox.Show(
                "Не удалось подготовить базу данных.\n" + ex.Message +
                "\n\nУбедитесь, что установлен SQL Server Express LocalDB (компонент Visual Studio).",
                "Ошибка БД",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        StartReloadDrinksFull();
    }

    private void StartReloadDrinksFull()
    {
        var loader = new Thread(() =>
        {
            try
            {
                DataTable table;
                lock (DatabaseBootstrap.DbSync)
                {
                    table = LoadDrinksTable("SELECT id, name, description FROM dbo.Drinks");
                }

                BeginInvoke(() =>
                {
                    _drinksTable = table;
                    dataGridView1.DataSource = _drinksTable;
                });
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                    MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
        })
        {
            IsBackground = true,
            Name = "DrinksLoader"
        };
        loader.Start();
    }

    private static DataTable LoadDrinksTable(string sql, string? like = null)
    {
        using var connection = new SqlConnection(DatabaseBootstrap.DatabaseConnectionString);
        connection.Open();
        using var adapter = new SqlDataAdapter(sql, connection);
        if (like != null)
        {
            adapter.SelectCommand!.Parameters.AddWithValue("@search", "%" + like + "%");
        }

        var table = new DataTable();
        adapter.Fill(table);
        if (table.Columns.Contains("id"))
            table.PrimaryKey = new[] { table.Columns["id"]! };
        return table;
    }

    private void buttonSave_Click(object? sender, EventArgs e)
    {
        if (_drinksTable == null)
            return;

        dataGridView1.EndEdit();

        try
        {
            lock (DatabaseBootstrap.DbSync)
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                using var adapter = new SqlDataAdapter("SELECT id, name, description FROM dbo.Drinks", connection);
                using var builder = new SqlCommandBuilder(adapter);
                adapter.Update(_drinksTable);
            }

            MessageBox.Show("Изменения сохранены успешно.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            StartReloadDrinksFull();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Произошла ошибка при сохранении изменений: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void buttonSearch_Click(object? sender, EventArgs e)
    {
        var query = textBoxSearch.Text.Trim();
        var searcher = new Thread(() =>
        {
            try
            {
                DataTable table;
                lock (DatabaseBootstrap.DbSync)
                {
                    const string sql = "SELECT id, name, description FROM dbo.Drinks WHERE name LIKE @search OR description LIKE @search";
                    table = LoadDrinksTable(sql, query);
                }

                BeginInvoke(() =>
                {
                    _drinksTable = table;
                    dataGridView1.DataSource = _drinksTable;
                });
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                    MessageBox.Show("Ошибка поиска: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
        })
        {
            IsBackground = true,
            Name = "DrinksSearch"
        };
        searcher.Start();
    }

    private void textBoxSearch_TextChanged(object? sender, EventArgs e)
    {
        if (!buttonSearch.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
        {
            StartReloadDrinksFull();
        }
        else
        {
            buttonSearch_Click(sender, e);
        }
    }

    private void buttonAuth_Click(object? sender, EventArgs e)
    {
        using var form = new AuthForm(this);
        form.ShowDialog();
    }

    private void buttonRegister_Click(object? sender, EventArgs e)
    {
        using var form = new RegisterForm(_connectionString);
        form.ShowDialog();
    }
}
