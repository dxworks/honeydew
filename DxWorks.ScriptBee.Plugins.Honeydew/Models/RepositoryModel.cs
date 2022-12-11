namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class RepositoryModel : ReferenceEntity
{
    public string Version { get; set; } = "";

    public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

    public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

    public IList<NamespaceModel> Namespaces { get; set; } = new List<NamespaceModel>();

    public IList<ClassModel> CreatedClasses { get; set; } = new List<ClassModel>();

    public IList<NamespaceModel> CreatedNamespaces { get; set; } = new List<NamespaceModel>();

    public IEnumerable<EntityModel> GetEnumerable()
    {
        foreach (var projectModel in Projects)
        {
            foreach (var fileModel in projectModel.Files)
            {
                foreach (var entity in fileModel.Entities)
                {
                    yield return entity;
                }
            }
        }
    }
}
