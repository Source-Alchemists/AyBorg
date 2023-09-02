using System.Buffers;
using Ayborg.Gateway.Result.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Result.Services;

public sealed class StorageServiceV1 : Storage.StorageBase
{
    private readonly IRepository _repository;

    public StorageServiceV1(IRepository repository)
    {
        _repository = repository;
    }

    public override async Task<Empty> Add(AddRequest request, ServerCallContext context)
    {
        var tmpPorts = new List<Port>();
        foreach (PortDto? portDto in request.Ports)
        {
            tmpPorts.Add(new Port
            {
                Id = Guid.Parse(portDto.Id),
                Direction = (PortDirection)portDto.Direction,
                Name = portDto.Name,
                Value = portDto.Value,
                Brand = (PortBrand)portDto.Brand
            });
        }

        await _repository.AddAsync(new WorkflowResult
        {
            ServiceUniqueName = request.AgentUniqueName,
            Id = request.Id,
            IterationId = request.IterationId,
            StartTime = request.StartTime.ToDateTime(),
            StopTime = request.StopTime.ToDateTime(),
            ElapsedMs = request.ElapsedMs,
            Success = request.Success,
            Ports = tmpPorts
        });

        return new Empty();
    }

    public override async Task<Empty> AddImage(IAsyncStreamReader<ImageChunkDto> requestStream, ServerCallContext context)
    {

        IMemoryOwner<byte> memoryOwner = null!;
        ImageResult imageResult = null!;

        try
        {
            int offset = 0;
            await foreach (ImageChunkDto? chunkDto in requestStream.ReadAllAsync())
            {
                if(memoryOwner == null)
                {
                    memoryOwner = MemoryPool<byte>.Shared.Rent((int)chunkDto.FullStreamLength);
                    imageResult = new ImageResult
                    {
                        IterationId = chunkDto.IterationId,
                        PortId = chunkDto.ResultId,
                        Width = chunkDto.FullWidth,
                        Height = chunkDto.FullHeight,
                        ScaledWidth = chunkDto.ScaledWidth,
                        ScaledHeight = chunkDto.ScaledHeight,
                        Data = memoryOwner
                    };
                }

                Memory<byte> targetMemorySlice = memoryOwner.Memory.Slice(offset, chunkDto.Data.Length);
                offset += chunkDto.Data.Length;
                chunkDto.Data.Memory.CopyTo(targetMemorySlice);
            }

            await _repository.AddImageAsync(imageResult);
        }
        finally
        {
            memoryOwner?.Dispose();
        }

        return new Empty();
    }
}
