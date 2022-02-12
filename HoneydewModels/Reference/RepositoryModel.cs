using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class RepositoryModel : ReferenceEntity
{
    public string Version { get; set; }

    public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

    public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

    public IList<NamespaceModel> Namespaces { get; set; } = new List<NamespaceModel>();

    public IList<ClassModel> CreatedClasses { get; set; } = new List<ClassModel>();

    public IEnumerable<ClassOption> GetEnumerable()
    {
        foreach (var projectModel in Projects)
        {
            foreach (var compilationUnitType in projectModel.Files)
            {
                foreach (var classType in compilationUnitType.Classes)
                {
                    yield return new ClassOption.Class(classType);
                }

                foreach (var delegateModel in compilationUnitType.Delegates)
                {
                    yield return new ClassOption.Delegate(delegateModel);
                }
            }
        }
    }
}
