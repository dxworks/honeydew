namespace HoneydewModels.Types
{
    public interface IMethodType : IMethodSkeletonType
    {
        public IReturnValueType ReturnValue { get; set; }
    }
}
