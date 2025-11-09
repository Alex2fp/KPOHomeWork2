using TaskPlanner.Domain.Entities;
using DomainTaskStatus = TaskPlanner.Domain.Entities.TaskStatus;
using TaskPlanner.Domain.ValueObjects;

namespace TaskPlanner.Infrastructure.Persistence;

internal static class EntityMapper
{
    public static ProjectRecord ToRecord(Project project) =>
        new(project.Id, project.Name, project.Description, project.CreatedAt, project.MemberIds.ToList());

    public static Project ToEntity(ProjectRecord record) =>
        Project.Restore(record.Id, record.Name, record.Description, record.MemberIds, record.CreatedAt);

    public static TaskRecord ToRecord(TaskItem task) =>
        new(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.DueDate,
            (int)task.Status,
            task.AssignedMemberId,
            task.CreatedAt,
            task.CompletedAt);

    public static TaskItem ToEntity(TaskRecord record) =>
        TaskItem.Restore(
            record.Id,
            record.ProjectId,
            record.Title,
            record.Description,
            record.DueDate,
            (DomainTaskStatus)record.Status,
            record.AssignedMemberId,
            record.CreatedAt,
            record.CompletedAt);

    public static MemberRecord ToRecord(TeamMember member) =>
        new(member.Id, member.FullName, member.Email.Value, member.JoinedAt);

    public static TeamMember ToEntity(MemberRecord record)
    {
        var email = Email.Create(record.Email);
        return TeamMember.Restore(record.Id, record.FullName, email, record.JoinedAt);
    }
}
