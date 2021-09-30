namespace HoneydewModels.Reference
{
    public class EntityType : ReferenceEntity
    {
        public ReferenceEntity TypeReference { get; set; }

        public string Name { get; set; }
        
        public GenericType FullType { get; set; }

        public bool IsExtern { get; set; }
    }
}
