using System.Collections;
using System.Collections.Generic;
using HoneydewCore.IO.Writers.Exporters;

namespace HoneydewCore.Models
{
    public class RepositoryModel : IExportable
    {
        public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

        public string Export(IExporter exporter)
        {
            if (exporter is IRepositoryModelExporter modelExporter)
            {
                return modelExporter.Export(this);
            }

            return string.Empty;
        }

        public IEnumerable<ClassModel> GetEnumerable()
        {
            foreach (var solutionModel in Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
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
}