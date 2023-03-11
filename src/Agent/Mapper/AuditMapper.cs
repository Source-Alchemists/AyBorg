using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Agent;

namespace AyBorg.Agent;

public static class AuditMapper
{
    public static AgentProjectAuditEntry Map(ProjectRecord projectRecord)
    {
        var result = new AgentProjectAuditEntry
        {
            Id = projectRecord.Meta.Id.ToString(),
            Name = projectRecord.Meta.Name,
            State = (int)projectRecord.Meta.State,
            VersionName = projectRecord.Meta.VersionName,
            VersionIteration = Convert.ToInt32(projectRecord.Meta.VersionIteration),
            Comment = projectRecord.Meta.Comment,
            ApprovedBy = projectRecord.Meta.ApprovedBy ?? string.Empty,
            Settings = new AgentProjectSettingsAuditEntry
            {
                IsForceResultCommunicationEnabled = projectRecord.Settings.IsForceResultCommunicationEnabled
            }
        };

        foreach (StepRecord step in projectRecord.Steps)
        {
            AgentStepAuditEntry stepDto = Map(step);
            result.Steps.Add(stepDto);
        }

        foreach (LinkRecord link in projectRecord.Links)
        {
            result.Links.Add(new AgentLinkAuditEntry
            {
                Id = link.Id.ToString(),
                SourceId = link.SourceId.ToString(),
                TargetId = link.TargetId.ToString()
            });
        }

        return result;
    }

    private static AgentStepAuditEntry Map(StepRecord step)
    {
        var stepDto = new AgentStepAuditEntry
        {
            Id = step.Id.ToString(),
            Name = step.Name,
            X = step.X,
            Y = step.Y,
            AssemblyName = step.MetaInfo.AssemblyName,
            AssemblyVersion = step.MetaInfo.AssemblyVersion,
            TypeName = step.MetaInfo.TypeName
        };
        foreach (PortRecord port in step.Ports)
        {
            string value = port.Direction == SDK.Common.Ports.PortDirection.Input ? port.Value : string.Empty;
            stepDto.Ports.Add(Map(port, value));
        }

        return stepDto;
    }

    private static AgentPortAuditEntry Map(PortRecord port, string value) => new()
    {
        Id = port.Id.ToString(),
        Name = port.Name,
        Value = value ?? string.Empty,
        Brand = (int)port.Brand,
        Direction = (int)port.Direction
    };
}
