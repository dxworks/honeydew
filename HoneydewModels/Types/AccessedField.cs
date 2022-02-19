namespace HoneydewModels.Types;

public record AccessedField : INamedType
{
    public string Name { get; set; }

    public string DefinitionClassName { get; set; } // base

    public string LocationClassName { get; set; } // derived

    public AccessKind Kind { get; set; }

    public enum AccessKind
    {
        Getter,
        Setter,
    }
}
