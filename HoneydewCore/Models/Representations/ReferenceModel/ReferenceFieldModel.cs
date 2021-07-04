namespace HoneydewCore.Models.Representations.ReferenceModel
{
    public record ReferenceFieldModel : ReferenceEntity
    {
        public ReferenceClassModel ContainingClass { get; init; }
        public ReferenceClassModel Type { get; set; }
        public string Modifier { get; set; } = "";
        public string AccessModifier { get; set; }
        public bool IsEvent { get; set; }
    }
}