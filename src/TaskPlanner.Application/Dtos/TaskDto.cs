namespace TaskPlanner.Application.Dtos;

public record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    DateTime DueDate,
    TaskPlanner.Domain.Entities.TaskStatus Status,
    Guid? AssignedMemberId,
    DateTime CreatedAt,
    DateTime? CompletedAt);
