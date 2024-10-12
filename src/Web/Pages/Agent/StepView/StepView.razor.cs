/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;
using System.Collections.Immutable;
using AyBorg.Communication.gRPC;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Toolbelt.Blazor.HotKeys2;

namespace AyBorg.Web.Pages.Agent.StepView;

#nullable disable

public partial class StepView : ComponentBase, IDisposable
{
    private readonly ConcurrentQueue<Guid?> _updateQueue = new();
    private bool _disposedValue;
    private HotKeysContext _hotKeysContext = null!;
    private string _serviceUniqueName;

    [Parameter] public string ServiceId { get; init; } = string.Empty;
    [Parameter] public string StepId { get; init; }
    [Inject] public HotKeys HotKeys { get; init; }
    [Inject] public IFlowService FlowService { get; init; }
    [Inject] public INotifyService NotifyService { get; init; }
    [Inject] IRegistryService RegistryService { get; init; }
    [Inject] NavigationManager NavigationManager { get; init; }
    [Inject] ILogger<StepView> Logger { get; init; }

    private bool _isLoading = true;
    private StepModel _step = new();
    private ImmutableList<PortModel> _ports = [];
    private NotifyService.Subscription _iterationFinishedSubscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _hotKeysContext = HotKeys.CreateContext();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            IEnumerable<ServiceInfoEntry> services = await RegistryService!.ReceiveServicesAsync();
            ServiceInfoEntry service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                return;
            }

            _serviceUniqueName = service.UniqueName;

            var stepGuid = Guid.Parse(StepId);
            _step = await FlowService.GetStepAsync(_serviceUniqueName, new StepModel { Id = stepGuid }, null, true, false, false);

            _ports = _ports.Clear();
            _ports = _ports.AddRange(_step.Ports);

            await UpdateNode(null);

            if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
            _iterationFinishedSubscription = NotifyService.Subscribe(_serviceUniqueName, NotifyType.AgentIterationFinished);
            _iterationFinishedSubscription.Callback += IterationFinishedNotificationReceived;
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask UpdateNode(Guid? iterationId)
    {
        try
        {
            _updateQueue.Enqueue(iterationId);
            if (_updateQueue.Count > 1)
            {
                _updateQueue.TryDequeue(out _);
            }

            foreach (Guid? id in _updateQueue)
            {
                await GetUpdatedStepAsync(id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(new EventId((int)EventLogType.Engine), ex, "Error during update node");
        }
    }

    private async ValueTask GetUpdatedStepAsync(Guid? id)
    {
        StepModel fullStep = await FlowService.GetStepAsync(_serviceUniqueName, _step, id, true, false, false);

        foreach (PortModel port in fullStep.Ports)
        {
            if (port.Direction == PortDirection.Input)
            {
                PortModel inputPort = _ports.Find(p => p.Id.Equals(port.Id));
                if (inputPort == null || !_ports.Contains(inputPort)) continue;
                _ports = _ports.Replace(inputPort, port);
            }
            else if (port.Direction == PortDirection.Output)
            {
                PortModel outputPort = _ports.Find(p => p.Id.Equals(port.Id));
                if (outputPort == null || !_ports.Contains(outputPort)) continue;
                _ports = _ports.Replace(outputPort, port);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private static bool IsDisabled(PortModel port)
    {
        if (port.Direction == PortDirection.Output)
        {
            return true;
        }

        if (port.IsConnected)
        {
            return true;
        }

        return false;
    }

    private async void IterationFinishedNotificationReceived(object obj)
    {
        Guid iterationId = (Guid)obj;
        await UpdateNode(iterationId);
    }

    private void BackClicked()
    {
        NavigationManager.NavigateTo($"/agents/editor/{ServiceId}");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
                _hotKeysContext.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
