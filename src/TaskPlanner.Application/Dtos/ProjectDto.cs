namespace TaskPlanner.Application.Dtos;

public record ProjectDto(Guid Id, string Name, string Description, DateTime CreatedAt, IReadOnlyCollection<Guid> MemberIds);
