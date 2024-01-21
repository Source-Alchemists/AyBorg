namespace AyBorg.Web.Shared.Models.Net;


public sealed record DatasetMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime CreationDate { get; init; }
    public DateTime GeneratedDate { get; init; }
    public bool IsActive { get; init; }
    public IEnumerable<int> Distribution { get; init; } = new int[] { 0, 0, 0 };
    public string Comment { get; init; } = string.Empty;
}
