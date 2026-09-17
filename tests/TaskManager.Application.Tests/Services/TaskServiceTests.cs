using FluentAssertions;
using NSubstitute;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepository);
    }

    [Fact]
    public async Task GetAllForUserAsync_ShouldReturnOnlyTasksOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new() { Title = "A", UserId = userId },
            new() { Title = "B", UserId = userId }
        };
        _taskRepository.GetAllByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(tasks);

        var result = await _sut.GetAllForUserAsync(userId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTask_OwnedByRequestingUser()
    {
        var userId = Guid.NewGuid();
        var dto = new CreateTaskDto("Title", "Description", DateTime.UtcNow.AddDays(1));
        _taskRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.CreateAsync(userId, dto);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Title");
        await _taskRepository.Received(1).AddAsync(
            Arg.Is<TaskItem>(t => t.UserId == userId && t.Title == "Title"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);
        var dto = new UpdateTaskDto("Title", "Description", TaskItemStatus.InProgress, DateTime.UtcNow.AddDays(1));

        var result = await _sut.UpdateAsync(userId, taskId, dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUnauthorized_WhenTaskBelongsToAnotherUser()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = ownerId };
        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var dto = new UpdateTaskDto("Title", "Description", TaskItemStatus.InProgress, DateTime.UtcNow.AddDays(1));

        var result = await _sut.UpdateAsync(otherUserId, task.Id, dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Unauthorized);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenUserOwnsTask()
    {
        var userId = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = userId, Title = "Old" };
        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _taskRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(true);
        var dto = new UpdateTaskDto("New", "New Description", TaskItemStatus.Completed, DateTime.UtcNow.AddDays(2));

        var result = await _sut.UpdateAsync(userId, task.Id, dto);

        result.IsSuccess.Should().BeTrue();
        task.Title.Should().Be("New");
        task.Status.Should().Be(TaskItemStatus.Completed);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnUnauthorized_WhenTaskBelongsToAnotherUser()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = ownerId };
        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var result = await _sut.DeleteAsync(otherUserId, task.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Unauthorized);
        _taskRepository.DidNotReceive().Delete(Arg.Any<TaskItem>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTask_WhenUserOwnsTask()
    {
        var userId = Guid.NewGuid();
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = userId };
        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _taskRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.DeleteAsync(userId, task.Id);

        result.IsSuccess.Should().BeTrue();
        _taskRepository.Received(1).Delete(task);
    }
}
