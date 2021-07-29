using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Processors
{
    public class FullNameModelProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        private class AmbiguousFullNameException : Exception
        {
            public string AmbiguousName { get; }
            public IList<string> PossibleNames { get; }

            public AmbiguousFullNameException(string ambiguousName, IList<string> possibleNames)
            {
                AmbiguousName = ambiguousName;
                PossibleNames = possibleNames;
            }
        }


        private readonly IProgressLogger _progressLogger;

        private readonly IDictionary<string, IList<string>> _ambiguousNames = new Dictionary<string, IList<string>>();

        public FullNameModelProcessor(IProgressLogger progressLogger)
        {
            _progressLogger = progressLogger;
        }

        public RepositoryModel Process(RepositoryModel repositoryModel)
        {
            SetFullNameForClassModels(repositoryModel);

            SetFullNameForClassModelComponents(repositoryModel);

            foreach (var (ambiguousName, possibilities) in _ambiguousNames)
            {
                _progressLogger.LogLine();
                _progressLogger.LogLine($"Multiple full names found for {ambiguousName}: ");
                foreach (var possibleName in possibilities)
                {
                    _progressLogger.LogLine(possibleName);
                }
            }

            return repositoryModel;
        }

        private void AddAmbiguousNames(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (AmbiguousFullNameException e)
            {
                if (!_ambiguousNames.ContainsKey(e.AmbiguousName))
                {
                    _ambiguousNames.Add(e.AmbiguousName, e.PossibleNames);
                }
            }
        }

        private void SetFullNameForClassModels(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            AddAmbiguousNames(() =>
                            {
                                classModel.FullName = FindClassFullName(classModel.FullName, namespaceModel,
                                    projectModel, solutionModel, repositoryModel, classModel.Usings);
                            });
                        }
                    }
                }
            }
        }

        private void SetFullNameForClassModelComponents(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            AddAmbiguousNames(() =>
                            {
                                classModel.BaseClassFullName = FindClassFullName(
                                    classModel.BaseClassFullName, namespaceModel, projectModel, solutionModel,
                                    repositoryModel, classModel.Usings);
                            });

                            for (var i = 0; i < classModel.BaseInterfaces.Count; i++)
                            {
                                var iCopy = i;
                                AddAmbiguousNames(() =>
                                {
                                    classModel.BaseInterfaces[iCopy] = FindClassFullName(
                                        classModel.BaseInterfaces[iCopy],
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Usings);
                                });
                            }

                            foreach (var fieldModel in classModel.Fields)
                            {
                                AddAmbiguousNames(() =>
                                {
                                    fieldModel.Type = FindClassFullName(fieldModel.Type,
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Usings);
                                });
                            }

                            foreach (var propertyModel in classModel.Properties)
                            {
                                AddAmbiguousNames(() =>
                                {
                                    propertyModel.Type = FindClassFullName(propertyModel.Type,
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Usings);
                                });
                            }

                            foreach (var methodModel in classModel.Methods)
                            {
                                AddAmbiguousNames(() =>
                                {
                                    methodModel.ReturnType = FindClassFullName(methodModel.ReturnType,
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Usings);
                                });

                                SetContainingClassAndCalledMethodsFullName(methodModel, namespaceModel,
                                    projectModel, solutionModel, classModel.Usings);
                            }

                            foreach (var methodModel in classModel.Constructors)
                            {
                                SetContainingClassAndCalledMethodsFullName(methodModel, namespaceModel,
                                    projectModel, solutionModel, classModel.Usings);
                            }

                            ChangeDependencyMetricFullName(repositoryModel, classModel, namespaceModel,
                                projectModel, solutionModel);
                        }
                    }
                }
            }

            void SetContainingClassAndCalledMethodsFullName(MethodModel methodModel, NamespaceModel namespaceModel,
                ProjectModel projectModel, SolutionModel solutionModel, IList<UsingModel> usings)
            {
                AddAmbiguousNames(() =>
                {
                    methodModel.ContainingClassName = FindClassFullName(methodModel.ContainingClassName,
                        namespaceModel, projectModel, solutionModel, repositoryModel, usings);
                });

                foreach (var parameterModel in methodModel.ParameterTypes)
                {
                    AddAmbiguousNames(() =>
                    {
                        parameterModel.Type = FindClassFullName(parameterModel.Type, namespaceModel,
                            projectModel, solutionModel, repositoryModel, usings);
                    });
                }

                foreach (var methodModelCalledMethod in methodModel.CalledMethods)
                {
                    AddAmbiguousNames(() =>
                    {
                        methodModelCalledMethod.ContainingClassName = FindClassFullName(
                            methodModelCalledMethod.ContainingClassName, namespaceModel, projectModel, solutionModel,
                            repositoryModel, usings);
                    });

                    foreach (var parameterModel in methodModelCalledMethod.ParameterTypes)
                    {
                        AddAmbiguousNames(() =>
                        {
                            parameterModel.Type = FindClassFullName(parameterModel.Type, namespaceModel,
                                projectModel, solutionModel, repositoryModel, usings);
                        });
                    }
                }
            }
        }

        private void ChangeDependencyMetricFullName(RepositoryModel repositoryModel, ClassModel classModel,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel)
        {
            var parameterDependenciesMetrics = classModel.Metrics.Where(metric =>
                typeof(CSharpDependencyMetric).IsAssignableFrom(Type.GetType(metric.ExtractorName)));

            foreach (var metric in parameterDependenciesMetrics)
            {
                var dependencies = metric.Value as IDictionary<string, int>;
                if (dependencies == null)
                {
                    continue;
                }

                IDictionary<string, int> fullNameDependencies = new Dictionary<string, int>();
                foreach (var (dependencyName, appearanceCount) in dependencies)
                {
                    var wasSet = false;

                    try
                    {
                        var fullClassName = FindClassFullName(dependencyName, namespaceModel, projectModel,
                            solutionModel, repositoryModel, classModel.Usings);

                        if (fullNameDependencies.ContainsKey(fullClassName))
                        {
                            continue;
                        }

                        if (fullClassName != dependencyName)
                        {
                            fullNameDependencies.Add(fullClassName, appearanceCount);
                            wasSet = true;
                        }

                        if (!wasSet)
                        {
                            fullNameDependencies.Add(dependencyName, appearanceCount);
                        }
                    }
                    catch (AmbiguousFullNameException e)
                    {
                        if (!fullNameDependencies.ContainsKey(dependencyName))
                        {
                            fullNameDependencies.Add(dependencyName, appearanceCount);
                        }

                        if (!_ambiguousNames.ContainsKey(e.AmbiguousName))
                        {
                            _ambiguousNames.Add(e.AmbiguousName, e.PossibleNames);
                        }
                    }
                }

                dependencies = fullNameDependencies;
                metric.Value = dependencies;
            }
        }


        private static string FindClassFullName(string className, NamespaceModel namespaceModelToStartSearchFrom,
            ProjectModel projectModelToStartSearchFrom, SolutionModel solutionModelToStartSearchFrom,
            RepositoryModel repositoryModel, IList<UsingModel> usings)
        {
            if (string.IsNullOrEmpty(className))
            {
                return className;
            }

            if (IsClassNameFullyQualified(className, projectModelToStartSearchFrom, solutionModelToStartSearchFrom,
                repositoryModel))
            {
                return className;
            }

            if (TryToGetClassNameFromNamespace(className, namespaceModelToStartSearchFrom,
                out var fullNameFromNamespace))
            {
                return fullNameFromNamespace;
            }

            // search in all provided usings
            List<string> fullNamePossibilities;
            if (usings != null)
            {
                fullNamePossibilities = new List<string>();
                foreach (var usingModel in usings)
                {
                    var usingNamespace =
                        projectModelToStartSearchFrom.Namespaces.FirstOrDefault(model => model.Name == usingModel.Name);
                    if (usingNamespace == null)
                    {
                        continue;
                    }

                    if (TryToGetClassNameFromNamespace(className, usingNamespace, out var fullNameFromUsingNamespace))
                    {
                        fullNamePossibilities.Add(fullNameFromUsingNamespace);
                    }
                }

                switch (fullNamePossibilities.Count)
                {
                    case 1:
                        return fullNamePossibilities.First();
                    case > 1:
                        throw new AmbiguousFullNameException(className, fullNamePossibilities);
                }
            }

            if (TryToGetClassNameFromProject(className, projectModelToStartSearchFrom, out var fullNameFromProject))
            {
                return fullNameFromProject;
            }

            // search in all projects of solutionModel
            if (TryToGetClassNameFromSolution(className, solutionModelToStartSearchFrom, out var fullNameFromSolution))
            {
                return fullNameFromSolution;
            }

            fullNamePossibilities = new List<string>();
            foreach (var solution in repositoryModel.Solutions)
            {
                if (solution == solutionModelToStartSearchFrom)
                {
                    continue;
                }

                if (TryToGetClassNameFromSolution(className, solution, out var fullName))
                {
                    fullNamePossibilities.Add(fullName);
                }
            }

            return fullNamePossibilities.Count switch
            {
                1 => fullNamePossibilities.First(),
                > 1 => throw new AmbiguousFullNameException(className, fullNamePossibilities),
                _ => className
            };
        }

        private static bool TryToGetClassNameFromSolution(string className, SolutionModel solutionModel,
            out string outFullName)
        {
            var fullNamePossibilities = new List<string>();
            outFullName = className;

            foreach (var projectModel in solutionModel.Projects)
            {
                if (TryToGetClassNameFromProject(className, projectModel, out var fullName))
                {
                    fullNamePossibilities.Add(fullName);
                }
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                {
                    outFullName = fullNamePossibilities.First();
                    return true;
                }
                case > 1:
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
            }

            return false;
        }

        private static bool TryToGetClassNameFromProject(string className, ProjectModel projectModel,
            out string outFullName)
        {
            var fullNamePossibilities = new List<string>();
            outFullName = className;

            foreach (var namespaceModel in projectModel.Namespaces)
            {
                if (TryToGetClassNameFromNamespace(className, namespaceModel, out var fullName))
                {
                    fullNamePossibilities.Add(fullName);
                }
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                {
                    outFullName = fullNamePossibilities.First();
                    return true;
                }
                case > 1:
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
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

        private static bool IsClassNameFullyQualified(string classNameToSearch,
            ProjectModel projectModelToStartSearchFrom,
            SolutionModel solutionModelToStartSearchFrom, RepositoryModel repositoryModel)
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
            foreach (var solutionModel in repositoryModel.Solutions)
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

                var namespaceModel =
                    projectModel.Namespaces.FirstOrDefault(model => model.Name == namespaceName.ToString());
                if (namespaceModel != null)
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
