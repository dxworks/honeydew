using System.Collections.Generic;

namespace HoneydewModels
{
    public record SolutionModel
    {
        public string FilePath { get; set; }
        public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

        public IEnumerable<ClassModel> GetEnumerable()
        {
            foreach (var projectModel in Projects)
            {
                foreach (var (_, namespaceModel) in projectModel.Namespaces)
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
