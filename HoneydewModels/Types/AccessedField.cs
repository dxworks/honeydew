namespace HoneydewModels.Types;

public record AccessedField : INamedType
{
    public string Name { get; set; }

    public string DefinitionClassName { get; set; }

    public string AccessLocationMethodName { get; set; }
    public AccessKind Kind { get; set; }

    public enum AccessKind
    {
        Getter,
        Setter,
    }
}
