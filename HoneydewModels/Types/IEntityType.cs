namespace HoneydewModels.Types
{
    public interface IEntityType : INamedType
    {
        public GenericType FullType { get; set; }
    }
}
