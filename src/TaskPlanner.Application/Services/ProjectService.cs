using TaskPlanner.Application.Abstractions;
using TaskPlanner.Application.Common;
using TaskPlanner.Application.Dtos;
using TaskPlanner.Application.Requests;
using TaskPlanner.Domain.Entities;

namespace TaskPlanner.Application.Services;

public class ProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITeamMemberRepository _memberRepository;

    public ProjectService(IProjectRepository projectRepository, ITeamMemberRepository memberRepository)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
    }

    public async Task<OperationResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = Project.Create(request.Name, request.Description);
            await _projectRepository.AddAsync(project, cancellationToken);
            return OperationResult<ProjectDto>.Ok(ToDto(project));
        }
        catch (Exception ex)
        {
            return OperationResult<ProjectDto>.Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyCollection<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return projects.Select(ToDto).ToList();
    }

    public async Task<OperationResult<ProjectDto>> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project is null
            ? OperationResult<ProjectDto>.Fail($"Project '{id}' was not found.")
            : OperationResult<ProjectDto>.Ok(ToDto(project));
    }

    public async Task<OperationResult> AttachMemberAsync(Guid projectId, Guid memberId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            return OperationResult.Fail($"Project '{projectId}' does not exist.");
        }

        var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member is null)
        {
            return OperationResult.Fail($"Member '{memberId}' does not exist.");
        }

        project.AddMember(member.Id);
        await _projectRepository.UpdateAsync(project, cancellationToken);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> DetachMemberAsync(Guid projectId, Guid memberId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            return OperationResult.Fail($"Project '{projectId}' does not exist.");
        }

        project.RemoveMember(memberId);
        await _projectRepository.UpdateAsync(project, cancellationToken);
        return OperationResult.Ok();
    }

    private static ProjectDto ToDto(Project project)
    {
        return new ProjectDto(project.Id, project.Name, project.Description, project.CreatedAt, project.MemberIds.ToArray());
    }
}
