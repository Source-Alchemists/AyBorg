namespace AyBorg.Gateway.Services.Audit;

public sealed class AuditPassthroughServiceV1 : Ayborg.Gateway.Audit.V1.Audit.AuditBase
{
    private readonly IGrpcChannelService _channelService;

    public AuditPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }
}
