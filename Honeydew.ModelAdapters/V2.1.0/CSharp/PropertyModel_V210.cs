namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record PropertyModel_V210
{
    public int CyclomaticComplexity { get; set; }
    public string Name { get; set; } = null!;

    public string ContainingTypeName { get; set; } = null!;

    public string AccessModifier { get; set; } = null!;

    public string Modifier { get; set; } = "";

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();

    public EntityTypeModel_V210 Type { get; set; } = null!;

    public bool IsEvent { get; set; }

    public bool IsNullable { get; set; }

    public IList<MethodModel_V210> Accessors { get; set; } = new List<MethodModel_V210>();

    public LinesOfCode_V210 Loc { get; set; }

    public IList<MetricModel_V210> Metrics { get; init; } = new List<MetricModel_V210>();
}
