namespace AyBorg.Agent;

[Serializable]
public sealed class ProjectException(string message) : Exception(message)
{
}
