namespace Honeydew.Models;

public class RepositoryModel
{
    public string Version { get; set; } = "";

    public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

    public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();
}
