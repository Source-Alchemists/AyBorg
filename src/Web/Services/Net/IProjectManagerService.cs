using AyBorg.Web.Shared.Models.Net;
using static AyBorg.Web.Services.Net.ProjectManagerService;

namespace AyBorg.Web.Services.Net;

public interface IProjectManagerService
{
    ValueTask<IEnumerable<ProjectMeta>> GetMetasAsync();
    ValueTask<ProjectMeta> CreateAsync(CreateRequestOptions options);
    ValueTask DeleteAsync(DeleteRequestOptions options);
}