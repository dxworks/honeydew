namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public record SolutionModel_V210
{
    public string FilePath { get; set; } = null!;

    public IList<string> ProjectsPaths { get; set; } = new List<string>();
}
