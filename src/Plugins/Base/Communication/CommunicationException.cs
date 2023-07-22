namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationException : Exception
{
    public CommunicationException(string message) : base(message)
    {
    }
}
