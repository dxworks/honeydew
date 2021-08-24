namespace HoneydewModels.Types
{
    public interface IReturnValueType : ITypeWithAttributes
    {
        public IEntityType Type { get; set; }
    }
}
