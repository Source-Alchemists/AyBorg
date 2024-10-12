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

using System.Text.Json;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Communication;
using AyBorg.Communication.gRPC;
using AyBorg.Communication.gRPC.Models;
using AyBorg.Runtime;
using static AyBorg.Web.Services.NotifyService;

namespace AyBorg.Web.Services;

public sealed class NotifyBackgroundService : BackgroundService
{
    private readonly Notify.NotifyClient _notifyClient;
    private readonly INotifyService _notifyService;
    private readonly IServiceConfiguration _serviceConfiguration;

    public NotifyBackgroundService(Notify.NotifyClient notifyClient, INotifyService notifyService, IServiceConfiguration serviceConfiguration)
    {
        _notifyClient = notifyClient;
        _notifyService = notifyService;
        _serviceConfiguration = serviceConfiguration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            Grpc.Core.AsyncServerStreamingCall<NotifyMessage> response = _notifyClient.CreateDownstream(new CreateNotifyStreamRequest { ServiceUniqueName = _serviceConfiguration.UniqueName }, cancellationToken: stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await response.ResponseStream.MoveNext(stoppingToken))
                {
                    NotifyMessage notifyMessage = response.ResponseStream.Current;
                    var notifyType = (NotifyType)notifyMessage.Type;
                    IEnumerable<Subscription> matchingSubscriptions = _notifyService.Subscriptions.Where(s => s.ServiceUniqueName.Equals(notifyMessage.AgentUniqueName, StringComparison.InvariantCultureIgnoreCase)
                                                                                            && s.Type.Equals(notifyType));

                    object callbackObject = null!;
                    switch ((NotifyType)notifyMessage.Type)
                    {
                        case NotifyType.AgentEngineStateChanged:
                            EngineMeta? engineMeta = JsonSerializer.Deserialize<EngineMeta>(notifyMessage.Payload);
                            callbackObject = engineMeta!;
                            break;
                        case NotifyType.AgentIterationFinished:
                            callbackObject = Guid.Parse(notifyMessage.Payload);
                            break;
                        case NotifyType.AgentAutomationFlowChanged:
                            AgentAutomationFlowChangeArgs? flowChangeArgs = JsonSerializer.Deserialize<AgentAutomationFlowChangeArgs>(notifyMessage.Payload);
                            callbackObject = flowChangeArgs!;
                            break;
                        default:
                            break;
                    }

                    if (callbackObject != null)
                    {
                        foreach (Subscription sub in matchingSubscriptions)
                        {
                            sub.Callback?.Invoke(callbackObject);
                        }
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }, TaskCreationOptions.LongRunning);
    }
}
