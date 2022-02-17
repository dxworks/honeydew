namespace HoneydewModels.Types;

public interface IAccessorType : IMethodSkeletonType, ITypeWithLocalFunctions
{
    public IReturnValueType ReturnValue { get; set; }
}
