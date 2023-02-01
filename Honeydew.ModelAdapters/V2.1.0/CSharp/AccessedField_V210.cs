namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record AccessedField_V210
{
    public string Name { get; set; } = null!;

    public string ContainingTypeName { get; set; } = null!;

    public AccessKind Kind { get; set; }

    public enum AccessKind
    {
        Getter,
        Setter,
    }
}
