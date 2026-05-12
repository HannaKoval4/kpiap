using System.Threading;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace Lab34Wpf;

public partial class AuthWindow : Window
{
    private readonly MainWindow _main;

    public AuthWindow(MainWindow main)
    {
        InitializeComponent();
        _main = main;
        Owner = main;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => LoginBox.Focus();

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            System.Windows.MessageBox.Show(this, "Логин и пароль обязательны для заполнения.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OkButton.IsEnabled = false;
        var users = new UserService(DatabaseBootstrap.DatabaseConnectionString);

        new Thread(() =>
        {
            try
            {
                var ok = users.AuthenticateUser(login, password);
                Dispatcher.Invoke(() =>
                {
                    OkButton.IsEnabled = true;
                    if (ok)
                    {
                        System.Windows.MessageBox.Show(this, $"Добрый день, {login}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        _main.UnlockPrint();
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(this, "Введены неверные данные. Попробуйте ещё раз.", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Clear_Click(sender, e);
                    }
                });
            }
            catch (SqlException ex)
            {
                Dispatcher.Invoke(() =>
                {
                    OkButton.IsEnabled = true;
                    System.Windows.MessageBox.Show(this, "Ошибка БД: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    OkButton.IsEnabled = true;
                    System.Windows.MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        })
        {
            IsBackground = true,
            Name = "AuthWorker"
        }.Start();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        LoginBox.Clear();
        PasswordBox.Clear();
        LoginBox.Focus();
    }
}
