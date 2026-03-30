using System;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace TaskReminder.Services;

/// <summary>
/// Windows Toast 通知服务
/// 使用 Microsoft.Windows.AppNotifications API 实现系统级通知
/// </summary>
public class NotificationService : IDisposable
{
    private readonly AppNotificationManager _notificationManager;
    private bool _disposed;

    public NotificationService()
    {
        _notificationManager = AppNotificationManager.Default;
        
        // 注册通知处理程序（用于处理用户点击按钮等操作）
        _notificationManager.NotificationInvoked += OnNotificationInvoked;
        
        // 初始化通知管理器
        _notificationManager.Initialize();
    }

    /// <summary>
    /// 显示 Toast 通知
    /// </summary>
    /// <param name="title">通知标题</param>
    /// <param name="message">通知内容</param>
    /// <param name="taskId">关联的任务ID（用于点击跳转和标记完成）</param>
    public void ShowNotification(string title, string message, int? taskId = null)
    {
        try
        {
            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message);

            // 如果有任务ID，添加按钮和参数
            if (taskId.HasValue)
            {
                // 添加标记完成按钮
                builder.AddButton(new AppNotificationButton("标记完成")
                    .AddArgument("action", "mark_complete")
                    .AddArgument("taskId", taskId.Value.ToString()));

                // 添加查看详情按钮
                builder.AddButton(new AppNotificationButton("查看详情")
                    .AddArgument("action", "view_detail")
                    .AddArgument("taskId", taskId.Value.ToString()));
            }

            // 设置超时时间（5分钟）
            builder.SetToastDuration(AppNotificationDuration.Long);

            var notification = builder.BuildNotification();

            // 设置标签用于分组
            notification.Tag = "task-reminder";
            notification.Group = "TaskReminder";

            // 显示通知
            _notificationManager.Show(notification);
            
            Console.WriteLine($"[通知已发送] {title}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[通知发送失败] {title}: {message}");
            Console.WriteLine($"错误: {ex.Message}");
            
            // 降级：显示简单通知
            ShowSimpleNotification(title, message);
        }
    }

    /// <summary>
    /// 显示到期任务提醒通知
    /// </summary>
    public void ShowTaskReminder(string taskTitle, string taskDescription, int taskId)
    {
        var title = "⏰ 任务到期提醒";
        var message = string.IsNullOrEmpty(taskDescription) 
            ? $"任务 \"{taskTitle}\" 已到期！" 
            : $"任务 \"{taskTitle}\" 已到期！\n{taskDescription}";
        
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
            5 => "5分钟后",
            10 => "10分钟后",
            15 => "15分钟后",
            30 => "30分钟后",
            60 => "1小时后",
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
        Console.WriteLine($"         {message}");
    }

    /// <summary>
    /// 处理通知被用户点击的事件
    /// </summary>
    private void OnNotificationInvoked(object? sender, AppNotificationActivatedEventArgs args)
    {
        try
        {
            var arguments = AppNotificationArgument.Parse(args.Argument);
            
            if (arguments.TryGetValue("action", out var action) && arguments.TryGetValue("taskId", out var taskIdStr))
            {
                if (int.TryParse(taskIdStr, out var taskId))
                {
                    Console.WriteLine($"[通知操作] action={action}, taskId={taskId}");

                    // 在 UI 线程上触发事件
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (action)
                        {
                            case "mark_complete":
                                OnMarkCompleteRequested?.Invoke(taskId);
                                break;
                            case "view_detail":
                                OnViewDetailRequested?.Invoke(taskId);
                                break;
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[通知处理错误] {ex.Message}");
        }
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
            _notificationManager.NotificationInvoked -= OnNotificationInvoked;
            _disposed = true;
        }
    }
}
