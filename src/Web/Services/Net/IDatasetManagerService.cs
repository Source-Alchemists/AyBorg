using static AyBorg.Web.Services.Net.DatasetManagerService;

namespace AyBorg.Web.Services.Net;

public interface IDatasetManagerService
{
    ValueTask<IEnumerable<Shared.Models.Net.DatasetMeta>> GetMetasAsync(GetMetasParameters parameters);
    ValueTask<Shared.Models.Net.DatasetMeta> CreateAsync(CreateParameters parameters);
    ValueTask AddImageAsync(AddImageParameters parameters);
    ValueTask EditAsync(EditParameters parameters);
    ValueTask GenerateAsync(GenerateParameters parameters);
}
