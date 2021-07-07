using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string FindClassFullName(string className, NamespaceModel namespaceModel, ProjectModel projectModel,
            SolutionModel solutionModel, IList<string> usings = null)
        {
            if (string.IsNullOrEmpty(className))
            {
                return className;
            }

            if (IsClassNameFullyQualified(className, projectModel, solutionModel))
            {
                return className;
            }

            if (TryToGetClassNameFromNamespace(className, namespaceModel, out var fullNameFromNamespace))
            {
                return fullNameFromNamespace;
            }

            if (usings != null)
            {
                // search in all provided usings
                foreach (var usingName in usings)
                {
                    var usingNamespace = FindNamespaceInProject(usingName, projectModel);
                    if (usingNamespace == default)
                    {
                        continue;
                    }

                    if (TryToGetClassNameFromNamespace(className, usingNamespace, out var fullNameFromUsingNamespace))
                    {
                        return fullNameFromUsingNamespace;
                    }
                }
            }

            if (TryToGetClassNameFromProject(className, projectModel, out var fullNameFromProject))
            {
                return fullNameFromProject;
            }

            // search in all projects of solutionModel
            if (TryToGetClassNameFromSolution(className, solutionModel, out var fullNameFromSolution))
            {
                return fullNameFromSolution;
            }

            foreach (var solution in Solutions)
            {
                if (solution == solutionModel)
                {
                    continue;
                }

                if (TryToGetClassNameFromSolution(className, solutionModel, out var fullName))
                {
                    return fullName;
                }
            }

            return className;
        }

        private static bool TryToGetClassNameFromSolution(string className, SolutionModel solutionModel,
            out string outFullName)
        {
            outFullName = className;

            foreach (var projectModel in solutionModel.Projects)
            {
                if (TryToGetClassNameFromProject(className, projectModel, out var fullName))
                {
                    outFullName = fullName;
                    return true;
                }
            }

            return false;
        }

        private static bool TryToGetClassNameFromProject(string className, ProjectModel projectModel,
            out string outFullName)
        {
            outFullName = className;

            foreach (var (_, namespaceModel) in projectModel.Namespaces)
            {
                if (TryToGetClassNameFromNamespace(className, namespaceModel, out var fullName))
                {
                    outFullName = fullName;
                    return true;
                }
            }

            return false;
        }

        private static bool TryToGetClassNameFromNamespace(string className, NamespaceModel namespaceModel,
            out string outClassFullName)
        {
            outClassFullName = className;

            var fullName = $"{namespaceModel.Name}.{className}";

            if (namespaceModel.ClassModels.Any(classModel =>
                classModel.FullName == fullName || classModel.FullName.Equals(className)))
            {
                outClassFullName = fullName;
                return true;
            }

            return false;
        }

        private NamespaceModel FindNamespaceInProject(string namespaceName, ProjectModel projectModel)
        {
            return projectModel.Namespaces.TryGetValue(namespaceName, out var namespaceModel)
                ? namespaceModel
                : default;
        }

        private bool IsClassNameFullyQualified(string classNameToSearch, ProjectModel projectModelToStartSearchFrom,
            SolutionModel solutionModelToStartSearchFrom)
        {
            IReadOnlyList<string> parts = classNameToSearch.Split(".");

            // search for fully name in provided ProjectModel 
            if (IsClassNameFullyQualified(parts, projectModelToStartSearchFrom))
            {
                return true;
            }

            // search for fully name in provided SolutionModel
            if (solutionModelToStartSearchFrom.Projects.Any(project => IsClassNameFullyQualified(parts, project)))
            {
                return true;
            }

            // search for fully name in all solutions
            foreach (var solutionModel in Solutions)
            {
                if (solutionModel == solutionModelToStartSearchFrom)
                {
                    continue;
                }

                if (solutionModel.Projects.Where(projectModel => projectModel != projectModelToStartSearchFrom)
                    .Any(projectModel => IsClassNameFullyQualified(parts, projectModel)))
                {
                    return true;
                }
            }

            return false;
        }

        // parts contains the class name split in parts
        // a fully qualified name is generated and compared with the namespaces found in the provided ProjectModel
        private static bool IsClassNameFullyQualified(IReadOnlyList<string> classNameParts, ProjectModel projectModel)
        {
            var namespaceName = new StringBuilder();
            var namespaceIndex = 0;
            while (namespaceIndex < classNameParts.Count)
            {
                namespaceName.Append(classNameParts[namespaceIndex]);

                if (projectModel.Namespaces.TryGetValue(namespaceName.ToString(), out var namespaceModel))
                {
                    var className = new StringBuilder();
                    var classIndex = namespaceIndex + 1;
                    while (classIndex < classNameParts.Count)
                    {
                        className.Append(classNameParts[classIndex]);
                        className.Append('.');
                        classIndex++;
                    }

                    var classModel =
                        namespaceModel.ClassModels.FirstOrDefault(model =>
                        {
                            var fullyQualifiedName = $"{namespaceName}.{className}";
                            fullyQualifiedName = fullyQualifiedName.Remove(fullyQualifiedName.Length - 1);

                            return model.FullName == fullyQualifiedName;
                        });
                    if (classModel != null)
                    {
                        return true;
                    }
                }

                namespaceIndex++;
                namespaceName.Append('.');
            }

            return false;
        }
    }
}