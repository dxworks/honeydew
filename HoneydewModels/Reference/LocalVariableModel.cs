namespace HoneydewModels.Reference
{
    public class LocalVariableModel : ReferenceEntity
    {
        public EntityType Type { get; set; }

        public bool IsNullable { get; set; }
        
        public ReferenceEntity ContainingType { get; set; }
    }
}
