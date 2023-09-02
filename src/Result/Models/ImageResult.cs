using System.Buffers;

namespace AyBorg.Result;


public record ImageResult
{
    public string IterationId { get; init; } = string.Empty;
    public string PortId { get; init; } = string.Empty;
    public IMemoryOwner<byte> Data { get; init; } = null!;
    public int Width { get; init; }
    public int Height { get; init; }
    public int ScaledWidth { get; init; }
    public int ScaledHeight { get; init; }
}
