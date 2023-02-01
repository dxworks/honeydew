namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record GenericType_V210
{
    public string Name { get; set; } = null!;

    public bool IsNullable { get; set; }

    public IList<GenericType_V210> ContainedTypes { get; set; } = new List<GenericType_V210>();
}
