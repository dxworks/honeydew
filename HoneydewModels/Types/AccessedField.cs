namespace HoneydewModels.Types
{
    public record AccessedField : IContainedType
    {
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public AccessType Type { get; set; }

        public enum AccessType
        {
            Getter,
            Setter,
        }
    }
}
