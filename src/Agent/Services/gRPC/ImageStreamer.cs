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
using System.Runtime.CompilerServices;
using Google.Protobuf;
using Grpc.Core;
using ImageTorque;
using Microsoft.IO;

namespace AyBorg.Agent.Services.gRPC;

internal static class ImageStreamer
{
    private const int CHUNK_SIZE = 32768;
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
            PrepareStream(targetImage, stream, out long fullStreamLength, out long bytesToSend, out int bufferSize, out int offset, out IMemoryOwner<byte> memoryOwner);
            _ = await stream.ReadAsync(memoryOwner.Memory, cancellationToken);

            while (!cancellationToken.IsCancellationRequested && bytesToSend > 0)
            {
                if (bytesToSend < bufferSize)
                {
                    bufferSize = (int)bytesToSend;
                }

                Memory<byte> slice = CreateMemorySlice(ref bytesToSend, bufferSize, ref offset, memoryOwner);

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
    public static async ValueTask SendImageAsync(Image originalImage, IClientStreamWriter<Ayborg.Gateway.Result.V1.ImageChunkDto> requestStream, string serviceUniqueName, string iterationId, string portId, float scaleFactor, CancellationToken cancellationToken)
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
            PrepareStream(targetImage, stream, out long fullStreamLength, out long bytesToSend, out int bufferSize, out int offset, out IMemoryOwner<byte> memoryOwner);
            _ = await stream.ReadAsync(memoryOwner.Memory, cancellationToken);

            while (!cancellationToken.IsCancellationRequested && bytesToSend > 0)
            {
                if (bytesToSend < bufferSize)
                {
                    bufferSize = (int)bytesToSend;
                }

                Memory<byte> slice = CreateMemorySlice(ref bytesToSend, bufferSize, ref offset, memoryOwner);

                await requestStream.WriteAsync(new Ayborg.Gateway.Result.V1.ImageChunkDto
                {
                    AgentUniqueName = serviceUniqueName,
                    IterationId = iterationId,
                    ResultId = portId,
                    Data = UnsafeByteOperations.UnsafeWrap(slice),
                    FullWidth = originalImage.Width,
                    FullHeight = originalImage.Height,
                    FullStreamLength = fullStreamLength,
                    ScaledWidth = targetImage.Width,
                    ScaledHeight = targetImage.Height
                }, cancellationToken);
            }

            await requestStream.CompleteAsync();
        }
        finally
        {
            targetImage?.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PrepareStream(IImage targetImage, MemoryStream stream, out long fullStreamLength, out long bytesToSend, out int bufferSize, out int offset, out IMemoryOwner<byte> memoryOwner)
    {
        targetImage.Save(stream, "jpeg");
        stream.Position = 0;
        fullStreamLength = stream.Length;
        bytesToSend = fullStreamLength;
        bufferSize = fullStreamLength < CHUNK_SIZE ? (int)fullStreamLength : CHUNK_SIZE;
        offset = 0;
        memoryOwner = MemoryPool<byte>.Shared.Rent((int)fullStreamLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Memory<byte> CreateMemorySlice(ref long bytesToSend, int bufferSize, ref int offset, IMemoryOwner<byte> memoryOwner)
    {
        Memory<byte> slice = memoryOwner.Memory.Slice(offset, bufferSize);

        bytesToSend -= bufferSize;
        offset += bufferSize;
        return slice;
    }
}
