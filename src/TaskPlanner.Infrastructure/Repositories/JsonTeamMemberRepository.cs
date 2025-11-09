using TaskPlanner.Application.Abstractions;
using TaskPlanner.Domain.Entities;
using TaskPlanner.Infrastructure.Persistence;

namespace TaskPlanner.Infrastructure.Repositories;

public class JsonTeamMemberRepository : ITeamMemberRepository
{
    private readonly JsonFileContext _context;

    public JsonTeamMemberRepository(JsonFileContext context)
    {
        _context = context;
    }

    public async Task<TeamMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        var record = data.Members.FirstOrDefault(m => m.Id == id);
        return record is null ? null : EntityMapper.ToEntity(record);
    }

    public async Task<TeamMember?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        var record = data.Members.FirstOrDefault(m => string.Equals(m.Email, email, StringComparison.OrdinalIgnoreCase));
        return record is null ? null : EntityMapper.ToEntity(record);
    }

    public async Task<IReadOnlyCollection<TeamMember>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        return data.Members.Select(EntityMapper.ToEntity).ToList();
    }

    public async Task AddAsync(TeamMember member, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        data.Members.RemoveAll(m => m.Id == member.Id);
        data.Members.Add(EntityMapper.ToRecord(member));
        await _context.WriteAsync(data);
    }

    public async Task UpdateAsync(TeamMember member, CancellationToken cancellationToken = default)
    {
        var data = await _context.ReadAsync();
        data.Members.RemoveAll(m => m.Id == member.Id);
        data.Members.Add(EntityMapper.ToRecord(member));
        await _context.WriteAsync(data);
    }
}
