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

using AyBorg.Runtime;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;

namespace AyBorg.Data.Mapper;

public class RuntimeMapper : IRuntimeMapper
{
    public StepModel FromRuntime(IStepProxy stepProxy, bool skipPorts = false)
    {
        var ports = new List<PortModel>();
        if (!skipPorts)
        {
            foreach (IPort port in stepProxy.Ports)
            {
                ports.Add(FromRuntime(port));
            }
        }

        return new StepModel
        {
            Id = stepProxy.Id,
            Name = stepProxy.Name,
            Categories = stepProxy.Categories,
            X = stepProxy.X,
            Y = stepProxy.Y,
            ExecutionTimeMs = stepProxy.ExecutionTimeMs,
            MetaInfo = stepProxy.MetaInfo with {},
            Ports = ports
        };
    }

    public PortModel FromRuntime(IPort runtimePort)
    {
        IPortMapper portMapper = PortMapperFactory.CreateMapper(runtimePort);
        return portMapper.ToModel(runtimePort);
    }
}
