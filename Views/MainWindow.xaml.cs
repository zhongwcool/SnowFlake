using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using SnowFlake.Models;

namespace SnowFlake.Views;

public partial class MainWindow : Window
{
    private readonly List<Snowflake> _snowflakes = new();
    private readonly DispatcherTimer _timer = new();
    private const int SnowflakeCount = 100;
    private readonly Random _random = new();

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += MainWindow_SourceInitialized;
        
        // 设置窗口大小覆盖所有屏幕
        SetWindowToCoverAllScreens();
        InitializeSnowfall();
    }
    
    private void SetWindowToCoverAllScreens()
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    private void InitializeSnowfall()
    {
        _timer.Interval = TimeSpan.FromMilliseconds(20);
        _timer.Tick += MoveSnowflakes;
        _timer.Start();

        for (var i = 0; i < SnowflakeCount; i++)
        {
            CreateSnowflake();
        }
    }

    private void CreateSnowflake()
    {
        var size = _random.NextDouble() * (15.0 - 5.0) + 5.0;
        var snowflake = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = Brushes.White
        };

        Canvas.SetLeft(snowflake, _random.NextDouble() * (this.Width - size));
        Canvas.SetTop(snowflake, -size);

        _snowflakes.Add(new Snowflake
        {
            Shape = snowflake,
            Velocity = _random.NextDouble() * (5.0 - 2.0) + 2.0
        });

        SnowCanvas.Children.Add(snowflake);
    }

    private void MoveSnowflakes(object? sender, EventArgs e)
    {
        foreach (var snowflake in _snowflakes)
        {
            double top = Canvas.GetTop(snowflake.Shape) + snowflake.Velocity;
            if (top > SnowCanvas.ActualHeight)
            {
                Canvas.SetTop(snowflake.Shape, -snowflake.Shape.Height);
                Canvas.SetLeft(snowflake.Shape, _random.NextDouble() * (SnowCanvas.ActualWidth - snowflake.Shape.Width));
            }
            else
            {
                Canvas.SetTop(snowflake.Shape, top);
            }
        }
    }
    
    // 导入 Windows API
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    // 常量定义
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x20;
    
    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        // 获取当前窗口句柄
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        // 设置窗口样式为透明
        SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_TRANSPARENT);
    }
}