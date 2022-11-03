using System.Text;
using System.Text.Json;
using Blazor.Diagrams.Core.Models;
using MQTTnet;
using Atomy.SDK;
using Atomy.SDK.DTOs;
using Atomy.SDK.Ports;
using Atomy.Web.Pages.Agent.Editor.Nodes;
using Atomy.Web.Services.Agent;
using Atomy.SDK.MQTT;
using Atomy.Web.Services;
using System.Globalization;

public class FlowPort : PortModel, IDisposable
{
    private readonly IFlowService _flowService;
    private readonly IStateService _stateService;
    private readonly IMqttClientProvider _mqttClientProvider;
    private readonly StepDto _step;
    private Atomy.SDK.MQTT.MqttSubscription? _subscription;
    private bool disposedValue;

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
    public PortDto Port { get; private set; }

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
    public FlowPort(FlowNode node, PortDto port, StepDto parent, 
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
        var newPort = await _flowService.GetPortAsync(_stateService.AgentState.BaseUrl, Port.Id);
        if (newPort == null) return;
        Port = newPort;
        PortChanged?.Invoke();
    }

    /// <summary>
    /// Sends the value to the server.
    /// </summary>
    public async Task SendValueAsync()
    {
        if (!await _flowService.TrySetPortValueAsync(_stateService.AgentState.BaseUrl, Port))
        {
            throw new Exception("Failed to set port value.");
        }
    }

    private async void MqttSubscribe()
    {
        _subscription = await _mqttClientProvider.SubscribeAsync($"atomy/agents/{_stateService.AgentState.UniqueName}/engine/steps/{_step.Id}/ports/{Port.Id}/#");
        _subscription.MessageReceived += MqttMessageReceived;
    }

    private void MqttMessageReceived(MqttApplicationMessage e)
    {
        var topic = e.Topic;
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if(_subscription != null)
                {
                    _subscription.MessageReceived -= MqttMessageReceived;
                    _mqttClientProvider.UnsubscribeAsync(_subscription);
                }
            }
            
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}