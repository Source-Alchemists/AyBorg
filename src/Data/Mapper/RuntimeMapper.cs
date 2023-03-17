using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Data.Mapper;

public class RuntimeMapper : IRuntimeMapper
{
    public Step FromRuntime(IStepProxy stepProxy, bool skipPorts = false)
    {
        var ports = new List<Port>();
        if (!skipPorts)
        {
            foreach (IPort port in stepProxy.Ports)
            {
                ports.Add(FromRuntime(port));
            }
        }

        return new Step
        {
            Id = stepProxy.Id,
            Name = stepProxy.Name,
            Categories = stepProxy.Categories,
            X = stepProxy.X,
            Y = stepProxy.Y,
            ExecutionTimeMs = stepProxy.ExecutionTimeMs,
            MetaInfo = stepProxy.MetaInfo,
            Ports = ports
        };
    }

    public Port FromRuntime(IPort runtimePort)
    {
        IPortMapper portMapper = PortMapperFactory.CreateMapper(runtimePort);
        return portMapper.ToModel(runtimePort);
    }
}
