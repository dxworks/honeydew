namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceFieldModel : ReferenceEntity
    {
        public ReferenceClassModel ContainingClass { get; init; }
        public ReferenceClassModel Type { get; init; }
        public string Modifier { get; init; } = "";
        public string AccessModifier { get; init; }
        public bool IsEvent { get; init; }
    }
}