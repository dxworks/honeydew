using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public class VisualBasicClassModel : IPropertyMembersClassType
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string ClassType { get; set; } = "";
    public string AccessModifier { get; set; } = "";
    public string ContainingNamespaceName { get; set; } = "";
    public string ContainingModuleName { get; set; } = "";
    public string ContainingClassName { get; set; } = "";
    public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();
    public IList<IImportType> Imports { get; set; } = new List<IImportType>();
    public IDestructorType Destructor { get; set; }
    public IList<IGenericParameterType> GenericParameters { get; set; } = new List<IGenericParameterType>();
    public IList<IFieldType> Fields { get; init; } = new List<IFieldType>();
    public IList<IConstructorType> Constructors { get; init; } = new List<IConstructorType>();
    public IList<IMethodType> Methods { get; init; } = new List<IMethodType>();
    public IList<IPropertyType> Properties { get; set; } = new List<IPropertyType>();
    public string Modifier { get; set; } = "";
    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    public LinesOfCode Loc { get; set; }
}
