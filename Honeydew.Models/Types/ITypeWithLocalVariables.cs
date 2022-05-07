namespace Honeydew.Models.Types;

public interface ITypeWithLocalVariables : IType
{
    public IList<ILocalVariableType> LocalVariableTypes { get; set; }
}
