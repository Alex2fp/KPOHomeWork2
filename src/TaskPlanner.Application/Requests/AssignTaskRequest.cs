namespace TaskPlanner.Application.Requests;

public record AssignTaskRequest(Guid TaskId, Guid MemberId);
