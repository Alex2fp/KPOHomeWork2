namespace TaskPlanner.Application.Dtos;

public record UpcomingTaskDto(
    Guid Id,
    string Title,
    DateTime DueDate,
    TaskPlanner.Domain.Entities.TaskStatus Status,
    string ProjectName,
    string? AssignedTo);
