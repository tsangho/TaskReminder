using System;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Cursor = System.Windows.Input.Cursor;
using MessageBox = System.Windows.MessageBox;

namespace TaskReminder.Services;

/// <summary>
/// 系统托盘图标服务
/// 提供最小化到托盘、托盘菜单等功能
/// </summary>
public class TrayIconService : IDisposable
{
    private NotifyIcon? _trayIcon;
    private Window? _mainWindow;
    private bool _disposed;

    /// <summary>
    /// 初始化托盘图标服务
    /// </summary>
    /// <param name="window">主窗口引用</param>
    public void Initialize(Window window)
    {
        _mainWindow = window;

        // 创建托盘图标（使用系统图标作为默认）
        _trayIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "任务提醒",
            Visible = true
        };

        // 创建托盘菜单
        var contextMenu = new ContextMenu();
        
        // 显示主界面菜单项
        var showMenuItem = new MenuItem("显示主界面", OnShowMenuItemClick);
        
        // 退出菜单项
        var exitMenuItem = new MenuItem("退出", OnExitMenuItemClick);
        
        contextMenu.MenuItems.AddRange(new[] { showMenuItem, exitMenuItem });
        _trayIcon.ContextMenu = contextMenu;

        // 点击托盘图标打开主界面
        _trayIcon.MouseClick += OnTrayIconClick;
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
        }
    }

    /// <summary>
    /// 从托盘恢复显示
    /// </summary>
    public void RestoreFromTray()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _mainWindow.ShowInTaskbar = true;
        }
    }

    private void OnTrayIconClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            RestoreFromTray();
        }
    }

    private void OnShowMenuItemClick(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    /// <summary>
    /// 显式显示主窗口（从托盘恢复）
    /// </summary>
    public void ShowMainWindow()
    {
        RestoreFromTray();
    }

    private void OnExitMenuItemClick(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "确定要退出任务提醒吗？",
            "确认退出",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _trayIcon?.Dispose();
            _disposed = true;
        }
    }
}
