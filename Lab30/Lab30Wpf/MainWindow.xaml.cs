using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lab30Wpf;

public partial class MainWindow : Window
{
    private Border? _border;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void closeButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _border = Template.FindName("newBorder", this) as Border;
        UpdateClip();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateClip();
    }

    private void UpdateClip()
    {
        if (_border is null) return;
        if (_border.ActualWidth <= 0 || _border.ActualHeight <= 0) return;

        _border.Clip = new EllipseGeometry(new Rect(0, 0, _border.ActualWidth, _border.ActualHeight));
    }
}