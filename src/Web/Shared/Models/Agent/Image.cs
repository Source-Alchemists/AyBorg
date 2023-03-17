using AyBorg.SDK.Common.Models;
using ImageTorque;
using ImageTorque.Processing;

namespace AyBorg.Web.Shared.Models;

public sealed record Image
{
    public int Width => Meta.Width;
    public int Height => Meta.Height;
    public PixelFormat PixelFormat => Meta.PixelFormat;
    public ImageMeta Meta { get; init; }
    public string Base64 { get; init; } = string.Empty;
    public EncoderType EncoderType { get; init; } = EncoderType.Png;
}
