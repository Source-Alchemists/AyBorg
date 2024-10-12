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

using System.Buffers;
using Ayborg.Gateway.Cognitive.V1;
using AyBorg.Types;
using AyBorg.Web.Shared.Models.Cognitive;
using Google.Protobuf;
using Grpc.Core;

namespace AyBorg.Web.Services.Cognitive;

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

    public async ValueTask ConfirmUploadAsync(ConfirmUploadParameters parameters)
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
        finally
        {
            memoryOwner?.Dispose();
        }

        return resultContainer;
    }

    public async ValueTask<ImageCollectionMeta> GetImageCollectionMetaAsync(GetImageCollectionMetaParameters parameters)
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

        Ayborg.Gateway.Cognitive.V1.ImageCollectionMeta response = await _fileManagerClient.GetImageCollectionMetaAsync(request);
        return new ImageCollectionMeta(
            UnannotatedFileNames: response.UnannotatedFileNames,
            AnnotatedFileNames: response.AnnotatedFilesNames,
            BatchNames: response.BatchNames,
            Tags: response.Tags
            );

    }

    public async ValueTask<IEnumerable<ModelMeta>> GetModelMetasAsync(GetModelMetasParameters parameters)
    {
        List<ModelMeta> result = new();
        AsyncServerStreamingCall<Ayborg.Gateway.Cognitive.V1.ModelMeta> response = _fileManagerClient.GetModelMetas(new GetModelMetasRequest
        {
            ProjectId = parameters.ProjectId
        });

        await foreach (Ayborg.Gateway.Cognitive.V1.ModelMeta modelMetaDto in response.ResponseStream.ReadAllAsync())
        {
            result.Add(new ModelMeta(
                Id: modelMetaDto.Id,
                Name: modelMetaDto.Name,
                Type: (ProjectType)modelMetaDto.Type,
                CreationDate: modelMetaDto.CreationDate.ToDateTime(),
                Classes: modelMetaDto.Classes.ToArray(),
                State: (ModelState)modelMetaDto.State,
                Comment: modelMetaDto.Comment
            ));
        }

        return result;

    }

    public async ValueTask EditModelAsync(EditModelParameters parameters)
    {
        await _fileManagerClient.EditModelAsync(new EditModelRequest
        {
            ProjectId = parameters.ProjectId,
            ModelId = parameters.ModelId,
            Name = parameters.NewName
        });
        if (!parameters.OldName.Equals(parameters.NewName, StringComparison.InvariantCulture))
        {
            _logger.LogInformation((int)EventLogType.UserInteraction, "Changed model [{ModelName} ({ModelId})] name to [{Name}].", parameters.OldName, parameters.ModelId, parameters.NewName);
        }

    }

    public async ValueTask ChangeModelStateAsync(ChangeModelStateParameters parameters)
    {
        await _fileManagerClient.ChangeModelStateAsync(new ChangeModelStateRequest
        {
            ProjectId = parameters.ProjectId,
            ModelId = parameters.ModelId,
            State = (Ayborg.Gateway.Cognitive.V1.ModelState)parameters.NewState,
            Comment = parameters.Comment
        });
        _logger.LogInformation((int)EventLogType.UserInteraction, "Model [{ModelName} ({ModelId})] state changed from [{OldModelState}] to [{NewModelState}].", parameters.ModelName, parameters.ModelId, parameters.OldState, parameters.NewState);
    }

    public async ValueTask DeleteModelAsync(DeleteModelParameters parameters)
    {
        await _fileManagerClient.DeleteModelAsync(new DeleteModelRequest
        {
            ProjectId = parameters.ProjectId,
            ModelId = parameters.ModelId
        });
        _logger.LogInformation((int)EventLogType.UserInteraction, "Deleted model [{ModelName} ({ModelId})].", parameters.ModelName, parameters.ModelId);

    }

    public sealed record UploadImageParameters(string ProjectId, byte[] Data, string ContentType, string CollectionId, int CollectionIndex = 0, int CollectionSize = 1);
    public sealed record ConfirmUploadParameters(string ProjectId, string CollectionId, string BatchName, IEnumerable<string> Tags, IEnumerable<int> Distribution);
    public sealed record DownloadImageParameters(string ProjectId, string ImageName, bool AsThumbnail);
    public sealed record GetImageCollectionMetaParameters(string ProjectId, string BatchName, string SplitGroup, IEnumerable<string> Tags);
    public sealed record GetModelMetasParameters(string ProjectId);
    public sealed record EditModelParameters(string ProjectId, string ModelId, string OldName, string NewName);
    public sealed record DeleteModelParameters(string ProjectId, string ModelId, string ModelName);
    public sealed record ChangeModelStateParameters(string ProjectId, string ModelId, string ModelName, ModelState OldState, ModelState NewState, string Comment);
    public sealed record ImageCollectionMeta(IEnumerable<string> UnannotatedFileNames, IEnumerable<string> AnnotatedFileNames, IEnumerable<string> BatchNames, IEnumerable<string> Tags);
    public sealed record ImageContainer(string ImageName, string Base64Image, string ContentType, int Width, int Height)
    {
        public string ToBase64String()
        {
            return $"data:{ContentType};base64,{Base64Image}";
        }
    }
    public sealed record ClassMeta(string Name, int Index);
    public sealed record ModelMeta(string Id, string Name, ProjectType Type, DateTime CreationDate, string[] Classes, ModelState State, string Comment);
    public enum ModelState
    {
        Draft = 0,
        Review = 1,
        Release = 2
    }
}
