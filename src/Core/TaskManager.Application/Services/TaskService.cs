using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<IReadOnlyList<TaskDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await taskRepository.GetAllByUserIdAsync(userId, cancellationToken);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<Result<TaskDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Result<TaskDto>.Failure("Task not found.", ResultError.NotFound);
        }

        if (!task.BelongsTo(userId))
        {
            return Result<TaskDto>.Failure("You do not have access to this task.", ResultError.Unauthorized);
        }

        return Result<TaskDto>.Success(ToDto(task));
    }

    public async Task<Result<TaskDto>> CreateAsync(Guid userId, CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            UserId = userId
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskDto>.Success(ToDto(task));
    }

    public async Task<Result<TaskDto>> UpdateAsync(Guid userId, Guid taskId, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Result<TaskDto>.Failure("Task not found.", ResultError.NotFound);
        }

        if (!task.BelongsTo(userId))
        {
            return Result<TaskDto>.Failure("You do not have access to this task.", ResultError.Unauthorized);
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.DueDate = dto.DueDate;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskDto>.Success(ToDto(task));
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Result<bool>.Failure("Task not found.", ResultError.NotFound);
        }

        if (!task.BelongsTo(userId))
        {
            return Result<bool>.Failure("You do not have access to this task.", ResultError.Unauthorized);
        }

        taskRepository.Delete(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static TaskDto ToDto(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate,
        task.CreatedAt,
        task.IsOverdue());
}
