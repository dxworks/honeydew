using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record ConstructorModel : IConstructorType
{
    public string Name { get; set; }

    public string AccessModifier { get; set; }

    public string Modifier { get; set; }

    public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

    public IList<IMethodCallType> CalledMethods { get; set; } = new List<IMethodCallType>();

    public IList<AccessedField> AccessedFields { get; set; } = new List<AccessedField>();

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

    public IList<IMethodTypeWithLocalFunctions> LocalFunctions { get; set; } =
        new List<IMethodTypeWithLocalFunctions>();

    public IList<ILocalVariableType> LocalVariableTypes { get; set; } = new List<ILocalVariableType>();

    public LinesOfCode Loc { get; set; }

    public int CyclomaticComplexity { get; set; }

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
}
