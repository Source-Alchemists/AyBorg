using Microsoft.EntityFrameworkCore;

namespace AyBorg.Data.Gateway;

public class RegistryContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="RegistryContext"/>.
    /// </summary>
    /// <param name="options">The options.</param>
    public RegistryContext(DbContextOptions<RegistryContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the service entries.
    /// </summary>
    public DbSet<ServiceEntryRecord>? ServiceEntries { get; set; }
}
