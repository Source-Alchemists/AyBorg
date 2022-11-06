namespace Autodroid.Web.Services;

public interface IStorageService
{
    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetDirectoriesAsync(string baseUrl, string path);
}