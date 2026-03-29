using System;
using System.Timers;
using TaskReminder.Data;
using TaskReminder.Models;

namespace TaskReminder.Services;

/// <summary>
/// 定时任务检查器
/// 每分钟检查一次到期任务并触发通知
/// </summary>
public class ReminderChecker : IDisposable
{
    private readonly Timer _timer;
    private readonly DatabaseService _databaseService;
    private readonly NotificationService _notificationService;
    private bool _disposed;

    public ReminderChecker(DatabaseService databaseService, NotificationService notificationService)
    {
        _databaseService = databaseService;
        _notificationService = notificationService;

        // 创建定时器，每分钟检查一次
        _timer = new Timer(60000); // 60 秒 = 1 分钟
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    /// <summary>
    /// 启动定时器
    /// </summary>
    public void Start()
    {
        _timer.Enabled = true;
        // 启动时立即检查一次
        CheckReminders();
    }

    /// <summary>
    /// 停止定时器
    /// </summary>
    public void Stop()
    {
        _timer.Enabled = false;
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await CheckRemindersAsync();
    }

    /// <summary>
    /// 检查到期任务
    /// </summary>
    private async void CheckReminders()
    {
        await CheckRemindersAsync();
    }

    private async Task CheckRemindersAsync()
    {
        try
        {
            var now = DateTime.Now;
            var tasks = await _databaseService.GetAllTasksAsync();
            var notifiedCount = 0;

            foreach (var task in tasks)
            {
                // 跳过已完成的任务
                if (task.IsCompleted)
                    continue;

                // 检查是否到期
                if (task.DueDate <= now)
                {
                    // 检查是否已发送过提醒（过去 1 小时内不再重复）
                    if (task.LastReminderTime.HasValue && 
                        (now - task.LastReminderTime.Value).TotalMinutes < 60)
                    {
                        continue;
                    }

                    // 发送通知
                    _notificationService.ShowNotification(
                        "任务到期提醒",
                        $"任务 \"{task.Title}\" 已到期！\n{task.Description}");

                    // 更新最后提醒时间
                    await UpdateLastReminderTimeAsync(task);
                    notifiedCount++;
                }
            }

            if (notifiedCount > 0)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 发送了 {notifiedCount} 个任务提醒");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 检查提醒时出错：{ex.Message}");
        }
    }

    private async Task UpdateLastReminderTimeAsync(TaskItem task)
    {
        try
        {
            task.LastReminderTime = DateTime.Now;
            await _databaseService.UpdateTaskAsync(task);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新任务 {task.Title} 的提醒时间失败：{ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _timer?.Dispose();
            _disposed = true;
        }
    }
}
