using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Tests.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class RuntimeServiceTests
{
    private static readonly NullLogger<RuntimeService> s_logger = new();
    private readonly Mock<IStateService> _mockStateService = new();
    private readonly Mock<Runtime.RuntimeClient> _mockRuntimeClient = new();
    private readonly RuntimeService _service;

    public RuntimeServiceTests()
    {
        _mockStateService.Setup(m => m.AgentState).Returns(new UiAgentState
        {
            UniqueName = "Test"
        });

        _service = new RuntimeService(s_logger, _mockStateService.Object, _mockRuntimeClient.Object);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_GetStatusAsync(bool isSuccessful)
    {
        // Arrange
        if (isSuccessful)
        {
            var response = new GetRuntimeStatusResponse();
            response.EngineMetaInfos.Add(new EngineMetaDto {
                Id = Guid.NewGuid().ToString(),
                State = 0,
                ExecutionType = 0,
                StartTime = new Google.Protobuf.WellKnownTypes.Timestamp(),
                StopTime = new Google.Protobuf.WellKnownTypes.Timestamp()
            });
            AsyncUnaryCall<GetRuntimeStatusResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
            _mockRuntimeClient.Setup(m => m.GetStatusAsync(It.IsAny<GetRuntimeStatusRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        }
        else
        {
            _mockRuntimeClient.Setup(m => m.GetStatusAsync(It.IsAny<GetRuntimeStatusRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));
        }

        // Act
        SDK.System.Runtime.EngineMeta result = await _service.GetStatusAsync();

        // Assert
        if (isSuccessful)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_StartRunAsync(bool isSuccessful)
    {
        // Arrange
        if (isSuccessful)
        {
            var response = new StartRunResponse();
            response.EngineMetaInfos.Add(new EngineMetaDto {
                Id = Guid.NewGuid().ToString(),
                State = 0,
                ExecutionType = 0,
                StartTime = new Google.Protobuf.WellKnownTypes.Timestamp(),
                StopTime = new Google.Protobuf.WellKnownTypes.Timestamp()
            });
            AsyncUnaryCall<StartRunResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
            _mockRuntimeClient.Setup(m => m.StartRunAsync(It.IsAny<StartRunRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        }
        else
        {
            _mockRuntimeClient.Setup(m => m.StartRunAsync(It.IsAny<StartRunRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));
        }

        // Act
        SDK.System.Runtime.EngineMeta result = await _service.StartRunAsync(SDK.System.Runtime.EngineExecutionType.SingleRun);

        // Assert
        if (isSuccessful)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_StopRunAsync(bool isSuccessful)
    {
        // Arrange
        if (isSuccessful)
        {
            var response = new StopRunResponse();
            response.EngineMetaInfos.Add(new EngineMetaDto {
                Id = Guid.NewGuid().ToString(),
                State = 0,
                ExecutionType = 0,
                StartTime = new Google.Protobuf.WellKnownTypes.Timestamp(),
                StopTime = new Google.Protobuf.WellKnownTypes.Timestamp()
            });
            AsyncUnaryCall<StopRunResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
            _mockRuntimeClient.Setup(m => m.StopRunAsync(It.IsAny<StopRunRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        }
        else
        {
            _mockRuntimeClient.Setup(m => m.StopRunAsync(It.IsAny<StopRunRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));
        }

        // Act
        SDK.System.Runtime.EngineMeta result = await _service.StopRunAsync();

        // Assert
        if (isSuccessful)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_AbortRunAsync(bool isSuccessful)
    {
        // Arrange
        if (isSuccessful)
        {
            var response = new AbortRunResponse();
            response.EngineMetaInfos.Add(new EngineMetaDto {
                Id = Guid.NewGuid().ToString(),
                State = 0,
                ExecutionType = 0,
                StartTime = new Google.Protobuf.WellKnownTypes.Timestamp(),
                StopTime = new Google.Protobuf.WellKnownTypes.Timestamp()
            });
            AsyncUnaryCall<AbortRunResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
            _mockRuntimeClient.Setup(m => m.AbortRunAsync(It.IsAny<AbortRunRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        }
        else
        {
            _mockRuntimeClient.Setup(m => m.AbortRunAsync(It.IsAny<AbortRunRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));
        }

        // Act
        SDK.System.Runtime.EngineMeta result = await _service.AbortRunAsync();

        // Assert
        if (isSuccessful)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }
}
