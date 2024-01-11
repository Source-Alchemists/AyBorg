using System.Buffers;
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

    public async ValueTask UploadImageAsync(UploadImageParameters parameters)
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
                    Data = UnsafeByteOperations.UnsafeWrap(slice),
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

    public async ValueTask ConfirmUploadAsync(ConfirmUploadParameters parameters)
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

    public async ValueTask<ImageContainer> DownloadImageAsync(DownloadImageParameters parameters)
    {
        IMemoryOwner<byte> memoryOwner = null!;
        string contentType = string.Empty;
        int width = 0;
        int height = 0;
        ImageContainer resultContainer = null!;

        try
        {
            AsyncServerStreamingCall<ImageChunk> response = _fileManagerClient.DownloadImage(new ImageDownloadRequest
            {
                ProjectId = parameters.ProjectId,
                ImageName = parameters.ImageName,
                AsThumbnail = parameters.AsThumbnail
            });

            int offset = 0;
            await foreach (ImageChunk? chunk in response.ResponseStream.ReadAllAsync())
            {
                if (memoryOwner == null)
                {
                    memoryOwner = MemoryPool<byte>.Shared.Rent((int)chunk.StreamLength);
                    contentType = chunk.ContentType;
                    width = chunk.Width;
                    height = chunk.Height;
                }

                Memory<byte> targetMemorySlice = memoryOwner.Memory.Slice(offset, chunk.Data.Length);
                offset += chunk.Data.Length;
                chunk.Data.Memory.CopyTo(targetMemorySlice);
            }

            resultContainer = new ImageContainer(parameters.ImageName,
                                                Convert.ToBase64String(memoryOwner.Memory.Span),
                                                contentType,
                                                width, height);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning((int)EventLogType.Download, ex, "Failed to download image [{ImageName}] from project [{ProjectId}]!", parameters.ImageName, parameters.ProjectId);
            throw;
        }
        finally
        {
            memoryOwner?.Dispose();
        }

        return resultContainer;
    }

    public async ValueTask<ImageCollectionMeta> GetImageCollectionMetaAsync(GetImageCollectionMetaParameters parameters)
    {
        try
        {
            var request = new GetImageCollectionMetaRequest
            {
                ProjectId = parameters.ProjectId,
                BatchName = parameters.BatchName ?? string.Empty,
                SplitGroup = parameters.SplitGroup ?? string.Empty,
            };
            foreach (string tag in parameters.Tags)
            {
                request.Tags.Add(tag);
            }

            Ayborg.Gateway.Net.V1.ImageCollectionMeta response = await _fileManagerClient.GetImageCollectionMetaAsync(request);
            return new ImageCollectionMeta(
                UnannotatedFileNames: response.UnannotatedFileNames,
                AnnotatedFileNames: response.AnnotatedFilesNames,
                BatchNames: response.BatchNames,
                Tags: response.Tags
                );
        }
        catch (RpcException ex)
        {
            _logger.LogWarning((int)EventLogType.Download, ex, "Failed to receive image collection meta informations!");
            throw;
        }
    }

    public async ValueTask<ImageAnnotationMeta> GetImageAnnotationMetaAsync(GetImageAnnotationMetaParameters parameters)
    {
        try
        {
            Ayborg.Gateway.Net.V1.ImageAnnotationMeta response = await _fileManagerClient.GetImageAnnotationMetaAsync(new GetImageAnnotationMetaRequest
            {
                ProjectId = parameters.ProjectId,
                ImageName = parameters.ImageName
            });

            return new ImageAnnotationMeta(response.Tags, response.Layers);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning((int)EventLogType.Download, ex, "Failed to receive image annotation meta informations!");
            throw;
        }
    }

    public sealed record UploadImageParameters(string ProjectId, byte[] Data, string ContentType, string CollectionId, int CollectionIndex = 0, int CollectionSize = 1);
    public sealed record ConfirmUploadParameters(string ProjectId, string CollectionId, string BatchName, IEnumerable<string> Tags, IEnumerable<int> Distribution);
    public sealed record DownloadImageParameters(string ProjectId, string ImageName, bool AsThumbnail);
    public sealed record GetImageCollectionMetaParameters(string ProjectId, string BatchName, string SplitGroup, IEnumerable<string> Tags);
    public sealed record ImageCollectionMeta(IEnumerable<string> UnannotatedFileNames, IEnumerable<string> AnnotatedFileNames, IEnumerable<string> BatchNames, IEnumerable<string> Tags);
    public sealed record ImageContainer(string ImageName, string Base64Image, string ContentType, int Width, int Height)
    {
        public string ToBase64String()
        {
            return $"data:{ContentType};base64,{Base64Image}";
        }
    }

    public sealed record GetImageAnnotationMetaParameters(string ProjectId, string ImageName);
    public sealed record ImageAnnotationMeta(IEnumerable<string> Tags, IEnumerable<int> LayerIndex);
    public sealed record ClassMeta(string Name, int Index);

}