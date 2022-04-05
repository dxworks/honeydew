using System.Collections.Generic;

namespace HoneydewModels.CSharp;

public class RepositoryModel : IRepositoryModel
{
    public string Version { get; set; }

    public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

    public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();
}
