using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Writers.Exporters;

namespace HoneydewCore.Models
{
    public record SolutionModel : IExportable
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
            foreach (var projectModel in Projects)
            {
                var fullName = projectModel.FindClassFullNameInUsings(usings, className);
                if (fullName != className)
                {
                    return fullName;
                }
            }

            return className;
        }

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

        public ClassModel GetClassModelByFullName(string className)
        {
            var lastIndexOf = className.LastIndexOf(".", StringComparison.Ordinal);


            var classNamespace = "";
            if (lastIndexOf >= 0)
            {
                classNamespace = className[..lastIndexOf];
            }

            foreach (var project in Projects)
            {
                if (project.Namespaces.TryGetValue(classNamespace, out var namespaceModel))
                {
                    return namespaceModel.ClassModels.FirstOrDefault(model => model.FullName == className);
                }
            }

            return default;
        }
        
        public string GetClassFullName(string className)
        {
            var classModel = GetClassModelByFullName(className);
            
            return classModel == default ? className : classModel.FullName;
        }
    }
}