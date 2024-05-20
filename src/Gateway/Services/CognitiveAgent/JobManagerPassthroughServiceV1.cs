using Ayborg.Gateway.Cognitive.Agent.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Cognitive.Agent;

public sealed class JobManagerPassthroughServiceV1 : JobManager.JobManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public JobManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<JobStatusResponse> GetStatus(GetJobStatusRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), null!);
        JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(request.ServiceUniqueName);
        return await client.GetStatusAsync(request, headers);
    }

    public override async Task<Empty> Upload(IAsyncStreamReader<UploadJobRequest> requestStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), null!);
        JobManager.JobManagerClient client = null!;
        AsyncClientStreamingCall<UploadJobRequest, Empty> requestCall = null!;
        try
        {
            await foreach (UploadJobRequest chunk in requestStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                if (client == null)
                {
                    client = _channelService.CreateClient<JobManager.JobManagerClient>(requestStream.Current.ServiceUniqueName);
                    requestCall = client.Upload(headers: headers, cancellationToken: context.CancellationToken);
                }

                await requestCall.RequestStream.WriteAsync(chunk, context.CancellationToken);
            }

            await requestCall.RequestStream.CompleteAsync();
            await requestCall;
        }
        finally
        {
            requestCall?.Dispose();
        }

        return new Empty();
    }

    public override async Task Download(DownloadJobRequest request, IServerStreamWriter<JobChunk> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), null!);
        JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(request.ServiceUniqueName);
        AsyncServerStreamingCall<JobChunk> stream = client.Download(request, headers: headers);
        await foreach (JobChunk chunk in stream.ResponseStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(chunk);
        }
    }

    public override async Task<Empty> ConfirmDownload(DownloadJobRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), null!);
        JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(request.ServiceUniqueName);
        return await client.ConfirmDownloadAsync(request, headers);
    }
}
