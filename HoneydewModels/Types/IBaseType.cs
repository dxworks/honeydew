namespace HoneydewModels.Types
{
    public interface IBaseType : IType
    {
        public IEntityType Type { get; set; }
        
        public string Kind { get; set; }
    }
}
