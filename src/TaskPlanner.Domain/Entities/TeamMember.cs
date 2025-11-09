using TaskPlanner.Domain.Exceptions;
using TaskPlanner.Domain.ValueObjects;

namespace TaskPlanner.Domain.Entities;

public class TeamMember
{
    public Guid Id { get; }
    public string FullName { get; private set; }
    public Email Email { get; private set; }
    public DateTime JoinedAt { get; }

    private TeamMember(Guid id, string fullName, Email email, DateTime joinedAt)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        FullName = ValidateName(fullName);
        Email = email;
        JoinedAt = joinedAt;
    }

    public static TeamMember Create(string fullName, Email email)
    {
        return new TeamMember(Guid.NewGuid(), fullName, email, DateTime.UtcNow);
    }

    public static TeamMember Restore(Guid id, string fullName, Email email, DateTime joinedAt)
    {
        return new TeamMember(id, fullName, email, joinedAt);
    }

    public void Rename(string fullName)
    {
        FullName = ValidateName(fullName);
    }

    public void UpdateEmail(Email email)
    {
        Email = email;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Member name must be specified.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 3)
        {
            throw new DomainException("Member name must contain at least three characters.");
        }

        return trimmed;
    }
}
