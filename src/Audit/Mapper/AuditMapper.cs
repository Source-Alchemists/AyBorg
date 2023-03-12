using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using Google.Protobuf.WellKnownTypes;

namespace AyBorg.Audit;

public static class AuditMapper
{
    public static ProjectAuditRecord Map(AuditEntry entry)
    {
        var result = new ProjectAuditRecord
        {
            Id = Guid.Parse(entry.Token),
            ServiceType = entry.ServiceType,
            ServiceUniqueName = entry.ServiceUniqueName,
            User = entry.User,
            ProjectId = Guid.Parse(entry.AgentProject.Id),
            ProjectName = entry.AgentProject.Name,
            ProjectState = (ProjectState)entry.AgentProject.State,
            VersionName = entry.AgentProject.VersionName,
            VersionIteration = entry.AgentProject.VersionIteration,
            Comment = entry.AgentProject.Comment,
            Approver = entry.AgentProject.ApprovedBy,
            Settings = new ProjectSettingsAuditRecord
            {
                IsForceResultCommunicationEnabled = entry.AgentProject.Settings.IsForceResultCommunicationEnabled
            }
        };

        foreach (AgentStepAuditEntry? step in entry.AgentProject.Steps)
        {
            result.Steps.Add(Map(step));
        }

        foreach (AgentLinkAuditEntry? link in entry.AgentProject.Links)
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

    public static AuditChangeset Map(ChangesetRecord changeset)
    {
        return new AuditChangeset
        {
            Token = changeset.Id.ToString(),
            ServiceType = changeset.ServiceType,
            ServiceUniqueName = changeset.ServiceUniqueName,
            ProjectId = changeset.ProjectId.ToString(),
            ProjectName = changeset.ProjectName,
            ProjectState = (int)changeset.ProjectState,
            VersionName = changeset.VersionName ?? string.Empty, // Can be empty
            VersionIteration = changeset.VersionIteration,
            User = changeset.User,
            Approver = changeset.Approver ?? string.Empty, // Can be empty
            Comment = changeset.Comment ?? string.Empty, // Can be empty
            Timestamp = Timestamp.FromDateTime(changeset.Timestamp)
        };
    }

    public static ChangesetRecord Map(AuditChangeset changeset)
    {
        return new ChangesetRecord
        {
            Id = Guid.Parse(changeset.Token),
            ServiceType = changeset.ServiceType,
            ServiceUniqueName = changeset.ServiceUniqueName,
            ProjectId = Guid.Parse(changeset.ProjectId),
            ProjectName = changeset.ProjectName,
            ProjectState = (ProjectState)changeset.ProjectState,
            VersionName = changeset.VersionName,
            VersionIteration = changeset.VersionIteration,
            User = changeset.User,
            Approver = changeset.Approver,
            Comment = changeset.Comment,
            Timestamp = changeset.Timestamp.ToDateTime()
        };
    }

    public static AuditChange Map(ChangeRecord change)
    {
        return new AuditChange
        {
            RelatedTokenA = change.ChangesetAId.ToString(),
            RelatedTokenB = change.ChangesetBId.ToString(),
            Label = change.Label,
            SubLabel = change.SubLabel,
            OriginalValue = change.ValueA ?? string.Empty,
            ChangedValue = change.ValueB ?? string.Empty
        };
    }

    public static AuditReport Map(AuditReportRecord record)
    {
        AuditReport result = new()
        {
            Id = record.Id.ToString(),
            Timestamp = Timestamp.FromDateTime(record.Timestamp),
            ReportName = record.Name,
            Comment = record.Comment ?? string.Empty
        };

        foreach (ChangesetRecord changeset in record.Changesets)
        {
            result.Changesets.Add(Map(changeset));
        }

        return result;
    }

    public static AuditReportRecord Map(AuditReport report)
    {
        AuditReportRecord result = new()
        {
            Id = Guid.Parse(report.Id),
            Timestamp = report.Timestamp.ToDateTime(),
            Name = report.ReportName,
            Comment = report.Comment
        };

        foreach (AuditChangeset changeset in report.Changesets)
        {
            result.Changesets.Add(Map(changeset));
        }

        return result;
    }

    private static StepAuditRecord Map(AgentStepAuditEntry step)
    {
        var result = new StepAuditRecord
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
            result.Ports.Add(Map(port));
        }

        return result;
    }

    private static PortAuditRecord Map(AgentPortAuditEntry port)
    {
        return new PortAuditRecord
        {
            Id = Guid.Parse(port.Id),
            Name = port.Name,
            Value = port.Value,
            Brand = (PortBrand)port.Brand,
            Direction = (PortDirection)port.Direction
        };
    }
}
