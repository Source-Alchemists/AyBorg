using System.Security.Claims;
using AyBorg.Agent.Tests.Helpers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public abstract class BaseGrpcServiceTests<TService, TClient>
    where TService : class
    where TClient : ClientBase<TClient>
{
    protected static readonly NullLogger<TService> s_logger = new();
    protected readonly Mock<TClient> _mockClient = new();
    protected readonly Mock<ClaimsPrincipal> _mockContextUser = new();
    protected readonly DefaultHttpContext _httpContext = new();
    protected readonly TestServerCallContext _serverCallContext;
    protected readonly CancellationTokenSource _serverCallContextCancellationTokenSource;

    protected TService _service = null!;

    protected BaseGrpcServiceTests()
    {
        _serverCallContextCancellationTokenSource = new CancellationTokenSource();
        _httpContext.Request.Headers.Add("Authorization", "TokenValue");
        _httpContext.User = _mockContextUser.Object;
        _serverCallContext = TestServerCallContext.Create(null, _serverCallContextCancellationTokenSource.Token);
        _serverCallContext.UserState["__HttpContext"] = _httpContext;
    }
}
