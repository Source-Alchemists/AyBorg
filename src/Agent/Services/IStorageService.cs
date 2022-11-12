namespace AyBorg.Agent.Services;

public interface IStorageService
{
    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="virtualPath">The virtual path.</param>
    /// <returns></returns>
    IEnumerable<string> GetDirectories(string virtualPath);
}