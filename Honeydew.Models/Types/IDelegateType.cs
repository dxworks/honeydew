namespace Honeydew.Models.Types;

public interface IDelegateType : IClassType, IMethodSignatureType, ITypeWithGenericParameters 
{
    public IReturnValueType ReturnValue { get; set; }
}
