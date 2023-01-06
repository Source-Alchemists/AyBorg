using AyBorg.SDK.Communication.gRPC;

namespace AyBorg.Gateway.Models;

public record Notification(string ServiceUniqueName, NotifyType NotifyType, string Payload);
