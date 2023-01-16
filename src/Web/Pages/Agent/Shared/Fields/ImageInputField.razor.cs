using AyBorg.SDK.Common.Models;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class ImageInputField : BaseInputField
{
    private string? _imageUrl;
    private int _imageWidth;
    private int _imageHeight;
    private string ImageTooltip => $"Width: {_imageWidth} Height: {_imageHeight}";

    protected override void OnParametersSet()
    {
        if (Port == null || Port.Value == null)
        {
            _imageUrl = string.Empty;
        }
        else if (Port.Value is Image image)
        {
                _imageWidth = image.Width;
                _imageHeight = image.Height;
                SetImageUrl(image);
        }
        base.OnParametersSet();
    }

    private void SetImageUrl(Image image)
    {
        switch (image.EncoderType)
        {
            case ImageTorque.Processing.EncoderType.Jpeg:
                _imageUrl = $"data:image/jpeg;base64,{image.Base64}";
                break;
            case ImageTorque.Processing.EncoderType.Png:
                _imageUrl = $"data:image/png;base64,{image.Base64}";
                break;
            case ImageTorque.Processing.EncoderType.Bmp:
                _imageUrl = $"data:image/bmp;base64,{image.Base64}";
                break;
        }
    }
}
