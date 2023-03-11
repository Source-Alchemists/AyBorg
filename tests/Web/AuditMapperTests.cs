using AyBorg.SDK.Projects;
using Google.Protobuf.WellKnownTypes;

namespace AyBorg.Web.Shared;

public class AuditMapperTests {

    [Fact]
    public void Test_MapRpcChangesetToUiChangeset()
    {
        // Arrange
        var changeset = new Ayborg.Gateway.Audit.V1.AuditChangeset {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "TestService",
            ServiceUniqueName = "TestService",
            ProjectId = Guid.NewGuid().ToString(),
            ProjectName = "TestProject",
            ProjectState = (int)ProjectState.Draft,
            VersionName = "TestVersion",
            VersionIteration = 1,
            User = "TestUser",
            Approver = "TestUser",
            Comment = "TestComment",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // Act
        Models.AuditChangeset result = AuditMapper.Map(changeset);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(changeset.Token, result.Token.ToString());
        Assert.Equal(changeset.ServiceType, result.ServiceType);
        Assert.Equal(changeset.ServiceUniqueName, result.ServiceUniqueName);
        Assert.Equal(changeset.ProjectId, result.ProjectId.ToString());
        Assert.Equal(changeset.ProjectName, result.ProjectName);
        Assert.Equal((ProjectState)changeset.ProjectState, result.ProjectState);
        Assert.Equal(changeset.VersionName, result.VersionName);
        Assert.Equal(changeset.VersionIteration, result.VersionIteration);
        Assert.Equal(changeset.User, result.User);
        Assert.Equal(changeset.Approver, result.Approver);
        Assert.Equal(changeset.Comment, result.Comment);
        Assert.Equal(changeset.Timestamp.ToDateTime(), result.Timestamp);
    }

    [Fact]
    public void Test_MapUiChangesetToRpcChangeset()
    {
        // Arrange
        var changeset = new Models.AuditChangeset {
            Token = Guid.NewGuid(),
            ServiceType = "TestService",
            ServiceUniqueName = "TestService",
            ProjectId = Guid.NewGuid(),
            ProjectName = "TestProject",
            ProjectState = ProjectState.Draft,
            VersionName = "TestVersion",
            VersionIteration = 1,
            User = "TestUser",
            Approver = "TestUser",
            Comment = "TestComment",
            Timestamp = DateTime.UtcNow
        };

        // Act
        Ayborg.Gateway.Audit.V1.AuditChangeset result = AuditMapper.Map(changeset);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(changeset.Token.ToString(), result.Token);
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
    }

    [Fact]
    public void Test_MapRpcChangeToUiChange()
    {
        // Arrange
        var change = new Ayborg.Gateway.Audit.V1.AuditChange {
            RelatedTokenA = Guid.NewGuid().ToString(),
            RelatedTokenB = Guid.NewGuid().ToString(),
            Label = "TestLabel",
            SubLabel = "TestSubLabel",
            OriginalValue = "TestOriginalValue",
            ChangedValue = "TestChangedValue"
        };

        // Act
        Models.AuditChange result = AuditMapper.Map(change);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(change.RelatedTokenA, result.ChangesetTokenA.ToString());
        Assert.Equal(change.RelatedTokenB, result.ChangesetTokenB.ToString());
        Assert.Equal(change.Label, result.Label);
        Assert.Equal(change.SubLabel, result.SubLabel);
        Assert.Equal(change.OriginalValue, result.ValueA);
        Assert.Equal(change.ChangedValue, result.ValueB);
    }

    [Fact]
    public void Test_MapRpcReportToUiReport()
    {
        // Arrange
        var report = new Ayborg.Gateway.Audit.V1.AuditReport {
            Id = Guid.NewGuid().ToString(),
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            ReportName = "TestReport",
            Comment = "TestComment"
        };

        report.Changesets.Add(new Ayborg.Gateway.Audit.V1.AuditChangeset {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "TestService",
            ServiceUniqueName = "TestService",
            ProjectId = Guid.NewGuid().ToString(),
            ProjectName = "TestProject",
            ProjectState = (int)ProjectState.Draft,
            VersionName = "TestVersion",
            VersionIteration = 1,
            User = "TestUser",
            Approver = "TestUser",
            Comment = "TestComment",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        });

        // Act
        Models.AuditReport result = AuditMapper.Map(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(report.Id, result.Id.ToString());
        Assert.Equal(report.Timestamp.ToDateTime(), result.Timestamp);
        Assert.Equal(report.ReportName, result.Name);
        Assert.Equal(report.Comment, result.Comment);
        Assert.NotEmpty(result.Changesets);
    }

    [Fact]
    public void Test_MapUiReportToRpcReport()
    {
        // Arrange
        var report = new Models.AuditReport {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Name = "TestReport",
            Comment = "TestComment"
        };

        report.Changesets.Add(new Models.AuditChangeset {
            Token = Guid.NewGuid(),
            ServiceType = "TestService",
            ServiceUniqueName = "TestService",
            ProjectId = Guid.NewGuid(),
            ProjectName = "TestProject",
            ProjectState = ProjectState.Draft,
            VersionName = "TestVersion",
            VersionIteration = 1,
            User = "TestUser",
            Approver = "TestUser",
            Comment = "TestComment",
            Timestamp = DateTime.UtcNow
        });

        // Act
        Ayborg.Gateway.Audit.V1.AuditReport result = AuditMapper.Map(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(report.Id.ToString(), result.Id);
        Assert.Equal(report.Timestamp, result.Timestamp.ToDateTime());
        Assert.Equal(report.Name, result.ReportName);
        Assert.Equal(report.Comment, result.Comment);
        Assert.NotEmpty(result.Changesets);
    }
}
