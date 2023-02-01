namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record GenericParameterModel_V210
{
    public string Name { get; set; } = null!;

    public string Modifier { get; set; } = null!;

    public IList<EntityTypeModel_V210> Constraints { get; set; } = new List<EntityTypeModel_V210>();

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();
}
