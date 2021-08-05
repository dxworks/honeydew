﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Processors
{
    public class FullNameModelProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        private class AmbiguousFullNameException : Exception
        {
            public string AmbiguousName { get; }
            public ISet<string> PossibleNames { get; }

            public AmbiguousFullNameException(string ambiguousName, ISet<string> possibleNames)
            {
                AmbiguousName = ambiguousName;
                PossibleNames = possibleNames;
            }
        }

        private readonly IProgressLogger _progressLogger;

        private readonly IDictionary<string, ISet<string>> _ambiguousNames = new Dictionary<string, ISet<string>>();

        private readonly IDictionary<string, FullNameNamespace> _namespacesDictionary =
            new Dictionary<string, FullNameNamespace>();

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
                if (_ambiguousNames.TryGetValue(e.AmbiguousName, out var possibleNamesSet))
                {
                    foreach (var possibleName in e.PossibleNames)
                    {
                        possibleNamesSet.Add(possibleName);
                    }
                }
                else
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

                                AddAmbiguousNames(() =>
                                {
                                    // var beforeName = usingModel.Name;
                                    usingModel.Name = FindNamespaceFullName(usingModel.Name, namespaceModel,
                                        classModel.Usings);
                                    // if (usingModel.Name == beforeName)
                                    // {
                                    //     usingModel.Name = FindClassFullName(usingModel.Name, namespaceModel,
                                    //         projectModel, solutionModel, repositoryModel, classModel.Usings);
                                    // }
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
                    if (!string.IsNullOrEmpty(methodModel.ReturnType) &&
                        methodModel.ReturnType.StartsWith(namespaceAccess))
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
                    if (!string.IsNullOrEmpty(calledMethod.ContainingClassName) &&
                        calledMethod.ContainingClassName.StartsWith(namespaceAccess))
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
                        return CSharpConstants.ConvertPrimitiveTypeToSystemType(fieldModel.Type);
                    }
                }

                foreach (var propertyModel in staticImportedClass.Properties)
                {
                    if (propertyModel.Name == containingClassName)
                    {
                        return CSharpConstants.ConvertPrimitiveTypeToSystemType(propertyModel.Type);
                    }
                }

                foreach (var methodModel in staticImportedClass.Methods)
                {
                    var hasTheSameSignature = MethodsHaveTheSameSignature(methodModel.Name, methodModel.ParameterTypes,
                        methodName, methodParameters);
                    if (hasTheSameSignature)
                    {
                        return CSharpConstants.ConvertPrimitiveTypeToSystemType(staticImportedClass.FullName);
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
                if (CSharpConstants.ConvertPrimitiveTypeToSystemType(firstMethodParameters[i].Type) !=
                    CSharpConstants.ConvertPrimitiveTypeToSystemType(secondMethodParameters[i].Type))
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

        private string FindNamespaceFullName(string namespaceName, NamespaceModel namespaceModel,
            IList<UsingModel> usingModels)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return namespaceName;
            }

            if (IsNameFullyQualified(namespaceName))
            {
                return namespaceName;
            }

            var combinedName = $"{namespaceModel.Name}.{namespaceName}";

            if (IsNameFullyQualified(combinedName))
            {
                return combinedName;
            }

            if (usingModels != null)
            {
                // try searching for the aliases
                // if one class alias is found that matched, return the name
                // if one namespace alias is found, replace the alias with the real name of the namespace
                foreach (var usingModel in usingModels)
                {
                    if (usingModel.AliasType == EAliasType.Class && namespaceName == usingModel.Alias)
                    {
                        return usingModel.Name;
                    }

                    var tempName = namespaceName;
                    if (usingModel.AliasType == EAliasType.Namespace && namespaceName.StartsWith(usingModel.Alias))
                    {
                        tempName = namespaceName.Replace(usingModel.Alias, usingModel.Name);
                    }

                    var fullNamePossibilities = GetPossibilitiesFromNamespace(usingModel.Name, tempName);
                    var firstOrDefault = fullNamePossibilities.FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        return firstOrDefault;
                    }
                }
            }

            return namespaceName;
        }

        private string FindClassFullName(string className, NamespaceModel namespaceModelToStartSearchFrom,
            ProjectModel projectModelToStartSearchFrom, SolutionModel solutionModelToStartSearchFrom,
            RepositoryModel repositoryModel, IList<UsingModel> usingModels)
        {
            if (string.IsNullOrEmpty(className))
            {
                return className;
            }

            if (CSharpConstants.IsPrimitive(className))
            {
                return CSharpConstants.ConvertPrimitiveTypeToSystemType(className);
            }

            if (IsNameFullyQualified(className))
            {
                return className;
            }

            // try to find class in provided namespace
            if (namespaceModelToStartSearchFrom.ClassModels.Any(classModel => classModel.FullName == className))
            {
                return AddClassModelToNamespaceGraph(className, namespaceModelToStartSearchFrom.Name);
            }

            if (namespaceModelToStartSearchFrom.ClassModels.Any(classModel =>
                classModel.FullName == $"{namespaceModelToStartSearchFrom.Name}.{className}"))
            {
                return AddClassModelToNamespaceGraph(className, namespaceModelToStartSearchFrom.Name);
            }

            // search in all provided usings
            var fullNamePossibilities = new HashSet<string>();
            if (usingModels != null)
            {
                // try searching for the aliases
                // if one class alias is found that matched, return the name
                // if one namespace alias is found, replace the alias with the real name of the namespace
                foreach (var usingModel in usingModels)
                {
                    if (usingModel.AliasType == EAliasType.Class && className == usingModel.Alias)
                    {
                        return usingModel.Name;
                    }

                    var tempName = className;
                    if (usingModel.AliasType == EAliasType.Namespace && className.StartsWith(usingModel.Alias))
                    {
                        tempName = className.Replace(usingModel.Alias, usingModel.Name);
                    }

                    foreach (var name in GetPossibilitiesFromNamespace(usingModel.Name, tempName))
                    {
                        fullNamePossibilities.Add(name);
                    }

                    switch (fullNamePossibilities.Count)
                    {
                        case 1:
                            return fullNamePossibilities.First();
                        case > 1:
                            throw new AmbiguousFullNameException(className, fullNamePossibilities);
                    }
                }
            }

            foreach (var name in TrySearchingInProjectModel(className, projectModelToStartSearchFrom))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First();
                case > 1:
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
            }

            foreach (var name in TrySearchingInSolutionModel(className, solutionModelToStartSearchFrom,
                projectModelToStartSearchFrom))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First();
                case > 1:
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
            }

            // try searching in all solutions
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                if (solutionModel == solutionModelToStartSearchFrom)
                {
                    continue;
                }

                foreach (var name in TrySearchingInSolutionModel(className, solutionModel,
                    projectModelToStartSearchFrom))
                {
                    fullNamePossibilities.Add(name);
                }

                if (fullNamePossibilities.Count > 1)
                {
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
                }
            }

            return fullNamePossibilities.Count switch
            {
                1 => CSharpConstants.ConvertPrimitiveTypeToSystemType(fullNamePossibilities.First()),
                > 1 => throw new AmbiguousFullNameException(className, fullNamePossibilities),

                // unknown className, must be some extern dependency
                _ => CSharpConstants.ConvertPrimitiveTypeToSystemType(className)
            };
        }

        private IEnumerable<string> TrySearchingInSolutionModel(string className,
            SolutionModel solutionModel, ProjectModel projectModelToBeIgnored)
        {
            var fullNamePossibilities = new HashSet<string>();
            foreach (var projectModel in solutionModel.Projects)
            {
                if (projectModel == projectModelToBeIgnored)
                {
                    continue;
                }

                foreach (var name in TrySearchingInProjectModel(className, projectModel))
                {
                    fullNamePossibilities.Add(name);
                }

                if (fullNamePossibilities.Count > 1)
                {
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
                }
            }

            return fullNamePossibilities;
        }

        private IEnumerable<string> TrySearchingInProjectModel(string className, ProjectModel projectModel)
        {
            var fullNamePossibilities = new HashSet<string>();
            foreach (var namespaceModel in projectModel.Namespaces)
            {
                foreach (var name in GetPossibilitiesFromNamespace(namespaceModel.Name, className))
                {
                    fullNamePossibilities.Add(name);
                }

                if (fullNamePossibilities.Count > 1)
                {
                    throw new AmbiguousFullNameException(className, fullNamePossibilities);
                }
            }

            return fullNamePossibilities;
        }

        private IEnumerable<string> GetPossibilitiesFromNamespace(string namespaceName, string className)
        {
            var nameParts = namespaceName.Split('.');
            if (!_namespacesDictionary.TryGetValue(nameParts[0], out var fullNameNamespace))
            {
                return new List<string>();
            }


            var childNamespace = fullNameNamespace.GetChild(nameParts);
            if (childNamespace != null)
            {
                return childNamespace.GetPossibleChildren(className);
            }

            return new List<string>();
        }

        private string AddClassModelToNamespaceGraph(string className, string namespaceName)
        {
            var namespaceNameParts = namespaceName.Split('.');

            FullNameNamespace rootNamespace;

            if (_namespacesDictionary.TryGetValue(namespaceNameParts[0], out var fullNameNamespace))
            {
                rootNamespace = fullNameNamespace;
            }
            else
            {
                var nameNamespace = new FullNameNamespace
                {
                    Name = namespaceNameParts[0]
                };
                rootNamespace = nameNamespace;

                _namespacesDictionary.Add(namespaceNameParts[0], rootNamespace);
            }

            if (namespaceNameParts.Length > 1) // create sub graph if namespaceNames is not trivial
            {
                rootNamespace.AddNamespaceChild(namespaceName, namespaceName);
            }

            // if (IsClassNameFullyQualified(className))
            // {
            //     rootNamespace.AddNamespaceChild(className);
            //     return className;
            // }

            var classModelFullName = rootNamespace.AddNamespaceChild(className, namespaceName);
            return classModelFullName.GetFullName();
        }

        private bool IsNameFullyQualified(string name)
        {
            var nameParts = name.Split(".");
            if (nameParts.Length > 0)
            {
                if (_namespacesDictionary.TryGetValue(nameParts[0], out var fullNameNamespace))
                {
                    if (fullNameNamespace.ContainsNameParts(nameParts))
                    {
                        return true;
                    }
                }
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
