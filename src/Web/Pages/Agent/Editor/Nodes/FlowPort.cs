using System.Globalization;
using System.Text;
using System.Text.Json;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Data.Bindings;
using AyBorg.SDK.Data.DTOs;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using Blazor.Diagrams.Core.Models;
using MQTTnet;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowPort : PortModel, IDisposable
{
    private readonly IFlowService _flowService;
    private readonly IStateService _stateService;
    private readonly IMqttClientProvider _mqttClientProvider;
    private readonly Step _step;
    private MqttSubscription? _subscription;
    private bool _disposedValue;

    /// <summary>
    /// Gets the port name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the port direction.
    /// </summary>
    public PortDirection Direction { get; }

    /// <summary>
    /// Gets the port type.
    /// </summary>
    public PortBrand Brand { get; }

    /// <summary>
    /// Gets the port dto.
    /// </summary>
    public Port Port { get; private set; }

    /// <summary>
    /// Called when a link is added or removed.
    /// </summary>
    public Action? PortChanged { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowPort"/> class.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="port">The port.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="flowService">The flow service.</param>
    /// <param name="mqttClientProvider">The MQTT client provider.</param>
    /// <param name="stateService">The state service.</param>
    public FlowPort(FlowNode node, Port port, Step parent,
                IFlowService flowService, IMqttClientProvider mqttClientProvider,
                IStateService stateService) : base(node, port.Direction == PortDirection.Input ? PortAlignment.Left : PortAlignment.Right)
    {
        Port = port;
        Name = port.Name;
        Direction = port.Direction;
        Brand = port.Brand;
        _flowService = flowService;
        _stateService = stateService;
        _mqttClientProvider = mqttClientProvider;
        _step = parent;

        MqttSubscribe();
    }

    /// <summary>
    /// Updates the port.
    /// </summary>
    public async Task UpdateAsync()
    {
        Port newPort = await _flowService.GetPortAsync(_stateService.AgentState.UniqueName, Port.Id);
        if (newPort == null) return;
        Port = newPort;
        PortChanged?.Invoke();
    }

    /// <summary>
    /// Sends the value to the server.
    /// </summary>
    public async Task SendValueAsync()
    {
        if (!await _flowService.TrySetPortValueAsync(_stateService.AgentState.UniqueName, Port))
        {
            throw new Exception("Failed to set port value.");
        }
    }

    private async void MqttSubscribe()
    {
        _subscription = await _mqttClientProvider.SubscribeAsync($"AyBorg/agents/{_stateService.AgentState.UniqueName}/engine/steps/{_step.Id}/ports/{Port.Id}/#");
        _subscription.MessageReceived += MqttMessageReceived;
    }

    private void MqttMessageReceived(MqttApplicationMessage e)
    {
        string topic = e.Topic;
        switch (Port.Brand)
        {
            case PortBrand.String:
            case PortBrand.Folder:
                Port.Value = Encoding.UTF8.GetString(e.Payload);
                break;
            case PortBrand.Numeric:
                {
                    Port.Value = Convert.ToDouble(Encoding.UTF8.GetString(e.Payload), CultureInfo.InvariantCulture);
                    break;
                }
            case PortBrand.Boolean:
                {
                    Port.Value = Convert.ToBoolean(Encoding.UTF8.GetString(e.Payload), CultureInfo.InvariantCulture);
                    break;
                }
            case PortBrand.Rectangle:
                {
                    Port.Value = JsonSerializer.Deserialize<RectangleDto>(Encoding.UTF8.GetString(e.Payload));
                    break;
                }
            case PortBrand.Image:
                {
                    var image = new ImageDto();
                    if (Port.Value != null) image = (ImageDto)Port.Value;
                    else Port.Value = image;
                    if (e.Topic.EndsWith("data")) image.Base64 = Convert.ToBase64String(e.Payload);
                    if (e.Topic.EndsWith("meta")) image.Meta = JsonSerializer.Deserialize<ImageMetaDto>(Encoding.UTF8.GetString(e.Payload))!;
                    break;
                }
        }

        PortChanged?.Invoke();
    }

    protected virtual async Task Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_subscription != null)
                {
                    _subscription.MessageReceived -= MqttMessageReceived;
                    await _mqttClientProvider.UnsubscribeAsync(_subscription);
                }
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true).GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}
