using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Lab29Wpf;

public partial class MainWindow : Window
{
    public ICommand OpenPrintDialogCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        OpenPrintDialogCommand = new RelayCommand(_ => OpenPrintDialog());
        DataContext = this;
    }

    private void OpenPrint_Click(object sender, RoutedEventArgs e) => OpenPrintDialog();

    private void OpenPrintDialog()
    {
        try
        {
            var dlg = new PrintDialog();
            dlg.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка печати", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
