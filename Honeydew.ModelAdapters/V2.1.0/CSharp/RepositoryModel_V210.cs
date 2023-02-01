namespace Honeydew.ModelAdapters.V2._1._0.CSharp;

public class RepositoryModel_V210
{
    public string Version { get; set; } = null!;

    public IList<SolutionModel_V210> Solutions { get; set; } = new List<SolutionModel_V210>();

    public IList<ProjectModel_V210> Projects { get; set; } = new List<ProjectModel_V210>();

    public IEnumerable<IClassType_V210> GetEnumerable()
    {
        foreach (var projectModel in Projects)
        {
            foreach (var compilationUnitType in projectModel.CompilationUnits)
            {
                foreach (var classType in compilationUnitType.ClassTypes)
                {
                    yield return classType;
                }
            }
        }
    }
}
