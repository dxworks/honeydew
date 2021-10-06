namespace HoneydewModels.Types
{
    public interface IParameterType : ITypeWithAttributes, INullableType
    {
        public IEntityType Type { get; set; }
    }
}
