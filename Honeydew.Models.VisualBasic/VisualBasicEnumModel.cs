using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicEnumModel : IEnumType
{
    public string Name { get; set; } = "";
    public string ClassType { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string AccessModifier { get; set; } = "";
    public string Modifier { get; set; } = "";
    public string Type { get; set; } = "int";
    public IList<IEnumLabelType> Labels { get; set; } = new List<IEnumLabelType>();
    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    public string ContainingNamespaceName { get; set; } = "";
    public string ContainingModuleName { get; set; } = "";
    public string ContainingClassName { get; set; } = "";
    public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();
    public IList<IImportType> Imports { get; set; } = new List<IImportType>();
    public LinesOfCode Loc { get; set; }
    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
}
