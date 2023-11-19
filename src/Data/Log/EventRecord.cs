using Microsoft.Extensions.Logging;

namespace AyBorg.Data.Log;

public record EventRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceUniqueName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public LogLevel LogLevel { get; set; }
    public int EventId { get; set; }
    public string Message { get; set; } = string.Empty;
}
