using System.Buffers;
using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Shared.Models;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public class FlowService : IFlowService
{
    private readonly ILogger<FlowService> _logger;
    private readonly IStateService _stateService;
    private readonly IRpcMapper _rpcMapper;
    private readonly Editor.EditorClient _editorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="rpcMapper">The RPC mapper.</param>
    /// <param name="editorClient">The editor client.</param>
    public FlowService(ILogger<FlowService> logger,
                        IStateService stateService,
                        IRpcMapper rpcMapper,
                        Editor.EditorClient editorClient)
    {
        _logger = logger;
        _stateService = stateService;
        _rpcMapper = rpcMapper;
        _editorClient = editorClient;
    }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <returns>The steps.</returns>
    public async ValueTask<IEnumerable<Step>> GetStepsAsync()
    {
        GetFlowStepsResponse response = await _editorClient.GetFlowStepsAsync(new GetFlowStepsRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName
        });
        var result = new List<Step>();
        foreach (StepDto? s in response.Steps)
        {
            Step stepModel = _rpcMapper.FromRpc(s);
            var ports = new List<Port>();
            foreach (Port portModel in stepModel.Ports!)
            {
                if (portModel.Brand == PortBrand.Image && portModel.Direction == PortDirection.Input)
                {
                    ports.Add(await LazyLoadImagePortAsync(portModel, null));
                }
                else
                {
                    ports.Add(portModel);
                }
            }
            result.Add(stepModel with { Ports = ports });
        }

        return result;
    }

    /// <summary>
    /// Gets the step.
    /// </summary>
    /// <param name="stepId">The step id.</param>
    /// <param name="iterationId">The iteration id.</param>
    /// <param name="updatePorts">if set to <c>true</c> [update ports].</param>
    /// <param name="skipOutputPorts">if set to <c>true</c> [skip output ports].</param>
    /// <returns>The step.</returns>
    public async ValueTask<Step> GetStepAsync(Guid stepId, Guid? iterationId = null, bool updatePorts = true, bool skipOutputPorts = true)
    {
        try
        {
            var request = new GetFlowStepsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                IterationId = iterationId == null ? Guid.Empty.ToString() : iterationId.ToString()
            };
            request.StepIds.Add(stepId.ToString());
            GetFlowStepsResponse response = await _editorClient.GetFlowStepsAsync(request);
            StepDto? resultStep = response.Steps.FirstOrDefault();
            if (resultStep == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), "Could not find step with id [{stepId}] in iteration [{iterationId}]!", stepId, iterationId);
                return null!;
            }

            Step stepModel = _rpcMapper.FromRpc(resultStep);
            if (updatePorts)
            {
                var ports = new List<Port>();
                foreach (Port portModel in stepModel.Ports!)
                {
                    if (portModel.Direction == PortDirection.Output && skipOutputPorts)
                    {
                        // Nothing to do as we only need to update input ports
                        continue;
                    }
                    ports.Add(await GetPortAsync(portModel.Id, iterationId));
                }

                stepModel = stepModel with { Ports = ports };
            }
            return stepModel;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error getting step!");
            return null!;
        }
    }

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <returns>The links.</returns>
    public async ValueTask<IEnumerable<Link>> GetLinksAsync()
    {
        GetFlowLinksResponse response = await _editorClient.GetFlowLinksAsync(new GetFlowLinksRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName
        });
        var result = new List<Link>();
        foreach (LinkDto? l in response.Links)
        {
            result.Add(_rpcMapper.FromRpc(l));
        }

        return result;
    }

    /// <summary>
    /// Gets the link.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns>The link.</returns>
    public async ValueTask<Link> GetLinkAsync(Guid linkId)
    {
        var request = new GetFlowLinksRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName
        };
        request.LinkIds.Add(linkId.ToString());
        GetFlowLinksResponse response = await _editorClient.GetFlowLinksAsync(request);
        LinkDto? resultLink = response.Links.FirstOrDefault();
        if (resultLink == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), "Could not find link with id [{linkId}]!", linkId);
            return new Link();
        }

        return _rpcMapper.FromRpc(resultLink);
    }

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>Added step.</returns>
    public async ValueTask<Step> AddStepAsync(Step step)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Adding step [{stepName}] at [{x}, {y}].", step.Name, step.X, step.Y);
            AddFlowStepResponse response = await _editorClient.AddFlowStepAsync(new AddFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                StepId = step.Id.ToString(),
                X = step.X,
                Y = step.Y
            });

            return _rpcMapper.FromRpc(response.Step);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error adding step!");
            return null!;
        }
    }

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveStepAsync(Step step)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Removing step [{stepName}] with id [{stepId}].", step.Name, step.Id);
            _ = await _editorClient.DeleteFlowStepAsync(new DeleteFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                StepId = step.Id.ToString()
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error deleting step!");
            return false;
        }
    }

    /// <summary>
    /// Moves the step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryMoveStepAsync(Guid stepId, int x, int y)
    {
        try
        {
            _logger.LogTrace(new EventId((int)EventLogType.UserInteraction), "Moving step [{stepId}] to [{x},{y}].", stepId, x, y);
            _ = await _editorClient.MoveFlowStepAsync(new MoveFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                StepId = stepId.ToString(),
                X = x,
                Y = y
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error moving step!");
            return false;
        }
    }

    /// <summary>
    /// Add link between ports.
    /// </summary>
    /// <param name="sourcePort">The source port.</param>
    /// <param name="targetPort">The target port.</param>
    /// <returns></returns>
    public async ValueTask<Guid?> AddLinkAsync(Port sourcePort, Port targetPort)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Linking port [{sourcePortName}] to [{targetPortName}].", sourcePort.Name, targetPort.Name);
            LinkFlowPortsResponse response = await _editorClient.LinkFlowPortsAsync(new LinkFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                SourceId = sourcePort.Id.ToString(),
                TargetId = targetPort.Id.ToString()
            });
            if (Guid.TryParse(response.LinkId, out Guid linkId))
            {
                return linkId;
            }
            else
            {
                return Guid.Empty;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error linking ports!");
            return null;
        }
    }

    /// <summary>
    /// Removes the link.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveLinkAsync(Link link)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Removing link [{link}].", link);
            _ = await _editorClient.LinkFlowPortsAsync(new LinkFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                SourceId = link.Id.ToString(),
                TargetId = string.Empty
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error linking ports!");
            return false;
        }
    }

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    public async ValueTask<Port> GetPortAsync(Guid portId, Guid? iterationId = null)
    {
        try
        {
            var request = new GetFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                IterationId = iterationId == null ? Guid.Empty.ToString() : iterationId.ToString()
            };
            request.PortIds.Add(portId.ToString());
            GetFlowPortsResponse response = await _editorClient.GetFlowPortsAsync(request);
            PortDto? resultPort = response.Ports.FirstOrDefault();
            if (resultPort == null)
            {
                return null!;
            }

            Port portModel = _rpcMapper.FromRpc(resultPort);
            if (portModel.Brand == PortBrand.Image)
            {
                return await LazyLoadImagePortAsync(portModel, iterationId);
            }
            else
            {
                return portModel;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error getting port!");
            return null!;
        }
    }

    /// <summary>
    /// Try to set the port value asynchronous.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    public async ValueTask<bool> TrySetPortValueAsync(Port port)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Change port value [{port}].", port);
            _ = await _editorClient.UpdateFlowPortAsync(new UpdateFlowPortRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                Port = _rpcMapper.ToRpc(port)
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Error changing port value!");
            return false;
        }
    }

    private async ValueTask<Port> LazyLoadImagePortAsync(Port portModel, Guid? iterationId)
    {
        // Need to transfer the image
        AsyncServerStreamingCall<ImageChunkDto> imageResponse = _editorClient.GetImageStream(new GetImageStreamRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            PortId = portModel.Id.ToString(),
            IterationId = iterationId == null ? Guid.Empty.ToString() : iterationId.ToString(),
            AsThumbnail = true

        });

        return portModel with { Value = await CreateImageFromChunksAsync(imageResponse, true) };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask<Image> CreateImageFromChunksAsync(AsyncServerStreamingCall<ImageChunkDto> imageResponse, bool isThumbnail)
    {
        IMemoryOwner<byte> memoryOwner = null!;
        var resultImage = new Image
        {
            EncoderType = isThumbnail ? ImageTorque.Processing.EncoderType.Jpeg : ImageTorque.Processing.EncoderType.Png
        };

        try
        {
            int offset = 0;
            await foreach (ImageChunkDto? chunk in imageResponse.ResponseStream.ReadAllAsync())
            {
                if (memoryOwner == null)
                {
                    memoryOwner = MemoryPool<byte>.Shared.Rent((int)chunk.FullStreamLength);
                    resultImage = resultImage with
                    {
                        Meta = new ImageMeta
                        {
                            Width = chunk.FullWidth,
                            Height = chunk.FullHeight
                        },
                        ScaledWidth = chunk.ScaledWidth,
                        ScaledHeight = chunk.ScaledHeight
                    };
                }

                Memory<byte> targetMemorySlice = memoryOwner.Memory.Slice(offset, chunk.Data.Length);
                offset += chunk.Data.Length;
                chunk.Data.Memory.CopyTo(targetMemorySlice);
            }

            if (memoryOwner != null)
            {
                resultImage = resultImage with { Base64 = Convert.ToBase64String(memoryOwner.Memory.Span) };
            }
        }
        finally
        {
            memoryOwner?.Dispose();
        }

        return resultImage;
    }
}
