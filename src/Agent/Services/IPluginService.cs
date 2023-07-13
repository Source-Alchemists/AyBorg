using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

public interface IPluginsService
{
    /// <summary>
    /// Gets the steps.
    /// </summary>
    IReadOnlyCollection<IStepProxy> Steps { get; }

    /// <summary>
    /// Gets the device providers.
    /// </summary>
    IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders { get; }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    void Load();

    /// <summary>
    /// Find plugin instance by step record.
    /// </summary>
    /// <param name="stepRecord">The step record.</param>
    /// <returns>Instance, else null.</returns>
    IStepProxy Find(StepRecord stepRecord);

    /// <summary>
    /// Finds the specified step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns>Instance, else null.</returns>
    IStepProxy Find(Guid stepId);

    /// <summary>
    /// Creates new instance of step.
    /// </summary>
    /// <param name="stepBody">The step body.</param>
    /// <returns></returns>
    IStepProxy CreateInstance(IStepBody stepBody);
}
