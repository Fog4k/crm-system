namespace CrmSystem.Api.Models;

public class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}