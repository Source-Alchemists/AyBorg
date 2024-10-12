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

using System.Runtime.CompilerServices;
using Ayborg.Gateway.Analytics.V1;
using AyBorg.Runtime;
using AyBorg.Types;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Shared.Mappers;

internal static class EventLogMapper
{
    public static EventLogEntry Map(EventEntry entry)
    {
        return new EventLogEntry
        {
            ServiceType = entry.ServiceType,
            ServiceUniqueName = entry.ServiceUniqueName,
            Timestamp = entry.Timestamp.ToDateTime(),
            LogLevel = (LogLevel)entry.LogLevel,
            EventId = entry.EventId,
            EventName = GetEventTypeDescription(entry.EventId),
            Message = entry.Message
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetEventTypeDescription(int id)
    {
        if (!Enum.IsDefined(typeof(EventLogType), id))
        {
            return "Undefined";
        }

        var eventLogType = (EventLogType)id;
        return $"{eventLogType.GetDescription()}";
    }
}
