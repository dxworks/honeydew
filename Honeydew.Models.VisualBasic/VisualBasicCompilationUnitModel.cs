using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicCompilationUnitModel : ICompilationUnitType
{
    public string FilePath { get; set; } = "";
    public IList<IClassType> ClassTypes { get; set; } = new List<IClassType>();
    public IList<IImportType> Imports { get; set; } = new List<IImportType>();
    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();

    public LinesOfCode Loc { get; set; }
}
