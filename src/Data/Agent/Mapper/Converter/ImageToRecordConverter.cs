using AutoMapper;
using ImageTorque;
using Sys = System;

namespace AyBorg.Data.Agent;

internal class ImageToRecordConverter : IValueConverter<Image, string>
{
    public string Convert(Image sourceMember, ResolutionContext context)
    {
        if (sourceMember == null)
        {
            return string.Empty;
        }

        var record = new ImageRecord
        {
            Width = sourceMember.Width,
            Height = sourceMember.Height,
            Size = sourceMember.Size,
            PixelFormat = sourceMember.PixelFormat,
            Value = string.Empty // No need to save images to the database as port.
        };

        return Sys.Text.Json.JsonSerializer.Serialize(record);
    }
}
