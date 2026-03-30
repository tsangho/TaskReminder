using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace TaskReminder.Services;

/// <summary>
/// 系统托盘图标服务
/// 使用 Hardcodet.NotifyIcon.Wpf 实现托盘图标、菜单和最小化到托盘功能
/// </summary>
public class TrayIconService : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private Window? _mainWindow;
    private bool _disposed;

    /// <summary>
    /// 初始化托盘图标服务
    /// </summary>
    /// <param name="window">主窗口引用</param>
    public void Initialize(Window window)
    {
        _mainWindow = window;

        // 创建托盘图标
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "任务提醒工具",
            Visibility = Visibility.Visible
        };

        // 尝试加载图标，如果失败则使用内置默认图标
        try
        {
            _trayIcon.IconSource = CreateDefaultIcon();
        }
        catch
        {
            // 降级：使用任务栏图标（不显示托盘图标）
            _trayIcon.IconSource = null;
        }
        
        // 创建上下文菜单
        _trayIcon.ContextMenu = CreateContextMenu();

        // 点击托盘图标左键显示主界面
        _trayIcon.TrayLeftMouseDown += OnTrayLeftMouseDown;
        
        // 双击托盘图标显示主界面
        _trayIcon.TrayMouseDoubleClick += OnTrayDoubleClick;
    }

    /// <summary>
    /// 创建默认图标
    /// </summary>
    /// <returns>图标图像源，如果加载失败返回 null</returns>
    private System.Windows.Media.ImageSource? CreateDefaultIcon()
    {
        // 尝试使用嵌入的 icon.ico 资源
        try
        {
            var iconUri = new Uri("pack://application:,,,/TaskReminder;component/icon.ico", UriKind.Absolute);
            var bitmap = new System.Windows.Media.Imaging.BitmapImage(iconUri);
            return bitmap;
        }
        catch
        {
            // 降级：使用 ExtractAssociatedIcon
            try
            {
                var appIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
                if (appIcon != null)
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        appIcon.Handle,
                        new System.Windows.Int32Rect(0, 0, appIcon.Width, appIcon.Height),
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch
            {
                // 如果所有方法都失败，返回 null
            }
            return null;
        }
    }

    /// <summary>
    /// 创建上下文菜单
    /// </summary>
    private System.Windows.Controls.ContextMenu CreateContextMenu()
    {
        var contextMenu = new System.Windows.Controls.ContextMenu();

        // 显示主界面菜单项
        var showMenuItem = new System.Windows.Controls.MenuItem
        {
            Header = "显示主界面",
            FontWeight = FontWeights.Bold
        };
        showMenuItem.Click += OnShowMenuItemClick;

        // 分隔线
        var separator = new System.Windows.Controls.Separator();

        // 退出菜单项
        var exitMenuItem = new System.Windows.Controls.MenuItem
        {
            Header = "退出程序"
        };
        exitMenuItem.Click += OnExitMenuItemClick;

        contextMenu.Items.Add(showMenuItem);
        contextMenu.Items.Add(separator);
        contextMenu.Items.Add(exitMenuItem);

        return contextMenu;
    }

    /// <summary>
    /// 最小化到托盘
    /// </summary>
    public void MinimizeToTray()
    {
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Minimized;
            _mainWindow.ShowInTaskbar = false;
            _mainWindow.Hide();
            
            // 显示气泡提示
            _trayIcon?.ShowBalloonTip(
                "任务提醒",
                "程序已最小化到系统托盘，双击图标可恢复窗口。",
                BalloonIcon.Info);
        }
    }

    /// <summary>
    /// 从托盘恢复显示（修复黑屏问题）
    /// </summary>
    public void RestoreFromTray()
    {
        if (_mainWindow != null)
        {
            // 修复黑屏问题：先隐藏再显示，刷新渲染上下文
            _mainWindow.Hide();
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.ShowInTaskbar = true;
            _mainWindow.Activate();
            
            // 强制刷新窗口内容
            _mainWindow.InvalidateVisual();
            _mainWindow.UpdateLayout();
        }
    }

    private void OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
    {
        RestoreFromTray();
    }

    private void OnTrayDoubleClick(object sender, RoutedEventArgs e)
    {
        RestoreFromTray();
    }

    private void OnShowMenuItemClick(object sender, RoutedEventArgs e)
    {
        RestoreFromTray();
    }

    private void OnExitMenuItemClick(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "确定要退出任务提醒工具吗？",
            "确认退出",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // 强制退出，不最小化到托盘
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// 显示气泡通知
    /// </summary>
    public void ShowBalloonTip(string title, string message, BalloonIcon icon = BalloonIcon.Info)
    {
        _trayIcon?.ShowBalloonTip(title, message, icon);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_trayIcon != null)
            {
                _trayIcon.TrayLeftMouseDown -= OnTrayLeftMouseDown;
                _trayIcon.TrayMouseDoubleClick -= OnTrayDoubleClick;
                _trayIcon.Dispose();
            }
            _disposed = true;
        }
    }
}
