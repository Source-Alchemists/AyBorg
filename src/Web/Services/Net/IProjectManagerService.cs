using AyBorg.Web.Shared.Models.Net;
using static AyBorg.Web.Services.Net.ProjectManagerService;

namespace AyBorg.Web.Services.Net;

public interface IProjectManagerService
{
    ValueTask<IEnumerable<ProjectMeta>> GetMetasAsync();
    ValueTask<ProjectMeta> CreateAsync(CreateRequestParameters parameters);
    ValueTask DeleteAsync(DeleteRequestParameters parameters);
    ValueTask<ClassLabel> AddOrUpdateAsync(AddOrUpdateClassLabelParameters parameters);
}