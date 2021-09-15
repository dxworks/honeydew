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
}
