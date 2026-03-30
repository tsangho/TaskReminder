using System;
using System.Threading;
using TaskReminder.Data;
using TaskReminder.Models;

namespace TaskReminder.Services;

/// <summary>
/// 定时任务检查器
/// 使用 System.Threading.Timer 后台服务，每分钟检查一次到期任务并触发通知
/// </summary>
public class ReminderChecker : IDisposable
{
    private readonly Timer _timer;
    private readonly DatabaseService _databaseService;
    private readonly NotificationService _notificationService;
    private readonly object _lockObject = new object();
    private bool _disposed;
    private bool _isRunning;

    // 检查间隔：60秒 = 1分钟
    private const int CHECK_INTERVAL_MS = 60000;

    public ReminderChecker(DatabaseService databaseService, NotificationService notificationService)
    {
        _databaseService = databaseService;
        _notificationService = notificationService;

        // 创建定时器：使用 System.Threading.Timer
        // 参数：callback, state, dueTime, period
        // dueTime: 首次执行前等待时间（0表示立即执行）
        // period: 后续执行间隔
        _timer = new Timer(
            CheckRemindersCallback,  // 回调函数
            null,                     // 传递给回调的对象
            Timeout.Infinite,         // 首次执行前等待时间（稍后手动启动）
            CHECK_INTERVAL_MS         // 周期（毫秒）
        );

        Console.WriteLine($"[ReminderChecker] 定时器已创建，检查间隔: {CHECK_INTERVAL_MS}ms");
    }

    /// <summary>
    /// 启动定时器
    /// </summary>
    public void Start()
    {
        lock (_lockObject)
        {
            // 设置周期为60秒，开始周期性检查
            _timer.Change(0, CHECK_INTERVAL_MS);
            Console.WriteLine("[ReminderChecker] 定时检查已启动");
        }
    }

    /// <summary>
    /// 停止定时器
    /// </summary>
    public void Stop()
    {
        lock (_lockObject)
        {
            // 将周期设置为 Infinite，停止自动触发
            _timer.Change(Timeout.Infinite, CHECK_INTERVAL_MS);
            Console.WriteLine("[ReminderChecker] 定时检查已停止");
        }
    }

    /// <summary>
    /// 定时器回调函数
    /// </summary>
    private void CheckRemindersCallback(object? state)
    {
        // 防止并发执行
        if (_isRunning)
        {
            Console.WriteLine("[ReminderChecker] 上一次检查尚未完成，跳过本次检查");
            return;
        }

        _isRunning = true;

        try
        {
            // 在线程池线程上执行异步检查
            CheckRemindersAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReminderChecker] 检查时出错: {ex.Message}");
        }
        finally
        {
            _isRunning = false;
        }
    }

    /// <summary>
    /// 检查到期任务
    /// </summary>
    private async Task CheckRemindersAsync()
    {
        try
        {
            var now = DateTime.Now;
            Console.WriteLine($"[ReminderChecker] 开始检查... ({now:HH:mm:ss})");

            var tasks = await _databaseService.GetAllAsync();
            var notifiedCount = 0;

            foreach (var task in tasks)
            {
                // 跳过已完成的任务
                if (task.IsCompleted)
                    continue;

                // 检查是否到期：DueTime <= Now && Status == Pending
                // 由于我们使用 IsCompleted 作为状态标志，检查 DueDate <= Now 即可
                if (task.DueDate <= now)
                {
                    // 检查是否已发送过提醒（避免重复通知）
                    // 如果过去 60 分钟内已发送过，则跳过
                    if (task.LastReminderTime.HasValue && 
                        (now - task.LastReminderTime.Value).TotalMinutes < 60)
                    {
                        Console.WriteLine($"[ReminderChecker] 任务 \"{task.Title}\" 最近已提醒过，跳过");
                        continue;
                    }

                    // 发送 Windows Toast 通知
                    _notificationService.ShowTaskReminder(
                        task.Title,
                        task.Description,
                        task.Id);

                    // 更新最后提醒时间
                    await UpdateLastReminderTimeAsync(task);
                    notifiedCount++;

                    // 如果是重复任务，计算并更新下次到期时间
                    if (task.RepeatType != RepeatType.None)
                    {
                        await ScheduleNextOccurrenceAsync(task);
                    }
                }
            }

            if (notifiedCount > 0)
            {
                Console.WriteLine($"[ReminderChecker] ✅ 发送了 {notifiedCount} 个任务提醒");
            }
            else
            {
                Console.WriteLine($"[ReminderChecker] 没有需要提醒的任务");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReminderChecker] ❌ 检查提醒时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新任务最后提醒时间
    /// </summary>
    private async Task UpdateLastReminderTimeAsync(TaskItem task)
    {
        try
        {
            task.LastReminderTime = DateTime.Now;
            await _databaseService.UpdateAsync(task);
            Console.WriteLine($"[ReminderChecker] 已更新任务 \"{task.Title}\" 的最后提醒时间");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReminderChecker] 更新任务 {task.Title} 的提醒时间失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据重复类型计算并更新下次到期时间
    /// </summary>
    private async Task ScheduleNextOccurrenceAsync(TaskItem task)
    {
        try
        {
            var nextDueDate = task.DueDate;
            var interval = task.RepeatInterval > 0 ? task.RepeatInterval : 1;

            switch (task.RepeatType)
            {
                case RepeatType.Daily:
                    nextDueDate = task.DueDate.AddDays(interval);
                    break;
                case RepeatType.Weekly:
                    nextDueDate = task.DueDate.AddDays(7 * interval);
                    break;
                case RepeatType.Monthly:
                    nextDueDate = task.DueDate.AddMonths(interval);
                    break;
                case RepeatType.Quarterly:
                    nextDueDate = task.DueDate.AddMonths(3 * interval);
                    break;
                case RepeatType.Yearly:
                    nextDueDate = task.DueDate.AddYears(interval);
                    break;
                default:
                    // 非重复任务，不需要处理
                    return;
            }

            task.DueDate = nextDueDate;
            task.LastReminderTime = null; // 重置提醒时间
            task.IsCompleted = false;     // 重置完成状态
            
            await _databaseService.UpdateAsync(task);
            Console.WriteLine($"[ReminderChecker] 已为任务 \"{task.Title}\" 安排下次提醒: {nextDueDate:yyyy-MM-dd HH:mm}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReminderChecker] 更新任务 {task.Title} 的下次到期时间失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 手动触发一次检查（用于测试）
    /// </summary>
    public async Task TriggerCheckNowAsync()
    {
        Console.WriteLine("[ReminderChecker] 手动触发检查...");
        await CheckRemindersAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lockObject)
            {
                _timer?.Dispose();
                Console.WriteLine("[ReminderChecker] 定时器已释放");
            }
            _disposed = true;
        }
    }
}
