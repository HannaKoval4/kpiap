using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace Lab34Wpf;

public partial class MainWindow : Window
{
    private bool _printUnlocked;
    public ICommand OpenPrintDialogCommand { get; }
    public ICommand OpenReportCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        OpenPrintDialogCommand = new RelayCommand(_ => OpenPrintDialog(), _ => _printUnlocked);
        OpenReportCommand = new RelayCommand(_ => OpenReportWindow(), _ => _printUnlocked);
        DataContext = this;
    }

    internal void UnlockPrint()
    {
        _printUnlocked = true;
        CommandManager.InvalidateRequerySuggested();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            DatabaseBootstrap.EnsureReady();
        }
        catch (SqlException ex)
        {
            System.Windows.MessageBox.Show(
                this,
                "Не удалось подготовить базу (пользователи и таблица заказов для отчёта).\n" + ex.Message +
                "\n\nУбедитесь, что установлен SQL Server Express LocalDB.",
                "Ошибка БД",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Auth_Click(object sender, RoutedEventArgs e)
    {
        var w = new AuthWindow(this);
        w.ShowDialog();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var w = new RegisterWindow(this, DatabaseBootstrap.DatabaseConnectionString);
        w.ShowDialog();
    }

    private void OpenPrintDialog()
    {
        try
        {
            var dlg = new System.Windows.Controls.PrintDialog();
            dlg.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "Ошибка печати", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenReportWindow()
    {
        var w = new ReportWindow { Owner = this };
        w.Show();
    }
}

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
