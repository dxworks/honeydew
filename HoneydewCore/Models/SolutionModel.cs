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

        public string FindClassFullNameInUsings(IList<string> usings, string className, string classModelNamespace = "")
        {
            if (string.IsNullOrEmpty(classModelNamespace))
            {
                if (FindClassName(className) != null)
                {
                    return className;
                }
            }
            else
            {
                var classModel = FindClassName($"{classModelNamespace}.{className}");
                if (classModel != null)
                {
                    return classModel.FullName;
                }
            }

            foreach (var usingName in usings)
            {
                var classModel = FindClassName($"{usingName}.{className}");
                if (classModel != null)
                {
                    return classModel.FullName;
                }
            }

            var firstOrDefault = GetEnumerable().FirstOrDefault(classModel => classModel.FullName.Contains(className));
            return firstOrDefault == null ? className : firstOrDefault.FullName;
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
            if (className == null)
            {
                return null;
            }

            if (!className.Contains("."))
                return GetEnumerable().FirstOrDefault(model => model.FullName.Contains(className));

            var classModel = FindClassName(className);

            return classModel != null
                ? classModel
                : GetEnumerable().FirstOrDefault(model => model.FullName.Contains(className));
        }

        public string GetClassFullName(string namespaceNameToStartSearchFrom, string className)
        {
            if (className == null)
            {
                return null;
            }

            if (className.Contains(namespaceNameToStartSearchFrom))
            {
                return className;
            }

            ClassModel findClassName;
            if (string.IsNullOrEmpty(namespaceNameToStartSearchFrom) || className.Contains("."))
            {
                findClassName = FindClassName(className);
                if (findClassName != null)
                {
                    return findClassName.FullName;
                }
            }

            if (!string.IsNullOrEmpty(namespaceNameToStartSearchFrom))
            {
                foreach (var projectModel in Projects)
                {
                    if (!projectModel.Namespaces.TryGetValue(namespaceNameToStartSearchFrom, out var namespaceModel))
                        continue;

                    if (namespaceModel.ClassModels.Any(model => model.FullName.Equals(className)))
                    {
                        return $"{namespaceNameToStartSearchFrom}.{className}";
                    }
                }
            }

            findClassName = FindClassName($"{namespaceNameToStartSearchFrom}.{className}");
            if (findClassName != null)
            {
                return findClassName.FullName;
            }

            var firstOrDefault = GetEnumerable().FirstOrDefault(classModel => classModel.FullName.Contains(className));
            return firstOrDefault == null ? className : firstOrDefault.FullName;
        }


        private ClassModel FindClassName(string classNameToSearch)
        {
            IReadOnlyList<string> parts = classNameToSearch.Split(".");

            foreach (var project in Projects)
            {
                var namespaceName = "";
                var namespaceIndex = 0;
                while (namespaceIndex < parts.Count)
                {
                    namespaceName += parts[namespaceIndex];

                    if (project.Namespaces.TryGetValue(namespaceName, out var namespaceModel))
                    {
                        var className = "";
                        var classIndex = namespaceIndex + 1;
                        while (classIndex < parts.Count)
                        {
                            className += parts[classIndex];
                            className += '.';
                            classIndex++;
                        }

                        className = className.Remove(className.Length - 1);

                        var classModel = namespaceModel.ClassModels.FirstOrDefault(model =>
                            model.FullName == $"{namespaceName}.{className}");
                        if (classModel != null)
                        {
                            return classModel;
                        }
                    }

                    namespaceIndex++;
                    namespaceName += '.';
                }
            }

            return null;
        }
    }
}