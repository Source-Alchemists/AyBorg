using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Atomy.ServiceRegistry.Controllers;
using Atomy.ServiceRegistry.Services;


namespace Atomy.ServiceRegistry.Tests;

public class ServiceControllerTests
{
    private readonly NullLogger<ServicesController> _logger = new NullLogger<ServicesController>();

    [Fact]
    public async Task RegisterAsync_success()
    {
        // Arrange
        var entry = new SDK.Data.DTOs.ServiceRegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };
        var mockedService = new Mock<IKeeperService>();
        mockedService.Setup(x => x.RegisterAsync(entry)).Returns(Task.Run(() => Guid.NewGuid()));

        var controller = new ServicesController(_logger, mockedService.Object);

        // Act
        var actionResult = await controller.RegisterAsync(entry);
        var result = actionResult.Result as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        Assert.NotEqual(Guid.Empty, result?.Value);
    }

    [Fact]
    public async Task RegisterAsync_alreadyReported()
    {
        // Arrange
        var entry = new SDK.Data.DTOs.ServiceRegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };
        var mockedService = new Mock<IKeeperService>();
        mockedService.Setup(x => x.RegisterAsync(entry)).Throws<InvalidOperationException>();

        var controller = new ServicesController(_logger, mockedService.Object);

        // Act
        var actionResult = await controller.RegisterAsync(entry);
        var result = actionResult.Result as StatusCodeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status208AlreadyReported, result?.StatusCode);
    }

    [Fact]
    public async Task UnegisterAsync_success()
    {
        // Arrange
        var mockedService = new Mock<IKeeperService>();
        mockedService.Setup(x => x.UnregisterAsync(Moq.It.IsAny<Guid>())).Returns(Task.Run(() => Guid.NewGuid()));

        var controller = new ServicesController(_logger, mockedService.Object);

        // Act
        var actionRresult = await controller.UnregisterAsync(Guid.NewGuid());
        var result = actionRresult as StatusCodeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
    }

    [Fact]
    public async Task UnegisterAsync_unknownService()
    {
        // Arrange
        var mockedService = new Mock<IKeeperService>();
        mockedService.Setup(x => x.UnregisterAsync(Moq.It.IsAny<Guid>())).Throws<KeyNotFoundException>();

        var controller = new ServicesController(_logger, mockedService.Object);

        // Act
        var actionRresult = await controller.UnregisterAsync(Guid.NewGuid());
        var result = actionRresult as StatusCodeResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status204NoContent, result?.StatusCode);
    }
}