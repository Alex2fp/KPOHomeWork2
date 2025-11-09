using TaskPlanner.Application.Abstractions;
using TaskPlanner.Domain.Entities;
using TaskPlanner.Infrastructure.Persistence;

namespace TaskPlanner.Infrastructure.Repositories;

public class JsonProjectRepository : IProjectRepository
{
    private readonly JsonFileContext _context;

    public JsonProjectRepository(JsonFileContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        var record = data.Projects.FirstOrDefault(p => p.Id == id);
        return record is null ? null : EntityMapper.ToEntity(record);
    }

    public async Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        return data.Projects.Select(EntityMapper.ToEntity).ToList();
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        data.Projects.RemoveAll(p => p.Id == project.Id);
        data.Projects.Add(EntityMapper.ToRecord(project));
        await _context.WriteAsync(data);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        data.Projects.RemoveAll(p => p.Id == project.Id);
        data.Projects.Add(EntityMapper.ToRecord(project));
        await _context.WriteAsync(data);
    }
}
