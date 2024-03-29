namespace AyBorg.Web.Shared.Models;

public record EventLogEntry
{
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceUniqueName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public LogLevel LogLevel { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
