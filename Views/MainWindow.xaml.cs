using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using SnowFlake.Models;
using SnowFlake.Utils;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;

namespace SnowFlake.Views;

public partial class MainWindow
{
    private readonly List<Snowflake> _snowflakes = new();
    private readonly DispatcherTimer _timer = new();
    private const int SnowflakeCount = 60;
    private readonly Random _random = new();
    private float _scaleFactor = 1.0f;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
        
        // 设置窗口大小覆盖所有屏幕
        SetWindowToCoverAllScreens();

        // 设置托盘图标
        InitializeTrayMenu();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var (dpiX, dpiY) = DpiUtil.GetDpi(this);
        _scaleFactor = (float)dpiX / 96.0f; // 96 DPI is the standard DPI for Windows
        
        // 初始化雪花
        InitializeSnowfall();
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
        _timer.Interval = TimeSpan.FromMilliseconds(30);
        _timer.Tick += MoveSnowflakes;
        _timer.Start();

        for (var i = 0; i < SnowflakeCount; i++)
        {
            CreateSnowflake();
        }
    }

    private void CreateSnowflake()
    {
        var size = _random.NextDouble() * (_selectedScale - 5.0) + 5.0;
        size *= _scaleFactor;
        var snowflake = new Path
        {
            Width = size,
            Height = size + _selectedOffset,
            Fill = System.Windows.Media.Brushes.White,
            Data = _selectedSnow,
            Stretch = Stretch.Fill
        };

        Canvas.SetLeft(snowflake, _random.NextDouble() * (this.Width - size));
        Canvas.SetTop(snowflake, -size);

        _snowflakes.Add(new Snowflake
        {
            Shape = snowflake,
            Velocity = _random.NextDouble() * (5.0 - 2.0) + 2.0,
            RtSpeed = _random.NextDouble() * (5.0 - 2.0) + 2.0
        });

        SnowCanvas.Children.Add(snowflake);
    }

    private void MoveSnowflakes(object? sender, EventArgs e)
    {
        foreach (var snowflake in _snowflakes)
        {
            var top = Canvas.GetTop(snowflake.Shape) + snowflake.Velocity;
            if (top > SnowCanvas.ActualHeight)
            {
                Canvas.SetTop(snowflake.Shape, -snowflake.Shape.Height);
                Canvas.SetLeft(snowflake.Shape, _random.NextDouble() * (SnowCanvas.ActualWidth - snowflake.Shape.Width));
            }
            else
            {
                Canvas.SetTop(snowflake.Shape, top);
            }
            
            // 旋转雪花
            if (snowflake.Shape.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = (rotateTransform.Angle + snowflake.RtSpeed) % 360;
            }
            else
            {
                snowflake.Shape.RenderTransform = new RotateTransform(snowflake.RtSpeed);
                snowflake.Shape.RenderTransformOrigin = new Point(0.5, 0.5); // 将旋转中心设置为形状的中心
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

    // 初始化托盘菜单
    private void InitializeTrayMenu()
    {
        // 创建托盘图标
        _trayIcon = new NotifyIcon();
        var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/ico_notify.ico"))?.Stream;
        _trayIcon.Icon = new Icon(iconStream!);
        _trayIcon.Text = "Let it snow";
        _trayIcon.Visible = true;
        
        // 菜单-雪花样式
        var trayMenu = new ContextMenuStrip();
        var mainMenuItem = new ToolStripMenuItem("样式");

        // 创建二级菜单
        _menuSnow = new ContextMenuStrip();
        var subOption1 = new ToolStripMenuItem(_snowShapes[1].Name);
        subOption1.CheckOnClick = true;
        subOption1.Click += SubOption_Click;

        var subOption2 = new ToolStripMenuItem(_snowShapes[2].Name);
        subOption2.CheckOnClick = true;
        subOption2.Click += SubOption_Click;
        
        var subOption3 = new ToolStripMenuItem(_snowShapes[3].Name);
        subOption3.CheckOnClick = true;
        subOption3.Click += SubOption_Click;
        
        var subOption4 = new ToolStripMenuItem(_snowShapes[4].Name);
        subOption4.CheckOnClick = true;
        subOption4.Click += SubOption_Click;
        
        var subOption0 = new ToolStripMenuItem(_snowShapes[0].Name);
        subOption0.CheckOnClick = true;
        subOption0.Click += SubOption_Click;

        _menuSnow.Items.AddRange(new ToolStripItem[] { subOption0, subOption1, subOption2, subOption3, subOption4 });
        mainMenuItem.DropDown = _menuSnow;

        trayMenu.Items.Add(mainMenuItem);
        
        trayMenu.Items.Add("关于", null, OnTrayIconAboutClicked);
        trayMenu.Items.Add("反馈", null, OnTrayIconTouchClicked);
        trayMenu.Items.Add("退出", null, OnTrayIconExitClicked);

        _trayIcon.ContextMenuStrip = trayMenu;
        
        var userChoice = ReadUserChoice();
        if (string.IsNullOrEmpty(userChoice))
        {
            subOption0.Checked = true;
            subOption0.PerformClick();
        }
        else
        {
            foreach (ToolStripMenuItem item in _menuSnow.Items)
            {
                if (item.Text == userChoice)
                {
                    item.Checked = true;
                    item.PerformClick();
                    break;
                }
            }
        }
    }
    
    // 单选逻辑
    private void SubOption_Click(object sender, EventArgs e)
    {
        // 处理UI
        foreach (ToolStripMenuItem item in _menuSnow.Items)
        {
            item.Checked = false;
        }

        var clickedItem = (ToolStripMenuItem)sender;
        clickedItem.Checked = true;
        SaveUserChoice(clickedItem.Text);
        
        // 根据用户选择，更新雪花样式
        foreach (var snowShape in _snowShapes)
        {
            if (snowShape.Name == clickedItem.Text)
            {
                _selectedSnow = (Geometry)FindResource(snowShape.Key);
                _selectedOffset = snowShape.Offset;
                _selectedScale = snowShape.Scale;
                break;
            }
        }
        
        // 重新生成雪花
        SnowCanvas.Children.Clear();
        _snowflakes.Clear();
        for (var i = 0; i < SnowflakeCount; i++)
        {
            CreateSnowflake();
        }
    }
    
    private Geometry _selectedSnow = new EllipseGeometry(new Point(50, 50), 40, 40); // 圆心(50,50) 半径40
    private double _selectedOffset = 5.0;
    private double _selectedScale = 15.0;

    private readonly SnowShape[] _snowShapes = {
        new() { Name = "波点", Key = "IconSnow0", Offset = 0.0, Scale = 20.0}, 
        new() { Name = "雪花1", Key = "IconSnow1", Offset = 5.0, Scale = 35.0}, 
        new() { Name = "雪花2", Key = "IconSnow2", Offset = 5.0, Scale = 35.0 }, 
        new() { Name = "雪花3", Key = "IconSnow3", Offset = -5.0, Scale = 35.0 }, 
        new() { Name = "雪花4", Key = "IconSnow4", Offset = 5.0, Scale = 35.0 }
    };
    
    // 保存用户选择
    private static void SaveUserChoice(string choice)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var myAppFolder = System.IO.Path.Combine(appDataPath, "SnowFlake");
        Directory.CreateDirectory(myAppFolder);
        File.WriteAllText(System.IO.Path.Combine(myAppFolder, "userChoice.txt"), choice);
    }

    // 读取用户选择
    private static string ReadUserChoice()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var myAppFolder = System.IO.Path.Combine(appDataPath, "SnowFlake");
        var userChoicePath = System.IO.Path.Combine(myAppFolder, "userChoice.txt");

        if (File.Exists(userChoicePath)) return File.ReadAllText(userChoicePath);
        return null;
    }

    private void OnTrayIconAboutClicked(object sender, EventArgs e)
    {
        // 获取当前程序集的版本号
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        MessageBox.Show($"版本：{version}");
    }

    private static void OnTrayIconTouchClicked(object? sender, EventArgs e)
    {
        var window = new FeedbackWindow();
        window.ShowDialog();
    }

    private NotifyIcon _trayIcon;
    private ContextMenuStrip _menuSnow;

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
        SnowMan.Margin = new Thickness(20, 0, 20, taskBarHeight / _scaleFactor);
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