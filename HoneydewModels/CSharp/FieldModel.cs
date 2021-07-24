namespace HoneydewModels.CSharp
{
    public record FieldModel
    {
        public string Name { get; init; }
        public string Type { get; set; }
        public string Modifier { get; init; } = "";
        public string AccessModifier { get; init; }
        public bool IsEvent { get; init; }
    }
}
