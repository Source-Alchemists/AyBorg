/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Runtime.Projects;
using AyBorg.Web.Shared.Models;
using Google.Protobuf.WellKnownTypes;

namespace AyBorg.Web.Shared;

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
            Timestamp = changeset.Timestamp.ToDateTime()
        };
    }

    public static Ayborg.Gateway.Audit.V1.AuditChangeset Map(AuditChangeset changeset)
    {
        return new Ayborg.Gateway.Audit.V1.AuditChangeset
        {
            Token = changeset.Token.ToString(),
            ServiceType = changeset.ServiceType,
            ServiceUniqueName = changeset.ServiceUniqueName,
            ProjectId = changeset.ProjectId.ToString(),
            ProjectName = changeset.ProjectName,
            ProjectState = (int)changeset.ProjectState,
            VersionName = changeset.VersionName,
            VersionIteration = changeset.VersionIteration,
            User = changeset.User,
            Approver = changeset.Approver,
            Comment = changeset.Comment,
            Timestamp = Timestamp.FromDateTime(changeset.Timestamp)
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

    public static AuditReport Map(Ayborg.Gateway.Audit.V1.AuditReport report)
    {
        var result = new AuditReport
        {
            Id = Guid.Parse(report.Id),
            Timestamp = report.Timestamp.ToDateTime(),
            Name = report.ReportName,
            Comment = report.Comment
        };
        foreach (Ayborg.Gateway.Audit.V1.AuditChangeset? changeset in report.Changesets)
        {
            result.Changesets.Add(Map(changeset));
        }
        return result;
    }

    public static Ayborg.Gateway.Audit.V1.AuditReport Map(AuditReport report)
    {
        var result = new Ayborg.Gateway.Audit.V1.AuditReport
        {
            Id = report.Id.ToString(),
            Timestamp = Timestamp.FromDateTime(report.Timestamp),
            ReportName = report.Name,
            Comment = report.Comment
        };
        foreach (AuditChangeset? changeset in report.Changesets)
        {
            result.Changesets.Add(Map(changeset));
        }
        return result;
    }
}
