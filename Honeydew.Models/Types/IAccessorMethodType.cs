namespace Honeydew.Models.Types;

public interface IAccessorMethodType : IMethodSkeletonType, ITypeWithLocalFunctions
{
    public IReturnValueType ReturnValue { get; set; }
}
