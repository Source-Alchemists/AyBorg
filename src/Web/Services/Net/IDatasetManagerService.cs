using static AyBorg.Web.Services.Net.DatasetManagerService;

namespace AyBorg.Web.Services.Net;

public interface IDatasetManagerService
{
    ValueTask<IEnumerable<Shared.Models.Net.DatasetMeta>> GetMetasAsync(GetMetasParameters parameters);
    ValueTask<IEnumerable<string>> GetImageNamesAsync(GetImageNamesParameters parameters);
    ValueTask<Shared.Models.Net.DatasetMeta> CreateAsync(CreateParameters parameters);
    ValueTask DeleteAsync(DeleteParameters parameters);
    ValueTask AddImageAsync(AddImageParameters parameters);
    ValueTask EditAsync(EditParameters parameters);
    ValueTask GenerateAsync(GenerateParameters parameters);
}
