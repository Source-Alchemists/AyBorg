using Ayborg.Gateway.V1;
using AyBorg.Gateway.Models;
using Moq;

namespace AyBorg.Gateway.Services.Tests;

public class RegisterServiceV1Tests : BaseGrpcServiceTests<RegisterServiceV1, Register.RegisterClient>
{
    private readonly Mock<IKeeperService> _mockKeeperService = new();

    public RegisterServiceV1Tests()
    {
        _service = new RegisterServiceV1(s_logger, _mockKeeperService.Object);
    }

    [Fact]
    public async Task Test_Register()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        _mockKeeperService.Setup(s => s.RegisterAsync(It.IsAny<ServiceEntry>())).ReturnsAsync(expectedId);
        var request = new RegisterRequest
        {
            Name = "Test",
            UniqueName = "Test-1",
            Type = "TestType",
            Url = "http://0.0.0.0:5000",
            Version = "1.0.0"
        };

        // Act
        StatusResponse result = await _service.Register(request, _serverCallContext);

        // Assert
        Assert.Equal(expectedId.ToString(), result.Id);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Test_Unregister(bool hasValidId, bool isRegistered)
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        _mockKeeperService.Setup(s => s.Unregister(expectedId)).Returns(new ServiceEntry());
        _mockKeeperService.Setup(s => s.Unregister(It.IsNotIn(expectedId))).Throws<KeyNotFoundException>();

        var request = new UnregisterRequest
        {
            Id = isRegistered ? expectedId.ToString() : Guid.Empty.ToString()
        };

        if (!hasValidId)
        {
            request = new UnregisterRequest
            {
                Id = string.Empty
            };
        }

        // Act
        StatusResponse result = await _service.Unregister(request, _serverCallContext);

        // Assert
        Assert.Equal(request.Id, result.Id);
        if (!hasValidId || !isRegistered)
        {
            Assert.False(result.Success);
            Assert.NotEqual(string.Empty, result.ErrorMessage);
        }
        else
        {
            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Test_Heartbeat(bool hasValidId, bool isRegistered)
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        _mockKeeperService.Setup(s => s.GetAllRegistryEntriesAsync()).ReturnsAsync(new List<ServiceEntry> {
            new ServiceEntry {
                Id = expectedId
            }
        });
        var request = new HeartbeatRequest
        {
            Id = isRegistered ? expectedId.ToString() : Guid.Empty.ToString()
        };

        if (!hasValidId)
        {
            request = new HeartbeatRequest
            {
                Id = string.Empty
            };
        }

        // Act
        StatusResponse result = await _service.Heartbeat(request, _serverCallContext);

        // Assert
        Assert.Equal(request.Id, result.Id);
        if (!hasValidId || !isRegistered)
        {
            Assert.False(result.Success);
            Assert.NotEqual(string.Empty, result.ErrorMessage);
        }
        else
        {
            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }
    }

    [Theory]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, false, false, false, false)]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(true, false, true, true, true, true)]
    [InlineData(true, false, false, false, true, true)]
    [InlineData(true, false, false, false, false, true)]
    [InlineData(false, true, true, true, true, true)]
    [InlineData(false, false, true, true, true, true)]
    [InlineData(false, false, false, false, true, true)]
    [InlineData(false, false, false, false, false, true)]
    public async Task Test_GetServices(bool hasValidId, bool filterId, bool filterName, bool filterUniqueName, bool filterType, bool filterVersion)
    {
        // Arrange
        Guid expectedId = Guid.NewGuid();
        string expectedName = "Test";
        string expectedUniqueName = "Test-1";
        string expectedType = "AyBorg.Agent";
        string expectedVersion = "1.0.0.0";
        Guid? searchId = hasValidId ? expectedId : null!;
        var request = new GetServicesRequest
        {
            Id = filterId ? expectedId.ToString() : string.Empty,
            Name = filterName ? expectedName : string.Empty,
            UniqueName = filterUniqueName ? expectedUniqueName : string.Empty,
            Type = filterType ? expectedType : string.Empty,
            Version = filterVersion ? expectedVersion : string.Empty
        };

        _mockKeeperService.Setup(s => s.GetAllRegistryEntriesAsync()).ReturnsAsync(new List<ServiceEntry> {
            new ServiceEntry {
                Id = expectedId,
                Name = expectedName,
                UniqueName = expectedUniqueName,
                Type = expectedType,
                Version = expectedVersion
            },
            new ServiceEntry {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                UniqueName = string.Empty,
                Type = string.Empty,
                Version = string.Empty
            }
        });

        // Act
        GetServicesResponse result = await _service.GetServices(request, _serverCallContext);

        // Assert
        Assert.NotNull(result);
        if (!filterId && !filterName && !filterUniqueName && !filterType && !filterVersion)
        {
            Assert.Equal(2, result.Services.Count);
            return;
        }

        Assert.Equal(expectedId.ToString(), result.Services.First().Id);
        Assert.Equal(expectedName, result.Services.First().Name);
        Assert.Equal(expectedUniqueName, result.Services.First().UniqueName);
        Assert.Equal(expectedType, result.Services.First().Type);
        Assert.Equal(expectedVersion, result.Services.First().Version);
    }
}
