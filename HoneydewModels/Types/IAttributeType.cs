namespace HoneydewModels.Types
{
    public interface IAttributeType : IMethodSignatureType
    {
        public string Target { get; set; }
    }
}
