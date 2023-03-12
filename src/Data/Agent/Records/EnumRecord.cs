namespace AyBorg.Data.Agent
{
    public record EnumRecord
    {
        public string Name { get; set; } = string.Empty;
        public string[] Names { get; set; } = Array.Empty<string>();
    }
}
