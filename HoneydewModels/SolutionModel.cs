using System.Collections.Generic;

namespace HoneydewModels;

public record SolutionModel
{
    public string FilePath { get; set; }

    public IList<string> ProjectsPaths { get; set; } = new List<string>();
}
