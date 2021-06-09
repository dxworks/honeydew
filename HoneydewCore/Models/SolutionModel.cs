using System.Collections.Generic;
using HoneydewCore.IO.Writers;

namespace HoneydewCore.Models
{
    public class SolutionModel : IExportable
    {
        public IList<ProjectModel> Projects { get; init; }

        public IList<ProjectClassModel> ProjectClassModels { get; } = new List<ProjectClassModel>();

        public void Add(IEnumerable<CompilationUnitModel> compilationUnitModels, string path)
        {
            if (compilationUnitModels == null) return;

            foreach (var model in compilationUnitModels)
            {
                foreach (var classModel in model.ClassModels)
                {
                    ProjectClassModels.Add(new ProjectClassModel
                    {
                        Model = classModel,
                        Path = path
                    });
                }
            }
        }

        public void MakeModel()
        {
        }

        public string Export(IExporter exporter)
        {
            if (typeof(ISolutionModelExporter) == exporter.GetType())
            {
                return ((ISolutionModelExporter) exporter).Export(this);
            }

            return string.Empty;
        }
    }
}