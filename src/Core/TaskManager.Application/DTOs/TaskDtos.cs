using TaskManager.Domain.Entities;

namespace TaskManager.Application.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    DateTime DueDate,
    DateTime CreatedAt,
    bool IsOverdue);

public record CreateTaskDto(
    string Title,
    string Description,
    DateTime DueDate);

public record UpdateTaskDto(
    string Title,
    string Description,
    TaskItemStatus Status,
    DateTime DueDate);
