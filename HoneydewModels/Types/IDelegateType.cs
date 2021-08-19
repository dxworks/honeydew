namespace HoneydewModels.Types
{
    public interface IDelegateType : IClassType, IMethodSignatureType
    {
        public IReturnType ReturnType { get; set; }
    }
}
