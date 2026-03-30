using System;
using System.Windows;

namespace TaskReminder.Services;

/// <summary>
/// 通知服务
/// 使用简单的 MessageBox 实现系统级通知
/// </summary>
public class NotificationService : IDisposable
{
    private bool _disposed;

    public NotificationService()
    {
        // 初始化通知服务
    }

    /// <summary>
    /// 显示通知
    /// </summary>
    /// <param name="title">通知标题</param>
    /// <param name="message">通知内容</param>
    /// <param name="taskId">关联的任务 ID（用于点击跳转和标记完成）</param>
    public void ShowNotification(string title, string message, int? taskId = null)
    {
        try
        {
            // 使用简单的 MessageBox 替代 Toast 通知
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            Console.WriteLine($"[通知已发送] {title}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[通知发送失败] {title}: {message}");
            Console.WriteLine($"错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 显示到期任务提醒通知
    /// </summary>
    public void ShowTaskReminder(string taskTitle, string taskDescription, int taskId)
    {
        var title = "⏰ 任务到期提醒";
        var message = string.IsNullOrEmpty(taskDescription) ? $"任务 \"{taskTitle}\" 已到期！" : $"任务 \"{taskTitle}\" 已到期！\n{taskDescription}";
        ShowNotification(title, message, taskId);
    }

    /// <summary>
    /// 显示任务即将到期提醒（提前提醒）
    /// </summary>
    public void ShowTaskUpcomingReminder(string taskTitle, string taskDescription, int taskId, int minutesUntilDue)
    {
        var title = "📋 任务即将到期";
        var timeText = minutesUntilDue switch
        {
            5 => "5 分钟后",
            10 => "10 分钟后",
            15 => "15 分钟后",
            30 => "30 分钟后",
            60 => "1 小时后",
            _ => $"{minutesUntilDue}分钟后"
        };
        var message = $"任务 \"{taskTitle}\" 将在 {timeText} 到期！";
        ShowNotification(title, message, taskId);
    }

    /// <summary>
    /// 显示简单文本通知（用于调试或降级）
    /// </summary>
    public void ShowSimpleNotification(string title, string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔔 {title}");
        Console.WriteLine($" {message}");
    }

    /// <summary>
    /// 当用户点击"标记完成"按钮时触发
    /// </summary>
    public event Action<int>? OnMarkCompleteRequested;

    /// <summary>
    /// 当用户点击"查看详情"按钮时触发
    /// </summary>
    public event Action<int>? OnViewDetailRequested;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
