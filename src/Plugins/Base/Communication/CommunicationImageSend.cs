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

using AyBorg.Communication;
using AyBorg.Types.Ports;
using AyBorg.Types;

using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationImageSend : CommunicationSendBase
{
    private readonly ILogger<CommunicationImageSend> _logger;
    private readonly ImagePort _messageValuePort = new("Value", PortDirection.Input, null!);
    private ImagePort? _imagePortClone;
    private bool _isDisposed = false;

    public CommunicationImageSend(ILogger<CommunicationImageSend> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager, communicationStateProvider)
    {
        _logger = logger;
        _ports = _ports.Add(_messageValuePort);
    }

    public override string Name => "Communication.Image.Send";

    public override async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (!_communicationStateProvider.IsResultCommunicationEnabled)
        {
            return true;
        }

        ChangeDevice();

        if (_parallelPort.Value)
        {
            if (_parallelTask != null)
            {
                await _parallelTask;
                _parallelTask.Dispose();
            }

            _imagePortClone?.Dispose();
            _imagePortClone = new ImagePort(_messageValuePort);
            _parallelTask = Task.Run(async () => await SendAsync(_imagePortClone), cancellationToken);
        }
        else
        {
            try
            {
                await SendAsync(_messageValuePort);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Result), ex, "Error while sending message to MQTT");
                return false;
            }
        }
        return true;
    }

    private async ValueTask SendAsync(ImagePort port)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("No device selected");
        }

        if (!await _device.TrySendAsync(_messageIdPort.Value, port))
        {
            throw new CommunicationException("Error while sending message to device");
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        if (isDisposing && !_isDisposed)
        {
            _imagePortClone?.Dispose();
            _isDisposed = true;
        }
    }
}
