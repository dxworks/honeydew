using System.Collections.Generic;
using HoneydewCore.IO.Writers;

namespace HoneydewCore.Models
{
    public class SolutionModel : IExportable
    {
        // Projects

        public IDictionary<string, ProjectNamespace> Namespaces { get; set; } =
            new Dictionary<string, ProjectNamespace>();

        public void Add(ProjectClassModel classModel)
        {
            if (string.IsNullOrEmpty(classModel.Namespace))
            {
                return;
            }

            if (Namespaces.TryGetValue(classModel.Namespace, out var projectNamespace))
            {
                projectNamespace.Add(classModel);
            }
            else
            {
                var p = new ProjectNamespace();
                p.Add(classModel);
                Namespaces.Add(classModel.Namespace, p);
            }
        }

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
                if (Namespaces.TryGetValue(usingName, out var projectNamespace))
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

            return className;
        }
    }
}