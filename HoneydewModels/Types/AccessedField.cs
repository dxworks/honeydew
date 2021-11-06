namespace HoneydewModels.Types
{
    public record AccessedField : IContainedType
    {
        public string Name { get; set; }

        public string ContainingTypeName { get; set; }

        public AccessKind Kind { get; set; }

        public enum AccessKind
        {
            Getter,
            Setter,
        }
    }
}
