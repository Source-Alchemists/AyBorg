using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Data.Agent;

public interface IFlowToStorageMapper
{
    /// <summary>
    /// Maps the specified step proxy.
    /// </summary>
    /// <param name="stepProxy">The step proxy.</param>
    /// <returns></returns>
    StepRecord Map(IStepProxy stepProxy);

    /// <summary>
    /// Maps the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    ProjectRecord Map(Project project);
}
