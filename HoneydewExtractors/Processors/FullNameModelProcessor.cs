using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewCore.Logging;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.Processors
{
    public class FullNameModelProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        private readonly ILogger _logger;
        private readonly IProgressLogger _progressLogger;

        public readonly IDictionary<string, NamespaceTree> NamespacesDictionary =
            new Dictionary<string, NamespaceTree>();

        private readonly IDictionary<AmbiguousName, ISet<string>> _ambiguousNames =
            new Dictionary<AmbiguousName, ISet<string>>();

        private readonly ISet<string> _notFoundClassNames = new HashSet<string>();

        private readonly RepositoryClassSet _repositoryClassSet = new();

        private int _classCount;

        public FullNameModelProcessor(ILogger logger, IProgressLogger progressLogger)
        {
            _logger = logger;
            _progressLogger = progressLogger;
        }

        public RepositoryModel Process(RepositoryModel repositoryModel)
        {
            _classCount = (from solutionModel in repositoryModel.Solutions
                from projectModel in solutionModel.Projects
                from namespaceModel in projectModel.Namespaces
                from classModel in namespaceModel.ClassModels
                select classModel).Count();

            _logger.Log("Resolving Class Names");
            SetFullNameForClassModels(repositoryModel);

            _logger.Log("Resolving Using Statements for Each Class");
            SetFullNameForUsings(repositoryModel);

            _logger.Log("Resolving Class Elements (Fields, Methods, Properties,...)");
            _logger.Log();
            SetFullNameForClassModelComponents(repositoryModel);

            foreach (var (ambiguousName, possibilities) in _ambiguousNames)
            {
                _logger.Log();
                _logger.Log($"Multiple full names found for {ambiguousName.Name} in {ambiguousName.Location} :",
                    LogLevels.Warning);
                _progressLogger.Log();
                _progressLogger.Log(
                    $"Multiple full names found for {ambiguousName.Name} in {ambiguousName.Location} :");
                foreach (var possibleName in possibilities)
                {
                    _logger.Log(possibleName);
                    _progressLogger.Log(possibleName);
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
            var progressBar = _progressLogger.CreateProgressLogger(_classCount, "Resolving Class Names");
            progressBar.Start();

            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            progressBar.Step($"{classModel.Name} from {classModel.FilePath}");

                            AddAmbiguousNames(() =>
                            {
                                classModel.Name = FindClassFullName(classModel.Name, namespaceModel,
                                    projectModel, solutionModel, repositoryModel, classModel.Imports,
                                    classModel.FilePath);

                                _repositoryClassSet.Add(projectModel.FilePath, classModel.Name);
                            });
                        }
                    }
                }
            }

            progressBar.Stop();
        }

        private void SetFullNameForUsings(RepositoryModel repositoryModel)
        {
            var progressBar =
                _progressLogger.CreateProgressLogger(_classCount, "Resolving Using Statements for Each Class");
            progressBar.Start();

            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            progressBar.Step($"{classModel.Name} from {classModel.FilePath}");

                            foreach (var usingModel in classModel.Imports)
                            {
                                if (usingModel.AliasType == nameof(EAliasType.NotDetermined))
                                {
                                    usingModel.AliasType = DetermineUsingType(usingModel, classModel);
                                }

                                AddAmbiguousNames(() =>
                                {
                                    // var beforeName = usingModel.Name;
                                    usingModel.Name = FindNamespaceFullName(usingModel.Name, namespaceModel,
                                        classModel.Imports);
                                    // if (usingModel.Name == beforeName)
                                    // {
                                    //     usingModel.Name = FindClassFullName(usingModel.Name, namespaceModel,
                                    //         projectModel, solutionModel, repositoryModel, classModel.Usings, classModel.FilePath);
                                    // }
                                });
                            }
                        }
                    }
                }
            }

            progressBar.Stop();
        }

        private static string DetermineUsingType(IImportType usingModel, IPropertyMembersClassType classModel)
        {
            var namespaceAccess = $"{usingModel.Alias}.";

            if (classModel.Fields.Any(fieldModel => fieldModel.Type.Name.StartsWith(namespaceAccess)))
            {
                return nameof(EAliasType.Namespace);
            }

            foreach (var propertyModel in classModel.Properties)
            {
                if (propertyModel.Type.Name.StartsWith(namespaceAccess))
                {
                    return nameof(EAliasType.Namespace);
                }

                if (IsAliasNamespaceSearchInCalledMethods(propertyModel.CalledMethods))
                {
                    return nameof(EAliasType.Namespace);
                }
            }

            if (IsAliasNamespaceSearchInMethodModels(classModel.Methods))
            {
                return nameof(EAliasType.Namespace);
            }

            if (IsAliasNamespaceSearchInConstructorModels(classModel.Constructors))
            {
                return nameof(EAliasType.Namespace);
            }

            return nameof(EAliasType.Class);

            bool IsAliasNamespaceSearchInMethodModels(IEnumerable<IMethodType> methodTypes)
            {
                foreach (var methodModel in methodTypes)
                {
                    if (!string.IsNullOrEmpty(methodModel.ReturnValue.Type.Name) &&
                        methodModel.ReturnValue.Type.Name.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (methodModel.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.Name.StartsWith(namespaceAccess)))
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

            bool IsAliasNamespaceSearchInConstructorModels(IEnumerable<IConstructorType> constructorTypes)
            {
                foreach (var methodModel in constructorTypes)
                {
                    if (methodModel.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.Name.StartsWith(namespaceAccess)))
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

            bool IsAliasNamespaceSearchInCalledMethods(IEnumerable<IMethodSignatureType> calledMethods)
            {
                foreach (var calledMethod in calledMethods)
                {
                    if (!string.IsNullOrEmpty(calledMethod.ContainingTypeName) &&
                        calledMethod.ContainingTypeName.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (calledMethod.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.Name.StartsWith(namespaceAccess)))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void SetFullNameForClassModelComponents(RepositoryModel repositoryModel)
        {
            var progressBar =
                _progressLogger.CreateProgressLogger(_classCount,
                    "Resolving Class Elements (Fields, Methods, Properties,...)");
            progressBar.Start();

            foreach (var solutionModel in repositoryModel.Solutions)
            {
                foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        foreach (var classModel in namespaceModel.ClassModels)
                        {
                            _logger.Log($"Resolving Elements for {classModel.Name} from {classModel.FilePath}");
                            progressBar.Step($"{classModel.Name} from {classModel.FilePath}");

                            for (var i = 0; i < classModel.BaseTypes.Count; i++)
                            {
                                var iCopy = i;
                                AddAmbiguousNames(() =>
                                {
                                    classModel.BaseTypes[iCopy].Type.Name = FindClassFullName(
                                        classModel.BaseTypes[iCopy].Type.Name,
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Imports, classModel.FilePath);
                                });
                            }

                            foreach (var fieldModel in classModel.Fields)
                            {
                                AddAmbiguousNames(() =>
                                {
                                    fieldModel.Type.Name = FindClassFullName(fieldModel.Type.Name,
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Imports, classModel.FilePath);
                                });
                            }

                            foreach (var propertyModel in classModel.Properties)
                            {
                                AddAmbiguousNames(() =>
                                {
                                    propertyModel.Type.Name = FindClassFullName(propertyModel.Type.Name,
                                        namespaceModel, projectModel, solutionModel, repositoryModel,
                                        classModel.Imports, classModel.FilePath);
                                });

                                SetContainingClassAndCalledMethodsFullNameProperty(classModel.FilePath, propertyModel,
                                    namespaceModel,
                                    projectModel, solutionModel, classModel.Imports);
                            }

                            foreach (var methodModel in classModel.Methods)
                            {
                                if (methodModel.ReturnValue != null)
                                {
                                    AddAmbiguousNames(() =>
                                    {
                                        methodModel.ReturnValue.Type.Name = FindClassFullName(methodModel.ReturnValue.Type.Name,
                                            namespaceModel, projectModel, solutionModel, repositoryModel,
                                            classModel.Imports, classModel.FilePath);
                                    });
                                }

                                SetContainingClassAndCalledMethodsFullName(classModel.FilePath, methodModel,
                                    namespaceModel,
                                    projectModel, solutionModel, classModel.Imports);
                            }

                            foreach (var constructorType in classModel.Constructors)
                            {
                                SetContainingClassAndCalledMethodsFullName(classModel.FilePath, constructorType,
                                    namespaceModel,
                                    projectModel, solutionModel, classModel.Imports);
                            }

                            ChangeDependencyMetricFullName(repositoryModel, classModel, namespaceModel,
                                projectModel, solutionModel);
                        }
                    }

                    _logger.Log();
                }

                progressBar.Stop();
            }

            void SetContainingClassAndCalledMethodsFullNameProperty(string classFilePath,
                ICallingMethodsType propertyModel,
                NamespaceModel namespaceModel,
                ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> usings)
            {
                AddAmbiguousNames(() =>
                {
                    propertyModel.ContainingTypeName = FindClassFullName(propertyModel.ContainingTypeName,
                        namespaceModel, projectModel, solutionModel, repositoryModel, usings, classFilePath);
                });

                SetFullNameForCalledMethods(classFilePath, propertyModel.CalledMethods, namespaceModel,
                    projectModel, solutionModel, repositoryModel, usings);
            }

            void SetContainingClassAndCalledMethodsFullName(string classFilePath, IMethodSkeletonType methodModel,
                NamespaceModel namespaceModel,
                ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> usings)
            {
                AddAmbiguousNames(() =>
                {
                    methodModel.ContainingTypeName = FindClassFullName(methodModel.ContainingTypeName,
                        namespaceModel, projectModel, solutionModel, repositoryModel, usings, classFilePath);
                });

                foreach (var parameterModel in methodModel.ParameterTypes)
                {
                    AddAmbiguousNames(() =>
                    {
                        parameterModel.Type.Name = FindClassFullName(parameterModel.Type.Name, namespaceModel,
                            projectModel, solutionModel, repositoryModel, usings, classFilePath);
                    });
                }

                SetFullNameForCalledMethods(classFilePath, methodModel.CalledMethods, namespaceModel,
                    projectModel, solutionModel, repositoryModel, usings);
            }
        }

        private void SetFullNameForCalledMethods(string classFilePath,
            IEnumerable<IMethodSignatureType> methodModelCalledMethods,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel,
            RepositoryModel repositoryModel, IList<IImportType> usings)
        {
            foreach (var calledMethod in methodModelCalledMethods)
            {
                foreach (var parameterModel in calledMethod.ParameterTypes)
                {
                    AddAmbiguousNames(() =>
                    {
                        parameterModel.Type.Name = FindClassFullName(parameterModel.Type.Name, namespaceModel,
                            projectModel, solutionModel, repositoryModel, usings, classFilePath);
                    });
                }

                AddAmbiguousNames(() =>
                {
                    calledMethod.ContainingTypeName = FindClassFullName(
                        calledMethod.ContainingTypeName, namespaceModel, projectModel, solutionModel,
                        repositoryModel, usings, classFilePath);

                    var classModel = GetClassModelFullyQualified(calledMethod.ContainingTypeName, projectModel,
                        solutionModel,
                        repositoryModel);

                    // search for static import of a class
                    if (classModel == null)
                    {
                        calledMethod.ContainingTypeName = SearchForMethodNameInStaticImports(
                            calledMethod.ContainingTypeName, calledMethod.Name, calledMethod.ParameterTypes,
                            projectModel,
                            solutionModel,
                            repositoryModel, usings);
                    }
                });
            }
        }

        private string SearchForMethodNameInStaticImports(string containingClassName, string methodName,
            IList<IParameterType> methodParameters,
            ProjectModel projectModel, SolutionModel solutionModel,
            RepositoryModel repositoryModel, IList<IImportType> usings)
        {
            if (!string.IsNullOrEmpty(containingClassName) && containingClassName.StartsWith("System."))
            {
                return containingClassName;
            }

            if (usings == null)
            {
                return "";
            }

            foreach (var importType in usings)
            {
                if (importType is UsingModel { IsStatic: false })
                {
                    continue;
                }

                var staticImportedClass =
                    GetClassModelFullyQualified(importType.Name, projectModel, solutionModel, repositoryModel);

                if (staticImportedClass == null)
                {
                    continue;
                }

                foreach (var fieldModel in staticImportedClass.Fields)
                {
                    if (fieldModel.Name == containingClassName)
                    {
                        return CSharpConstants.ConvertPrimitiveTypeToSystemType(fieldModel.Type.Name);
                    }
                }

                foreach (var propertyModel in staticImportedClass.Properties)
                {
                    if (propertyModel.Name == containingClassName)
                    {
                        return CSharpConstants.ConvertPrimitiveTypeToSystemType(propertyModel.Type.Name);
                    }
                }

                foreach (var methodModel in staticImportedClass.Methods)
                {
                    var hasTheSameSignature = MethodsHaveTheSameSignature(methodModel.Name,
                        methodModel.ParameterTypes,
                        methodName, methodParameters);
                    if (hasTheSameSignature)
                    {
                        return CSharpConstants.ConvertPrimitiveTypeToSystemType(staticImportedClass.Name);
                    }
                }
            }

            return containingClassName;
        }

        private static bool MethodsHaveTheSameSignature(string firstMethodName,
            IList<IParameterType> firstMethodParameters,
            string secondMethodName, IList<IParameterType> secondMethodParameters)
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
                if (CSharpConstants.ConvertPrimitiveTypeToSystemType(firstMethodParameters[i].Type.Name) !=
                    CSharpConstants.ConvertPrimitiveTypeToSystemType(secondMethodParameters[i].Type.Name))
                {
                    return false;
                }

                if (firstMethodParameters[i] is ParameterModel firstMethodParameter &&
                    secondMethodParameters[i] is ParameterModel secondMethodParameter)
                {
                    if (string.IsNullOrEmpty(firstMethodParameter.Modifier) &&
                        !string.IsNullOrEmpty(secondMethodParameter.Modifier))
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(firstMethodParameter.Modifier) &&
                        string.IsNullOrEmpty(secondMethodParameter.Modifier))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ChangeDependencyMetricFullName(RepositoryModel repositoryModel, ClassModel classModel,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel)
        {
            var parameterDependenciesMetrics = classModel.Metrics.Where(metric =>
                typeof(IRelationMetric).IsAssignableFrom(Type.GetType(metric.ExtractorName)));

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
                            solutionModel, repositoryModel, classModel.Imports, classModel.FilePath);

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
            IList<IImportType> usingModels)
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
                    if (usingModel.AliasType == nameof(EAliasType.Class) && namespaceName == usingModel.Alias)
                    {
                        return usingModel.Name;
                    }

                    var tempName = namespaceName;
                    if (usingModel.AliasType == nameof(EAliasType.Namespace) &&
                        namespaceName.StartsWith(usingModel.Alias))
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
            RepositoryModel repositoryModel, IList<IImportType> usingModels, string classFilePath = "")
        {
            if (string.IsNullOrEmpty(className))
            {
                return className;
            }

            if (_notFoundClassNames.Contains(className))
            {
                return className;
            }

            if (CSharpConstants.IsPrimitive(className))
            {
                return CSharpConstants.ConvertPrimitiveTypeToSystemType(className);
            }

            if (_repositoryClassSet.Contains(projectModelToStartSearchFrom.FilePath, className))
            {
                return className;
            }

            if (IsNameFullyQualified(className))
            {
                return className;
            }

            // try to find class in provided namespace
            if (namespaceModelToStartSearchFrom.ClassModels.Any(classModel => classModel.Name == className))
            {
                return AddClassModelToNamespaceGraph(className, classFilePath, namespaceModelToStartSearchFrom.Name);
            }

            if (namespaceModelToStartSearchFrom.ClassModels.Any(classModel =>
                classModel.Name == $"{namespaceModelToStartSearchFrom.Name}.{className}"))
            {
                return AddClassModelToNamespaceGraph(className, classFilePath, namespaceModelToStartSearchFrom.Name);
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
                    if (usingModel.AliasType == nameof(EAliasType.Class) && className == usingModel.Alias)
                    {
                        return usingModel.Name;
                    }

                    var tempName = className;
                    if (usingModel.AliasType == nameof(EAliasType.Namespace) && className.StartsWith(usingModel.Alias))
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
                            throw new AmbiguousFullNameException(new AmbiguousName
                            {
                                Name = className,
                                Location = classFilePath
                            }, fullNamePossibilities);
                    }
                }
            }

            // search in current namespace for classes that used a class located in the parent namespace 
            foreach (var name in GetPossibilitiesFromNamespace(namespaceModelToStartSearchFrom.Name, className))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First();
                case > 1:
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = className,
                        Location = classFilePath
                    }, fullNamePossibilities);
            }

            foreach (var name in TrySearchingInProjectModel(className, classFilePath, projectModelToStartSearchFrom))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First();
                case > 1:
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = className,
                        Location = classFilePath
                    }, fullNamePossibilities);
            }

            foreach (var name in TrySearchingInSolutionModel(className, classFilePath, solutionModelToStartSearchFrom,
                projectModelToStartSearchFrom))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First();
                case > 1:
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = className,
                        Location = classFilePath
                    }, fullNamePossibilities);
            }

            // try searching in all solutions
            foreach (var solutionModel in repositoryModel.Solutions)
            {
                if (solutionModel == solutionModelToStartSearchFrom)
                {
                    continue;
                }

                foreach (var name in TrySearchingInSolutionModel(className, classFilePath, solutionModel,
                    projectModelToStartSearchFrom))
                {
                    fullNamePossibilities.Add(name);
                }

                if (fullNamePossibilities.Count > 1)
                {
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = className,
                        Location = classFilePath
                    }, fullNamePossibilities);
                }
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return CSharpConstants.ConvertPrimitiveTypeToSystemType(fullNamePossibilities.First());
                case > 1:
                    throw new AmbiguousFullNameException(
                        new AmbiguousName { Name = className, Location = classFilePath }, fullNamePossibilities);
                default:
                {
                    _notFoundClassNames.Add(className);
                    return className;
                }
            }
        }

        private IEnumerable<string> TrySearchingInSolutionModel(string className, string classFilePath,
            SolutionModel solutionModel, ProjectModel projectModelToBeIgnored)
        {
            var fullNamePossibilities = new HashSet<string>();
            foreach (var projectModel in solutionModel.Projects)
            {
                if (projectModel == projectModelToBeIgnored)
                {
                    continue;
                }

                if (!projectModelToBeIgnored.ProjectReferences.Contains(projectModel.FilePath))
                {
                    continue;
                }


                foreach (var name in TrySearchingInProjectModel(className, classFilePath, projectModel))
                {
                    fullNamePossibilities.Add(name);
                }

                if (fullNamePossibilities.Count > 1)
                {
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = className,
                        Location = classFilePath
                    }, fullNamePossibilities);
                }
            }

            return fullNamePossibilities;
        }

        private IEnumerable<string> TrySearchingInProjectModel(string className, string classFilePath,
            ProjectModel projectModel)
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
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = className,
                        Location = classFilePath
                    }, fullNamePossibilities);
                }
            }

            return fullNamePossibilities;
        }

        private IEnumerable<string> GetPossibilitiesFromNamespace(string namespaceName, string className)
        {
            var nameParts = namespaceName.Split('.');
            if (!NamespacesDictionary.TryGetValue(nameParts[0], out var fullNameNamespace))
            {
                return new List<string>();
            }

            var childNamespace = fullNameNamespace.GetChild(nameParts);
            if (childNamespace == null)
            {
                return new List<string>();
            }

            var childClassNamespace = GetClassInNamespaceAmongChildrenNamespaces(childNamespace, className);
            if (childClassNamespace != null)
            {
                return new List<string>
                {
                    childClassNamespace.GetFullName()
                };
            }

            if (childNamespace.Parent != null)
            {
                var brotherClassNamespace =
                    GetClassInNamespaceAmongChildrenNamespaces(childNamespace.Parent, className);
                if (brotherClassNamespace != null)
                {
                    return new List<string>
                    {
                        brotherClassNamespace.GetFullName()
                    };
                }
            }

            return childNamespace.GetPossibleChildren(className);
        }

        private NamespaceTree GetClassInNamespaceAmongChildrenNamespaces(NamespaceTree namespaceTree, string className)
        {
            foreach (var (childName, childNode) in namespaceTree.Children)
            {
                if (className == childName && childNode.Children.Count == 0)
                {
                    return childNode;
                }
            }

            return null;
        }

        private string AddClassModelToNamespaceGraph(string className, string classFilePath, string namespaceName)
        {
            var namespaceNameParts = namespaceName.Split('.');

            NamespaceTree rootNamespaceTree;

            if (NamespacesDictionary.TryGetValue(namespaceNameParts[0], out var fullNameNamespace))
            {
                rootNamespaceTree = fullNameNamespace;
            }
            else
            {
                var nameNamespace = new NamespaceTree
                {
                    Name = namespaceNameParts[0]
                };
                rootNamespaceTree = nameNamespace;

                NamespacesDictionary.Add(namespaceNameParts[0], rootNamespaceTree);
            }

            if (namespaceNameParts.Length > 1) // create sub graph if namespaceNames is not trivial
            {
                rootNamespaceTree.AddNamespaceChild(namespaceName, namespaceName);
            }

            var classModelFullName = rootNamespaceTree.AddNamespaceChild(className, namespaceName);

            if (classModelFullName == null)
            {
                return className;
            }

            classModelFullName.FilePath = classFilePath;

            return classModelFullName.GetFullName();
        }

        private bool IsNameFullyQualified(string name)
        {
            var nameParts = name.Split(".");
            if (nameParts.Length > 0)
            {
                if (NamespacesDictionary.TryGetValue(nameParts[0], out var fullNameNamespace))
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

                            return model.Name == fullyQualifiedName;
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

        private class AmbiguousFullNameException : Exception
        {
            public AmbiguousName AmbiguousName { get; }
            public ISet<string> PossibleNames { get; }

            public AmbiguousFullNameException(AmbiguousName ambiguousName, ISet<string> possibleNames)
            {
                AmbiguousName = ambiguousName;
                PossibleNames = possibleNames;
            }
        }

        private record AmbiguousName
        {
            public string Name { get; set; }
            public string Location { get; set; }
        }
    }
}
