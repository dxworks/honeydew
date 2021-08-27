namespace HoneydewModels.Types
{
    public interface IDelegateType : IClassType, IMethodSignatureType
    {
        public IReturnValueType ReturnValue { get; set; }
    }
}
