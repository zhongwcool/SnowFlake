using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Threading;
using SnowFlake.Models;
using Application = System.Windows.Application;

namespace SnowFlake.Views;

public partial class MainWindow
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

        // 设置托盘图标
        SetupNotifyIcon();
        // 摆放雪人
        PlaceTheSnowman();
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
            Fill = System.Windows.Media.Brushes.White
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
    
    #region 鼠标穿透
    
    // 导入 Windows API
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
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
    
    #endregion
    
    #region 托盘功能区

    private void SetupNotifyIcon()
    {
        // 创建托盘图标
        _trayIcon = new NotifyIcon();
        var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/ico_notify.ico"))?.Stream;
        _trayIcon.Icon = new Icon(iconStream!);
        _trayIcon.Text = "Let it snow";
        _trayIcon.Visible = true;

        // 可以添加一个右键菜单等
        _trayIcon.ContextMenuStrip = new ContextMenuStrip();
        _trayIcon.ContextMenuStrip.Items.Add("退出", null, OnTrayIconExitClicked);
    }

    private NotifyIcon _trayIcon;
    
    private void OnTrayIconExitClicked(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        Application.Current.Shutdown();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _trayIcon.Dispose();
    }

    #endregion

    #region 获得任务栏高度以摆放雪人

    private void PlaceTheSnowman()
    {
        // 获取任务栏高度，兼容多屏幕的情况
        var taskBarHeight = GetTaskBarHeight();
        SnowMan.Margin = new Thickness(20, 0, 20, taskBarHeight - 10);
    }
    
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref Rect pvParam, uint fWinIni);

    private const uint SpiGetWorkArea = 0x0030;

    private struct Rect
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public int Left, Top, Right, Bottom;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }

    private static int GetTaskBarHeight()
    {
        var rect = new Rect();
        if (!SystemParametersInfo(SpiGetWorkArea, 0, ref rect, 0)) return 0;
        var screenHeight = Screen.PrimaryScreen.Bounds.Height;
        var taskBarHeight = screenHeight - rect.Bottom;
        return taskBarHeight;
    }

    #endregion
}