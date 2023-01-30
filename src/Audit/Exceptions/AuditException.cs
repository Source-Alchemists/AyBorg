using System.Runtime.Serialization;

namespace AyBorg.Audit;

[Serializable]
public sealed class AuditException : Exception
{
    public AuditException(string message) : base(message)
    {
    }

    private AuditException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }
}
