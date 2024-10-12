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
using AyBorg.Types;
using AyBorg.Types.Ports;

using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationSendBase : CommunicationBase
{
    private bool _isDisposed = false;
    protected readonly ICommunicationStateProvider _communicationStateProvider;
    protected Task _parallelTask = null!;
    protected readonly BooleanPort _parallelPort = new("Parallel", PortDirection.Input, false);

    protected CommunicationSendBase(ILogger<CommunicationSendBase> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager)
    {
        _communicationStateProvider = communicationStateProvider;
        _ports = _ports.Add(_parallelPort);
        _ports = _ports.Add(_messageIdPort);
    }

    public abstract override ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);

    protected override void Dispose(bool isDisposing)
    {
        if(isDisposing && !_isDisposed)
        {
            if (_parallelTask != null)
            {
                _parallelTask.Wait(1000); // Wait for 1 second
                _parallelTask.Dispose();
            }

            _isDisposed = true;
        }

        base.Dispose(isDisposing);
    }
}
