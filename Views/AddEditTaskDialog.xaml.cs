using System.Windows;
using TaskReminder.Models;

namespace TaskReminder.Views;

/// <summary>
/// AddEditTaskDialog.xaml 的交互逻辑
/// </summary>
public partial class AddEditTaskDialog : Window
{
    private readonly TaskItem? _existingTask;
    private bool _isEditMode;

    public AddEditTaskDialog(TaskItem? existingTask = null)
    {
        _existingTask = existingTask;
        _isEditMode = existingTask != null;
        InitializeComponent();
        InitializeTimePickers();
        LoadTaskData();
    }

    /// <summary>
    /// Result task after dialog closes
    /// </summary>
    public TaskItem? ResultTask { get; private set; }

    private void InitializeTimePickers()
    {
        // 小时
        for (int i = 0; i < 24; i++)
        {
            HourComboBox.Items.Add(i.ToString("D2"));
        }
        HourComboBox.SelectedIndex = 9; // 默认 9 点

        // 分钟
        for (int i = 0; i < 60; i += 5)
        {
            MinuteComboBox.Items.Add(i.ToString("D2"));
        }
        MinuteComboBox.SelectedIndex = 0; // 默认 0 分
    }

    private void LoadTaskData()
    {
        if (_isEditMode && _existingTask != null)
        {
            TitleText.Text = "编辑任务";
            TitleTextBox.Text = _existingTask.Title;
            DescriptionTextBox.Text = _existingTask.Description;
            DueDatePicker.SelectedDate = _existingTask.DueDate.Date;
            
            // 设置时间
            HourComboBox.SelectedIndex = _existingTask.DueDate.Hour;
            MinuteComboBox.SelectedIndex = _existingTask.DueDate.Minute / 5;

            // 设置重复类型
            RepeatTypeComboBox.SelectedIndex = (int)_existingTask.RepeatType;
            RepeatIntervalTextBox.Text = _existingTask.RepeatInterval.ToString();
        }
        else
        {
            // 新建模式：默认当前日期，9 点
            DueDatePicker.SelectedDate = DateTime.Today;
            RepeatTypeComboBox.SelectedIndex = 0;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // 验证必填字段
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("请输入任务标题", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            TitleTextBox.Focus();
            return;
        }

        if (!DueDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("请选择到期时间", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 构建任务对象
        var dueDate = DueDatePicker.SelectedDate.Value;
        var hour = int.Parse(HourComboBox.SelectedItem?.ToString() ?? "09");
        var minute = int.Parse(MinuteComboBox.SelectedItem?.ToString() ?? "00");
        
        dueDate = dueDate.AddHours(hour).AddMinutes(minute);

        var repeatTypeIndex = RepeatTypeComboBox.SelectedIndex;
        if (repeatTypeIndex < 0) repeatTypeIndex = 0;

        ResultTask = new TaskItem
        {
            Id = _existingTask?.Id ?? 0,
            Title = TitleTextBox.Text.Trim(),
            Description = DescriptionTextBox?.Text?.Trim() ?? string.Empty,
            DueDate = dueDate,
            RepeatType = (RepeatType)repeatTypeIndex,
            RepeatInterval = int.TryParse(RepeatIntervalTextBox.Text, out var interval) ? interval : 1,
            IsCompleted = _existingTask?.IsCompleted ?? false,
            LastReminderTime = _existingTask?.LastReminderTime,
            CreatedAt = _existingTask?.CreatedAt ?? DateTime.Now
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
