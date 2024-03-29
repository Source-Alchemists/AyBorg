using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class RuntimeMapper : SDK.Common.IRuntimeMapper
{
    public Step FromRuntime(SDK.Common.IStepProxy stepProxy, bool skipPorts = false)
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
            MetaInfo = stepProxy.MetaInfo with {},
            Ports = ports
        };
    }

    public Port FromRuntime(IPort runtimePort)
    {
        IPortMapper portMapper = PortMapperFactory.CreateMapper(runtimePort);
        return portMapper.ToModel(runtimePort);
    }
}
