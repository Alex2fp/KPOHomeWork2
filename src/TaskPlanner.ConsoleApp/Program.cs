using TaskPlanner.Application.Requests;
using TaskPlanner.Application.Services;
using TaskPlanner.Domain.Entities;
using TaskPlanner.Infrastructure.Persistence;
using TaskPlanner.Infrastructure.Repositories;

var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "taskplanner.json");
var context = new JsonFileContext(dataPath);
var projectRepository = new JsonProjectRepository(context);
var taskRepository = new JsonTaskRepository(context);
var memberRepository = new JsonTeamMemberRepository(context);

var projectService = new ProjectService(projectRepository, memberRepository);
var taskService = new TaskService(taskRepository, projectRepository, memberRepository);
var teamMemberService = new TeamMemberService(memberRepository);

Console.WriteLine("Task Planner CLI");
Console.WriteLine("================");

var exitRequested = false;
while (!exitRequested)
{
    Console.WriteLine();
    Console.WriteLine("Выберите действие:");
    Console.WriteLine("1. Зарегистрировать участника");
    Console.WriteLine("2. Создать проект");
    Console.WriteLine("3. Добавить участника к проекту");
    Console.WriteLine("4. Создать задачу");
    Console.WriteLine("5. Назначить задачу участнику");
    Console.WriteLine("6. Изменить статус задачи");
    Console.WriteLine("7. Показать задачи на ближайшую неделю");
    Console.WriteLine("8. Показать проекты и задачи");
    Console.WriteLine("0. Выход");

    Console.Write("Введите номер команды: ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await RegisterMemberAsync(teamMemberService);
            break;
        case "2":
            await CreateProjectAsync(projectService);
            break;
        case "3":
            await AttachMemberToProjectAsync(projectService);
            break;
        case "4":
            await CreateTaskAsync(taskService);
            break;
        case "5":
            await AssignTaskAsync(taskService);
            break;
        case "6":
            await ChangeTaskStatusAsync(taskService);
            break;
        case "7":
            await ShowUpcomingTasksAsync(taskService);
            break;
        case "8":
            await ShowProjectsAsync(projectService, taskService);
            break;
        case "0":
            exitRequested = true;
            break;
        default:
            Console.WriteLine("Неизвестная команда. Попробуйте ещё раз.");
            break;
    }
}

static async Task RegisterMemberAsync(TeamMemberService service)
{
    Console.Write("ФИО: ");
    var fullName = Console.ReadLine() ?? string.Empty;
    Console.Write("Email: ");
    var email = Console.ReadLine() ?? string.Empty;

    var result = await service.RegisterMemberAsync(new RegisterMemberRequest(fullName, email));
    if (result.Success)
    {
        Console.WriteLine($"Участник зарегистрирован: {result.Value!.FullName} ({result.Value.Email})");
    }
    else
    {
        Console.WriteLine($"Ошибка: {result.Error}");
    }
}

static async Task CreateProjectAsync(ProjectService service)
{
    Console.Write("Название проекта: ");
    var name = Console.ReadLine() ?? string.Empty;
    Console.Write("Описание: ");
    var description = Console.ReadLine() ?? string.Empty;

    var result = await service.CreateProjectAsync(new CreateProjectRequest(name, description));
    if (result.Success)
    {
        Console.WriteLine($"Проект создан. Идентификатор: {result.Value!.Id}");
    }
    else
    {
        Console.WriteLine($"Ошибка: {result.Error}");
    }
}

static async Task AttachMemberToProjectAsync(ProjectService service)
{
    var projectId = ReadGuid("Идентификатор проекта");
    var memberId = ReadGuid("Идентификатор участника");

    var result = await service.AttachMemberAsync(projectId, memberId);
    Console.WriteLine(result.Success ? "Участник добавлен к проекту." : $"Ошибка: {result.Error}");
}

