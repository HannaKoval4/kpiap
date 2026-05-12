using System.Threading;

namespace Lab33WinForms;

public partial class RegisterForm : Form
{
    private readonly string _connectionString;

    public RegisterForm(string connectionString)
    {
        InitializeComponent();
        _connectionString = connectionString;
    }

    private void RegisterForm_Load(object? sender, EventArgs e) => textBoxLogin.Focus();

    private void buttonRegister_Click(object? sender, EventArgs e)
    {
        var login = textBoxLogin.Text.Trim();
        var password = textBoxPassword.Text;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Логин и пароль обязательны для заполнения.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        buttonRegister.Enabled = false;
        buttonClear.Enabled = false;

        new Thread(() =>
        {
            try
            {
                var users = new UserService(_connectionString);
                var ok = users.RegisterUser(login, password);
                BeginInvoke(() =>
                {
                    buttonRegister.Enabled = true;
                    buttonClear.Enabled = true;
                    if (ok)
                    {
                        MessageBox.Show("Вы успешно зарегистрированы. Пожалуйста, пройдите авторизацию.", "Регистрация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Пользователь с таким именем уже зарегистрирован. Введите другое имя пользователя или пройдите авторизацию.",
                            "Регистрация",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        buttonClear_Click(sender, e);
                    }
                });
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                {
                    buttonRegister.Enabled = true;
                    buttonClear.Enabled = true;
                    MessageBox.Show("Ошибка при обращении к базе данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        })
        {
            IsBackground = true,
            Name = "RegisterWorker"
        }.Start();
    }

    private void buttonClear_Click(object? sender, EventArgs e)
    {
        textBoxLogin.Clear();
        textBoxPassword.Clear();
        textBoxLogin.Focus();
    }
}
