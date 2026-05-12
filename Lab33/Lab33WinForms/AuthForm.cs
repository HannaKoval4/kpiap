using System.Threading;

namespace Lab33WinForms;

public partial class AuthForm : Form
{
    private readonly MainForm _mainForm;
    private readonly string _connectionString;

    public AuthForm(MainForm mainForm)
    {
        InitializeComponent();
        _mainForm = mainForm;
        _connectionString = DatabaseBootstrap.DatabaseConnectionString;
    }

    private void AuthForm_Load(object? sender, EventArgs e) => textBoxLogin.Focus();

    private void buttonOk_Click(object? sender, EventArgs e)
    {
        var login = textBoxLogin.Text.Trim();
        var password = textBoxPassword.Text;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Логин и пароль обязательны для заполнения.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        buttonOk.Enabled = false;
        buttonClear.Enabled = false;

        var main = _mainForm;
        new Thread(() =>
        {
            try
            {
                var users = new UserService(_connectionString);
                var ok = users.AuthenticateUser(login, password);
                BeginInvoke(() =>
                {
                    buttonOk.Enabled = true;
                    buttonClear.Enabled = true;
                    if (ok)
                    {
                        MessageBox.Show($"Добрый день, {login}!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        main.GetSaveButton().Enabled = true;
                        main.GetSearchButton().Enabled = true;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Введены неверные данные. Попробуйте ещё раз.", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        buttonClear_Click(sender, e);
                    }
                });
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                {
                    buttonOk.Enabled = true;
                    buttonClear.Enabled = true;
                    MessageBox.Show("Ошибка при обращении к базе данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        })
        {
            IsBackground = true,
            Name = "AuthWorker"
        }.Start();
    }

    private void buttonClear_Click(object? sender, EventArgs e)
    {
        textBoxLogin.Clear();
        textBoxPassword.Clear();
        textBoxLogin.Focus();
    }
}
