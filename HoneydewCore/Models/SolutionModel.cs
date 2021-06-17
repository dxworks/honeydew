using System.Collections.Generic;
using HoneydewCore.IO.Writers;

namespace HoneydewCore.Models
{
    public class SolutionModel : IExportable
    {
        public IList<ProjectModel> Projects { get; set; } = new List<ProjectModel>();

        public string Export(IExporter exporter)
        {
            if (exporter is ISolutionModelExporter modelExporter)
            {
                return modelExporter.Export(this);
            }

            return string.Empty;
        }


        public string FindClassFullNameInUsings(IList<string> usings, string className)
        {
            foreach (var usingName in usings)
            {
                foreach (var projectModel in Projects)
                {
                    if (projectModel.Namespaces.TryGetValue(usingName, out var projectNamespace))
                    {
                        foreach (var classModel in projectNamespace.ClassModels)
                        {
                            if (classModel.FullName == $"{usingName}.{className}")
                            {
                                return classModel.FullName;
                            }
                        }
                    }
                }
            }

            return className;
        }
    }
}