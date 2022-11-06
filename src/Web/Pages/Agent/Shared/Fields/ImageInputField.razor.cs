using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Autodroid.SDK.Data.DTOs;
using Autodroid.SDK.ImageProcessing.Encoding;

namespace Autodroid.Web.Pages.Agent.Shared.Fields;

public partial class ImageInputField : IComponent
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
        else if (Port.Value is JsonElement jsonElement)
        {
            var imageDto = JsonSerializer.Deserialize<ImageDto>(jsonElement, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (imageDto.Meta != null)
            {
                _imageWidth = imageDto.Meta.Width;
                _imageHeight = imageDto.Meta.Height;
                SetImageUrl(imageDto);
            }

            Port.Value = imageDto;
        }
        else if (Port.Value is ImageDto imageDto)
        {
            if (imageDto.Meta != null)
            {
                _imageWidth = imageDto.Meta.Width;
                _imageHeight = imageDto.Meta.Height;
                SetImageUrl(imageDto);
            }
            
        }
        base.OnParametersSet();
    }

    private void SetImageUrl(ImageDto imageDto)
    {
        switch (imageDto.Meta.EncoderType)
        {
            case EncoderType.Jpeg:
                _imageUrl = $"data:image/jpeg;base64,{imageDto.Base64}";
                break;
            case EncoderType.Png:
                _imageUrl = $"data:image/png;base64,{imageDto.Base64}";
                break;
            case EncoderType.Bmp:
                _imageUrl = $"data:image/bmp;base64,{imageDto.Base64}";
                break;
        }
    }
}