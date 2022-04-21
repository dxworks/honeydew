using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record AccessorModel : IAccessorType
{
    public string Name { get; set; }

    public IReturnValueType ReturnValue { get; set; }

    public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

    public IList<IMethodCallType> CalledMethods { get; set; } = new List<IMethodCallType>();

    public IList<AccessedField> AccessedFields { get; set; } = new List<AccessedField>();

    public string AccessModifier { get; set; } = "";

    public string Modifier { get; set; } = "";

    public int CyclomaticComplexity { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();

    public LinesOfCode Loc { get; set; }

    public IList<ILocalVariableType> LocalVariableTypes { get; set; } = new List<ILocalVariableType>();

    public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; } =
        new List<IMethodTypeWithLocalFunctions>();
}
