using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record PropertyModel : IPropertyType
{
    public int CyclomaticComplexity { get; set; }
    public string Name { get; set; }

    public string AccessModifier { get; set; }

    public string Modifier { get; set; } = "";

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

    public IEntityType Type { get; set; }

    public bool IsEvent { get; set; }

    public bool IsNullable { get; set; }

    public IList<IAccessorType> Accessors { get; set; } = new List<IAccessorType>();

    public LinesOfCode Loc { get; set; }

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
}
