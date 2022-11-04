using Atomy.SDK.Common;

namespace Atomy.Agent.Services;

public class StorageService : IStorageService
{
    private const string VIRTUAL_ROOT_PATH = "/";
    private readonly ILogger<StorageService> _logger;
    private readonly IEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="environment">The environment.</param>
    public StorageService(ILogger<StorageService> logger, IEnvironment environment)
    {
        _logger = logger;
        _environment = environment;

        if (!Directory.Exists(_environment.StorageLocation))
        {
            try
            {
                Directory.CreateDirectory(_environment.StorageLocation);
                _logger.LogInformation("Created storage directory at {StorageLocation}", _environment.StorageLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create storage directory at {StorageLocation}", _environment.StorageLocation);
            }
        }
        else
        {
            _logger.LogInformation("Using storage directory at {StorageLocation}", _environment.StorageLocation);
        }
    }

    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="virtualPath">The virtual path.</param>
    /// <returns></returns>
    public IEnumerable<string> GetDirectories(string virtualPath)
    {
        string realPath;

        if (virtualPath.Equals(VIRTUAL_ROOT_PATH)
            || virtualPath.Equals(Path.DirectorySeparatorChar.ToString())
            || virtualPath.Equals(Path.AltDirectorySeparatorChar.ToString()))
        {
            realPath = _environment.StorageLocation;
        }
        else
        {
            if (virtualPath.StartsWith(VIRTUAL_ROOT_PATH)
                || virtualPath.StartsWith(Path.DirectorySeparatorChar.ToString())
                || virtualPath.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                virtualPath = virtualPath[1..];
            }

            virtualPath = virtualPath.Replace("..", string.Empty);

            realPath = Path.Combine(_environment.StorageLocation, virtualPath);
        }

        if (Directory.Exists(realPath))
        {
            foreach (var dir in Directory.GetDirectories(realPath))
            {
                var virtualSubPath = dir.Replace(_environment.StorageLocation, VIRTUAL_ROOT_PATH);
                virtualSubPath = virtualSubPath.Replace("\\", "/");
                virtualSubPath = virtualSubPath.Replace("//", "/");
                yield return virtualSubPath;
            }
        }
        else
        {
            yield return VIRTUAL_ROOT_PATH;
        }
    }
}