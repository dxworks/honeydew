namespace HoneydewCore.Models
{
    public record ParameterModel
    {
        public string Type { get; init; }

        public string Modifier { get; init; } = "";

        public string DefaultValue { get; init; }
    }
}