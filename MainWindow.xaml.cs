using System.Windows;
using System.Windows.Controls;
using TaskReminder.Data;
using TaskReminder.Models;
using TaskReminder.ViewModels;
using TaskReminder.Views;
using TaskReminder.Services;

namespace TaskReminder;

public partial class MainWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly TaskViewModel _viewModel;
    private readonly TrayIconService _trayIconService;
    private readonly NotificationService _notificationService;
    private readonly ReminderChecker _reminderChecker;
    private bool _isClosing = false;

    public MainWindow()
    {
        InitializeComponent();

        // 初始化数据库
        _databaseService = new DatabaseService("tasks.db");
        _ = InitializeDatabaseAsync();

        // 初始化 ViewModel
        _viewModel = new TaskViewModel();
        DataContext = _viewModel;

        // 初始化通知服务
        _notificationService = new NotificationService();

        // 初始化托盘图标服务
        _trayIconService = new TrayIconService();
        _trayIconService.Initialize(this);

        // 初始化定时检查器
        _reminderChecker = new ReminderChecker(_databaseService, _notificationService);
        _reminderChecker.Start();

        // 注册窗口关闭事件
        Closing += MainWindow_Closing;

        // 加载任务列表
        _ = LoadTasksAsync();
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // 如果不是主动退出，则最小化到托盘
        if (!_isClosing && e != null)
        {
            e.Cancel = true;
            _trayIconService.MinimizeToTray();
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            await _databaseService.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"数据库初始化失败:{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            await _viewModel.LoadTasksAsync();
            TaskListView.ItemsSource = _viewModel.Tasks;
            UpdateTaskCount();
            UpdateStatus(_viewModel.StatusMessage);
        }
        catch (Exception ex)
        {
            UpdateStatus($"加载失败:{ex.Message}");
        }
    }

    private void UpdateStatus(string message)
    {
        StatusText.Text = message;
    }

    private void UpdateTaskCount()
    {
        var count = _viewModel.Tasks?.Count ?? 0;
        TaskCountText.Text = $"共 {count} 个任务";
    }

    /// <summary>
    /// 添加新任务
    /// </summary>
    private async void AddTask_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEditTaskDialog();
        dialog.Owner = this;

        if (dialog.ShowDialog() == true)
        {
            if (dialog.ResultTask != null)
            {
                try
                {
                    await _viewModel.AddTaskAsync(dialog.ResultTask);
                    UpdateTaskCount();
                    UpdateStatus(_viewModel.StatusMessage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"添加任务失败:{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// 编辑选中的任务
    /// </summary>
    private async void EditTask_Click(object sender, RoutedEventArgs e)
    {
        if (TaskListView.SelectedItem is TaskItem selectedTask)
        {
            var dialog = new AddEditTaskDialog(selectedTask);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                if (dialog.ResultTask != null)
                {
                    try
                    {
                        await _viewModel.UpdateTaskAsync(dialog.ResultTask);
                        UpdateTaskCount();
                        UpdateStatus(_viewModel.StatusMessage);
                        ShowTaskDetail(dialog.ResultTask);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"更新任务失败:{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    private async void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        TaskItem? taskToDelete = null;

        // 如果是按钮点击,从 CommandParameter 获取
        if (sender is Button button && button.CommandParameter is TaskItem taskParam)
        {
            taskToDelete = taskParam;
        }
        // 否则使用选中的任务
        else if (TaskListView.SelectedItem is TaskItem selectedTask)
        {
            taskToDelete = selectedTask;
        }

        if (taskToDelete != null)
        {
            var result = MessageBox.Show(
                $"确定要删除任务 \"{taskToDelete.Title}\" 吗?",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _viewModel.DeleteTaskAsync(taskToDelete);
                    UpdateTaskCount();
                    UpdateStatus(_viewModel.StatusMessage);
                    ClearDetail();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除任务失败:{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// 刷新任务列表
    /// </summary>
    private async void RefreshTasks_Click(object sender, RoutedEventArgs e)
    {
        await LoadTasksAsync();
    }

    /// <summary>
    /// ListView 选择变化
    /// </summary>
    private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TaskListView.SelectedItem is TaskItem selectedTask)
        {
            ShowTaskDetail(selectedTask);
            EditButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
        }
        else
        {
            ClearDetail();
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }
    }

    /// <summary>
    /// 显示任务详情
    /// </summary>
    private void ShowTaskDetail(TaskItem task)
    {
        DetailContent.Children.Clear();

        var stackPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

        // 标题
        var titleBlock = new TextBlock
        {
            Text = task.Title,
            Style = (Style)FindResource("MaterialDesignHeadline6TextBlock"),
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        };
        stackPanel.Children.Add(titleBlock);

        // 描述
        if (!string.IsNullOrWhiteSpace(task.Description))
        {
            var descBlock = new TextBlock
            {
                Text = task.Description,
                Style = (Style)FindResource("MaterialDesignBody1TextBlock"),
                Opacity = 0.8,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 12, 0, 0)
            };
            stackPanel.Children.Add(descBlock);
        }

        // 到期时间
        var dueDatePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 0) };
        var clockIcon = new materialDesign:PackIcon { Kind = materialDesign:PackIconKind.ClockOutline, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center };
        var dueDateText = new TextBlock
        {
            Text = $"到期时间:{task.DueDate:yyyy-MM-dd HH:mm}",
            Style = (Style)FindResource("MaterialDesignBody1TextBlock"),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        dueDatePanel.Children.Add(clockIcon);
        dueDatePanel.Children.Add(dueDateText);
        stackPanel.Children.Add(dueDatePanel);

        // 重复类型
        var repeatPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        var repeatIcon = new materialDesign:PackIcon { Kind = materialDesign:PackIconKind.Repeat, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center };
        var repeatText = new TextBlock
        {
            Text = $"重复:{task.RepeatType} (每{task.RepeatInterval}个单位)",
            Style = (Style)FindResource("MaterialDesignBody1TextBlock"),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        repeatPanel.Children.Add(repeatIcon);
        repeatPanel.Children.Add(repeatText);
        stackPanel.Children.Add(repeatPanel);

        // 状态
        var statusPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        var statusIcon = new materialDesign:PackIcon
        {
            Kind = task.IsCompleted ? materialDesign:PackIconKind.CheckCircle : materialDesign:PackIconKind.CircleOutline,
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center
        };
        var statusText = new TextBlock
        {
            Text = $"状态:{(task.IsCompleted ? "已完成" : "未完成")}",
            Style = (Style)FindResource("MaterialDesignBody1TextBlock"),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        statusPanel.Children.Add(statusIcon);
        statusPanel.Children.Add(statusText);
        stackPanel.Children.Add(statusPanel);

        // 创建时间
        var createdPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        var calendarIcon = new materialDesign:PackIcon { Kind = materialDesign:PackIconKind.CalendarPlus, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center };
        var createdText = new TextBlock
        {
            Text = $"创建时间:{task.CreatedAt:yyyy-MM-dd HH:mm}",
            Style = (Style)FindResource("MaterialDesignBody1TextBlock"),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.6
        };
        createdPanel.Children.Add(calendarIcon);
        createdPanel.Children.Add(createdText);
        stackPanel.Children.Add(createdPanel);

        DetailContent.Children.Add(stackPanel);
    }

    private void ClearDetail()
    {
        DetailContent.Children.Clear();
        var noSelectionText = new TextBlock
        {
            Text = "未选择任务",
            Style = (Style)FindResource("MaterialDesignBody1TextBlock"),
            Opacity = 0.5,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        DetailContent.Children.Add(noSelectionText);
    }

    /// <summary>
    /// 从托盘图标显示主界面
    /// </summary>
    public void ShowFromTray()
    {
        _trayIconService.RestoreFromTray();
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _trayIconService?.Dispose();
        _reminderChecker?.Stop();
        _reminderChecker?.Dispose();
        base.OnClosed(e);
    }
}
