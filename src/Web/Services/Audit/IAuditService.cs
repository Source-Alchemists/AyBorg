namespace AyBorg.Web.Services;

public interface IAuditService
{
    IAsyncEnumerable<Shared.Models.AuditChangeset> GetAuditChangesetsAsync();
}
