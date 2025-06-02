namespace CrmSystem.Api.Services;

public class LogService
{
    public Task LogAsync(string action, string entity, string message)
    {
        Console.WriteLine($"[{DateTime.Now}] {action} {entity}: {message}");
        return Task.CompletedTask;
    }
}