using Sys = System;
using AutoMapper;

namespace AyBorg.Data.Agent;

internal class EnumToRecordConverter : IValueConverter<Enum, string>
{
    public string Convert(Enum sourceMember, ResolutionContext context)
    {
        string[] names = Enum.GetNames(sourceMember.GetType());
        var record = new EnumRecord
        {
            Name = sourceMember.ToString(),
            Names = names
        };

        return Sys.Text.Json.JsonSerializer.Serialize(record);
    }
}
