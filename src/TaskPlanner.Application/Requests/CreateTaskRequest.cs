namespace TaskPlanner.Application.Requests;

public record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string Description,
    DateTime DueDate,
    Guid? AssignedMemberId = null);
