namespace HoneydewModels.Types
{
    public interface IParameterType : ITypeWithAttributes
    {
        public IEntityType Type { get; set; }
    }
}
