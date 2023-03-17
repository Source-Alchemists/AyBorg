using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Data.Mapper;

public interface IRuntimeMapper
{
    Step FromRuntime(IStepProxy stepProxy, bool skipPorts = false);
    Port FromRuntime(IPort runtimePort);
}
