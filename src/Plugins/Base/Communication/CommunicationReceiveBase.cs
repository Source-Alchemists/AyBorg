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

using AyBorg.Types;
using AyBorg.Types.Communication;
using AyBorg.Types.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationReceiveBase : CommunicationBase
{
    private readonly ILogger<CommunicationReceiveBase> _logger;
    private bool _isDisposed = false;
    protected readonly NumericPort _timeoutMsPort = new("Timeout (ms)", PortDirection.Input, 10000, -1, int.MaxValue);
    protected string _lastMessageId = string.Empty;
    protected IMessageSubscription _subscription = null!;
    protected bool _hasNewMessage = false;

    protected CommunicationReceiveBase(ILogger<CommunicationReceiveBase> logger, IDeviceManager deviceManager) : base(logger, deviceManager)
    {
        _logger = logger;
        _ports = _ports.Add(_timeoutMsPort);
        _ports = _ports.Add(_messageIdPort);
    }

    public override async ValueTask BeforeStartAsync()
    {
        await base.BeforeStartAsync();
        await SubcripeAsync();
    }

    public override async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _hasNewMessage = false;
        ChangeDevice();
        await SubcripeAsync();

        int count = 0;
        while (!_hasNewMessage && !cancellationToken.IsCancellationRequested)
        {
            // Dont add the cancellation token here, because we want to wait for the timeout
            await Task.Delay(1, default);
            count++;
            if (count > _timeoutMsPort.Value && _timeoutMsPort.Value != -1)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Result), "Timeout while waiting for message");
                return false;
            }
        }

        return !cancellationToken.IsCancellationRequested;
    }

    protected virtual async ValueTask SubcripeAsync()
    {
        if (_device == null)
        {
            throw new InvalidOperationException("No device selected");
        }

        if (_subscription != null && _lastMessageId != _messageIdPort.Value)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Unsubscribing from message id {messageId}", _lastMessageId);
            }
            _subscription.Received -= OnMessageReceived;
            await _device!.UnsubscribeAsync(_subscription);
            _subscription = null!;
        }

        if (_subscription == null)
        {
            _subscription = await _device!.SubscribeAsync(_messageIdPort.Value);
            _subscription.Received += OnMessageReceived;
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Subscribed to message id {messageId}", _messageIdPort.Value);
            }
        }

        _lastMessageId = _messageIdPort.Value;
    }

    protected override void Dispose(bool isDisposing)
    {
        if (!_isDisposed && isDisposing)
        {
            if (_subscription != null)
            {
                _subscription.Received -= OnMessageReceived;
            }

            _isDisposed = true;
        }
        base.Dispose(isDisposing);
    }

    protected abstract void OnMessageReceived(object? sender, MessageEventArgs e);
}
