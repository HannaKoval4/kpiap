using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lab28Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private bool _isInitialized;

    public MainWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (_, _) => Tick(), Dispatcher.CurrentDispatcher);
        Loaded += (_, _) =>
        {
            _isInitialized = true;
            Tick();
            _timer.Start();
        };
    }

    private void HideButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Visibility != Visibility.Visible) return;

        var sb = new Storyboard();

        var opacity = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(420),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(opacity, btn);
        Storyboard.SetTargetProperty(opacity, new PropertyPath(OpacityProperty));
        sb.Children.Add(opacity);

        var scaleX = new DoubleAnimation
        {
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(420),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 0.2 }
        };
        Storyboard.SetTarget(scaleX, btn);
        Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.(ScaleTransform.ScaleX)"));
        sb.Children.Add(scaleX);

        var scaleY = new DoubleAnimation
        {
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(420),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 0.2 }
        };
        Storyboard.SetTarget(scaleY, btn);
        Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.(ScaleTransform.ScaleY)"));
        sb.Children.Add(scaleY);

        sb.Completed += (_, _) => btn.Visibility = Visibility.Collapsed;
        sb.Begin();
    }

    private void ResetButtons_Click(object sender, RoutedEventArgs e)
    {
        ResetButton(Btn1);
        ResetButton(Btn2);
        ResetButton(Btn3);
    }

    private static void ResetButton(Button btn)
    {
        btn.Visibility = Visibility.Visible;
        btn.Opacity = 1;
        if (btn.RenderTransform is ScaleTransform st)
        {
            st.ScaleX = 1;
            st.ScaleY = 1;
        }
        else
        {
            btn.RenderTransform = new ScaleTransform(1, 1);
        }
    }

    private void Tick()
    {
        if (!_isInitialized) return;
        NowText.Text = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        UpdateSun(animated: true);
    }

    private void SkyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSun(animated: false);
    }

    private void UpdateSun(bool animated)
    {
        double w = Math.Max(0, SkyCanvas.ActualWidth);
        double h = Math.Max(0, SkyCanvas.ActualHeight);
        if (w <= 1 || h <= 1) return;

        UpdateArcGeometry(w, h);

        var now = DateTime.Now.TimeOfDay;
        var t = (now.TotalMinutes - 6 * 60) / (12 * 60);
        t = Math.Clamp(t, 0, 1);

        double margin = 16;
        double left = margin;
        double right = w - margin;
        double ground = h - margin;
        double peak = Math.Max(margin, h * 0.12);

        double x = left + (right - left) * t;
        double u = (x - left) / (right - left);
        double y = peak + (ground - peak) * 4 * Math.Pow(u - 0.5, 2);

        double sunX = x - Sun.Width / 2.0;
        double sunY = y - Sun.Height / 2.0;

        if (!animated)
        {
            Canvas.SetLeft(Sun, sunX);
            Canvas.SetTop(Sun, sunY);
            return;
        }

        var sb = new Storyboard();

        var ax = new DoubleAnimation
        {
            To = sunX,
            Duration = TimeSpan.FromMilliseconds(650),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(ax, Sun);
        Storyboard.SetTargetProperty(ax, new PropertyPath("(Canvas.Left)"));
        sb.Children.Add(ax);

        var ay = new DoubleAnimation
        {
            To = sunY,
            Duration = TimeSpan.FromMilliseconds(650),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(ay, Sun);
        Storyboard.SetTargetProperty(ay, new PropertyPath("(Canvas.Top)"));
        sb.Children.Add(ay);

        sb.Begin();
    }

    private void UpdateArcGeometry(double w, double h)
    {
        double margin = 16;
        double left = margin;
        double right = w - margin;
        double ground = h - margin;
        double peak = Math.Max(margin, h * 0.12);

        var fig = new PathFigure { StartPoint = new Point(left, ground), IsClosed = false };
        var seg = new QuadraticBezierSegment(new Point(w / 2.0, peak), new Point(right, ground), true);
        fig.Segments.Add(seg);

        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        ArcPath.Data = geo;
    }
}