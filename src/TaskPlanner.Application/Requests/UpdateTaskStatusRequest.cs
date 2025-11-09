namespace TaskPlanner.Application.Requests;

public record UpdateTaskStatusRequest(Guid TaskId, TaskPlanner.Domain.Entities.TaskStatus Status);
