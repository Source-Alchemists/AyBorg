using Microsoft.EntityFrameworkCore;

namespace AyBorg.Data.Agent;

public class ProjectContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public ProjectContext(DbContextOptions<ProjectContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the project meta records.
    /// </summary>
    public DbSet<ProjectMetaRecord>? AyBorgProjectMetas { get; set; }

    /// <summary>
    /// Gets or sets the project setting records.
    /// </summary>
    public DbSet<ProjectSettingsRecord>? AyBorgProjectSettings { get; set; }

    /// <summary>
    /// Gets or sets the projects.
    /// </summary>
    public DbSet<ProjectRecord>? AyBorgProjects { get; set; }

    /// <summary>
    /// Gets or sets the steps.
    /// </summary>
    public DbSet<StepRecord>? AyBorgSteps { get; set; }

    /// <summary>
    /// Gets or sets the ports.
    /// </summary>
    public DbSet<PortRecord>? AyBorgPorts { get; set; }

    /// <summary>
    /// Gets or sets the links.
    /// </summary>
    public DbSet<LinkRecord>? AyBorgLinks { get; set; }
}
