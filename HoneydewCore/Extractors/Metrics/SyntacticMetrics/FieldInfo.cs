namespace HoneydewCore.Extractors.Metrics.SyntacticMetrics
{
    public record FieldInfo
    {
        public string Name { get; init; }
        public string Type { get; init; }
        public string Modifier { get; init; } = "";
        public string Visibility { get; init; }
        public bool IsEvent { get; init; }
    }
}