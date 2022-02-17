using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record DelegateModel : IDelegateType
{
    public string ClassType { get; set; }

    public string FilePath { get; set; }

    public string Name { get; set; }

    public IList<IGenericParameterType> GenericParameters { get; set; } = new List<IGenericParameterType>();

    public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();

    public IList<IImportType> Imports { get; set; } = new List<IImportType>();

    public string ContainingNamespaceName { get; set; }

    public string ContainingClassName { get; set; }

    public string AccessModifier { get; set; }

    public string Modifier { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

    public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

    public IReturnValueType ReturnValue { get; set; }

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
}
