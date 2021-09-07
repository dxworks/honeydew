namespace HoneydewModels.Types
{
    public interface ILocalVariableType : IType
    {
       public IEntityType Type { get; set; }  
    }
}
