using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Result;

namespace AyBorg.Plugins.Base;

public sealed class ResultStore : IStepBody
{
    private readonly IResultStorageProvider _resultStorageProvider;
    private readonly IRuntimeMapper _runtimeMapper;
    private readonly StringPort _idPort = new("Id", PortDirection.Input, Guid.Empty.ToString());
    private readonly StringPort _valuePort = new("Value", PortDirection.Input, string.Empty);
    public IReadOnlyCollection<IPort> Ports { get; }

    public string Name => "Result.Store";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Result };

    public ResultStore(IResultStorageProvider resultStorageProvider, IRuntimeMapper runtimeMapper)
    {
        _resultStorageProvider = resultStorageProvider;
        _runtimeMapper = runtimeMapper;

        Ports = new List<IPort> {
            _idPort,
            _valuePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}
