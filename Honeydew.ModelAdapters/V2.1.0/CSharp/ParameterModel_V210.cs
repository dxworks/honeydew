namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record ParameterModel_V210
{
    public EntityTypeModel_V210 Type { get; set; } = null!;

    public string Modifier { get; set; } = "";

    public string DefaultValue { get; set; } = null!;

    public bool IsNullable { get; set; }

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();
}
