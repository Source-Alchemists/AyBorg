namespace AyBorg.Web.Shared.Models;

public sealed record ImageSource(byte[] Data, string ContentType, string Hash)
{
    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;
    private string _base64 = string.Empty;
    public string ToBase64String()
    {
        if (string.IsNullOrEmpty(_base64))
        {
            _base64 = $"data:{ContentType};base64,{Convert.ToBase64String(Data)}";
        }

        return _base64;
    }
}
