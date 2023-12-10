using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Result;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base;

public sealed class ResultStore : ResultStoreBase
{
    private readonly StringPort _valuePort = new("Value", PortDirection.Input, string.Empty);

    public override string Name => "Result.Store";

    public ResultStore(ILogger<ResultStore> logger, IResultStorageProvider resultStorageProvider, IRuntimeMapper runtimeMapper)
                : base(logger, resultStorageProvider, runtimeMapper)
    {
        _ports = _ports.Add(_valuePort);
    }

    protected override Port Map() => _runtimeMapper.FromRuntime(_valuePort);
}
