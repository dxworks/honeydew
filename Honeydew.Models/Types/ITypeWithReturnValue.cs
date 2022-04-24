namespace Honeydew.Models.Types;

public interface ITypeWithReturnValue : IType
{
    public IReturnValueType ReturnValue { get; set; }
}
