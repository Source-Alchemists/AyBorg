using System.Buffers;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using Grpc.Core;
using ImageTorque;
using Microsoft.IO;

namespace AyBorg.Agent.Services.gRPC;

internal static class ImageStreamer
{
    private const int ChunkSize = 32768;
    private static readonly RecyclableMemoryStreamManager s_memoryManager = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask SendImageAsync(Image originalImage, IServerStreamWriter<Ayborg.Gateway.Agent.V1.ImageChunkDto> responseStream, bool asThumbnail, CancellationToken cancellationToken)
    {
        const int maxSize = 250;
        IImage targetImage = null!;
        try
        {
            if (originalImage.Width <= maxSize && originalImage.Height <= maxSize || !asThumbnail)
            {
                targetImage = originalImage;
            }
            else
            {
                originalImage.CalculateClampSize(maxSize, out int w, out int h);
                targetImage = originalImage.Resize(w, h, ResizeMode.NearestNeighbor);
            }

            using MemoryStream stream = s_memoryManager.GetStream();
            targetImage.Save(stream, ImageTorque.Processing.EncoderType.Jpeg);
            stream.Position = 0;
            long fullStreamLength = stream.Length;
            long bytesToSend = fullStreamLength;
            int bufferSize = fullStreamLength < ChunkSize ? (int)fullStreamLength : ChunkSize;
            int offset = 0;
            using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent((int)fullStreamLength);
            await stream.ReadAsync(memoryOwner.Memory, cancellationToken);

            while (!cancellationToken.IsCancellationRequested && bytesToSend > 0)
            {
                if (bytesToSend < bufferSize)
                {
                    bufferSize = (int)bytesToSend;
                }

                Memory<byte> slice = memoryOwner.Memory.Slice(offset, bufferSize);

                bytesToSend -= bufferSize;
                offset += bufferSize;

                await responseStream.WriteAsync(new Ayborg.Gateway.Agent.V1.ImageChunkDto
                {
                    Data = UnsafeByteOperations.UnsafeWrap(slice),
                    FullWidth = originalImage.Width,
                    FullHeight = originalImage.Height,
                    FullStreamLength = fullStreamLength,
                    ScaledWidth = targetImage.Width,
                    ScaledHeight = targetImage.Height
                }, cancellationToken);
            }
        }
        finally
        {
            targetImage?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask SendImageAsync(Image originalImage, IClientStreamWriter<Ayborg.Gateway.Result.V1.ImageChunkDto> responseStream, string resultId, float scaleFactor, CancellationToken cancellationToken)
    {
        IImage targetImage = null!;
        try
        {
            if (scaleFactor.Equals(1f))
            {
                targetImage = originalImage;
            }
            else
            {
                int w = (int)(originalImage.Width * scaleFactor);
                int h = (int)(originalImage.Height * scaleFactor);
                targetImage = originalImage.Resize(w, h, ResizeMode.NearestNeighbor);
            }

            using MemoryStream stream = s_memoryManager.GetStream();
            targetImage.Save(stream, ImageTorque.Processing.EncoderType.Jpeg);
            stream.Position = 0;
            long fullStreamLength = stream.Length;
            long bytesToSend = fullStreamLength;
            int bufferSize = fullStreamLength < ChunkSize ? (int)fullStreamLength : ChunkSize;
            int offset = 0;
            using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent((int)fullStreamLength);
            await stream.ReadAsync(memoryOwner.Memory, cancellationToken);

            while (!cancellationToken.IsCancellationRequested && bytesToSend > 0)
            {
                if (bytesToSend < bufferSize)
                {
                    bufferSize = (int)bytesToSend;
                }

                Memory<byte> slice = memoryOwner.Memory.Slice(offset, bufferSize);

                bytesToSend -= bufferSize;
                offset += bufferSize;

                await responseStream.WriteAsync(new Ayborg.Gateway.Result.V1.ImageChunkDto
                {
                    ResultId = resultId,
                    Data = UnsafeByteOperations.UnsafeWrap(slice),
                    FullWidth = originalImage.Width,
                    FullHeight = originalImage.Height,
                    FullStreamLength = fullStreamLength,
                    ScaledWidth = targetImage.Width,
                    ScaledHeight = targetImage.Height
                }, cancellationToken);
            }

            await responseStream.CompleteAsync();
        }
        finally
        {
            targetImage?.Dispose();
        }
    }
}
