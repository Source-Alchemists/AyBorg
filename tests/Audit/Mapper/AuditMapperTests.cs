using AyBorg.Data.Audit.Models;
using AyBorg.SDK.Common.Ports;
using Google.Protobuf.WellKnownTypes;

namespace AyBorg.Audit.Tests;

public class AuditMapperTests
{
    [Fact]
    public void Test_MapRecordChangesetToRpcChangeset()
    {
        // Arrange
        var changeset = new ChangesetRecord
        {
            Id = Guid.NewGuid(),
            ServiceType = "Test.Type",
            ServiceUniqueName = "Test.Name",
            ProjectId = Guid.NewGuid(),
            ProjectName = "Test.Project",
            ProjectState = SDK.Projects.ProjectState.Ready,
            VersionName = "V1",
            VersionIteration = 1,
            User = "Test.User",
            Approver = "Test.Approver",
            Comment = "Test comment!",
            Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes(5),
            Type = SDK.System.AuditEntryType.Project
        };

        // Act
        Ayborg.Gateway.Audit.V1.AuditChangeset result = AuditMapper.Map(changeset);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(changeset.Id.ToString(), result.Token);
        Assert.Equal(changeset.ServiceType, result.ServiceType);
        Assert.Equal(changeset.ServiceUniqueName, result.ServiceUniqueName);
        Assert.Equal(changeset.ProjectId.ToString(), result.ProjectId);
        Assert.Equal(changeset.ProjectName, result.ProjectName);
        Assert.Equal((int)changeset.ProjectState, result.ProjectState);
        Assert.Equal(changeset.VersionName, result.VersionName);
        Assert.Equal(changeset.VersionIteration, result.VersionIteration);
        Assert.Equal(changeset.User, result.User);
        Assert.Equal(changeset.Approver, result.Approver);
        Assert.Equal(changeset.Comment, result.Comment);
        Assert.Equal(changeset.Timestamp, result.Timestamp.ToDateTime());
        Assert.Equal((int)changeset.Type, result.Type);
    }

    [Fact]
    public void Test_MapRpcChangesetToRecordChangeset()
    {
        // Arrange
        var changeset = new Ayborg.Gateway.Audit.V1.AuditChangeset
        {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "Test.Type",
            ServiceUniqueName = "Test.Name",
            ProjectId = Guid.NewGuid().ToString(),
            ProjectName = "Test.Project",
            ProjectState = (int)SDK.Projects.ProjectState.Ready,
            VersionName = "V1",
            VersionIteration = 1,
            User = "Test.User",
            Approver = "Test.Approver",
            Comment = "Test comment!",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow - TimeSpan.FromMinutes(5)),
            Type = (int)SDK.System.AuditEntryType.Project
        };

