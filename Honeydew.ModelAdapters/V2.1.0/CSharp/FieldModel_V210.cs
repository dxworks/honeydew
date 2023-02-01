namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record FieldModel_V210 
{
    public string Name { get; set; } = null!;

    public EntityTypeModel_V210 Type { get; set; } = null!;

    public string Modifier { get; set; } = "";

    public string AccessModifier { get; set; } = null!;

    public string ContainingTypeName { get; set; } = null!;

    public bool IsEvent { get; set; }

    public bool IsNullable { get; set; }

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();

    public IList<MetricModel_V210> Metrics { get; init; } = new List<MetricModel_V210>();
}
