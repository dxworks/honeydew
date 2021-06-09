using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Models;
using HoneydewCore.IO.Writers;

namespace HoneydewCore.Models
{
    public class SolutionModel : IExportable
    {
        // Projects

        public IList<ProjectNamespace> Namespaces { get; set; } = new List<ProjectNamespace>();

        public void Add(ClassModel classModel)
        {
            var firstOrDefault = Namespaces.FirstOrDefault(ns => ns.Name == classModel.Namespace);
            if (firstOrDefault == default)
            {
                var projectNamespace = new ProjectNamespace();
                projectNamespace.Add(classModel);
                Namespaces.Add(projectNamespace);
            }
            else
            {
                firstOrDefault.Add(classModel);
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
    }
}