using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System.Runtime;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public class RuntimeServiceV1Tests : BaseGrpcServiceTests<RuntimeServiceV1, Ayborg.Gateway.Agent.V1.Runtime.RuntimeClient>
{
    private readonly Mock<IEngineHost> _mockEngineHost = new();

    public RuntimeServiceV1Tests()
    {
        _service = new RuntimeServiceV1(_mockEngineHost.Object);
    }

    [Fact]
    public async Task Test_GetStatus()
    {
        // Arrange
        _mockEngineHost.Setup(m => m.GetEngineStatus()).Returns(new EngineMeta());
        var request = new GetRuntimeStatusRequest();

        // Act
        GetRuntimeStatusResponse response = await _service.GetStatus(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.EngineMetaInfos);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_StartRun(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockEngineHost.Setup(m => m.StartRunAsync(It.IsAny<EngineExecutionType>())).ReturnsAsync(new EngineMeta());
        var request = new StartRunRequest {
            EngineExecutionType = 0
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.StartRun(request, _serverCallContext));
            return;
        }

        StartRunResponse response = await _service.StartRun(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.EngineMetaInfos);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_StopRun(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockEngineHost.Setup(m => m.StopRunAsync()).ReturnsAsync(new EngineMeta());
        var request = new StopRunRequest();

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.StopRun(request, _serverCallContext));
            return;
        }

        StopRunResponse response = await _service.StopRun(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.EngineMetaInfos);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_AbortRun(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockEngineHost.Setup(m => m.AbortRunAsync()).ReturnsAsync(new EngineMeta());
        var request = new AbortRunRequest();

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AbortRun(request, _serverCallContext));
            return;
        }

        AbortRunResponse response = await _service.AbortRun(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.EngineMetaInfos);
    }
}
