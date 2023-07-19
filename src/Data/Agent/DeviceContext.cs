using Microsoft.EntityFrameworkCore;

namespace AyBorg.Data.Agent;

public sealed class DeviceContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public DeviceContext(DbContextOptions<DeviceContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the device records.
    /// </summary>
    public DbSet<DeviceRecord>? AyBorgDevices { get; init; }

    /// <summary>
    /// Gets or sets the device port records.
    /// </summary>
    public DbSet<DevicePortRecord>? AyBorgDevicePorts { get; init; }

    /// <summary>
    /// Gets or sets the device plugin meta info.
    /// </summary>
    public DbSet<PluginMetaInfoRecord>? AyBorgDevicePluginMetaInfo { get; init; }
}
