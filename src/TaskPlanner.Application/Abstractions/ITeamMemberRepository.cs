using TaskPlanner.Domain.Entities;

namespace TaskPlanner.Application.Abstractions;

public interface ITeamMemberRepository
{
    Task<TeamMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamMember?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TeamMember>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TeamMember member, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeamMember member, CancellationToken cancellationToken = default);
}
