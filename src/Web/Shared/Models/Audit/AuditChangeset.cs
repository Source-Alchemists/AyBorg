using AyBorg.SDK.Projects;
using AyBorg.SDK.System;

namespace AyBorg.Web.Shared.Models;

public sealed record AuditChangeset
{
    public Guid Token { get; init; }
    public string ServiceType { get; init; } = string.Empty;
    public string ServiceUniqueName { get; init; } = string.Empty;
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public ProjectState ProjectState { get; init; }
    public string VersionName { get; init; } = string.Empty;
    public int VersionIteration { get; init; }
    public string User { get; init; } = string.Empty;
    public string Approver { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public AuditEntryType Type { get; init; }
}
