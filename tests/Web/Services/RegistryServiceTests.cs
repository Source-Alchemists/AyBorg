using Ayborg.Gateway.V1;
using AyBorg.Web.Services;
using AyBorg.Web.Tests.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Tests.Services;

public class RegistryServiceTests
{
    private static readonly NullLogger<RegistryService> s_logger = new();
    protected readonly Mock<Register.RegisterClient> _mockRegisterClient = new();
    private readonly RegistryService _service;

    public RegistryServiceTests()
    {
        _service = new RegistryService(s_logger, _mockRegisterClient.Object);
    }

    [Fact]
    public async ValueTask Test_ReceiveServicesAsync()
    {
        // Arrange
        var response = new GetServicesResponse();
        response.Services.Add(new ServiceInfo());
        AsyncUnaryCall<GetServicesResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockRegisterClient.Setup(m => m.GetServicesAsync(It.IsAny<GetServicesRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        IEnumerable<Shared.Models.ServiceInfoEntry> result = await _service.ReceiveServicesAsync("Test");

        // Assert
        Assert.Single(result);
    }
}
