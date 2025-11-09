using TaskPlanner.Domain.Entities;

namespace TaskPlanner.Application.Abstractions;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}
