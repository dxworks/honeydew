namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record DelegateModel_V210 : IClassType_V210
{
    public string ClassType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string Name { get; set; } = null!;

    public IList<GenericParameterModel_V210> GenericParameters { get; set; } = new List<GenericParameterModel_V210>();

    public IList<BaseTypeModel_V210> BaseTypes { get; set; } = new List<BaseTypeModel_V210>();

    public IList<UsingModel_V210> Imports { get; set; } = new List<UsingModel_V210>();

    public string ContainingTypeName { get; set; } = null!;

    public string AccessModifier { get; set; } = null!;

    public string Modifier { get; set; } = null!;

    public IList<AttributeModel_V210> Attributes { get; set; } = new List<AttributeModel_V210>();

    public IList<ParameterModel_V210> ParameterTypes { get; set; } = new List<ParameterModel_V210>();

    public ReturnValueModel_V210? ReturnValue { get; set; }

    public IList<MetricModel_V210> Metrics { get; init; } = new List<MetricModel_V210>();
}
