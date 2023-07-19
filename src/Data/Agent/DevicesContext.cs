using Microsoft.EntityFrameworkCore;

namespace AyBorg.Data.Agent;

public sealed class DevicesContext : DbContext
{
    public DevicesContext(DbContextOptions<DevicesContext> options) : base(options) { }

    public DbSet<DeviceRecord>? AyBorgDevices { get; set; }
    public DbSet<PortRecord>? AyBorgDevicePorts { get; set; }
}
