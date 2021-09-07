namespace HoneydewModels.Types
{
    public interface IMethodType : IMethodSkeletonType, ITypeWithGenericParameters
    {
        public IReturnValueType ReturnValue { get; set; }
    }
}
