using System.Runtime.CompilerServices;
using Ayborg.Gateway.Analytics.V1;
using AyBorg.SDK.Common;
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
