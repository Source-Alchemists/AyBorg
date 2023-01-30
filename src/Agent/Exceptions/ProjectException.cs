using System.Runtime.Serialization;

namespace AyBorg.Agent;

[Serializable]
public sealed class ProjectException : Exception
{
    public ProjectException(string message) : base(message)
    {
    }

    private ProjectException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }
}
