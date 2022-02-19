using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record ClassModel : IPropertyMembersClassType
{
    public string ClassType { get; set; }

    public string Name { get; set; }

    public IList<IGenericParameterType> GenericParameters { get; set; } = new List<IGenericParameterType>();

    public string FilePath { get; set; }

    public LinesOfCode Loc { get; set; }

    public string AccessModifier { get; set; }

    public string Modifier { get; set; } = "";

    public string ContainingNamespaceName { get; set; } = "";

    public string ContainingClassName { get; set; } = "";

    public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();

    public IList<IImportType> Imports { get; set; } = new List<IImportType>();

    public IList<IFieldType> Fields { get; init; } = new List<IFieldType>();

    public IList<IPropertyType> Properties { get; set; } = new List<IPropertyType>();

    public IList<IConstructorType> Constructors { get; init; } = new List<IConstructorType>();

    public IList<IMethodType> Methods { get; init; } = new List<IMethodType>();

    public IDestructorType Destructor { get; set; }

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
