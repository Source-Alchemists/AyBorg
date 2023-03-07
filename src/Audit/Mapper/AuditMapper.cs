using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;

namespace AyBorg.Audit;

public static class AuditMapper
{
    public static ProjectAuditRecord MapToProjectRecord(AuditEntry entry)
    {
        var result = new ProjectAuditRecord
        {
            Id = Guid.Parse(entry.Token),
            ServiceType = entry.ServiceType,
            ServiceUniqueName = entry.ServiceUniqueName,
            User = entry.User,
            Type = (AuditEntryType)entry.Type,
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
                var portRecord = new PortAuditRecord
                {
                    Id = Guid.Parse(port.Id),
                    Name = port.Name,
                    Value = port.Value,
                    Brand = (PortBrand)port.Brand,
                    Direction = (PortDirection)port.Direction
                };
                stepRecord.Ports.Add(portRecord);
            }

            result.Steps.Add(stepRecord);
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
            Timestamp = Timestamp.FromDateTime(changeset.Timestamp),
            Type = (int)changeset.Type
        };
    }

    public static AuditReport Map(AuditReportRecord record)
    {
        AuditReport result = new()
        {
            Id = record.Id.ToString(),
            Timestamp = Timestamp.FromDateTime(record.Timestamp),
            ReportName = record.Name,
            Comment = record.Comment
        };

        foreach (ChangesetRecord changeset in record.Changesets)
        {
            result.Changesets.Add(Map(changeset));
        }

        return result;
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
}