        // Act
        ChangesetRecord result = AuditMapper.Map(changeset);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse(changeset.Token), result.Id);
        Assert.Equal(changeset.ServiceType, result.ServiceType);
        Assert.Equal(changeset.ServiceUniqueName, result.ServiceUniqueName);
        Assert.Equal(Guid.Parse(changeset.ProjectId), result.ProjectId);
        Assert.Equal(changeset.ProjectName, result.ProjectName);
        Assert.Equal((SDK.Projects.ProjectState)changeset.ProjectState, result.ProjectState);
        Assert.Equal(changeset.VersionName, result.VersionName);
        Assert.Equal(changeset.VersionIteration, result.VersionIteration);
        Assert.Equal(changeset.User, result.User);
        Assert.Equal(changeset.Approver, result.Approver);
        Assert.Equal(changeset.Comment, result.Comment);
        Assert.Equal(changeset.Timestamp.ToDateTime(), result.Timestamp);
        Assert.Equal((SDK.System.AuditEntryType)changeset.Type, result.Type);
    }

    [Fact]
    public void Test_MapRecordChangeTpRpcChange()
    {
        // Arrange
        var change = new ChangeRecord
        {
            ChangesetAId = Guid.NewGuid(),
            ChangesetBId = Guid.NewGuid(),
            Label = "Test.Label",
            SubLabel = "Test.SubLabel",
            ValueA = "Test.ValueA",
            ValueB = "Test.ValueB"
        };

        // Act
        Ayborg.Gateway.Audit.V1.AuditChange result = AuditMapper.Map(change);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(change.ChangesetAId.ToString(), result.RelatedTokenA);
        Assert.Equal(change.ChangesetBId.ToString(), result.RelatedTokenB);
        Assert.Equal(change.Label, result.Label);
        Assert.Equal(change.SubLabel, result.SubLabel);
        Assert.Equal(change.ValueA, result.OriginalValue);
        Assert.Equal(change.ValueB, result.ChangedValue);
    }

    [Fact]
    public void Test_MapRecordReportToRpcReport()
    {
        // Arrange
        var report = new AuditReportRecord
        {
            Id = Guid.NewGuid(),
            Name = "Test.Report",
            Comment = "Test comment!",
            Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes(5),
            Changesets = new List<ChangesetRecord> {
                new ChangesetRecord {
                    Id = Guid.NewGuid(),
                    ServiceType = "Test.Type",
                    ServiceUniqueName = "Test.Name",
                    ProjectId = Guid.NewGuid(),
                    ProjectName = "Test.Project",
                    ProjectState = SDK.Projects.ProjectState.Ready,
                    VersionName = "V1",
                    VersionIteration = 1,
                    User = "Test.User",
                    Approver = "Test.Approver",
                    Comment = "Test comment!",
                    Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                    Type = SDK.System.AuditEntryType.Project
                }
            }
        };

        // Act
        Ayborg.Gateway.Audit.V1.AuditReport result = AuditMapper.Map(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(report.Id.ToString(), result.Id);
        Assert.Equal(report.Name, result.ReportName);
        Assert.Equal(report.Comment, result.Comment);
        Assert.Equal(report.Timestamp, result.Timestamp.ToDateTime());
        Assert.Single(result.Changesets);
    }

    [Fact]
    public void Test_MapRpcReportToRecordReport()
    {
        // Arrange
        var report = new Ayborg.Gateway.Audit.V1.AuditReport
        {
            Id = Guid.NewGuid().ToString(),
            ReportName = "Test.Report",
            Comment = "Test comment!",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow - TimeSpan.FromMinutes(5))
        };

        report.Changesets.Add(new Ayborg.Gateway.Audit.V1.AuditChangeset
        {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "Test.Type",
            ServiceUniqueName = "Test.Name",
            ProjectId = Guid.NewGuid().ToString(),
            ProjectName = "Test.Project",
            ProjectState = (int)SDK.Projects.ProjectState.Ready,
            VersionName = "V1",
            VersionIteration = 1,
            User = "Test.User",
            Approver = "Test.Approver",
            Comment = "Test comment!",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow - TimeSpan.FromMinutes(5)),
            Type = (int)SDK.System.AuditEntryType.Project
        });

        // Act
        AuditReportRecord result = AuditMapper.Map(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse(report.Id), result.Id);
        Assert.Equal(report.ReportName, result.Name);
        Assert.Equal(report.Comment, result.Comment);
        Assert.Equal(report.Timestamp.ToDateTime(), result.Timestamp);
        Assert.Single(result.Changesets);
    }

    [Fact]
    public void Test_MapRpcAuditEntryToProjectAuditRecord()
    {
        // Arrange
        var entry = new Ayborg.Gateway.Audit.V1.AuditEntry
        {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "Test.Type",
            ServiceUniqueName = "Test.Name",
            User = "Test.User",
            Type = (int)SDK.System.AuditEntryType.Project,
            AgentProject = new Ayborg.Gateway.Audit.V1.AgentProjectAuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test.Project",
                State = (int)SDK.Projects.ProjectState.Ready,
                VersionName = "V1",
                VersionIteration = 1,
                Comment = "Test comment!",
                ApprovedBy = "Test.Approver",
                Settings = new Ayborg.Gateway.Audit.V1.AgentProjectSettingsAuditEntry
                {
                    IsForceResultCommunicationEnabled = true
                }
            }
        };

        var step = new Ayborg.Gateway.Audit.V1.AgentStepAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test.Step",
            X = 1,
            Y = 2,
            AssemblyName = "Test.Assembly",
            AssemblyVersion = "V1",
            TypeName = "Test.Type"
        };

        var port = new Ayborg.Gateway.Audit.V1.AgentPortAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test.Port",
            Value = "Test.Value",
            Brand = (int)PortBrand.String,
            Direction = (int)PortDirection.Input
        };

        step.Ports.Add(port);
        entry.AgentProject.Steps.Add(step);

        // Act
        Data.Audit.Models.Agent.ProjectAuditRecord result = AuditMapper.Map(entry);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse(entry.Token), result.Id);
        Assert.Equal(entry.ServiceType, result.ServiceType);
        Assert.Equal(entry.ServiceUniqueName, result.ServiceUniqueName);
        Assert.Equal(entry.User, result.User);
        Assert.Equal((SDK.System.AuditEntryType)entry.Type, result.Type);
        Assert.Equal(Guid.Parse(entry.AgentProject.Id), result.ProjectId);
        Assert.Equal(entry.AgentProject.Name, result.ProjectName);
        Assert.Equal((SDK.Projects.ProjectState)entry.AgentProject.State, result.ProjectState);
        Assert.Equal(entry.AgentProject.VersionName, result.VersionName);
        Assert.Equal(entry.AgentProject.VersionIteration, result.VersionIteration);
        Assert.Equal(entry.AgentProject.Comment, result.Comment);
        Assert.Equal(entry.AgentProject.ApprovedBy, result.Approver);
        Assert.Equal(entry.AgentProject.Settings.IsForceResultCommunicationEnabled, result.Settings.IsForceResultCommunicationEnabled);
        Assert.Single(result.Steps);
        Assert.Equal(Guid.Parse(step.Id), result.Steps[0].Id);
        Assert.Equal(step.Name, result.Steps[0].Name);
        Assert.Equal(step.X, result.Steps[0].X);
        Assert.Equal(step.Y, result.Steps[0].Y);
        Assert.Equal(step.AssemblyName, result.Steps[0].AssemblyName);
        Assert.Equal(step.AssemblyVersion, result.Steps[0].AssemblyVersion);
        Assert.Equal(step.TypeName, result.Steps[0].TypeName);
        Assert.Single(result.Steps[0].Ports);
        Assert.Equal(Guid.Parse(port.Id), result.Steps[0].Ports[0].Id);
        Assert.Equal(port.Name, result.Steps[0].Ports[0].Name);
        Assert.Equal(port.Value, result.Steps[0].Ports[0].Value);
        Assert.Equal((PortBrand)port.Brand, result.Steps[0].Ports[0].Brand);
        Assert.Equal((PortDirection)port.Direction, result.Steps[0].Ports[0].Direction);
    }
}
