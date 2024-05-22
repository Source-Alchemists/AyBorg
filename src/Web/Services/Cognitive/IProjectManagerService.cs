using AyBorg.Web.Shared.Models.Cognitive;
using static AyBorg.Web.Services.Cognitive.ProjectManagerService;

namespace AyBorg.Web.Services.Cognitive;

public interface IProjectManagerService
{
    ValueTask<IEnumerable<ProjectMeta>> GetMetasAsync();
    ValueTask<ProjectMeta> CreateAsync(CreateRequestParameters parameters);
    ValueTask DeleteAsync(DeleteRequestParameters parameters);
    ValueTask<ClassLabel> AddOrUpdateAsync(AddOrUpdateClassLabelParameters parameters);
}
