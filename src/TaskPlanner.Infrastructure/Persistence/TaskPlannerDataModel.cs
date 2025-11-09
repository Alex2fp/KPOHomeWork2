namespace TaskPlanner.Infrastructure.Persistence;

internal class TaskPlannerDataModel
{
    public List<ProjectRecord> Projects { get; set; } = new();
    public List<TaskRecord> Tasks { get; set; } = new();
    public List<MemberRecord> Members { get; set; } = new();
}

internal record ProjectRecord(Guid Id, string Name, string Description, DateTime CreatedAt, List<Guid> MemberIds);

internal record TaskRecord(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    DateTime DueDate,
    int Status,
    Guid? AssignedMemberId,
    DateTime CreatedAt,
    DateTime? CompletedAt);

internal record MemberRecord(Guid Id, string FullName, string Email, DateTime JoinedAt);
