using System.Buffers;
using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Services.AppState;
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
            foreach (Port portModel in stepModel.Ports!)
            {
                await LazyLoadAsync(portModel, null);
            }
            result.Add(stepModel);
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
            _logger.LogWarning("Could not find step with id {StepId} in iteration {IterationId}", stepId, iterationId);
            return null!;
        }

        Step stepModel = _rpcMapper.FromRpc(resultStep);
        if (updatePorts)
        {
            foreach (Port portModel in stepModel.Ports!)
            {
                if (portModel.Direction == SDK.Common.Ports.PortDirection.Output && skipOutputPorts)
                {
                    // Nothing to do as we only need to update input ports
                    continue;
                }
                await LazyLoadAsync(portModel, iterationId);
            }
        }
        return stepModel;
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
            _logger.LogWarning("Could not find link with id {LinkId}", linkId);
            return null!;
        }

        return _rpcMapper.FromRpc(resultLink);
    }

    /// <summary>
    /// Adds the step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<Step> AddStepAsync(Guid stepId, int x, int y)
    {
        try
        {
            AddFlowStepResponse response = await _editorClient.AddFlowStepAsync(new AddFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                StepId = stepId.ToString(),
                X = x,
                Y = y
            });

            return _rpcMapper.FromRpc(response.Step);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error adding step");
            return null!;
        }
    }

    /// <summary>
    /// Removes the step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveStepAsync(Guid stepId)
    {
        try
        {
            _ = await _editorClient.DeleteFlowStepAsync(new DeleteFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                StepId = stepId.ToString()
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error deleting step");
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
            _logger.LogWarning(ex, "Error moving step");
            return false;
        }
    }

    /// <summary>
    /// Add link between ports asynchronous.
    /// </summary>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    public async ValueTask<Guid?> AddLinkAsync(Guid sourcePortId, Guid targetPortId)
    {
        try
        {
            LinkFlowPortsResponse response = await _editorClient.LinkFlowPortsAsync(new LinkFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                SourceId = sourcePortId.ToString(),
                TargetId = targetPortId.ToString()
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
            _logger.LogWarning(ex, "Error linking ports");
            return null;
        }
    }

    /// <summary>
    /// Removes the link asynchronous.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveLinkAsync(Guid linkId)
    {
        try
        {
            _ = await _editorClient.LinkFlowPortsAsync(new LinkFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                SourceId = linkId.ToString(),
                TargetId = string.Empty
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error linking ports");
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
        await LazyLoadAsync(portModel, iterationId);
        return portModel;
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
            _ = await _editorClient.UpdateFlowPortAsync(new UpdateFlowPortRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                Port = _rpcMapper.ToRpc(port)
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error setting port value");
            return false;
        }
    }

    private async ValueTask LazyLoadAsync(Port portModel, Guid? iterationId)
    {
        if (portModel.Brand == SDK.Common.Ports.PortBrand.Image && portModel.Direction == SDK.Common.Ports.PortDirection.Input)
        {
            // Need to transfer the image
            AsyncServerStreamingCall<ImageChunkDto> imageResponse = _editorClient.GetImageStream(new GetImageStreamRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                PortId = portModel.Id.ToString(),
                IterationId = iterationId == null ? Guid.Empty.ToString() : iterationId.ToString(),
                AsThumbnail = true

            });

            portModel.Value = await CreateImageFromChunksAsync(imageResponse, true);
        }
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
                    resultImage.Width = chunk.FullWidth;
                    resultImage.Height = chunk.FullHeight;
                }

                Memory<byte> targetMemorySlice = memoryOwner.Memory.Slice(offset, chunk.Data.Length);
                offset += chunk.Data.Length;
                chunk.Data.Memory.CopyTo(targetMemorySlice);
            }

            if (memoryOwner != null)
            {
                resultImage.Base64 = Convert.ToBase64String(memoryOwner.Memory.Span);
            }
        }
        finally
        {
            memoryOwner?.Dispose();
        }

        return resultImage;
    }
}
