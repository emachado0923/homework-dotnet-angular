using FluentAssertions;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Domain.Tests.Entities;

public class TaskItemTests
{
    [Fact]
    public void NewTaskItem_ShouldDefaultToPendingStatus()
    {
        var task = new TaskItem();

        task.Status.Should().Be(TaskItemStatus.Pending);
    }

    [Fact]
    public void MarkAsCompleted_ShouldSetStatusToCompleted()
    {
        var task = new TaskItem { Status = TaskItemStatus.InProgress };

        task.MarkAsCompleted();

        task.Status.Should().Be(TaskItemStatus.Completed);
    }

    [Fact]
    public void IsOverdue_ShouldReturnTrue_WhenDueDateIsInThePastAndNotCompleted()
    {
        var task = new TaskItem
        {
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = TaskItemStatus.Pending
        };

        task.IsOverdue().Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_ShouldReturnFalse_WhenDueDateIsInThePastButTaskIsCompleted()
    {
        var task = new TaskItem
        {
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = TaskItemStatus.Completed
        };

        task.IsOverdue().Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_ShouldReturnFalse_WhenDueDateIsInTheFuture()
    {
        var task = new TaskItem
        {
            DueDate = DateTime.UtcNow.AddDays(1),
            Status = TaskItemStatus.Pending
        };

        task.IsOverdue().Should().BeFalse();
    }

    [Fact]
    public void BelongsTo_ShouldReturnTrue_WhenUserIdMatches()
    {
        var userId = Guid.NewGuid();
        var task = new TaskItem { UserId = userId };

        task.BelongsTo(userId).Should().BeTrue();
    }

    [Fact]
    public void BelongsTo_ShouldReturnFalse_WhenUserIdDoesNotMatch()
    {
        var task = new TaskItem { UserId = Guid.NewGuid() };

        task.BelongsTo(Guid.NewGuid()).Should().BeFalse();
    }
}
