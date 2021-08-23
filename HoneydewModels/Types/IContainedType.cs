namespace HoneydewModels.Types
{
    public interface IContainedType : INamedType
    {
        public string ContainingTypeName { get; set; }
    }
}
