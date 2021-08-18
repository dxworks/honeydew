namespace HoneydewModels.Types
{
    public interface IContainedType : IType
    {
        public string ContainingTypeName { get; set; }
    }
}
