using System.Security.Claims;
using AyBorg.Gateway.Services;
using AyBorg.Gateway.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AyBorg.Gateway.Tests.Services;

public abstract class BaseGrpcServiceTests<TClient> where TClient : Grpc.Core.ClientBase<TClient>
{
    protected readonly Mock<TClient> _mockClient = new();
    protected readonly Mock<IGrpcChannelService> _mockGrpcChannelService = new();
    protected readonly Mock<ClaimsPrincipal> _mockContextUser = new();
    protected readonly DefaultHttpContext _httpContext = new();
    protected readonly TestServerCallContext _serverCallContext;
    protected readonly CancellationTokenSource _serverCallContextCancellationTokenSource;

    public BaseGrpcServiceTests()
    {
        _serverCallContextCancellationTokenSource = new CancellationTokenSource();
        _httpContext.Request.Headers.Add("Authorization", "TokenValue");
        _httpContext.User = _mockContextUser.Object;
        _serverCallContext = TestServerCallContext.Create(null, _serverCallContextCancellationTokenSource.Token);
        _serverCallContext.UserState["__HttpContext"] = _httpContext;

        _mockGrpcChannelService.Setup(s => s.CreateClient<TClient>(It.IsAny<string>())).Returns(_mockClient.Object);
    }
}
