using TaskPlanner.Domain.Exceptions;

namespace TaskPlanner.Domain.Entities;

public class Project
{
    private readonly HashSet<Guid> _memberIds = new();

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyCollection<Guid> MemberIds => _memberIds;
    public DateTime CreatedAt { get; }

    private Project(Guid id, string name, string description, IEnumerable<Guid> memberIds, DateTime createdAt)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = ValidateName(name);
        Description = description?.Trim() ?? string.Empty;
        CreatedAt = createdAt;

        foreach (var memberId in memberIds)
        {
            if (memberId != Guid.Empty)
            {
                _memberIds.Add(memberId);
            }
        }
    }

    public static Project Create(string name, string description)
    {
        var createdAt = DateTime.UtcNow;
        return new Project(Guid.NewGuid(), name, description, Array.Empty<Guid>(), createdAt);
    }

    public static Project Restore(Guid id, string name, string description, IEnumerable<Guid> memberIds, DateTime createdAt)
    {
        return new Project(id, name, description, memberIds, createdAt);
    }

    public void Rename(string name, string description)
    {
        Name = ValidateName(name);
        Description = description?.Trim() ?? string.Empty;
    }

    public void AddMember(Guid memberId)
    {
        if (memberId == Guid.Empty)
        {
            throw new DomainException("Cannot add a member with an empty identifier.");
        }

        _memberIds.Add(memberId);
    }

    public void RemoveMember(Guid memberId)
    {
        _memberIds.Remove(memberId);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Project name must be specified.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 3)
        {
            throw new DomainException("Project name should contain at least three characters.");
        }

        return trimmed;
    }
}
