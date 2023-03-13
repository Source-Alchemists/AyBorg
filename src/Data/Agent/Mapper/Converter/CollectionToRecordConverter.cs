using System.Text.Json;
using AutoMapper;

namespace AyBorg.Data.Agent;

internal class CollectionToRecordConverter<T> : IValueConverter<ICollection<T>, string>
{
    public string Convert(ICollection<T> sourceMember, ResolutionContext context)
    {
        return JsonSerializer.Serialize(sourceMember);
    }
}
