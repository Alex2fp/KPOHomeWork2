using TaskPlanner.Application.Requests;
using TaskPlanner.Application.Services;
using TaskPlanner.Domain.Entities;
using TaskPlanner.Infrastructure.Persistence;
using TaskPlanner.Infrastructure.Repositories;
using Xunit;

namespace TaskPlanner.Tests;

public class ServiceTests : IDisposable
{
    private readonly List<string> _filesToCleanup = new();

    [Fact]
    public async Task CreateTask_PersistsAndReturnsInQueries()
    {
        var (projectService, taskService, memberService, path) = CreateServices();

        var memberResult = await memberService.RegisterMemberAsync(new RegisterMemberRequest("Иван Иванов", "ivan@example.com"));
        Assert.True(memberResult.Success);
        var memberId = memberResult.Value!.Id;

        var projectResult = await projectService.CreateProjectAsync(new CreateProjectRequest("CRM", "Система для отдела продаж"));
        Assert.True(projectResult.Success);
        var projectId = projectResult.Value!.Id;

        var attachResult = await projectService.AttachMemberAsync(projectId, memberId);
        Assert.True(attachResult.Success);

        var taskResult = await taskService.CreateTaskAsync(
            new CreateTaskRequest(projectId, "Настроить отчёты", "Создать набор аналитических отчётов", DateTime.UtcNow.AddDays(3), memberId));

        Assert.True(taskResult.Success);
        var tasks = await taskService.GetTasksByProjectAsync(projectId);
        Assert.Single(tasks);
        Assert.Equal("Настроить отчёты", tasks.First().Title);
        Assert.Equal(memberId, tasks.First().AssignedMemberId);
    }

    [Fact]
    public async Task CreateTask_WithPastDueDate_ReturnsError()
    {
        var (projectService, taskService, memberService, path) = CreateServices();

        var projectResult = await projectService.CreateProjectAsync(new CreateProjectRequest("Переезд", "Организация переезда офиса"));
        Assert.True(projectResult.Success);
        var projectId = projectResult.Value!.Id;

        var taskResult = await taskService.CreateTaskAsync(
            new CreateTaskRequest(projectId, "Упаковка", "Подготовить коробки", DateTime.UtcNow.AddDays(-1)));

        Assert.False(taskResult.Success);
        Assert.False(string.IsNullOrWhiteSpace(taskResult.Error));
    }

    [Fact]
    public async Task AssignTask_ToMemberNotInProject_ReturnsError()
    {
        var (projectService, taskService, memberService, path) = CreateServices();

        var memberResult = await memberService.RegisterMemberAsync(new RegisterMemberRequest("Мария Петрова", "maria@example.com"));
        Assert.True(memberResult.Success);
        var memberId = memberResult.Value!.Id;

        var projectResult = await projectService.CreateProjectAsync(new CreateProjectRequest("Релиз", "Подготовка релиза"));
        Assert.True(projectResult.Success);
        var projectId = projectResult.Value!.Id;

        var taskResult = await taskService.CreateTaskAsync(
            new CreateTaskRequest(projectId, "Сборка", "Собрать билд", DateTime.UtcNow.AddDays(2)));
        Assert.True(taskResult.Success);
        var taskId = taskResult.Value!.Id;

        var assignResult = await taskService.AssignTaskAsync(new AssignTaskRequest(taskId, memberId));
        Assert.False(assignResult.Success);
        Assert.False(string.IsNullOrWhiteSpace(assignResult.Error));
    }

    private (ProjectService projectService, TaskService taskService, TeamMemberService memberService, string filePath) CreateServices()
    {
        var path = Path.Combine(Path.GetTempPath(), $"taskplanner-{Guid.NewGuid():N}.json");
        _filesToCleanup.Add(path);

        var context = new JsonFileContext(path);
        var projectRepository = new JsonProjectRepository(context);
        var taskRepository = new JsonTaskRepository(context);
        var memberRepository = new JsonTeamMemberRepository(context);

        var projectService = new ProjectService(projectRepository, memberRepository);
        var taskService = new TaskService(taskRepository, projectRepository, memberRepository);
        var memberService = new TeamMemberService(memberRepository);

        return (projectService, taskService, memberService, path);
    }

    public void Dispose()
    {
        foreach (var file in _filesToCleanup)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
