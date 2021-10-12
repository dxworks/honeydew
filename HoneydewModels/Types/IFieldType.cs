namespace HoneydewModels.Types
{
    public interface IFieldType : IContainedType, ITypeWithModifiers, ITypeWithAttributes, ITypeWithMetrics, INullableType
    {
        public IEntityType Type { get; set; }

        public bool IsEvent { get; set; }
    }
}
