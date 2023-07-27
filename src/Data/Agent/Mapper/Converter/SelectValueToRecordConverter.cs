using System.Text.Json;
using AutoMapper;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Agent;

internal class SelectValueToRecordConverter : IValueConverter<SelectPort.ValueContainer, string>
{
    public string Convert(SelectPort.ValueContainer sourceMember, ResolutionContext context)
    {
        return JsonSerializer.Serialize(sourceMember);
    }
}
