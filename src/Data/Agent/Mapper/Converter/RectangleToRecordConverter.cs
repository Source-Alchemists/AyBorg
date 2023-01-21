using AutoMapper;
using ImageTorque;
using Sys = System;

namespace AyBorg.Data.Agent;

internal class RectangleToRecordConverter : IValueConverter<Rectangle, string>
{
    public string Convert(Rectangle sourceMember, ResolutionContext context)
    {
        var record = new RectangleRecord
        {
            X = sourceMember.X,
            Y = sourceMember.Y,
            Width = sourceMember.Width,
            Height = sourceMember.Height
        };
        return Sys.Text.Json.JsonSerializer.Serialize(record);
    }
}
