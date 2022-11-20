﻿using System.Text;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Data.DTOs;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using MQTTnet;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowNode : NodeModel, IDisposable
{
    private readonly IFlowService _flowService;
    private readonly IStateService _stateService;
    private readonly IMqttClientProvider _mqttClientProvider;
    private MqttSubscription? _subscription;
    private bool _disposedValue;

    public StepDto Step { get; private set; }

    /// <summary>
    /// Called when a step is updated.
    /// </summary>
    public Action? StepChanged { get; set; }
    public Action? OnDelete { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowNode"/> class.
    /// </summary>
    /// <param name="flowService">The flow service.</param>
    /// <param name="mqttClientProvider">The MQTT client provider.</param>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="step">The step.</param>
    public FlowNode(IFlowService flowService, IMqttClientProvider mqttClientProvider, IStateService stateService, StepDto step, bool locked = false) : base(new Point(step.X, step.Y))
    {
        _flowService = flowService;
        _mqttClientProvider = mqttClientProvider;
        _stateService = stateService;
        Title = step.Name;
        Step = step;
        Locked = locked;

        if (step.Ports == null) return;
        foreach (PortDto port in step.Ports)
        {
            _ = AddPort(new FlowPort(this, port, step, _flowService, _mqttClientProvider, _stateService) { Locked = locked });
        }

        MqttSubscribe();
    }

    private async void MqttSubscribe()
    {
        _subscription = await _mqttClientProvider.SubscribeAsync($"AyBorg/agents/{_stateService.AgentState.UniqueName}/engine/steps/{Step.Id}/executionTimeMs");
        _subscription.MessageReceived += MqttMessageReceived;
    }

    private void MqttMessageReceived(MqttApplicationMessage e)
    {
        Step.ExecutionTimeMs = long.Parse(Encoding.UTF8.GetString(e.Payload));
        StepChanged?.Invoke();
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

    public void Delete()
    {
        OnDelete?.Invoke();
    }
}
