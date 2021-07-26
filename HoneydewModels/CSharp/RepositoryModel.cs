using System.Collections.Generic;

namespace HoneydewModels.CSharp
{
    public class RepositoryModel : IRepositoryModel
    {
        public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

        public IEnumerable<ClassModel> GetEnumerable()
        {
            foreach (var solutionModel in Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            yield return classModel;
                        }
                    }
                }
            }
        }
    }
}
