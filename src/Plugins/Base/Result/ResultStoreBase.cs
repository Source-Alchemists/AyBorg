using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Result;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base;

public abstract class ResultStoreBase : IStepBody
{
    protected readonly ILogger<ResultStoreBase> _logger;
    protected readonly IResultStorageProvider _resultStorageProvider;
    protected readonly IRuntimeMapper _runtimeMapper;
    protected readonly StringPort _idPort = new("Id", PortDirection.Input, "topic/id");
    protected ImmutableList<IPort> _ports = ImmutableList.Create<IPort>();
    public IReadOnlyCollection<IPort> Ports => _ports;

    public abstract string Name { get; }

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Result };

    protected ResultStoreBase(ILogger<ResultStoreBase> logger, IResultStorageProvider resultStorageProvider, IRuntimeMapper runtimeMapper)
    {
        _logger = logger;
        _resultStorageProvider = resultStorageProvider;
        _runtimeMapper = runtimeMapper;

        _ports = _ports.Add(_idPort);
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_idPort.Value))
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to store result. Id is empty.");
            return ValueTask.FromResult(false);
        }

        Port portModel = Map();
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

    protected abstract Port Map();
}
