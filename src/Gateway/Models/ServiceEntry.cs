using AyBorg.SDK.Data.DAL;

namespace AyBorg.Gateway.Models;

/// <summary>
/// Service entry.
/// </summary>
public record ServiceEntry : ServiceEntryRecord
{
    /// <summary>
    /// Gets or sets the last connection time (UTC).
    /// </summary>
    public DateTime LastConnectionTime { get; set; } = DateTime.UtcNow;
}
