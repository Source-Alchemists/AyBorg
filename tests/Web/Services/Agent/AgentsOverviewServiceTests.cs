using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Agent;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class AgentsOverviewServiceTests
{
    private readonly Mock<IRegistryService> _mockRegistryService = new();
    private readonly Mock<IRuntimeService> _mockRuntimeService = new();
    private readonly Mock<IProjectManagementService> _mockProjectManagementService = new();
    private readonly AgentsOverviewService _service;

    public AgentsOverviewServiceTests()
    {
        _service = new AgentsOverviewService(_mockRegistryService.Object, _mockRuntimeService.Object, _mockProjectManagementService.Object);
    }

    [Fact]
    public async ValueTask Test_UpdateAsync()
    {
        // Arrange
        _mockRegistryService.Setup(m => m.ReceiveServicesAsync(It.IsAny<string>())).ReturnsAsync(new List<ServiceInfoEntry> {
            new ServiceInfoEntry(new Ayborg.Gateway.V1.ServiceInfo {
                UniqueName = "Agent.NoActiveProject",
                Name = "Agent-NoActiveProject"
            }),
            new ServiceInfoEntry(new Ayborg.Gateway.V1.ServiceInfo {
                UniqueName = "Agent.ActiveProject",
                Name = "Agent-ActiveProject"
            }),
            new ServiceInfoEntry(new Ayborg.Gateway.V1.ServiceInfo {
                UniqueName = "Agent.ActiveProject2",
                Name = "Agent"
            }),
            new ServiceInfoEntry(new Ayborg.Gateway.V1.ServiceInfo {
                UniqueName = "Agent.ActiveProject3",
                Name = "Agent"
            })
        });
        _mockProjectManagementService.Setup(m => m.GetActiveMetaAsync(It.IsIn("Agent.NoActiveProject"))).ReturnsAsync((ProjectMeta)null!);
        _mockProjectManagementService.Setup(m => m.GetActiveMetaAsync(It.IsIn("Agent.ActiveProject"))).ReturnsAsync(new ProjectMeta { Name = "ProjectName" });
        _mockProjectManagementService.Setup(m => m.GetActiveMetaAsync(It.IsIn("Agent.ActiveProject2"))).ReturnsAsync(new ProjectMeta { Name = "ProjectName2" });
        _mockProjectManagementService.Setup(m => m.GetActiveMetaAsync(It.IsIn("Agent.ActiveProject3"))).ReturnsAsync(new ProjectMeta { Name = "ProjectName3" });
        _mockRuntimeService.Setup(m => m.GetStatusAsync(It.IsIn("Agent.ActiveProject"))).ReturnsAsync(new SDK.System.Runtime.EngineMeta
        {
            State = SDK.System.Runtime.EngineState.Running
        });

        // Act
        await _service.UpdateAsync();

        // Assert
        Assert.Equal(4, _service.AgentsCount);
        Assert.Equal(1, _service.ActiveAgentsCount);
        Assert.Equal(3, _service.InactiveAgentsCount);
        Assert.Equal(4, _service.AgentServices.Count());
        Assert.Single(_service.AgentServices.Where(a => a.Name.Equals("None")));
        Assert.Single(_service.AgentServices.Where(a => a.Name.Equals("ProjectName")));
        Assert.Single(_service.AgentServices.Where(a => a.Name.Equals("ProjectName2")));
        Assert.Single(_service.AgentServices.Where(a => a.Name.Equals("ProjectNam3")));
    }
}
