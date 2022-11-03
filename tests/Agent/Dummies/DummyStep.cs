using Atomy.SDK;
using Atomy.SDK.Ports;

namespace Atomy.Agent.Tests.Dummies;

/// <summary>
/// We need a "real" step instead of a mock to test the valid behaviour.
/// </summary>
/// <seealso cref="Atomy.SDK.IStepBody" />
public class DummyStep : IStepBody
{
    public DummyStep()
    {
        Ports = new List<IPort>
        {
            new StringPort("String input", PortDirection.Input, "Test"),
            new NumericPort("Numeric input", PortDirection.Input, 123),
            new NumericPort("Output", PortDirection.Output, 123, 100, 200),
        };
    }

    public string DefaultName =>  "Dummy";
    public IEnumerable<IPort> Ports { get; }
    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}