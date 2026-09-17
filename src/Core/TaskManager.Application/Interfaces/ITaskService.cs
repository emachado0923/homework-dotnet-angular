using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> CreateAsync(Guid userId, CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> UpdateAsync(Guid userId, Guid taskId, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
}
