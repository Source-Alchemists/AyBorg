using Ayborg.Gateway.Net.V1;
using AyBorg.SDK.Common;
using Google.Protobuf;
using Grpc.Core;

namespace AyBorg.Web.Services.Net;

public class FileManagerService : IFileManagerService
{
    private const int CHUNK_SIZE = 32768;
    private readonly ILogger<FileManagerService> _logger;
    private readonly FileManager.FileManagerClient _fileManagerClient;

    public FileManagerService(ILogger<FileManagerService> logger, FileManager.FileManagerClient client)
    {
        _logger = logger;
        _fileManagerClient = client;
    }

    public async ValueTask SendImageAsync(SendImageParameters parameters)
    {
        try
        {
            using AsyncClientStreamingCall<ImageUploadRequest, Google.Protobuf.WellKnownTypes.Empty> request = _fileManagerClient.UploadImage(cancellationToken: CancellationToken.None);

            int bytesToSend = parameters.Data.Length;
            int bufferSize = bytesToSend < CHUNK_SIZE ? bytesToSend : CHUNK_SIZE;
            int offset = 0;

            ReadOnlyMemory<byte> readOnlyMemory = new(parameters.Data);

            while (bytesToSend > 0)
            {
                if (bytesToSend < bufferSize)
                {
                    bufferSize = bytesToSend;
                }

                ReadOnlyMemory<byte> slice = readOnlyMemory.Slice(offset, bufferSize);
                bytesToSend -= bufferSize;
                offset += bufferSize;

                var chunkRequest = new ImageUploadRequest
                {
                    ProjectId = parameters.ProjectId,
                    StreamLength = parameters.Data.Length,
                    Chunk = UnsafeByteOperations.UnsafeWrap(slice),
                    CollectionId = parameters.CollectionId,
                    CollectionIndex = parameters.CollectionIndex,
                    CollectionSize = parameters.CollectionSize,
                    ContentType = parameters.ContentType
                };

                await request.RequestStream.WriteAsync(chunkRequest, cancellationToken: CancellationToken.None);
            }

            await request.RequestStream.CompleteAsync();
            await request;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning((int)EventLogType.Upload, ex, "Could not upload image!");
            throw;
        }
    }

    public async ValueTask ConfirmUpload(ConfirmUploadParameters parameters)
    {
        try
        {
            var request = new ConfirmUploadRequest
            {
                ProjectId = parameters.ProjectId,
                CollectionId = parameters.CollectionId,
                BatchName = parameters.BatchName
            };

            foreach (string tag in parameters.Tags)
            {
                request.Tags.Add(tag);
            }

            foreach (int dis in parameters.Distribution)
            {
                request.Distribution.Add(dis);
            }

            await _fileManagerClient.ConfirmUploadAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning((int)EventLogType.Upload, ex, "Failed to confirm upload [{UploadId}]!", parameters.CollectionId);
            throw;
        }
    }

    public sealed record SendImageParameters(string ProjectId, byte[] Data, string ContentType, string CollectionId, int CollectionIndex = 0, int CollectionSize = 1);
    public sealed record ConfirmUploadParameters(string ProjectId, string CollectionId, string BatchName, IEnumerable<string> Tags, IEnumerable<int> Distribution);
}