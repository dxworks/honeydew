namespace Honeydew.Models.Types;

public interface IAccessorType : IMethodSkeletonType, ITypeWithLocalFunctions
{
    public IReturnValueType ReturnValue { get; set; }
}
