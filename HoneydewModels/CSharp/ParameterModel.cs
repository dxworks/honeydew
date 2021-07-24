namespace HoneydewModels.CSharp
{
    public record ParameterModel
    {
        public string Type { get; set; }

        public string Modifier { get; init; } = "";

        public string DefaultValue { get; init; }
    }
}
