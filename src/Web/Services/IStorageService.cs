namespace AyBorg.Web.Services;

public interface IStorageService
{
    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetDirectoriesAsync(string path);
}
