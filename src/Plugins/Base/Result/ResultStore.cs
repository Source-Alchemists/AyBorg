using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Result;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base;

public sealed class ResultStore : IStepBody
{
    private readonly ILogger<ResultStore> _logger;
    private readonly IResultStorageProvider _resultStorageProvider;
    private readonly IRuntimeMapper _runtimeMapper;
    private readonly StringPort _idPort = new("Id", PortDirection.Input, "topic/id");
    private readonly StringPort _valuePort = new("Value", PortDirection.Input, string.Empty);
    public IReadOnlyCollection<IPort> Ports { get; }

    public string Name => "Result.Store";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Result };

    public ResultStore(ILogger<ResultStore> logger, IResultStorageProvider resultStorageProvider, IRuntimeMapper runtimeMapper)
    {
        _logger = logger;
        _resultStorageProvider = resultStorageProvider;
        _runtimeMapper = runtimeMapper;

        Ports = new List<IPort> {
            _idPort,
            _valuePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_idPort.Value))
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to store result. Id is empty.");
            return ValueTask.FromResult(false);
        }

        Port portModel = _runtimeMapper.FromRuntime(_valuePort);
        try
        {
            _resultStorageProvider.Add(new PortResult(
                _idPort.Value,
                portModel
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed to store result {portResultId}'.", _idPort.Value);
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);
    }
}
