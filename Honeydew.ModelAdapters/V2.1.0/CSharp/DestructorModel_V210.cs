namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public class DestructorModel_V210
{
    public string Name { get; set; } = null!;

    public string ContainingTypeName { get; set; } = null!;

    public IList<ParameterModel_V210> ParameterTypes { get; set; } = new List<ParameterModel_V210>();

    public IList<MethodCallModel_V210> CalledMethods { get; set; } = new List<MethodCallModel_V210>();

    public IList<AccessedField_V210> AccessedFields { get; set; } = new List<AccessedField_V210>();

    public string AccessModifier { get; set; } = null!;

    public string Modifier { get; set; } = null!;

    public int CyclomaticComplexity { get; set; }

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();

    public IList<MetricModel_V210> Metrics { get; init; } = new List<MetricModel_V210>();

    public LinesOfCode_V210 Loc { get; set; }

    public IList<LocalVariableModel_V210> LocalVariableTypes { get; set; } = new List<LocalVariableModel_V210>();

    public IList<MethodModel_V210> LocalFunctions { get; set; } = new List<MethodModel_V210>();
}
