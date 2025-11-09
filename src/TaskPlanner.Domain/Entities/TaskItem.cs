using TaskPlanner.Domain.Exceptions;

namespace TaskPlanner.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; }
    public Guid ProjectId { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime DueDate { get; private set; }
    public TaskStatus Status { get; private set; }
    public Guid? AssignedMemberId { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? CompletedAt { get; private set; }

    private TaskItem(
        Guid id,
        Guid projectId,
        string title,
        string description,
        DateTime dueDate,
        TaskStatus status,
        Guid? assignedMemberId,
        DateTime createdAt,
        DateTime? completedAt)
    {
        if (projectId == Guid.Empty)
        {
            throw new DomainException("Task must belong to a project.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ProjectId = projectId;
        Title = ValidateTitle(title);
        Description = description?.Trim() ?? string.Empty;
        DueDate = ValidateDueDate(dueDate, createdAt);
        Status = status;
        AssignedMemberId = assignedMemberId;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
    }

    public static TaskItem Create(Guid projectId, string title, string description, DateTime dueDate)
    {
        var createdAt = DateTime.UtcNow;
        return new TaskItem(Guid.NewGuid(), projectId, title, description, dueDate, TaskStatus.Planned, null, createdAt, null);
    }

    public static TaskItem Restore(
        Guid id,
        Guid projectId,
        string title,
        string description,
        DateTime dueDate,
        TaskStatus status,
        Guid? assignedMemberId,
        DateTime createdAt,
        DateTime? completedAt)
    {
        return new TaskItem(id, projectId, title, description, dueDate, status, assignedMemberId, createdAt, completedAt);
    }

    public void UpdateDetails(string title, string description, DateTime dueDate)
    {
        EnsureNotArchived();
        Title = ValidateTitle(title);
        Description = description?.Trim() ?? string.Empty;
        DueDate = ValidateDueDate(dueDate, CreatedAt);
    }

    public void ChangeStatus(TaskStatus status)
    {
        if (Status == TaskStatus.Archived && status != TaskStatus.Archived)
        {
            throw new DomainException("Archived tasks cannot change their status.");
        }

        if (status == TaskStatus.Completed)
        {
            CompletedAt = DateTime.UtcNow;
        }

        Status = status;
    }

    public void AssignTo(Guid memberId)
    {
        EnsureNotArchived();

        if (memberId == Guid.Empty)
        {
            throw new DomainException("Member identifier must be provided.");
        }

        AssignedMemberId = memberId;
    }

    public void RemoveAssignment()
    {
        EnsureNotArchived();
        AssignedMemberId = null;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Task title must be specified.");
        }

        return title.Trim();
    }

    private static DateTime ValidateDueDate(DateTime dueDate, DateTime createdAt)
    {
        if (dueDate.Date < createdAt.Date)
        {
            throw new DomainException("Task due date cannot be in the past.");
        }

        return dueDate;
    }

    private void EnsureNotArchived()
    {
        if (Status == TaskStatus.Archived)
        {
            throw new DomainException("Archived tasks cannot be updated.");
        }
    }
}
