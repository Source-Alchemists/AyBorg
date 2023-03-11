using AyBorg.Data.Audit.Models.Agent;
using AyBorg.Data.Audit.Repositories.Agent;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Audit.Services.Tests;

public class AgentAuditServiceTests
{
    private static readonly NullLogger<AgentAuditService> s_logger = new();
    private readonly Mock<IProjectAuditRepository> _mockProjectAuditRepository = new();
    private readonly AgentAuditService _service;

    public AgentAuditServiceTests()
    {
        _service = new AgentAuditService(s_logger, _mockProjectAuditRepository.Object);
    }

    [Fact]
    public void Test_GetChangesets()
    {
        // Arrange
        _mockProjectAuditRepository.Setup(m => m.FindAll()).Returns(new List<ProjectAuditRecord> {
            new ProjectAuditRecord()
        });

        // Act
        IEnumerable<Data.Audit.Models.ChangesetRecord> result = _service.GetChangesets();

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_TryAdd(bool canAdd)
    {
        // Arrange
        _mockProjectAuditRepository.Setup(m => m.TryAdd(It.IsAny<ProjectAuditRecord>())).Returns(canAdd);

        // Act
        bool result = _service.TryAdd(new ProjectAuditRecord());

        // Assert
        Assert.Equal(canAdd, result);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    public void Test_TryRemove(bool exists, bool inTime, bool canRemove)
    {
        // Arrange
        var record = new ProjectAuditRecord
        {
            Timestamp = inTime ? DateTime.UtcNow : DateTime.UtcNow - TimeSpan.FromHours(2)
        };
        _mockProjectAuditRepository.Setup(m => m.Find(It.IsAny<Guid>())).Returns(exists ? record : null!);
        _mockProjectAuditRepository.Setup(m => m.TryRemove(It.IsAny<ProjectAuditRecord>())).Returns(canRemove);

        // Act
        bool resul = _service.TryRemove(Guid.NewGuid());

        // Assert
        Assert.Equal(exists && inTime && canRemove, resul);
    }
}
