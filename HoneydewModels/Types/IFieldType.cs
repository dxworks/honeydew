namespace HoneydewModels.Types
{
    public interface IFieldType : IContainedType, IModifierType, ITypeWithAttributes, ITypeWithMetrics
    {
        public string Type { get; set; }

        public bool IsEvent { get; init; }
    }
}
