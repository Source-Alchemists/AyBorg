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
using ImageTorque;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationImageReceive : CommunicationReceiveBase
{
    private readonly ILogger<CommunicationImageReceive> _logger;
    private readonly ImagePort _valuePort = new("Value", PortDirection.Output, null!);
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    public override string Name => "Communication.Image.Receive";

    public CommunicationImageReceive(ILogger<CommunicationImageReceive> logger, IDeviceManager deviceManager) : base(logger, deviceManager)
    {
        _logger = logger;
        _ports = _ports.Add(_valuePort);
    }

    protected override void OnMessageReceived(object? sender, MessageEventArgs e)
    {
        if (e.Message.Payload == ArraySegment<byte>.Empty)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Received message with empty payload");
            _valuePort.Value = null!;
        }
        else
        {
            using MemoryStream stream = _memoryStreamManager.GetStream(e.Message.Payload);
            var image = Image.Load(stream);
            _valuePort.Value?.Dispose();
            _valuePort.Value = image;
        }

        _hasNewMessage = true;
    }
}
