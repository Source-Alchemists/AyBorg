using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Data.DAL;
using Autodroid.SDK.Projects;

namespace Autodroid.Agent.Services;

public interface IRuntimeConverterService
{
    /// <summary>
    /// Converts the specified project.
    /// </summary>
    /// <param name="projectRecord">The project.</param>
    /// <returns></returns>
    ValueTask<Project> ConvertAsync(ProjectRecord projectRecord);

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    ValueTask<bool> TryUpdatePortValueAsync(IPort port, object value);
}