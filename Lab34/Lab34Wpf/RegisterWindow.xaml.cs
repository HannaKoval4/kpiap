using System.Threading;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace Lab34Wpf;

public partial class RegisterWindow : Window
{
    private readonly string _connectionString;

    public RegisterWindow(Window owner, string connectionString)
    {
        InitializeComponent();
        Owner = owner;
        _connectionString = connectionString;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => LoginBox.Focus();

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            System.Windows.MessageBox.Show(this, "Логин и пароль обязательны для заполнения.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        RegisterButton.IsEnabled = false;
        var users = new UserService(_connectionString);

        new Thread(() =>
        {
            try
            {
                var ok = users.RegisterUser(login, password);
                Dispatcher.Invoke(() =>
                {
                    RegisterButton.IsEnabled = true;
                    if (ok)
                    {
                        System.Windows.MessageBox.Show(this, "Вы успешно зарегистрированы. Пожалуйста, пройдите авторизацию.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(this,
                            "Пользователь с таким именем уже зарегистрирован. Введите другое имя пользователя или пройдите авторизацию.",
                            "Регистрация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        Clear_Click(sender, e);
                    }
                });
            }
            catch (SqlException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    RegisterButton.IsEnabled = true;
                    System.Windows.MessageBox.Show(this, "Ошибка БД: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    RegisterButton.IsEnabled = true;
                    System.Windows.MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        })
        {
            IsBackground = true,
            Name = "RegisterWorker"
        }.Start();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        LoginBox.Clear();
        PasswordBox.Clear();
        LoginBox.Focus();
    }
}
