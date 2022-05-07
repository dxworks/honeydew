namespace Honeydew.Models;

public record SolutionModel
{
    public string FilePath { get; set; } = "";

    public IList<string> ProjectsPaths { get; set; } = new List<string>();
    
    public IDictionary<string, Dictionary<string, string>> Metadata =
        new Dictionary<string, Dictionary<string, string>>();
}
