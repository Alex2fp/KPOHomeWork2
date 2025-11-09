namespace TaskPlanner.Application.Dtos;

public record TeamMemberDto(Guid Id, string FullName, string Email, DateTime JoinedAt);
