using System.Text.Json;

namespace TaskPlanner.Infrastructure.Persistence;

public class JsonFileContext
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    public JsonFileContext(string filePath)
    {
        _filePath = filePath;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    internal async Task<TaskPlannerDataModel> ReadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                return new TaskPlannerDataModel();
            }

            await using var stream = File.OpenRead(_filePath);
            var data = await JsonSerializer.DeserializeAsync<TaskPlannerDataModel>(stream, _options);
            return data ?? new TaskPlannerDataModel();
        }
        finally
        {
            _lock.Release();
        }
    }

    internal async Task WriteAsync(TaskPlannerDataModel data)
    {
        await _lock.WaitAsync();
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, data, _options);
        }
        finally
        {
            _lock.Release();
        }
    }
}
