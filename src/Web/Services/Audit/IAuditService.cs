namespace AyBorg.Web.Services;

public interface IAuditService
{
    IAsyncEnumerable<Shared.Models.AuditChangeset> GetChangesetsAsync();
    IAsyncEnumerable<Shared.Models.AuditChange> GetChangesAsync(IEnumerable<Shared.Models.AuditChangeset> changesets);
    ValueTask<bool> TryAddReport(string reportName, string comment, IEnumerable<Shared.Models.AuditChangeset> changesets);
    IAsyncEnumerable<Shared.Models.AuditReport> GetReportsAsync();
    ValueTask<bool> TryDeleteReport(Shared.Models.AuditReport report);
}
