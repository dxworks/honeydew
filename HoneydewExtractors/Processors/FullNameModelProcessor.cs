using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
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

            SetFullNameForUsings(repositoryModel);

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

        private void SetFullNameForUsings(RepositoryModel repositoryModel)
        {
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            foreach (var usingModel in classModel.Usings)
                            {
                                if (usingModel.AliasType == EAliasType.NotDetermined)
                                {
                                    usingModel.AliasType = DetermineUsingType(usingModel, classModel);
                                }

                                if (usingModel.AliasType != EAliasType.Class)
                                {
                                    continue;
                                }

                                AddAmbiguousNames(() =>
                                {
                                    usingModel.Name = FindClassFullName(usingModel.Name, namespaceModel,
                                        projectModel, solutionModel, repositoryModel, classModel.Usings);
                                });
                            }
                        }
                    }
                }
            }
        }

        private static EAliasType DetermineUsingType(UsingModel usingModel, ClassModel classModel)
        {
            var namespaceAccess = $"{usingModel.Alias}.";

            if (classModel.Fields.Any(fieldModel => fieldModel.Type.StartsWith(namespaceAccess)))
            {
                return EAliasType.Namespace;
            }

            foreach (var propertyModel in classModel.Properties)
            {
                if (propertyModel.Type.StartsWith(namespaceAccess))
                {
                    return EAliasType.Namespace;
                }

                if (IsAliasNamespaceSearchInCalledMethods(propertyModel.CalledMethods))
                {
                    return EAliasType.Namespace;
                }
            }

            if (IsAliasNamespaceSearchInMethodModels(classModel.Methods))
            {
                return EAliasType.Namespace;
            }

            if (IsAliasNamespaceSearchInMethodModels(classModel.Constructors))
            {
                return EAliasType.Namespace;
            }

            return EAliasType.Class;

            bool IsAliasNamespaceSearchInMethodModels(IEnumerable<MethodModel> methodModels)
            {
                foreach (var methodModel in methodModels)
                {
                    if (!string.IsNullOrEmpty(methodModel.ReturnType) && methodModel.ReturnType.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (methodModel.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.StartsWith(namespaceAccess)))
                    {
                        return true;
                    }

                    if (IsAliasNamespaceSearchInCalledMethods(methodModel.CalledMethods))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool IsAliasNamespaceSearchInCalledMethods(IEnumerable<MethodCallModel> calledMethods)
            {
                foreach (var calledMethod in calledMethods)
                {
                    if (!string.IsNullOrEmpty(calledMethod.ContainingClassName) && calledMethod.ContainingClassName.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (calledMethod.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.StartsWith(namespaceAccess)))
                    {
                        return true;
                    }
                }

                return false;
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

                                SetContainingClassAndCalledMethodsFullNameProperty(propertyModel, namespaceModel,
                                    projectModel, solutionModel, classModel.Usings);
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

            void SetContainingClassAndCalledMethodsFullNameProperty(PropertyModel propertyModel,
                NamespaceModel namespaceModel,
                ProjectModel projectModel, SolutionModel solutionModel, IList<UsingModel> usings)
            {
                AddAmbiguousNames(() =>
                {
                    propertyModel.ContainingClassName = FindClassFullName(propertyModel.ContainingClassName,
                        namespaceModel, projectModel, solutionModel, repositoryModel, usings);
                });

                SetFullNameForCalledMethods(propertyModel.CalledMethods, namespaceModel,
                    projectModel, solutionModel, repositoryModel, usings);
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

                SetFullNameForCalledMethods(methodModel.CalledMethods, namespaceModel,
                    projectModel, solutionModel, repositoryModel, usings);
            }
        }

        private void SetFullNameForCalledMethods(IEnumerable<MethodCallModel> methodModelCalledMethods,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel,
            RepositoryModel repositoryModel, IList<UsingModel> usings)
        {
            foreach (var calledMethod in methodModelCalledMethods)
            {
                foreach (var parameterModel in calledMethod.ParameterTypes)
                {
                    AddAmbiguousNames(() =>
                    {
                        parameterModel.Type = FindClassFullName(parameterModel.Type, namespaceModel,
                            projectModel, solutionModel, repositoryModel, usings);
                    });
                }

                AddAmbiguousNames(() =>
                {
                    calledMethod.ContainingClassName = FindClassFullName(
                        calledMethod.ContainingClassName, namespaceModel, projectModel, solutionModel,
                        repositoryModel, usings);

                    var classModel = GetClassModelFullyQualified(calledMethod.ContainingClassName, projectModel,
                        solutionModel,
                        repositoryModel);

                    // search for static import of a class
                    if (classModel == null)
                    {
                        calledMethod.ContainingClassName = SearchForMethodNameInStaticImports(
                            calledMethod.ContainingClassName, calledMethod.MethodName,
                            calledMethod.ParameterTypes, projectModel, solutionModel, repositoryModel, usings);
                    }
                });
            }
        }

        private string SearchForMethodNameInStaticImports(string containingClassName, string methodName,
            IList<ParameterModel> methodParameters,
            ProjectModel projectModel, SolutionModel solutionModel,
            RepositoryModel repositoryModel, IList<UsingModel> usings)
        {
            if (!string.IsNullOrEmpty(containingClassName) && containingClassName.StartsWith("System."))
            {
                return containingClassName;
            }

            if (usings == null)
            {
                return "";
            }

            foreach (var usingModel in usings)
            {
                if (!usingModel.IsStatic)
                {
                    continue;
                }

                var staticImportedClass =
                    GetClassModelFullyQualified(usingModel.Name, projectModel, solutionModel, repositoryModel);

                if (staticImportedClass == null)
                {
                    continue;
                }

                foreach (var fieldModel in staticImportedClass.Fields)
                {
                    if (fieldModel.Name == containingClassName)
                    {
                        return ConvertToSystemName(fieldModel.Type);
                    }
                }

                foreach (var propertyModel in staticImportedClass.Properties)
                {
                    if (propertyModel.Name == containingClassName)
                    {
                        return ConvertToSystemName(propertyModel.Type);
                    }
                }

                foreach (var methodModel in staticImportedClass.Methods)
                {
                    var hasTheSameSignature = MethodsHaveTheSameSignature(methodModel.Name, methodModel.ParameterTypes,
                        methodName, methodParameters);
                    if (hasTheSameSignature)
                    {
                        return ConvertToSystemName(staticImportedClass.FullName);
                    }
                }
            }

            return containingClassName;
        }

        private static bool MethodsHaveTheSameSignature(string firstMethodName,
            IList<ParameterModel> firstMethodParameters,
            string secondMethodName, IList<ParameterModel> secondMethodParameters)
        {
            if (firstMethodName != secondMethodName)
            {
                return false;
            }

            if (firstMethodParameters.Count != secondMethodParameters.Count)
            {
                return false;
            }

            for (var i = 0; i < firstMethodParameters.Count; i++)
            {
                if (ConvertToSystemName(firstMethodParameters[i].Type) !=
                    ConvertToSystemName(secondMethodParameters[i].Type))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(firstMethodParameters[i].Modifier) &&
                    !string.IsNullOrEmpty(secondMethodParameters[i].Modifier))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(firstMethodParameters[i].Modifier) &&
                    string.IsNullOrEmpty(secondMethodParameters[i].Modifier))
                {
                    return false;
                }
            }

            return true;
        }

        // private static string ConvertToPrimitiveName(string type)
        // {
        //     return type switch
        //     {
        //         "System.Byte" => "byte",
        //         "System.SByte" => "sbyte",
        //         "System.Int32" => "int",
        //         "System.UInt32" => "uint",
        //         "System.Int16" => "short",
        //         "System.UInt16" => "ushort",
        //         "System.Int64" => "long",
        //         "System.UInt64" => "ulong",
        //         "System.Single" => "float",
        //         "System.Double" => "double",
        //         "System.Char" => "char",
        //         "System.Boolean" => "bool",
        //         "System.Object" => "object",
        //         "System.String" => "string",
        //         "System.Decimal" => "decimal",
        //         "System.DateTime" => "DateTime",
        //         _ => type
        //     };
        // }

        private static string ConvertToSystemName(string type)
        {
            return type switch
            {
                "byte" => "System.Byte",
                "sbyte" => "System.SByte",
                "int" => "System.Int32",
                "uint" => "System.UInt32",
                "short" => "System.Int16",
                "ushort" => "System.UInt16",
                "long" => "System.Int64",
                "ulong" => "System.UInt64",
                "float" => "System.Single",
                "double" => "System.Double",
                "char" => "System.Char",
                "bool" => "System.Boolean",
                "object" => "System.Object",
                "string" => "System.String",
                "decimal" => "System.Decimal",
                "DateTime" => "System.DateTime",
                _ => type
            };
        }

        private void ChangeDependencyMetricFullName(RepositoryModel repositoryModel, ClassModel classModel,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel)
        {
            var parameterDependenciesMetrics = classModel.Metrics.Where(metric =>
                typeof(CSharpRelationMetric).IsAssignableFrom(Type.GetType(metric.ExtractorName)));

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

            if (GetClassModelFullyQualified(className, projectModelToStartSearchFrom, solutionModelToStartSearchFrom,
                repositoryModel) != null)
            {
                return ConvertToSystemName(className);
            }

            if (TryToGetClassNameFromNamespace(className, namespaceModelToStartSearchFrom,
                out var fullNameFromNamespace))
            {
                return ConvertToSystemName(fullNameFromNamespace);
            }

            // search in all provided usings
            List<string> fullNamePossibilities;
            if (usings != null)
            {
                // try searching for the aliases
                // if one class alias is found that matched, return the name
                // if one namespace alias is found, replace the alias with the real name of the namespace
                foreach (var usingModel in usings)
                {
                    if (usingModel.AliasType == EAliasType.Class && className == usingModel.Alias)
                    {
                        return usingModel.Name;
                    }
                    
                    if (usingModel.AliasType != EAliasType.Namespace)
                    {
                        continue;
                    }

                    if (!className.StartsWith(usingModel.Alias))
                    {
                        continue;
                    }

                    if (projectModelToStartSearchFrom.Namespaces.FirstOrDefault(model =>
                        model.Name == usingModel.Name) != null)
                    {
                        return className.Replace(usingModel.Alias, usingModel.Name);
                    }
                }

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
                        return ConvertToSystemName(fullNamePossibilities.First());
                    case > 1:
                        throw new AmbiguousFullNameException(className, fullNamePossibilities);
                }
            }

            if (TryToGetClassNameFromProject(className, projectModelToStartSearchFrom, out var fullNameFromProject))
            {
                return ConvertToSystemName(fullNameFromProject);
            }

            // search in all projects of solutionModel
            if (TryToGetClassNameFromSolution(className, solutionModelToStartSearchFrom, out var fullNameFromSolution))
            {
                return ConvertToSystemName(fullNameFromSolution);
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
                1 => ConvertToSystemName(fullNamePossibilities.First()),
                > 1 => throw new AmbiguousFullNameException(className, fullNamePossibilities),
                _ => ConvertToSystemName(className)
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

        private static ClassModel GetClassModelFullyQualified(string classNameToSearch,
            ProjectModel projectModelToStartSearchFrom,
            SolutionModel solutionModelToStartSearchFrom, RepositoryModel repositoryModel)
        {
            if (string.IsNullOrEmpty(classNameToSearch))
            {
                return null;
            }

            IReadOnlyList<string> parts = classNameToSearch.Split(".");

            // search for fully name in provided ProjectModel 
            var classModel = GetClassModelFullyQualified(parts, projectModelToStartSearchFrom);
            if (classModel != null)
            {
                return classModel;
            }

            // search for fully name in provided SolutionModel
            classModel = solutionModelToStartSearchFrom.Projects
                .Select(project => GetClassModelFullyQualified(parts, project))
                .FirstOrDefault();
            if (classModel != null)
            {
                return classModel;
            }

            // search for fully name in all solutions
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                if (solutionModel == solutionModelToStartSearchFrom)
                {
                    continue;
                }

                classModel = solutionModel.Projects
                    .Where(projectModel => projectModel != projectModelToStartSearchFrom)
                    .Select(projectModel => GetClassModelFullyQualified(parts, projectModel))
                    .FirstOrDefault();
                if (classModel != null)
                {
                    return classModel;
                }
            }

            return null;
        }

// parts contains the class name split in parts
// a fully qualified name is generated and compared with the namespaces found in the provided ProjectModel

        private static ClassModel GetClassModelFullyQualified(IReadOnlyList<string> classNameParts,
            ProjectModel projectModel)
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
                        return classModel;
                    }
                }

                namespaceIndex++;
                namespaceName.Append('.');
            }

            return null;
        }
    }
}
