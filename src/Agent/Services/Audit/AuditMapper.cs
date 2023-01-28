using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Agent;

namespace AyBorg.Agent.Services;

public sealed class AuditMapper
{
    public AgentProjectAuditEntry Map(ProjectRecord projectRecord)
    {
        var result = new AgentProjectAuditEntry
        {
            Id = projectRecord.Meta.Id.ToString(),
            Name = projectRecord.Meta.Name,
            State = (int)projectRecord.Meta.State,
            VersionName = projectRecord.Meta.VersionName,
            VersionIteration = Convert.ToInt32(projectRecord.Meta.VersionIteration),
            Comment = projectRecord.Meta.Comment,
            ApprovedBye = projectRecord.Meta.ApprovedBy ?? string.Empty,
            Settings = new AgentProjectSettingsAuditEntry
            {
                IsForceResultCommunicationEnabled = projectRecord.Settings.IsForceResultCommunicationEnabled
            }
        };

        foreach (StepRecord step in projectRecord.Steps)
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
                stepDto.Ports.Add(new AgentPortAuditEntry
                {
                    Id = port.Id.ToString(),
                    Name = port.Name,
                    Value = value ?? string.Empty,
                    Brand = (int)port.Brand
                });
            }
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
}
