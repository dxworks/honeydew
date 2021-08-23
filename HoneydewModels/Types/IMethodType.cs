namespace HoneydewModels.Types
{
    public interface IMethodType : IMethodSkeletonType
    {
        public IReturnType ReturnType { get; set; }
    }
}
