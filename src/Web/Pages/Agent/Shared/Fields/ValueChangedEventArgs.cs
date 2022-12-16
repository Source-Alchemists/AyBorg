using AyBorg.SDK.Data.Bindings;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public class ValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the port.
    /// </summary>
    public Port Port { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="value">The value.</param>
    public ValueChangedEventArgs(Port port, object? value)
    {
        Port = port;
        Value = value;
    }
}
