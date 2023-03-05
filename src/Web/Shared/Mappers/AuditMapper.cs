using AyBorg.SDK.Projects;
using AyBorg.SDK.System;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Shared.Mappers;

internal static class AuditMapper
{
    public static AuditChangeset Map(Ayborg.Gateway.Audit.V1.AuditChangeset changeset)
    {
        return new AuditChangeset
        {
            Token = Guid.Parse(changeset.Token),
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
            Timestamp = changeset.Timestamp.ToDateTime(),
            Type = (AuditEntryType)changeset.Type
        };
    }

    public static AuditChange Map(Ayborg.Gateway.Audit.V1.AuditChange change)
    {
        return new AuditChange
        {
            ChangesetTokenA = Guid.Parse(change.RelatedTokenA),
            ChangesetTokenB = Guid.Parse(change.RelatedTokenB),
            Label = change.Label,
            SubLabel = change.SubLabel,
            ValueA = change.OriginalValue,
            ValueB = change.ChangedValue
        };
    }
}
