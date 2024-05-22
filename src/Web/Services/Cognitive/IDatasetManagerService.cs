using static AyBorg.Web.Services.Cognitive.DatasetManagerService;

namespace AyBorg.Web.Services.Cognitive;

public interface IDatasetManagerService
{
    ValueTask<IEnumerable<Shared.Models.Cognitive.DatasetMeta>> GetMetasAsync(GetMetasParameters parameters);
    ValueTask<IEnumerable<string>> GetImageNamesAsync(GetImageNamesParameters parameters);
    ValueTask<Shared.Models.Cognitive.DatasetMeta> CreateAsync(CreateParameters parameters);
    ValueTask DeleteAsync(DeleteParameters parameters);
    ValueTask AddImageAsync(AddImageParameters parameters);
    ValueTask EditAsync(EditParameters parameters);
    ValueTask GenerateAsync(GenerateParameters parameters);
}