static async Task CreateTaskAsync(TaskService service)
{
    var projectId = ReadGuid("Идентификатор проекта");
    Console.Write("Название задачи: ");
    var title = Console.ReadLine() ?? string.Empty;
    Console.Write("Описание: ");
    var description = Console.ReadLine() ?? string.Empty;
    var dueDate = ReadDate("Срок (гггг-мм-дд)");
    Console.Write("Идентификатор участника (опционально): ");
    var memberInput = Console.ReadLine();
    Guid? memberId = Guid.TryParse(memberInput, out var parsedMember) ? parsedMember : null;

    var result = await service.CreateTaskAsync(new CreateTaskRequest(projectId, title, description, dueDate, memberId));
    if (result.Success)
    {
        Console.WriteLine($"Задача создана. Идентификатор: {result.Value!.Id}");
    }
    else
    {
        Console.WriteLine($"Ошибка: {result.Error}");
    }
}

static async Task AssignTaskAsync(TaskService service)
{
    var taskId = ReadGuid("Идентификатор задачи");
    var memberId = ReadGuid("Идентификатор участника");

    var result = await service.AssignTaskAsync(new AssignTaskRequest(taskId, memberId));
    Console.WriteLine(result.Success ? "Задача назначена." : $"Ошибка: {result.Error}");
}

static async Task ChangeTaskStatusAsync(TaskService service)
{
    var taskId = ReadGuid("Идентификатор задачи");

    Console.WriteLine("Выберите новый статус:");
    foreach (var status in Enum.GetValues<TaskStatus>())
    {
        Console.WriteLine($"{(int)status} - {status}");
    }

    Console.Write("Введите номер статуса: ");
    var statusInput = Console.ReadLine();
    if (!int.TryParse(statusInput, out var statusValue) || !Enum.IsDefined(typeof(TaskStatus), statusValue))
    {
        Console.WriteLine("Некорректный статус.");
        return;
    }

    var result = await service.UpdateStatusAsync(new UpdateTaskStatusRequest(taskId, (TaskStatus)statusValue));
    Console.WriteLine(result.Success ? "Статус обновлён." : $"Ошибка: {result.Error}");
}

static async Task ShowUpcomingTasksAsync(TaskService service)
{
    var until = DateTime.UtcNow.Date.AddDays(7);
    var tasks = await service.GetUpcomingTasksAsync(until);

    if (!tasks.Any())
    {
        Console.WriteLine("Нет задач на ближайшую неделю.");
        return;
    }

    Console.WriteLine("Ближайшие задачи:");
    foreach (var task in tasks)
    {
        var assigned = string.IsNullOrWhiteSpace(task.AssignedTo) ? "(не назначена)" : $"({task.AssignedTo})";
        Console.WriteLine($"[{task.DueDate:yyyy-MM-dd}] {task.ProjectName}: {task.Title} {assigned} - {task.Status}");
    }
}

static async Task ShowProjectsAsync(ProjectService projectService, TaskService taskService)
{
    var projects = await projectService.GetProjectsAsync();
    if (!projects.Any())
    {
        Console.WriteLine("Проекты отсутствуют.");
        return;
    }

    foreach (var project in projects)
    {
        Console.WriteLine($"Проект: {project.Name} ({project.Id})");
        Console.WriteLine($"  Участники: {project.MemberIds.Count}");
        var tasks = await taskService.GetTasksByProjectAsync(project.Id);
        Console.WriteLine(tasks.Any() ? "  Задачи:" : "  Задачи отсутствуют.");
        foreach (var task in tasks)
        {
            Console.WriteLine($"    - {task.Title} [{task.Status}] до {task.DueDate:yyyy-MM-dd}");
        }
    }
}

static Guid ReadGuid(string prompt)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine();
        if (Guid.TryParse(input, out var value))
        {
            return value;
        }

        Console.WriteLine("Некорректный GUID. Попробуйте ещё раз.");
    }
}

static DateTime ReadDate(string prompt)
{
    while (true)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine();
        if (DateTime.TryParse(input, out var value))
        {
            return value.Date;
        }

        Console.WriteLine("Некорректная дата. Используйте формат гггг-мм-дд.");
    }
}
