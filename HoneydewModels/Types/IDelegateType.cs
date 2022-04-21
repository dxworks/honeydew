namespace HoneydewModels.Types;

public interface IDelegateType : IClassType, IMethodSignatureType, ITypeWithGenericParameters 
{
    public IReturnValueType ReturnValue { get; set; }
}
