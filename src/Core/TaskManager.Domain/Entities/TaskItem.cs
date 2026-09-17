namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public DateTime DueDate { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void MarkAsCompleted() => Status = TaskItemStatus.Completed;

    public void MarkAsInProgress() => Status = TaskItemStatus.InProgress;

    public void MarkAsPending() => Status = TaskItemStatus.Pending;

    public bool IsOverdue() => Status != TaskItemStatus.Completed && DueDate < DateTime.UtcNow;

    public bool BelongsTo(Guid userId) => UserId == userId;
}
