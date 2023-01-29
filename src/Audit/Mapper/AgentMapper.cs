using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Audit.Models.Agent;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Audit;

public sealed class AgentMapper
{
    public ProjectAuditRecord Map(AgentProjectAuditEntry entry)
    {
        var result = new ProjectAuditRecord
        {
            Id = Guid.Parse(entry.Id),
            Name = entry.Name,
            State = (ProjectState)entry.State,
            VersionName = entry.VersionName,
            VersionIteration = entry.VersionIteration,
            Comment = entry.Comment,
            ApprovedBy = entry.ApprovedBy,
            Settings = new ProjectSettingsAuditRecord
            {
                IsForceResultCommunicationEnabled = entry.Settings.IsForceResultCommunicationEnabled
            }
        };

        foreach (AgentStepAuditEntry? step in entry.Steps)
        {
            var stepRecord = new StepAuditRecord
            {
                Id = Guid.Parse(step.Id),
                Name = step.Name,
                X = step.X,
                Y = step.Y,
                AssemblyName = step.AssemblyName,
                AssemblyVersion = step.AssemblyVersion,
                TypeName = step.TypeName
            };

            foreach (AgentPortAuditEntry? port in step.Ports)
            {
                stepRecord.Ports.Add(new PortAuditRecord
                {
                    Id = Guid.Parse(port.Id),
                    Name = port.Name,
                    Value = port.Value,
                    Brand = (PortBrand)port.Brand
                });
            }

            result.Steps.Add(stepRecord);
        }

        foreach (AgentLinkAuditEntry? link in entry.Links)
        {
            result.Links.Add(new LinkAuditRecord
            {
                Id = Guid.Parse(link.Id),
                SourceId = Guid.Parse(link.SourceId),
                TargetId = Guid.Parse(link.TargetId)
            });
        }

        return result;
    }
}
