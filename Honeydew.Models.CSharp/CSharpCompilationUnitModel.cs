using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record CSharpCompilationUnitModel : ICompilationUnitType
{
    public IList<IClassType> ClassTypes { get; set; } = new List<IClassType>();

    public string FilePath { get; set; } = "";

    public IList<IImportType> Imports { get; set; } = new List<IImportType>();

    public LinesOfCode Loc { get; set; }

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
}
