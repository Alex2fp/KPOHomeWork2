using TaskPlanner.Domain.Entities;

namespace TaskPlanner.Application.Abstractions;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaskItem>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
}
