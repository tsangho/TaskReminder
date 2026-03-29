using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskReminder.Data;
using TaskReminder.Models;

namespace TaskReminder.ViewModels;

/// <summary>
/// Main ViewModel for TaskReminder - implements MVVM pattern
/// </summary>
public class TaskViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<TaskItem> _tasks;
    private TaskItem? _selectedTask;
    private bool _isLoading;
    private string? _statusMessage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TaskViewModel()
    {
        _databaseService = new DatabaseService("tasks.db");
        _tasks = new ObservableCollection<TaskItem>();
        _ = LoadTasksAsync();
    }

    /// <summary>
    /// Collection of tasks - automatically notifies UI of changes
    /// </summary>
    public ObservableCollection<TaskItem> Tasks
    {
        get => _tasks;
        set
        {
            _tasks = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Currently selected task in the ListView
    /// </summary>
    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            _selectedTask = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanEditTask));
            OnPropertyChanged(nameof(CanDeleteTask));
        }
    }

    public bool CanEditTask => SelectedTask != null;
    public bool CanDeleteTask => SelectedTask != null;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Load all tasks from database
    /// </summary>
    public async Task LoadTasksAsync()
    {
        IsLoading = true;
        try
        {
            var tasks = await _databaseService.GetAllAsync();
            Tasks = new ObservableCollection<TaskItem>(tasks);
            StatusMessage = $"已加载 {Tasks.Count} 个任务";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败：{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Add a new task
    /// </summary>
    public async Task AddTaskAsync(TaskItem task)
    {
        try
        {
            task.CreatedAt = DateTime.Now;
            var id = await _databaseService.InsertAsync(task);
            task.Id = id;
            Tasks.Add(task);
            StatusMessage = $"任务已添加：{task.Title}";
            OnPropertyChanged(nameof(CanEditTask));
            OnPropertyChanged(nameof(CanDeleteTask));
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加失败：{ex.Message}";
            throw;
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    public async Task UpdateTaskAsync(TaskItem task)
    {
        try
        {
            await _databaseService.UpdateAsync(task);
            // Refresh the item in the collection
            var index = Tasks.IndexOf(task);
            if (index >= 0)
            {
                Tasks[index] = task;
            }
            StatusMessage = $"任务已更新：{task.Title}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"更新失败：{ex.Message}";
            throw;
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    public async Task DeleteTaskAsync(TaskItem task)
    {
        try
        {
            await _databaseService.DeleteAsync(task.Id);
            Tasks.Remove(task);
            StatusMessage = $"任务已删除：{task.Title}";
            SelectedTask = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败：{ex.Message}";
            throw;
        }
    }

    /// <summary>
    /// Delete selected task
    /// </summary>
    public async Task DeleteSelectedTaskAsync()
    {
        if (SelectedTask != null)
        {
            await DeleteTaskAsync(SelectedTask);
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
