namespace TaskReminder.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public RepeatType RepeatType { get; set; }
    public int RepeatInterval { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? LastReminderTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum RepeatType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}
