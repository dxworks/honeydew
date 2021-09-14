using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly bool _disableLocalVariablesBinding;

        public readonly ConcurrentDictionary<string, NamespaceTree> NamespacesDictionary = new();

        private readonly ConcurrentDictionary<AmbiguousName, ISet<string>> _ambiguousNames = new();

        private readonly ConcurrentDictionary<string, string> _notFoundClassNames = new();

        private readonly RepositoryClassSet _repositoryClassSet = new();

        private int _classCount;

        public FullNameModelProcessor(ILogger logger, IProgressLogger progressLogger, bool disableLocalVariablesBinding)
        {
            _logger = logger;
            _progressLogger = progressLogger;
            _disableLocalVariablesBinding = disableLocalVariablesBinding;
        }

        public RepositoryModel Process(RepositoryModel repositoryModel)
        {
            _classCount = (from solutionModel in repositoryModel.Solutions
                from projectModel in solutionModel.Projects
                from namespaceModel in projectModel.Namespaces
                from classModel in namespaceModel.ClassModels
                select classModel).Count();

            _logger.Log("Resolving Class Names");
            _progressLogger.Log();
            _progressLogger.Log("Resolving Class Names");
            SetFullNameForClassModels(repositoryModel);

            _logger.Log("Resolving Using Statements for Each Class");
            _progressLogger.Log();
            _progressLogger.Log("Resolving Using Statements for Each Class");
            SetFullNameForUsings(repositoryModel);

            _logger.Log("Resolving Class Elements (Fields, Methods, Properties,...)");
            _logger.Log();
            _progressLogger.Log();
            _progressLogger.Log("Resolving Class Elements (Fields, Methods, Properties,...)");
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
                    _ambiguousNames.TryAdd(e.AmbiguousName, e.PossibleNames);
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
                        foreach (var classType in namespaceModel.ClassModels)
                        {
                            progressBar.Step($"{classType.Name} from {classType.FilePath}");

                            foreach (var usingModel in classType.Imports)
                            {
                                if (usingModel.AliasType == nameof(EAliasType.NotDetermined))
                                {
                                    if (classType is ClassModel classModel)
                                    {
                                        usingModel.AliasType = DetermineUsingType(usingModel, classModel);
                                    }
                                }

                                AddAmbiguousNames(() =>
                                {
                                    // var beforeName = usingModel.Name;
                                    usingModel.Name = FindNamespaceFullName(usingModel.Name, namespaceModel,
                                        classType.Imports);
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

                foreach (var accessor in propertyModel.Accessors)
                {
                    if (IsAliasNamespaceSearchInCalledMethods(accessor.CalledMethods))
                    {
                        return nameof(EAliasType.Namespace);
                    }
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
                    if (methodModel.ReturnValue.Type != null &&
                        !string.IsNullOrEmpty(methodModel.ReturnValue.Type.Name) &&
                        methodModel.ReturnValue.Type.Name.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (methodModel.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type != null && parameterModel.Type.Name.StartsWith(namespaceAccess)))
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

            var currentClassCount = 1;

            foreach (var solutionModel in repositoryModel.Solutions)
            {
                Parallel.ForEach(solutionModel.Projects, projectModel =>
                    // foreach (var projectModel in solutionModel.Projects)
                {
                    foreach (var namespaceModel in projectModel.Namespaces)
                    {
                        Parallel.ForEach(namespaceModel.ClassModels, classType =>
                            // foreach (var classType in namespaceModel.ClassModels)
                        {
                            _logger.Log(
                                $"Resolving Elements for {classType.Name} from {classType.FilePath} ({currentClassCount}/{_classCount})");
                            currentClassCount++;
                            progressBar.Step($"{classType.Name} from {classType.FilePath}");

                            for (var i = 0; i < classType.BaseTypes.Count; i++)
                            {
                                var iCopy = i;
                                AddAmbiguousNames(() =>
                                {
                                    SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel,
                                        solutionModel, classType.Imports, classType.FilePath,
                                        classType.BaseTypes[iCopy].Type);
                                });
                            }

                            SetEntityAttributesFullName(repositoryModel, classType, namespaceModel, projectModel,
                                solutionModel, classType.Imports, classType.FilePath);

                            SetGenericParametersFullName(repositoryModel, classType, namespaceModel, projectModel,
                                solutionModel, classType.Imports, classType.FilePath);

                            if (classType is IPropertyMembersClassType classModel)
                            {
                                foreach (var fieldModel in classModel.Fields)
                                {
                                    AddAmbiguousNames(() =>
                                    {
                                        SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel,
                                            solutionModel, classModel.Imports, classModel.FilePath,
                                            fieldModel.Type);
                                    });

                                    SetEntityAttributesFullName(repositoryModel, fieldModel, namespaceModel,
                                        projectModel,
                                        solutionModel, classType.Imports, classType.FilePath);
                                }

                                foreach (var propertyModel in classModel.Properties)
                                {
                                    AddAmbiguousNames(() =>
                                    {
                                        SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel,
                                            solutionModel, classModel.Imports, classModel.FilePath,
                                            propertyModel.Type);
                                    });

                                    SetEntityAttributesFullName(repositoryModel, propertyModel, namespaceModel,
                                        projectModel,
                                        solutionModel, classType.Imports, classType.FilePath);

                                    foreach (var accessor in propertyModel.Accessors)
                                    {
                                        SetMethodSkeletonFullName(classModel.FilePath, accessor, namespaceModel,
                                            projectModel, solutionModel, repositoryModel, classModel.Imports);
                                    }
                                }

                                foreach (var methodModel in classModel.Methods)
                                {
                                    if (methodModel.ReturnValue != null)
                                    {
                                        AddAmbiguousNames(() =>
                                        {
                                            SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel,
                                                solutionModel, classModel.Imports, classModel.FilePath,
                                                methodModel.ReturnValue.Type);
                                        });

                                        SetEntityAttributesFullName(repositoryModel, methodModel.ReturnValue,
                                            namespaceModel, projectModel,
                                            solutionModel, classType.Imports, classType.FilePath);
                                    }

                                    SetMethodSkeletonFullName(classModel.FilePath, methodModel, namespaceModel,
                                        projectModel, solutionModel, repositoryModel, classModel.Imports);
                                }

                                foreach (var constructorType in classModel.Constructors)
                                {
                                    SetMethodSkeletonFullName(classModel.FilePath, constructorType, namespaceModel,
                                        projectModel, solutionModel, repositoryModel, classModel.Imports);
                                }

                                ChangeDependencyMetricFullName(repositoryModel, classModel, namespaceModel,
                                    projectModel, solutionModel);
                            }
                        });
                        // }
                    }

                    _logger.Log();
                });
                // }

                progressBar.Stop();
            }
        }

        private void SetMethodSkeletonFullName(string classFilePath, IMethodSkeletonType methodSkeletonType,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel,
            RepositoryModel repositoryModel, IList<IImportType> importTypes)
        {
            AddAmbiguousNames(() =>
            {
                methodSkeletonType.ContainingTypeName = FindClassFullName(methodSkeletonType.ContainingTypeName,
                    namespaceModel, projectModel, solutionModel, repositoryModel, importTypes, classFilePath);
            });

            foreach (var parameterModel in methodSkeletonType.ParameterTypes)
            {
                SetParameterFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                    classFilePath, parameterModel);
            }

            SetEntityAttributesFullName(repositoryModel, methodSkeletonType, namespaceModel, projectModel,
                solutionModel, importTypes, classFilePath);

            SetFullNameForCalledMethods(classFilePath, methodSkeletonType.CalledMethods, namespaceModel,
                projectModel, solutionModel, repositoryModel, importTypes);

            if (!_disableLocalVariablesBinding)
            {
                SetLocalVariablesFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                    classFilePath, methodSkeletonType);
            }
        }

        private void SetGenericParametersFullName(RepositoryModel repositoryModel, IClassType classType,
            NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath)
        {
            foreach (var genericParameter in classType.GenericParameters)
            {
                SetEntityAttributesFullName(repositoryModel, genericParameter, namespaceModel, projectModel,
                    solutionModel, importTypes, filePath);

                foreach (var constraint in genericParameter.Constraints)
                {
                    SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                        filePath, constraint);
                }
            }
        }


        private void SetEntityAttributesFullName(RepositoryModel repositoryModel, ITypeWithAttributes classType,
            NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath)
        {
            foreach (var attributeType in classType.Attributes)
            {
                SetAttributeFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                    filePath, attributeType);
            }
        }

        private void SetAttributeFullName(RepositoryModel repositoryModel, NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath,
            IMethodSignatureType attributeType)
        {
            AddAmbiguousNames(() =>
            {
                var suffix = "";
                if (!attributeType.Name.EndsWith("Attribute"))
                {
                    suffix = "Attribute";
                }

                attributeType.Name = FindClassFullName(attributeType.Name + suffix,
                    namespaceModel, projectModel, solutionModel, repositoryModel,
                    importTypes, filePath);
            });

            AddAmbiguousNames(() =>
            {
                attributeType.ContainingTypeName = FindClassFullName(attributeType.ContainingTypeName,
                    namespaceModel, projectModel, solutionModel, repositoryModel,
                    importTypes, filePath);
            });

            foreach (var parameterType in attributeType.ParameterTypes)
            {
                SetParameterFullName(repositoryModel, namespaceModel, projectModel, solutionModel,
                    importTypes, filePath, parameterType);
            }
        }

        private void SetParameterFullName(RepositoryModel repositoryModel, NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath,
            IParameterType parameterType)
        {
            AddAmbiguousNames(() =>
            {
                SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                    filePath, parameterType.Type);
            });

            foreach (var attributeType in parameterType.Attributes)
            {
                SetAttributeFullName(repositoryModel, namespaceModel, projectModel, solutionModel,
                    importTypes, filePath, attributeType);
            }
        }

        private void SetEntityTypeFullName(RepositoryModel repositoryModel, NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath,
            IEntityType entityType)
        {
            AddAmbiguousNames(() =>
            {
                SetGenericTypeFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                    filePath, entityType.FullType);

                if (entityType.FullType != null)
                {
                    entityType.Name = ReconstructFullName(entityType.FullType);
                }
                else
                {
                    AddAmbiguousNames(() =>
                    {
                        entityType.Name = FindClassFullName(entityType.Name,
                            namespaceModel, projectModel, solutionModel, repositoryModel,
                            importTypes, filePath);
                    });
                }
            });

            string ReconstructFullName(GenericType genericType)
            {
                if (genericType == null)
                {
                    return "";
                }

                var stringBuilder = new StringBuilder();
                var name = genericType.Name;


                AddAmbiguousNames(() =>
                {
                    name = FindClassFullName(name,
                        namespaceModel, projectModel, solutionModel, repositoryModel,
                        importTypes, filePath, genericType.ContainedTypes.Count);
                });


                stringBuilder.Append(name);

                if (genericType.ContainedTypes.Count <= 0)
                {
                    return stringBuilder.ToString();
                }

                stringBuilder.Append('<');
                for (var i = 0; i < genericType.ContainedTypes.Count; i++)
                {
                    var containedType = genericType.ContainedTypes[i];
                    stringBuilder.Append(ReconstructFullName(containedType));
                    if (i != genericType.ContainedTypes.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Append('>');

                return stringBuilder.ToString();
            }
        }

        private void SetGenericTypeFullName(RepositoryModel repositoryModel, NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath,
            GenericType genericType)
        {
            if (genericType == null || string.IsNullOrEmpty(genericType.Name))
            {
                return;
            }

            AddAmbiguousNames(() =>
            {
                genericType.Name = FindClassFullName(genericType.Name, namespaceModel, projectModel,
                    solutionModel, repositoryModel, importTypes, filePath, genericType.ContainedTypes.Count);

                foreach (var type in genericType.ContainedTypes)
                {
                    SetGenericTypeFullName(repositoryModel, namespaceModel, projectModel, solutionModel,
                        importTypes,
                        filePath, type);
                }
            });
        }

        private void SetLocalVariablesFullName(RepositoryModel repositoryModel, NamespaceModel namespaceModel,
            ProjectModel projectModel, SolutionModel solutionModel, IList<IImportType> importTypes, string filePath,
            ITypeWithLocalVariables typeWithLocalVariables)
        {
            foreach (var localVariableType in typeWithLocalVariables.LocalVariableTypes)
            {
                SetEntityTypeFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                    filePath, localVariableType.Type);
            }
        }

        private void SetFullNameForCalledMethods(string classFilePath,
            IEnumerable<IMethodSignatureType> methodModelCalledMethods,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel,
            RepositoryModel repositoryModel, IList<IImportType> importTypes)
        {
            foreach (var calledMethod in methodModelCalledMethods)
            {
                foreach (var parameterModel in calledMethod.ParameterTypes)
                {
                    SetParameterFullName(repositoryModel, namespaceModel, projectModel, solutionModel, importTypes,
                        classFilePath, parameterModel);
                }

                AddAmbiguousNames(() =>
                {
                    calledMethod.ContainingTypeName = FindClassFullName(
                        calledMethod.ContainingTypeName, namespaceModel, projectModel, solutionModel,
                        repositoryModel, importTypes, classFilePath);

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
                            repositoryModel, importTypes);
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

                var staticImportedClassType =
                    GetClassModelFullyQualified(importType.Name, projectModel, solutionModel, repositoryModel);

                if (staticImportedClassType == null)
                {
                    continue;
                }

                if (staticImportedClassType is not ClassModel staticImportedClass)
                {
                    continue;
                }

                foreach (var fieldModel in staticImportedClass.Fields)
                {
                    if (fieldModel.Name == containingClassName)
                    {
                        return fieldModel.Type.Name;
                    }
                }

                foreach (var propertyModel in staticImportedClass.Properties)
                {
                    if (propertyModel.Name == containingClassName)
                    {
                        return propertyModel.Type.Name;
                    }
                }

                foreach (var methodModel in staticImportedClass.Methods)
                {
                    var hasTheSameSignature = MethodsHaveTheSameSignature(methodModel.Name,
                        methodModel.ParameterTypes,
                        methodName, methodParameters);
                    if (hasTheSameSignature)
                    {
                        return staticImportedClass.Name;
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

        private void ChangeDependencyMetricFullName(RepositoryModel repositoryModel,
            IPropertyMembersClassType classModel,
            NamespaceModel namespaceModel, ProjectModel projectModel, SolutionModel solutionModel)
        {
            var parameterDependenciesMetrics = classModel.Metrics.Where(metric =>
                typeof(IRelationVisitor).IsAssignableFrom(Type.GetType(metric.ExtractorName)));

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
                            _ambiguousNames.TryAdd(e.AmbiguousName, e.PossibleNames);
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

                    var fullNamePossibilities = GetPossibilitiesFromNamespace(usingModel.Name, tempName, 0);
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
            RepositoryModel repositoryModel, IList<IImportType> usingModels, string classFilePath = "",
            int genericParameterCount = 0)
        {
            if (string.IsNullOrEmpty(className))
            {
                return className;
            }

            if (_notFoundClassNames.ContainsKey(className))
            {
                return className;
            }

            if (CSharpConstants.IsPrimitive(className))
            {
                return className;
            }

            var classFullName = className;
            var nullableString = "";
            if (className.EndsWith("?"))
            {
                nullableString = "?";
                classFullName = classFullName.Replace("?", "");
            }

            if (_repositoryClassSet.Contains(projectModelToStartSearchFrom.FilePath, classFullName))
            {
                return classFullName + nullableString;
            }

            if (IsNameFullyQualified(classFullName))
            {
                return classFullName + nullableString;
            }

            // try to find class in provided namespace
            if (namespaceModelToStartSearchFrom.ClassModels.Any(classModel => classModel.Name == classFullName))
            {
                return AddClassModelToNamespaceGraph(classFullName, classFilePath,
                    namespaceModelToStartSearchFrom.Name) + nullableString;
            }

            if (namespaceModelToStartSearchFrom.ClassModels.Any(classModel =>
                classModel.Name == $"{namespaceModelToStartSearchFrom.Name}.{classFullName}"))
            {
                return AddClassModelToNamespaceGraph(classFullName, classFilePath,
                    namespaceModelToStartSearchFrom.Name) + nullableString;
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
                    if (usingModel.AliasType == nameof(EAliasType.Class) && classFullName == usingModel.Alias)
                    {
                        return usingModel.Name + nullableString;
                    }

                    var tempName = classFullName;
                    if (usingModel.AliasType == nameof(EAliasType.Namespace) &&
                        classFullName.StartsWith(usingModel.Alias))
                    {
                        tempName = classFullName.Replace(usingModel.Alias, usingModel.Name);
                    }

                    foreach (var name in
                        GetPossibilitiesFromNamespace(usingModel.Name, tempName, genericParameterCount))
                    {
                        fullNamePossibilities.Add(name);
                    }

                    switch (fullNamePossibilities.Count)
                    {
                        case 1:
                            return fullNamePossibilities.First() + nullableString;
                        case > 1:
                            throw new AmbiguousFullNameException(new AmbiguousName
                            {
                                Name = classFullName,
                                Location = classFilePath
                            }, fullNamePossibilities);
                    }
                }
            }

            // search in current namespace for classes that used a class located in the parent namespace 
            foreach (var name in GetPossibilitiesFromNamespace(namespaceModelToStartSearchFrom.Name, classFullName,
                genericParameterCount))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First() + nullableString;
                case > 1:
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = classFullName,
                        Location = classFilePath
                    }, fullNamePossibilities);
            }

            foreach (var name in
                TrySearchingInProjectModel(classFullName, classFilePath, projectModelToStartSearchFrom,
                    genericParameterCount))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First() + nullableString;
                case > 1:
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = classFullName,
                        Location = classFilePath
                    }, fullNamePossibilities);
            }

            foreach (var name in TrySearchingInSolutionModel(classFullName, classFilePath,
                solutionModelToStartSearchFrom, projectModelToStartSearchFrom, genericParameterCount))
            {
                fullNamePossibilities.Add(name);
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First() + nullableString;
                case > 1:
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = classFullName,
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

                foreach (var name in TrySearchingInSolutionModel(classFullName, classFilePath, solutionModel,
                    projectModelToStartSearchFrom, genericParameterCount))
                {
                    fullNamePossibilities.Add(name);
                }

                if (fullNamePossibilities.Count > 1)
                {
                    throw new AmbiguousFullNameException(new AmbiguousName
                    {
                        Name = classFullName,
                        Location = classFilePath
                    }, fullNamePossibilities);
                }
            }

            switch (fullNamePossibilities.Count)
            {
                case 1:
                    return fullNamePossibilities.First() + nullableString;
                case > 1:
                    throw new AmbiguousFullNameException(
                        new AmbiguousName { Name = classFullName, Location = classFilePath },
                        fullNamePossibilities);
                default:
                {
                    _notFoundClassNames.TryAdd(classFullName, classFullName);
                    return classFullName + nullableString;
                }
            }
        }

        private IEnumerable<string> TrySearchingInSolutionModel(string className, string classFilePath,
            SolutionModel solutionModel, ProjectModel projectModelToBeIgnored, int genericParameterCount)
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


                foreach (var name in TrySearchingInProjectModel(className, classFilePath, projectModel,
                    genericParameterCount))
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
            ProjectModel projectModel, int genericParameterCount)
        {
            var fullNamePossibilities = new HashSet<string>();
            foreach (var namespaceModel in projectModel.Namespaces)
            {
                foreach (var name in GetPossibilitiesFromNamespace(namespaceModel.Name, className,
                    genericParameterCount))
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

        private IEnumerable<string> GetPossibilitiesFromNamespace(string namespaceName, string className,
            int genericParametersCount)
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

            return childNamespace.GetPossibleChildren(className, genericParametersCount);
        }

        private NamespaceTree GetClassInNamespaceAmongChildrenNamespaces(NamespaceTree namespaceTree,
            string className)
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

                NamespacesDictionary.TryAdd(namespaceNameParts[0], rootNamespaceTree);
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


        private static IClassType GetClassModelFullyQualified(string classNameToSearch,
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

        private static IClassType GetClassModelFullyQualified(IReadOnlyList<string> classNameParts,
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
