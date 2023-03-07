namespace AyBorg.Web.Services;

public interface IAuditService
{
    IAsyncEnumerable<Shared.Models.AuditChangeset> GetAuditChangesetsAsync();
    IAsyncEnumerable<Shared.Models.AuditChange> GetAuditChangesAsync(IEnumerable<Shared.Models.AuditChangeset> changesets);
    ValueTask<bool> TrySaveReport(string reportName, string comment, IEnumerable<Shared.Models.AuditChangeset> changesets);
}
