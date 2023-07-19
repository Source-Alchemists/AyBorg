using Microsoft.EntityFrameworkCore;

namespace AyBorg.Data.Agent;

public sealed class DeviceContext : DbContext
{
    public DeviceContext(DbContextOptions<DeviceContext> options) : base(options) { }

    public DbSet<DeviceRecord>? AyBorgDevices { get; init; }
    public DbSet<DevicePortRecord>? AyBorgDevicePorts { get; init; }
}
