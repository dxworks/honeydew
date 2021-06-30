namespace HoneydewCore.Models
{
    public record FieldModel
    {
        public string Name { get; init; }
        public string Type { get; init; }
        public string Modifier { get; init; } = "";
        public string Visibility { get; init; }
        public bool IsEvent { get; init; }
    }
}