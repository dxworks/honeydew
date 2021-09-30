namespace HoneydewModels.Reference
{
    public class MethodCallModel : ReferenceEntity
    {
        public MethodModel Method { get; set; }
        
        public ReferenceEntity Caller { get; set; }
    }
}
