using System.Security.Claims;
using AyBorg.Gateway.Tests.Helpers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Gateway.Services.Tests;

public abstract class BaseGrpcServiceTests<TService, TClient>
    where TService : class
    where TClient : ClientBase<TClient>
{
    protected static readonly NullLogger<TService> s_logger = new();
    protected readonly Mock<TClient> _mockClient = new();
    protected readonly Mock<IGrpcChannelService> _mockGrpcChannelService = new();
    protected readonly Mock<ClaimsPrincipal> _mockContextUser = new();
    protected readonly DefaultHttpContext _httpContext = new();
    protected readonly TestServerCallContext _serverCallContext;
    protected readonly CancellationTokenSource _serverCallContextCancellationTokenSource;

    protected TService _service = null!;

    protected BaseGrpcServiceTests()
    {
        _serverCallContextCancellationTokenSource = new CancellationTokenSource();
        _httpContext.Request.Headers.Append("Authorization", "TokenValue");
        _httpContext.User = _mockContextUser.Object;
        _serverCallContext = TestServerCallContext.Create(null, _serverCallContextCancellationTokenSource.Token);
        _serverCallContext.UserState["__HttpContext"] = _httpContext;

        _mockGrpcChannelService.Setup(s => s.CreateClient<TClient>(It.IsAny<string>())).Returns(_mockClient.Object);
    }
}
