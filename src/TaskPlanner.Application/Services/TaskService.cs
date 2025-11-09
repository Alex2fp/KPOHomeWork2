using TaskPlanner.Application.Abstractions;
using TaskPlanner.Application.Common;
using TaskPlanner.Application.Dtos;
using TaskPlanner.Application.Requests;
using TaskPlanner.Domain.Entities;
using DomainTaskStatus = TaskPlanner.Domain.Entities.TaskStatus;

namespace TaskPlanner.Application.Services;

public class TaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITeamMemberRepository _memberRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ITeamMemberRepository memberRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
    }

    public async Task<OperationResult<TaskDto>> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return OperationResult<TaskDto>.Fail($"Project '{request.ProjectId}' does not exist.");
        }

        if (request.AssignedMemberId is Guid memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken);
            if (member is null)
            {
                return OperationResult<TaskDto>.Fail($"Member '{memberId}' does not exist.");
            }

            if (!project.MemberIds.Contains(memberId))
            {
                return OperationResult<TaskDto>.Fail("Member must belong to the project before assignment.");
            }
        }

        try
        {
            var task = TaskItem.Create(request.ProjectId, request.Title, request.Description, request.DueDate);
            if (request.AssignedMemberId is Guid memberIdToAssign)
            {
                task.AssignTo(memberIdToAssign);
            }

            await _taskRepository.AddAsync(task, cancellationToken);
            return OperationResult<TaskDto>.Ok(ToDto(task));
        }
        catch (Exception ex)
        {
            return OperationResult<TaskDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<TaskDto>> UpdateStatusAsync(UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return OperationResult<TaskDto>.Fail($"Task '{request.TaskId}' does not exist.");
        }

        try
        {
            task.ChangeStatus(request.Status);
            await _taskRepository.UpdateAsync(task, cancellationToken);
            return OperationResult<TaskDto>.Ok(ToDto(task));
        }
        catch (Exception ex)
        {
            return OperationResult<TaskDto>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult> AssignTaskAsync(AssignTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return OperationResult.Fail($"Task '{request.TaskId}' does not exist.");
        }

        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
        {
            return OperationResult.Fail($"Member '{request.MemberId}' does not exist.");
        }

        var project = await _projectRepository.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null)
        {
            return OperationResult.Fail("Task project was not found.");
        }

        if (!project.MemberIds.Contains(member.Id))
        {
            return OperationResult.Fail("Member must be attached to the project before assignment.");
        }

        try
        {
            task.AssignTo(member.Id);
            await _taskRepository.UpdateAsync(task, cancellationToken);
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyCollection<TaskDto>> GetTasksByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetByProjectAsync(projectId, cancellationToken);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyCollection<UpcomingTaskDto>> GetUpcomingTasksAsync(DateTime until, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        var members = await _memberRepository.GetAllAsync(cancellationToken);

        var projectLookup = projects.ToDictionary(p => p.Id, p => p.Name);
        var memberLookup = members.ToDictionary(m => m.Id, m => m.FullName);

        return tasks
            .Where(t => t.DueDate <= until && t.Status is DomainTaskStatus.Planned or DomainTaskStatus.InProgress)
            .OrderBy(t => t.DueDate)
            .Select(t =>
            {
                projectLookup.TryGetValue(t.ProjectId, out var projectName);
                string? assignedTo = null;
                if (t.AssignedMemberId is Guid memberId && memberLookup.TryGetValue(memberId, out var memberName))
                {
                    assignedTo = memberName;
                }

                return new UpcomingTaskDto(t.Id, t.Title, t.DueDate, t.Status, projectName ?? "Unknown project", assignedTo);
            })
            .ToList();
    }

    private static TaskDto ToDto(TaskItem task)
    {
        return new TaskDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.DueDate,
            task.Status,
            task.AssignedMemberId,
            task.CreatedAt,
            task.CompletedAt);
    }
}
