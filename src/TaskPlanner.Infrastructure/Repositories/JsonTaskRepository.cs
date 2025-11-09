using TaskPlanner.Application.Abstractions;
using TaskPlanner.Domain.Entities;
using TaskPlanner.Infrastructure.Persistence;

namespace TaskPlanner.Infrastructure.Repositories;

public class JsonTaskRepository : ITaskRepository
{
    private readonly JsonFileContext _context;

    public JsonTaskRepository(JsonFileContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        var record = data.Tasks.FirstOrDefault(t => t.Id == id);
        return record is null ? null : EntityMapper.ToEntity(record);
    }

    public async Task<IReadOnlyCollection<TaskItem>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        return data.Tasks.Where(t => t.ProjectId == projectId).Select(EntityMapper.ToEntity).ToList();
    }

    public async Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        return data.Tasks.Select(EntityMapper.ToEntity).ToList();
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        data.Tasks.RemoveAll(t => t.Id == task.Id);
        data.Tasks.Add(EntityMapper.ToRecord(task));
        await _context.WriteAsync(data);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        data.Tasks.RemoveAll(t => t.Id == task.Id);
        data.Tasks.Add(EntityMapper.ToRecord(task));
        await _context.WriteAsync(data);
    }
}
