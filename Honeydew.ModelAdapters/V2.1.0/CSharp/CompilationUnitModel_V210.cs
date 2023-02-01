namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public class CompilationUnitModel_V210
{
    public IList<IClassType_V210> ClassTypes { get; set; } = new List<IClassType_V210>();

    public string FilePath { get; set; } = null!;

    public IList<UsingModel_V210> Imports { get; set; } = new List<UsingModel_V210>();

    public LinesOfCode_V210 Loc { get; set; }

    public IList<MetricModel_V210> Metrics { get; init; } = new List<MetricModel_V210>();
}
