using AyBorg.SDK.Common.Models;
using AyBorg.SDK.ImageProcessing.Encoding;

namespace AyBorg.Web.Shared.Models;

public sealed record Image : ImageMeta
{
    public string Base64 { get; set; } = string.Empty;
    public EncoderType EncoderType { get; set; } = EncoderType.Png;
}
