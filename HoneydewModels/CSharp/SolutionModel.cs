using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record SolutionModel
    {
        public string FilePath { get; set; }
        public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

        public IEnumerable<IClassType> GetEnumerable()
        {
            foreach (var projectModel in Projects)
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
